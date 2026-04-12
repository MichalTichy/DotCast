using System.Data;
using Marten;
using Marten.Services;
using Npgsql;
using DotCast.Infrastructure.CurrentTenancyProvider;

namespace DotCast.Infrastructure.Persistence.Marten.SessionFactory;

public class AppMartenSessionFactory(IDocumentStore store, ICurrentTenancyProvider currentTenancyProvider) : IAsyncSessionFactory
{
    protected readonly ICurrentTenancyProvider CurrentTenancyProvider = currentTenancyProvider;
    protected readonly IDocumentStore Store = store;

    public virtual async Task<IQuerySession> QuerySessionAsync(UnitOfWorks.UnitOfWork unitOfWork)
    {
        var tenantId = await GetTenantIdAsync();
        var sessionOptions = await CreateSessionOptionsAsync(tenantId, unitOfWork);

        return Store.QuerySession(sessionOptions);
    }

    public virtual async Task<IDocumentSession> OpenSessionAsync(UnitOfWorks.UnitOfWork unitOfWork)
    {
        var tenantId = await GetTenantIdAsync();
        var sessionOptions = await CreateSessionOptionsAsync(tenantId, unitOfWork);

        return await Store.OpenSerializableSessionAsync(sessionOptions);
    }

    protected virtual async Task<SessionOptions> CreateSessionOptionsAsync(string? tenantId, UnitOfWorks.UnitOfWork unitOfWork)
    {
        await unitOfWork.EnsureInitializedAsync(GetOpenConnectionAsync, GetIsolationLevel());
        var options = SessionOptions.ForTransaction(unitOfWork.Transaction!);

        options.IsolationLevel = GetIsolationLevel();

        if (tenantId != null)
        {
            options.TenantId = tenantId;
        }

        return options;
    }

    private async Task<NpgsqlConnection> GetOpenConnectionAsync()
    {
        var connection = Store.Storage.Database.CreateConnection();
        await connection.OpenAsync();
        return connection;
    }

    private IsolationLevel GetIsolationLevel()
    {
        return IsolationLevel.ReadCommitted;
    }

    private async Task<string> GetTenantIdAsync()
    {
        return await CurrentTenancyProvider.GetUserTenantAsync();
    }
}
