using AWorkFlow.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Runner
{
    /// <summary>
    /// provides work related operations
    /// </summary>
    public class WorkManager
    {
        public WorkFlowEngine Engine { get; set; }

        private List<WorkDto> runningWorks = new List<WorkDto>();
        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public WorkManager()
        {
        }

        public async Task<IEnumerable<WorkDto>> StartWork(string category, object data)
        {
            // get workflows
            var workflows = await Engine.WorkFlowManager.GetWorkFlows(category);
            // try all selectors to choose workflow(s) to start
            var workingFlows = workflows.Where(wf => wf.Suit(data).Result);
            // create work(s)
            var works = workingFlows.Select(wf =>
                new WorkDto
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkFlow = wf
                }
            ).ToList();
            runningWorks.AddRange(works);

            works.ForEach(x => x.Start());

            // distribute jobs
            Engine.JobManager.PushJobs(works.SelectMany(w => w.RunningJobs)?.ToArray());

            return works;
        }

        public void Start()
        {
            NestedLoops(cancelTokenSource.Token);
        }

        private async Task NestedLoops(CancellationToken token)
        {
            int delayMs = 1;// 1ms
            int maxDelayMs = 1 * 60 * 1000; // 1min
            while (true)
            {
                token.ThrowIfCancellationRequested();
                // delay
                await Task.Delay(delayMs);
                // TODO: check work changed
                List<WorkDto> changedWorks = new List<WorkDto>();
                if (changedWorks.Any())
                {
                    foreach (var work in changedWorks)
                    {
                        work.FireJobs();
                    }

                    // distribute jobs
                    Engine.JobManager.PushJobs(changedWorks.SelectMany(w => w.RunningJobs)?.ToArray());
                }
                else
                {
                    delayMs = Math.Min(delayMs * 2, maxDelayMs);
                }

            }
        }

        public void Stop()
        {
            cancelTokenSource.Cancel();
        }
    }
}
