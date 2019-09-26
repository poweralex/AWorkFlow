using AWorkFlow.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow.Core.Runner
{
    public class JobManager
    {
        public WorkFlowEngine Engine { get; set; }

        private List<JobExecutor> workers;

        private List<JobDto> runningJobs = new List<JobDto>();

        public JobManager()
        {
        }

        public void SetWorkers(IEnumerable<JobExecutor> workers)
        {
            this.workers = workers.ToList();
        }

        internal void PushJobs(IEnumerable<JobDto> jobs)
        {
            runningJobs.AddRange(jobs);
        }
    }
}
