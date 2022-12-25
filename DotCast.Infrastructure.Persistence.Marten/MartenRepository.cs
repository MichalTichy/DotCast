using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotCast.Infrastructure.Persistence.Base;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Infrastructure.Persistence.Base.Specifications;
using Marten;
using Marten.Linq;
using Microsoft.Extensions.Logging;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenRepository<T, TId> : IRepository<T, TId> where T : IItemWithId<TId>
    {
        protected readonly ILogger<MartenRepository<T, TId>> Logger;
        protected readonly ISessionFactoryWithAlternateTenantSettings SessionFactory;

        public MartenRepository(
            ISessionFactoryWithAlternateTenantSettings sessionFactory,
            ILogger<MartenRepository<T, TId>> logger)
        {
            SessionFactory = sessionFactory;
            Logger = logger;
        }

        public virtual async Task AddAsync(ICollection<T> entities, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.OpenSessionAsync(tenantId);
            session.Insert(entities.AsEnumerable());

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.OpenSessionAsync(tenantId);

            session.Insert(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);

            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.OpenSessionAsync(tenantId);

            session.Update(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task ForceUpdateAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.OpenSessionAsync(tenantId);

            session.Delete(entity);
            session.Insert(entity);

            await SaveChangesAsync(session, true, cancellationToken);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.OpenSessionAsync();
            session.Delete(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities,
            CancellationToken cancellationToken = default,
            string? tenantId = null)
        {
            await using var session = await SessionFactory.OpenSessionAsync(tenantId);

            foreach (var entity in entities)
            {
                session.Delete(entity);
            }

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.QuerySessionAsync(tenantId);

            return await session.LoadAsync<T>(id, cancellationToken);
        }

        public virtual async Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.QuerySessionAsync(tenantId);

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            return await specification.ApplyAsync(queryable, cancellationToken);
        }

        public virtual async Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.QuerySessionAsync(tenantId);


            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            return await specification.ApplyAsync(queryable, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.QuerySessionAsync(tenantId);
            return await session.Query<T>().ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(IListSpecification<T> specification, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.QuerySessionAsync(tenantId);

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            var aggregates = await specification.ApplyAsync(queryable, cancellationToken);


            return aggregates;
        }

        public virtual async Task<IReadOnlyList<TResult>> ListAsync<TResult>(IListSpecification<T, TResult> specification, CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.QuerySessionAsync(tenantId);

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            return await specification.ApplyAsync(queryable, cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default, string? tenantId = null)
        {
            await using var session = await SessionFactory.QuerySessionAsync(tenantId);

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            return await queryable.CountAsync(cancellationToken);
        }

        protected virtual async Task SaveChangesAsync(IDocumentSession session, bool force = false, CancellationToken cancellationToken = default)
        {
            await session.SaveChangesAsync(cancellationToken);
        }

        public virtual IMartenQueryable<T> PreprocessQuery(IMartenQueryable<T> queryable)
        {
            return queryable;
        }
    }
}