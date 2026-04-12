using DotCast.Infrastructure.Persistence.Marten.SessionFactory;
using DotCast.Infrastructure.Persistence.Repositories;

namespace DotCast.Infrastructure.Persistence.Marten.Repository.Document;

public interface INoTenancyRepository<T> : IRepository<T> where T : IItemWithId;

public interface INoTenancyReadOnlyRepository<T> : IReadOnlyRepository<T> where T : IItemWithId;

public class NoTenancyMartenRepository<T>(INoTenancyByDefaultSessionFactory sessionFactory)
    : MartenRepository<T>(sessionFactory), INoTenancyRepository<T>, INoTenancyReadOnlyRepository<T> where T : IItemWithId;
