using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Runner
{
    public class JobManager
    {
        public WorkFlowEngine Engine { get; set; }

        private List<JobExecutor> workers;

        private List<JobDto> runningJobs = new List<JobDto>();
        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public JobManager()
        {
        }

        public void SetWorkers(IEnumerable<JobExecutor> workers)
        {
            foreach (var worker in workers)
            {
                worker.JobCompleted += Worker_JobCompleted;
                worker.JobFailed += Worker_JobFailed;
            }
            this.workers = workers.ToList();
        }

        private void Worker_JobFailed(object sender, JobFailedEventArgs e)
        {
            PickJob((JobExecutor)sender);
        }

        private void Worker_JobCompleted(object sender, JobCompletedEventArgs e)
        {
            PickJob((JobExecutor)sender);
        }

        internal void PushJobs(IEnumerable<JobDto> jobs)
        {
            runningJobs.AddRange(jobs);
        }

        private void PickJob(JobExecutor worker)
        {
            var job = runningJobs.Take(1);
            if (job?.Any() != true)
            {
                return;
            }
            worker.Execute(job.First());

        }

    }
}
