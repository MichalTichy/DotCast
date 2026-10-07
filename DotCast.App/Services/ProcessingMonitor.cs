using DotCast.Infrastructure.Messaging.Base;
using DotCast.SharedKernel.Messages;

namespace DotCast.App.Services
{
    public class ProcessingMonitor : IMessageHandler<ProcessingStatusChanged>, IMessageHandler<ProcessingJobChanged>
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ProcessingJobChanged> Jobs = new();
        public static IReadOnlyList<ProcessingJobChanged> RecentJobs => Jobs.Values.OrderByDescending(job => job.TimestampUtc).Take(200).ToList();
        public Task Handle(ProcessingJobChanged message)
        {
            Jobs.AddOrUpdate(message.AudioBookId, message, (_, previous) => previous.TimestampUtc > message.TimestampUtc ? previous : message);
            if (Jobs.Count > 200)
                foreach (var old in Jobs.Values.Where(job => job.Succeeded != null).OrderBy(job => job.TimestampUtc).Take(Jobs.Count - 200))
                    Jobs.TryRemove(old.AudioBookId, out _);
            return Task.CompletedTask;
        }
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
