using System;
using System.Threading.Tasks;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Distributes
{
    public interface IJobDistribute
    {
        event JobCompletedHandle JobCompleted;
        event JobFailedHandle JobFailed;
        event JobStartingHandle JobStarting;

        bool IsBusy { get; }

        Task<JobExecutionResultDto> Execute(JobDto job);
    }

    public delegate void JobStartingHandle(object sender, JobExecuteEventArgs e);
    public delegate void JobCompletedHandle(object sender, JobCompletedEventArgs e);
    public delegate void JobFailedHandle(object sender, JobFailedEventArgs e);

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
