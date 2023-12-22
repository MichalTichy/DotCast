using System;
using Marten;
using Marten.Schema.Identity;

namespace DotCast.Infrastructure.Persistence.Marten.StorageConfiguration
{
    public abstract class StorageConfigurationBase<T> : IStorageConfiguration where T : notnull
    {
        private readonly IMartenLogger? martenLogger;

        public StorageConfigurationBase(IMartenLogger? martenLogger)
        {
            this.martenLogger = martenLogger;
        }

        public virtual void Configure(StoreOptions options)
        {
            ConfigureInternal(options);

            options.Policies.ForAllDocuments(m =>
            {
                if (m.IdType == typeof(Guid))
                {
                    m.IdStrategy = new NoOpIdGeneration();
                }
            });

            if (martenLogger != null)
            {
                options.Logger(martenLogger);
            }
        }

        protected abstract void ConfigureInternal(StoreOptions options);
    }
}