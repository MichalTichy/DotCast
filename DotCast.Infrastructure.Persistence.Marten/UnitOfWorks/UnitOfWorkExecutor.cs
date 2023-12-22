using System;
using System.Threading;
using System.Threading.Tasks;
using DotCast.Infrastructure.UnitOfWorkBase;

namespace DotCast.Infrastructure.Persistence.Marten.UnitOfWorks
{
    public class UnitOfWorkExecutor : IUnitOfWorkExecutor
    {
        public async Task RunInIsolationFromAmbientUnitOfWorkAsync(Func<Task> workload)
        {
            using (ExecutionContext.SuppressFlow())
            {
                await workload.Invoke();
            }
        }

        public void RunInIsolationFromAmbientUnitOfWork(Action workload)
        {
            using (ExecutionContext.SuppressFlow())
            {
                workload.Invoke();
            }
        }
    }
}