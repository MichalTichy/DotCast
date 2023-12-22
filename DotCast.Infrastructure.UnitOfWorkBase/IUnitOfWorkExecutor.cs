using System;
using System.Threading.Tasks;

namespace DotCast.Infrastructure.UnitOfWorkBase
{
    public interface IUnitOfWorkExecutor
    {
        Task RunInIsolationFromAmbientUnitOfWorkAsync(Func<Task> workload);
        void RunInIsolationFromAmbientUnitOfWork(Action workload);
    }
}