using System;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.UnitOfWorkBase;

public class NoUnitOfWorkExecutor : IUnitOfWorkExecutor
{
    public async Task RunInIsolationFromAmbientUnitOfWorkAsync(Func<Task> workload)
    {
        await workload();
    }

    public void RunInIsolationFromAmbientUnitOfWork(Action workload)
    {
        workload();
    }
}
