using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ardalis.GuardClauses;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using Marten;
using Marten.Services;
using Marten.Services.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Weasel.Core;

namespace DotCast.Infrastructure.Persistence.Marten
{
    public class MartenLikeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (JsonNetCollectionToArrayJsonConverter.Instance.CanConvert(property.PropertyType))
                property.Converter = JsonNetCollectionToArrayJsonConverter.Instance;

            var propertyInfo = member as PropertyInfo;
            if ((object) propertyInfo != null)
            {
                property.Readable = propertyInfo.GetMethod != null;
                property.Writable = propertyInfo.SetMethod != null;
                return property;
            }

            return property;
        }
    }

    public class DomainContractResolver : MartenLikeContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Select(p => base.CreateProperty(p, memberSerialization))
                .ToList();

            var writableProps = props.Where(p => p.Writable || p.HasMemberAttribute).ToList();

            foreach (var prop in writableProps.Where(prop => prop != null))
            {
                //This ensures that marten queries can ask for values of public domain props (eq. Assets) that are readonly,
                //but there are their writable alternatives with lower case (assets).
                prop.PropertyName = prop.PropertyName!.Substring(0, 1).ToUpper() + prop.PropertyName!.Substring(1);
            }

            return writableProps;
        }

        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (JsonNetCollectionToArrayJsonConverter.Instance.CanConvert(property.PropertyType))
                property.Converter = JsonNetCollectionToArrayJsonConverter.Instance;
            var propertyInfo = member as PropertyInfo;
            if ((object) propertyInfo != null)
            {
                property.Readable = propertyInfo.GetMethod != null;
                property.Writable = propertyInfo.SetMethod != null;
                return property;
            }

            return property;
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMartenPostgresPersistence(this IServiceCollection services,
            string connectionString, Action<StoreOptions> configureStore)
        {
            Guard.Against.NullOrWhiteSpace(connectionString, nameof(connectionString));

            var serializer = new JsonNetSerializer
            {
                EnumStorage = EnumStorage.AsString
            };

            serializer.Customize(jsonSerializer =>
            {
                //some serialization options are set inside the resolver!
                jsonSerializer.ContractResolver = new MartenJsonNetContractResolver();

                jsonSerializer.TypeNameHandling = TypeNameHandling.Objects;

                jsonSerializer.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
            });

            services.AddMarten(options =>
            {
                options.Serializer(serializer);

                options.Connection(connectionString);

                options.Policies.AllDocumentsAreMultiTenanted();

                configureStore.Invoke(options);


                options.AutoCreateSchemaObjects = AutoCreate.All;

                options.CreateDatabasesForTenants(expressions => { expressions.ForTenant().ConnectionLimit(-1).CheckAgainstPgDatabase(); });
            });


            services.AddTransient<IAsyncSessionFactory, AppMartenSessionFactory>();

            services.Scan(
                selector => selector.FromCallingAssembly()
                    .AddClasses(filter =>
                        filter.AssignableTo(typeof(MartenRepository<,>)))
                    .As(typeof(IRepository<,>))
                    .As(typeof(IReadOnlyRepository<,>))
                    .WithScopedLifetime());

            services.Scan(
                selector => selector.FromCallingAssembly()
                    .AddClasses(filter =>
                        filter.AssignableTo(typeof(MartenReadEventRepository<>)))
                    .As(typeof(IReadEventRepository<>))
                    .WithScopedLifetime());

            services.AddScoped<IWriteEventRepository, MartenWriteEventRepository>();

            return services;
        }
    }
}