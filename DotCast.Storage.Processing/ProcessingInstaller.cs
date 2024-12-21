using DotCast.Infrastructure.Messaging.Base;
using DotCast.Storage.Abstractions;
using DotCast.Storage.Processing.Abstractions;
using DotCast.Storage.Processing.Steps.FileNameNormalization;
using DotCast.Storage.Processing.Steps.MP4A;
using DotCast.Storage.Processing.Steps.Unzip;
using DotCast.Storage.Processing.Steps.UpdateMetadata;
using DotCast.Storage.Processing.Steps.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.IoC;
using Wolverine;

namespace DotCast.Storage.Processing
{
    public class ProcessingInstaller : IInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration, bool isProduction)
        {
            services.AddTransient<ProcessingPipeline>();
            services.AddSingleton<ICollection<IProcessingStep>>(provider => new List<IProcessingStep>
            {
                provider.GetRequiredService<UnzipProcessingStep>(),
                provider.GetRequiredService<Mp4aToMp3sProcessingStep>(),
                provider.GetRequiredService<NormalizeFileNamesProcessingStep>(),
                provider.GetRequiredService<UpdateMetadataProcessingStep>(),
                provider.GetRequiredService<ZipProcessingStep>()
            });

            services.AddSingleton<UnzipProcessingStep>();
            services.AddSingleton<Mp4aToMp3sProcessingStep>();
            services.AddSingleton<NormalizeFileNamesProcessingStep>();
            services.AddSingleton<UpdateMetadataProcessingStep>();
            services.AddSingleton<ZipProcessingStep>();
        }
    }
}