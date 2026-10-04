using DotCast.App.API.PersonalApiTokens;
using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.AppUser.Identity;
using DotCast.Infrastructure.Blazor.ClaimsManagement;
using DotCast.Infrastructure.CurrentTenancyProvider;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.CurrentUserProvider.Blazor;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Messaging.Wolverine;
using DotCast.Infrastructure.Persistence.Marten.Extensions;
using DotCast.Infrastructure.PersonalApiTokens;
using DotCast.Infrastructure.PersonalApiTokens.Authentication;
using DotCast.Infrastructure.PersonalApiTokens.Models;
using DotCast.Infrastructure.PersonalApiTokens.Persistence;
using DotCast.Infrastructure.PersonalApiTokens.UseCases;
using DotCast.Infrastructure.UserManagement.Abstractions;
using DotCast.Library;
using DotCast.Library.Mcp.Hosting;
using DotCast.Library.Mcp.UseCases;
using Marten;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;
using Wolverine;
using Xunit;
namespace DotCast.Library.Mcp.Tests;
public sealed class McpHostFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder("postgres:17-alpine").Build();
    public WebApplication App { get; private set; } = null!;
    public Uri Address { get; private set; } = null!;
    public TestTokenOwnerResolver Owners { get; } = new();
    public TestLogProvider Logs { get; } = new();
    public async Task InitializeAsync()
    {
        await database.StartAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration["Mcp:AllowLoopbackHttp"] = "true";
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(Logs);
        builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddCookie(IdentityConstants.ApplicationScheme);
        builder.Services.AddAuthorization();
        builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
        builder.Services.AddControllersWithViews().AddApplicationPart(typeof(PersonalApiTokensController).Assembly);
        builder.Services.AddSingleton<ITokenOwnerResolver>(Owners);
        var users = new Mock<IUserManager<UserInfo>>();
        users.Setup(u => u.GetUserAsync(It.IsAny<string>())).Returns((string id) => Task.FromResult(Owners.Users.GetValueOrDefault(id)));
        builder.Services.AddSingleton(users.Object);
        builder.Services.AddSingleton<IUserRoleManager<UserRole>, UserRoleManager>();
        builder.Services.AddScoped<IUserClaimsProvider, UserClaimsProvider>();
        builder.Services.AddScoped<IHttpContextUserInfoSetter, HttpContextUserInfoSetter>();
        builder.Services.AddScoped<ICurrentUserProvider<UserInfo>, BlazorUserInfoProvider<UserInfo, UserRole>>();
        builder.Services.AddSingleton<ICurrentTenancyProvider, CurrentTenancyProviderNoTenancy>();
        builder.Services.AddSingleton<DotCast.Infrastructure.BookInfoProvider.Base.IBookInfoProvider, TestBookInfoProvider>();
        new PersonalApiTokensInstaller().Install(builder.Services, builder.Configuration, false);
        // Replace the production resolver's Identity dependency with the mutable fixture owner directory.
        builder.Services.RemoveAll<ITokenOwnerResolver>();
        builder.Services.AddSingleton<ITokenOwnerResolver>(Owners);
        builder.Services.AddScoped<LibraryTokenAccess>();
        builder.Services.AddScoped<DotCast.Library.Mcp.Persistence.ITransactionalAudioBookMetadataWriter, DotCast.Library.Mcp.Persistence.TransactionalAudioBookMetadataWriter>();
        builder.Services.AddTransient<DotCast.Infrastructure.Persistence.Marten.StorageConfiguration.IStorageConfiguration, DotCast.Library.Storage.AudioBookStorageConfiguration>();
        builder.Services.AddScoped<DotCast.Infrastructure.Persistence.Repositories.IReadOnlyRepository<DotCast.SharedKernel.Models.AudioBook>, AudioBookRepository>();
        builder.Services.AddNpgsqlDataSource(database.GetConnectionString());
        builder.Services.AddMartenPostgresPersistence();
        builder.Services.AddTransient<IMessagePublisher, WolverineMessagePublisher>();
        builder.Services.AddLibraryMcp();
        builder.Host.UseWolverine(o => {
            o.Discovery.DisableConventionalDiscovery();
            o.Discovery.IncludeType<SearchAudioBooksHandler>();
            o.Discovery.IncludeType<GetAudioBookMetadataHandler>();
            o.Discovery.IncludeType<DotCast.BookInfoProvider.AudiobookInfoSuggestionsRequestHandler>();
            o.Discovery.IncludeType<UpdateAudioBookMetadataHandler>();
            o.Discovery.IncludeType<CreatePersonalTokenHandler>();
            o.Discovery.IncludeType<ListPersonalTokensHandler>();
            o.Discovery.IncludeType<RevokePersonalTokenHandler>();
            o.Policies.AddMiddleware<UserIdSetterWolverineMiddleware>();
        });
        App = builder.Build();
        App.UseRouting(); App.UseAuthentication(); App.UseLibraryMcpBoundary(); App.UseAuthorization();
        App.UseMiddleware<UserClaimsMiddleware>();
        App.MapControllers(); App.MapLibraryMcp();
        App.MapGet("/fixture/login/{id}", async (HttpContext http, string id) => {
            await http.SignInAsync(IdentityConstants.ApplicationScheme, Owners.Users[id].GetClaimsIdentity());
            return Results.Ok();
        });
        await App.StartAsync();
        Address = new Uri(App.Urls.Single());
        await SeedAsync();
    }
    public async Task SeedAsync()
    {
        Owners.Users["owner"] = new UserInfo { Id = "owner", Email = "owner@example.test", Name = "Owner", UsersLibraryName = "own", SharedLibraries = ["shared"] };
        await using var session = App.Services.GetRequiredService<IDocumentStore>().LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        session.Store(MetadataPatchTests.Book("own-book", "own"), MetadataPatchTests.Book("shared-book", "shared"), MetadataPatchTests.Book("foreign-book", "foreign"));
        await session.SaveChangesAsync();
    }
    public async Task<(string Credential, string Id)> IssueAsync(bool canWrite = false, DateTimeOffset? expiry = null)
    {
        var generated = PersonalTokenCredential.Generate();
        using var scope = App.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IPersonalApiTokenStore>().AddAsync(new PersonalApiToken {
            Id = generated.Id, OwnerId = "owner", Name = "test", SecretHash = generated.Hash, CanWrite = canWrite,
            CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = expiry ?? DateTimeOffset.UtcNow.AddDays(90)
        }, CancellationToken.None);
        return (generated.Credential, generated.Id);
    }
    public async Task DisposeAsync()
    {
        if (App is not null) await App.DisposeAsync();
        await database.DisposeAsync();
    }
}
