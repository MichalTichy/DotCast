using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Infrastructure.Persistence.Base.Specifications;
using Marten;
using Marten.Linq;
using Microsoft.Extensions.Logging;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenRepository<T> : IRepository<T> where T : notnull
    {
        protected readonly ILogger<MartenRepository<T>> Logger;
        protected readonly IAsyncSessionFactory SessionFactory;

        public MartenRepository(
            ILogger<MartenRepository<T>> logger,
            IAsyncSessionFactory sessionFactory)
        {
            Logger = logger;
            SessionFactory = sessionFactory;
        }

        public virtual async Task AddAsync(ICollection<T> entities, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.OpenSessionAsync();
            session.Insert(entities.AsEnumerable());

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.OpenSessionAsync();

            session.Insert(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);

            return entity;
        }

        public async Task<T> StoreAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.OpenSessionAsync();

            session.Store(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);

            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.OpenSessionAsync();

            session.Update(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task ForceUpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.OpenSessionAsync();

            session.Delete(entity);
            session.Insert(entity);

            await SaveChangesAsync(session, true, cancellationToken);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.OpenSessionAsync();
            session.Delete(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.OpenSessionAsync();

            foreach (var entity in entities)
            {
                session.Delete(entity);
            }

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.QuerySessionAsync();

            return await session.LoadAsync<T>(id, cancellationToken);
        }

        public virtual async Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.QuerySessionAsync();

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            return await specification.ApplyAsync(queryable, cancellationToken);
        }

        public virtual async Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.QuerySessionAsync();


            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            return await specification.ApplyAsync(queryable, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.QuerySessionAsync();
            return await session.Query<T>().ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(IListSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.QuerySessionAsync();

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            var aggregates = await specification.ApplyAsync(queryable, cancellationToken);


            return aggregates;
        }

        public virtual async Task<IReadOnlyList<TResult>> ListAsync<TResult>(IListSpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.QuerySessionAsync();

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            return await specification.ApplyAsync(queryable, cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            await using var session = await SessionFactory.QuerySessionAsync();

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
