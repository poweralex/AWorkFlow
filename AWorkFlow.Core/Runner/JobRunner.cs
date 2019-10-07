using System.Threading.Tasks;
using Autofac;
using AWorkFlow.Core.JobExecutors;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Runner
{
    public class JobRunner
    {
        private readonly IContainer _iocContainer;
        public JobRunner(IContainer iocContainer)
        {
            _iocContainer = iocContainer;
        }

        public async Task<JobExecutionResultDto> Execute(JobDto job)
        {
            System.Console.WriteLine($"going to find an executor for job of {job?.Type}");
            if (!_iocContainer.IsRegisteredWithName<IJobExecutor>(job.Type))
            {
                System.Console.WriteLine($"Job type {job?.Type} not registered.");
                return new JobExecutionResultDto { Completed = true, Fail = true, Message = $"Job type {job?.Type} not registered." };
            }
            var jobExecutor = _iocContainer.ResolveNamed<IJobExecutor>(job.Type);
            var result = await jobExecutor.Execute(job);

            return result;

        }
    }
}
