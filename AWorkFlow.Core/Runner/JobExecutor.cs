using System;
using System.Threading.Tasks;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Runner
{
    public class JobExecutor
    {
        public delegate void JobStartingHandle(object sender, JobExecuteEventArgs e);
        public delegate void JobCompletedHandle(object sender, JobCompletedEventArgs e);
        public delegate void JobFailedHandle(object sender, JobFailedEventArgs e);
        //定义事件
        public event JobStartingHandle JobStarting;
        public event JobCompletedHandle JobCompleted;
        public event JobFailedHandle JobFailed;

        public async Task<JobExecutionResultDto> Execute(JobDto job)
        {
            try
            {
                JobStarting?.Invoke(this, new JobExecuteEventArgs(job));

                var result = await job.Execute();

                JobCompleted?.Invoke(this, new JobCompletedEventArgs(job, result));

                return result;
            }
            catch (Exception ex)
            {
                JobFailed?.Invoke(this, new JobFailedEventArgs(job, ex));
            }

            return null;
        }
    }

    public class JobExecuteEventArgs : EventArgs
    {
        public JobDto Job { get; set; }
        public JobExecuteEventArgs(JobDto job)
        {
            Job = job;
        }
    }

    public class JobCompletedEventArgs : JobExecuteEventArgs
    {
        public JobExecutionResultDto Result { get; set; }
        public JobCompletedEventArgs(JobDto job, JobExecutionResultDto result) : base(job)
        {
            Result = result;
        }
    }

    public class JobFailedEventArgs : JobExecuteEventArgs
    {
        public Exception Exception { get; set; }
        public JobFailedEventArgs(JobDto job, Exception ex) : base(job)
        {
            Exception = ex;
        }
    }
}
