using DotCast.Infrastructure.IoC;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;
using DotCast.Storage.Processing.Steps.FileNameNormalization;
using DotCast.Storage.Processing.Steps.Unzip;
using DotCast.Storage.Processing.Steps.UpdateMetadata;
using DotCast.Storage.Processing.Steps.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace DotCast.Storage.Processing
{
    public class ProcessingInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddSingleton(provider => new ProcessingPipeline(new List<IProcessingStep>
                {
                    provider.GetRequiredService<UnzipProcessingStep>(),
                    provider.GetRequiredService<NormalizeFileNamesProcessingStep>(),
                    provider.GetRequiredService<UpdateMetadataProcessingStep>(),
                    provider.GetRequiredService<ZipProcessingStep>()
                },
                provider.GetRequiredService<IMessageBus>(),
                provider.GetRequiredService<IStorage>(),
                provider.GetRequiredService<ILogger<ProcessingPipeline>>()));

            services.AddSingleton<UnzipProcessingStep>();
            services.AddSingleton<ZipProcessingStep>();
        }
    }
}