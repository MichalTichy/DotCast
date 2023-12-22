using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Infrastructure.Persistence.Base.Specifications;
using DotCast.Infrastructure.Persistence.Marten.SessionFactory;
using DotCast.Infrastructure.Persistence.Marten.UnitOfWorks;
using Marten;
using Marten.Linq;
using Microsoft.Extensions.Logging;

namespace DotCast.Infrastructure.Persistence.Marten.Repository.Document
{
    public class MartenRepository<T> : IRepository<T> where T : notnull
    {
        protected readonly ILogger<MartenRepository<T>> Logger;
        protected readonly IAsyncSessionFactory SessionFactory;

        public MartenRepository(
            IAsyncSessionFactory sessionFactory,
            ILogger<MartenRepository<T>> logger)
        {
            SessionFactory = sessionFactory;
            Logger = logger;
        }

        public virtual async Task AddAsync(ICollection<T> entities, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.OpenSessionAsync(uow.Get());
            session.Insert(entities.AsEnumerable());

            await SaveChangesAsync(session, cancellationToken: cancellationToken);

            await uow.CommitAsync();
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.OpenSessionAsync(uow.Get());

            session.Insert(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);

            await uow.CommitAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.OpenSessionAsync(uow.Get());

            session.Update(entity);
            await SaveChangesAsync(session, cancellationToken: cancellationToken);

            await uow.CommitAsync();
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.OpenSessionAsync(uow.Get());

            session.Delete(entity);

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
            await uow.CommitAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.OpenSessionAsync(uow.Get());

            foreach (var entity in entities)
            {
                session.Delete(entity);
            }

            await SaveChangesAsync(session, cancellationToken: cancellationToken);
            await uow.CommitAsync();
        }

        public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.QuerySessionAsync(uow.Get());

            var result = await session.LoadAsync<T>(id, cancellationToken);

            await uow.CommitAsync();
            return result;
        }

        public virtual async Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.QuerySessionAsync(uow.Get());

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            var result = await specification.ApplyAsync(queryable, cancellationToken);

            await uow.CommitAsync();
            return result;
        }

        public virtual async Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.QuerySessionAsync(uow.Get());


            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            var result = await specification.ApplyAsync(queryable, cancellationToken);

            await uow.CommitAsync();
            return result;
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.QuerySessionAsync(uow.Get());

            var result = await session.Query<T>().ToListAsync(cancellationToken);

            await uow.CommitAsync();
            return result;
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(IListSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.QuerySessionAsync(uow.Get());

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            var aggregates = await specification.ApplyAsync(queryable, cancellationToken);

            await uow.CommitAsync();
            return aggregates;
        }

        public virtual async Task<IReadOnlyList<TResult>> ListAsync<TResult>(IListSpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.QuerySessionAsync(uow.Get());

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            var results = await specification.ApplyAsync(queryable, cancellationToken);

            await uow.CommitAsync();
            return results;
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            await using var uow = new UnitOfWorkProvider();
            using var session = await SessionFactory.QuerySessionAsync(uow.Get());

            var queryable = session.Query<T>();
            queryable = PreprocessQuery(queryable);

            var result = await queryable.CountAsync(cancellationToken);

            await uow.CommitAsync();
            return result;
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