using DotCast.Infrastructure.AppUser;
using DotCast.Infrastructure.AppUser.Identity;
using DotCast.Infrastructure.Blazor.ClaimsManagement;
using DotCast.Infrastructure.CurrentTenancyProvider;
using DotCast.Infrastructure.CurrentUserProvider;
using DotCast.Infrastructure.CurrentUserProvider.Blazor;
using DotCast.Infrastructure.Messaging.Base;
using DotCast.Infrastructure.Messaging.Wolverine;
using DotCast.Infrastructure.Persistence.Marten.Extensions;
using DotCast.Infrastructure.UserManagement.Abstractions;
using DotCast.Library;
using DotCast.Infrastructure.ApiKeys;
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
    public TestApiKeyOwnerResolver Owners { get; } = new();
    public TestLogProvider Logs { get; } = new();
    public async Task InitializeAsync()
    {
        await database.StartAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(Logs);
        builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddCookie(IdentityConstants.ApplicationScheme);
        builder.Services.AddAuthorization();
        var users = new Mock<IUserManager<UserInfo>>();
        users.Setup(u => u.GetUserAsync(It.IsAny<string>())).Returns((string id) => Task.FromResult(Owners.Users.GetValueOrDefault(id)));
        builder.Services.AddSingleton(users.Object);
        builder.Services.AddSingleton<IUserRoleManager<UserRole>, UserRoleManager>();
        builder.Services.AddScoped<IUserClaimsProvider, UserClaimsProvider>();
        builder.Services.AddScoped<IHttpContextUserInfoSetter, HttpContextUserInfoSetter>();
        builder.Services.AddScoped<ICurrentUserProvider<UserInfo>, BlazorUserInfoProvider<UserInfo, UserRole>>();
        builder.Services.AddSingleton<ICurrentTenancyProvider, CurrentTenancyProviderNoTenancy>();
        builder.Services.AddSingleton<DotCast.Infrastructure.BookInfoProvider.Base.IBookInfoProvider, TestBookInfoProvider>();
        new ApiKeysInstaller().Install(builder.Services, builder.Configuration, false);
        builder.Services.AddLibraryMcp();
        builder.Services.RemoveAll<IApiKeyOwnerResolver>();
        builder.Services.AddSingleton<IApiKeyOwnerResolver>(Owners);
        builder.Services.AddTransient<DotCast.Infrastructure.Persistence.Marten.StorageConfiguration.IStorageConfiguration, DotCast.Library.Storage.AudioBookStorageConfiguration>();
        builder.Services.AddScoped<DotCast.Infrastructure.Persistence.Repositories.IReadOnlyRepository<DotCast.SharedKernel.Models.AudioBook>, AudioBookRepository>();
        builder.Services.AddScoped<DotCast.Infrastructure.Persistence.Repositories.IRepository<DotCast.SharedKernel.Models.AudioBook>, AudioBookRepository>();
        builder.Services.AddNpgsqlDataSource(database.GetConnectionString());
        builder.Services.AddMartenPostgresPersistence();
        builder.Services.AddTransient<IMessagePublisher, WolverineMessagePublisher>();
        builder.Host.UseWolverine(o => {
            o.Discovery.DisableConventionalDiscovery();
            o.Discovery.IncludeType<SearchAudioBooksHandler>();
            o.Discovery.IncludeType<DotCast.Library.Handlers.AudioBooksRetrievalRequestHandler>();
            o.Discovery.IncludeType<DotCast.Library.Handlers.AudioBookDetailRequestHandler>();
            o.Discovery.IncludeType<GetAudioBookMetadataHandler>();
            o.Discovery.IncludeType<DotCast.BookInfoProvider.AudiobookInfoSuggestionsRequestHandler>();
            o.Discovery.IncludeType<UpdateAudioBookMetadataHandler>();
            o.Discovery.IncludeType<GenerateApiKeyHandler>();
            o.Discovery.IncludeType<RevokeApiKeyHandler>();
            o.Policies.AddMiddleware<UserIdSetterWolverineMiddleware>();
        });
        App = builder.Build();
        App.UseRouting(); App.UseAuthentication(); App.UseLibraryMcpBoundary(); App.UseAuthorization();
        App.UseMiddleware<UserClaimsMiddleware>();
        App.MapLibraryMcp();
        App.MapGet("/fixture/login/{id}", async (HttpContext http, string id) => {
            await http.SignInAsync(IdentityConstants.ApplicationScheme, Owners.Users[id].GetClaimsIdentity());
            return Results.Ok();
        });
        await App.StartAsync();
        Address = new Uri(App.Urls.Single());
        Owners.Users["owner"] = new UserInfo { Id = "owner", Email = "owner@example.test", Name = "Owner", UsersLibraryName = "own", SharedLibraries = ["shared"] };
        Owners.Users["other"] = new UserInfo { Id = "other", Email = "other@example.test", Name = "Other", UsersLibraryName = "foreign", SharedLibraries = [] };
        await using var session = App.Services.GetRequiredService<IDocumentStore>().LightweightSession(CurrentTenancyProviderNoTenancy.NoTenancyName);
        session.Store(MetadataPatchTests.Book("own-book", "own"), MetadataPatchTests.Book("shared-book", "shared"), MetadataPatchTests.Book("foreign-book", "foreign"));
        await session.SaveChangesAsync();
    }
    public async Task<T> RunAsAsync<T>(string? ownerId, Func<IMessagePublisher, Task<T>> operation)
    {
        using var scope = App.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IUserClaimsProvider>().User = ownerId is null ? null : Owners.Users[ownerId].GetClaimsIdentity();
        return await operation(scope.ServiceProvider.GetRequiredService<IMessagePublisher>());
    }
    public Task<string> IssueAsync(string ownerId = "owner") =>
        RunAsAsync(ownerId, messenger => messenger.RequestAsync<GenerateApiKey, string>(new()));
    public Task RevokeAsync(string ownerId = "owner") =>
        RunAsAsync(ownerId, async messenger => { await messenger.ExecuteAsync(new RevokeApiKey()); return true; });
    public async Task DisposeAsync()
    {
        if (App is not null) await App.DisposeAsync();
        await database.DisposeAsync();
    }
}
