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
        public WorkFlowEngine Engine { get; private set; }

        private List<WorkDto> runningWorks = new List<WorkDto>();
        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public WorkManager(WorkFlowEngine engine)
        {
            Engine = engine;
            NestedLoops(cancelTokenSource.Token);
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
            );
            runningWorks.AddRange(works);

            works.Select(x => x.Start());

            // distribute jobs
            Engine.JobManager.PushJobs(works.SelectMany(w => w.RunningJobs));

            return works;
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
                // check work changed
                List<WorkDto> changedWorks = new List<WorkDto>();
                if (changedWorks.Any())
                {
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
