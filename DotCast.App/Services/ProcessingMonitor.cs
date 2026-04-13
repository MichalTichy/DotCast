using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;

namespace DotCast.App.Services
{
    public class ProcessingMonitor : IMessageHandler<ProcessingStatusChanged>
    {
        public static IReadOnlyCollection<string> RunningProcessings { get; private set; } = [];
        public static int CountOfRunningProcessings { get; private set; }
        public static bool IsProcessingRunning => CountOfRunningProcessings != 0;

        public Task Handle(ProcessingStatusChanged message)
        {
            RunningProcessings = message.RunningProcessings.ToArray();
            CountOfRunningProcessings = message.RunningProcessings.Count;
            return Task.CompletedTask;
        }
    }
}
