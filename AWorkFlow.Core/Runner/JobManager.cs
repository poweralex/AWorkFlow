using System.Collections.Generic;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Runner
{
    public class JobManager
    {
        public WorkFlowEngine Engine { get; set; }

        private List<JobExecutor> workers;

        private List<JobDto> runningJobs = new List<JobDto>();

        public JobManager(WorkFlowEngine engine)
        {
            Engine = engine;
            workers = engine.Settings.Workers;
        }

        internal void PushJobs(IEnumerable<JobDto> jobs)
        {
            runningJobs.AddRange(jobs);
        }
    }
}
