using System;
using Ardalis.GuardClauses;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Infrastructure.Persistence.Marten.Repository.Document;
using DotCast.Infrastructure.Persistence.Marten.SessionFactory;
using DotCast.Infrastructure.Persistence.Marten.UnitOfWorks;
using DotCast.Infrastructure.UnitOfWorkBase;
using Marten;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Weasel.Core;

namespace DotCast.Infrastructure.Persistence.Marten.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMartenPostgresPersistence(this IServiceCollection services,
            string connectionString,
            Action<StoreOptions> configureStore)
        {
            Guard.Against.NullOrWhiteSpace(connectionString, nameof(connectionString));

            var serializer = new JsonNetSerializer
            {
                EnumStorage = EnumStorage.AsString
            };

            serializer.Customize(jsonSerializer =>
            {
                //some serialization options are set inside the resolver!

                jsonSerializer.TypeNameHandling = TypeNameHandling.Objects;

                jsonSerializer.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
            });

            var configurationExpression = services.AddMarten(options =>
            {
                options.Serializer(serializer);

                options.Connection(connectionString);

                configureStore.Invoke(options);

                options.AutoCreateSchemaObjects = AutoCreate.None;

                options.CreateDatabasesForTenants(expressions => { expressions.ForTenant().ConnectionLimit(-1).CheckAgainstPgDatabase(); });
            });
            configurationExpression.ApplyAllDatabaseChangesOnStartup();
            configurationExpression.AssertDatabaseMatchesConfigurationOnStartup();
            //configurationExpression.AddAsyncDaemon(DaemonMode.HotCold);

            services.AddTransient<IAsyncSessionFactory, AppMartenSessionFactory>();
            services.AddSingleton<IConnectionFactory, ConnectionFactory>(provider => new ConnectionFactory(connectionString));

            services.Scan(
                selector => selector.FromCallingAssembly()
                    .AddClasses(filter =>
                        filter.AssignableTo(typeof(MartenRepository<>)))
                    .As(typeof(IRepository<>))
                    .As(typeof(IReadOnlyRepository<>))
                    .WithScopedLifetime());


            services.AddSingleton<IUnitOfWorkExecutor, UnitOfWorkExecutor>();
            return services;
        }
    }
}
