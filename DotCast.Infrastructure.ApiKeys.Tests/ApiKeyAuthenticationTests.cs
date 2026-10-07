using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.CurrentTenancyProvider;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.Persistence.Marten.Extensions;
using DotCast.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;
namespace DotCast.Infrastructure.ApiKeys.Tests;
public sealed class ApiKeyAuthenticationTests
{
    [Fact]
    public async Task KeysAuthenticateAnOrdinaryEndpointWithoutMcpAndSupportReplacementAndRevocation()
    {
        await using var database = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await database.StartAsync();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        new ApiKeysInstaller().Install(builder.Services, builder.Configuration, false);
        builder.Services.AddAuthorization();
        var owner = new UserInfo { Id = "owner", Name = "Owner", Email = "owner@example.test", UsersLibraryName = "own" };
        var currentUser = new Mock<ICurrentUserProvider<UserInfo>>();
        currentUser.Setup(u => u.GetCurrentUserRequiredAsync()).ReturnsAsync(owner);
        var owners = new Mock<IApiKeyOwnerResolver>();
        owners.Setup(o => o.FindAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        builder.Services.AddSingleton(owners.Object);
        builder.Services.AddSingleton<ICurrentTenancyProvider, CurrentTenancyProviderNoTenancy>();
        builder.Services.AddNpgsqlDataSource(database.GetConnectionString());
        builder.Services.AddMartenPostgresPersistence();
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/account", (HttpContext context) => Results.Ok(context.User.FindFirstValue(ClaimTypes.NameIdentifier)))
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(ApiKeyDefaults.Scheme).RequireAuthenticatedUser());
        await app.StartAsync();
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        using var scope = app.Services.CreateScope();
        var keys = scope.ServiceProvider.GetRequiredService<IRepository<AccountApiKey>>();
        var generate = new GenerateApiKeyHandler(keys, currentUser.Object, NullLogger<GenerateApiKeyHandler>.Instance);
        var revoke = new RevokeApiKeyHandler(keys, currentUser.Object, NullLogger<RevokeApiKeyHandler>.Instance);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync("/account")).StatusCode);
        var originalKey = await generate.Handle(new(), CancellationToken.None);

        Assert.Equal(owner.Id, (await keys.GetByIdAsync(owner.Id))!.Id);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", originalKey);
        Assert.Equal("\"owner\"", await http.GetStringAsync("/account"));
        var replacement = await generate.Handle(new(), CancellationToken.None);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync("/account")).StatusCode);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", replacement);
        Assert.Equal("\"owner\"", await http.GetStringAsync("/account"));
        await revoke.Handle(new(), CancellationToken.None);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync("/account")).StatusCode);
        Assert.Null(await keys.GetByIdAsync(owner.Id));
    }
}
