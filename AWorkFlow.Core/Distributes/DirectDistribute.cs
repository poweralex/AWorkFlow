using System;
using System.Threading.Tasks;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Runner;

namespace AWorkFlow.Core.Distributes
{
    public class DirectDistribute : IJobDistribute
    {
        public bool IsBusy { get; private set; }
        private readonly JobRunner _jobExecutor;
        public DirectDistribute(JobRunner jobExecutor)
        {
            _jobExecutor = jobExecutor;
        }

        //定义事件
        public event JobStartingHandle JobStarting;
        public event JobCompletedHandle JobCompleted;
        public event JobFailedHandle JobFailed;

        public async Task<JobExecutionResultDto> Execute(JobDto job)
        {
            IsBusy = true;
            try
            {
                JobStarting?.Invoke(this, new JobExecuteEventArgs(job));

                var result = await _jobExecutor.Execute(job);

                JobCompleted?.Invoke(this, new JobCompletedEventArgs(job, result));

                return result;
            }
            catch (Exception ex)
            {
                JobFailed?.Invoke(this, new JobFailedEventArgs(job, ex));
            }
            finally
            {
                IsBusy = false;
            }

            return null;
        }
    }
}
