using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AWorkFlow.Core.Distributes;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Runner
{
    public class JobManager
    {
        public WorkFlowEngine Engine { get; set; }

        private List<IJobDistribute> workers;

        private ConcurrentQueue<JobDto> runningJobs = new ConcurrentQueue<JobDto>();
        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public JobManager()
        {
        }

        public void SetWorkers(IEnumerable<IJobDistribute> workers)
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
            System.Console.WriteLine($"job failed: {e?.Exception?.Message}, requeue job {e?.Job?.Type}");
            runningJobs.Enqueue(e.Job);
            PickJob((IJobDistribute)sender);
        }

        private void Worker_JobCompleted(object sender, JobCompletedEventArgs e)
        {
            System.Console.WriteLine($"job completed: success:{e?.Result?.Success}, fail:{e?.Result?.Fail}, message:{e?.Result?.Message}");
            PickJob((IJobDistribute)sender);
        }

        internal void PushJobs(params JobDto[] jobs)
        {
            System.Console.WriteLine($"{jobs?.Length} new job(s) comming");
            foreach (var job in jobs)
            {
                runningJobs.Enqueue(job);
            }
            System.Console.WriteLine($"current jobs: {runningJobs.Count}");
            Task.Run(() =>
            {
                foreach (var worker in workers)
                {
                    if (!worker.IsBusy)
                    {
                        PickJob(worker);
                    }
                }
            });
        }

        private async Task PickJob(IJobDistribute worker)
        {
            if (runningJobs.TryDequeue(out JobDto job))
            {
                System.Console.WriteLine($"current jobs: {runningJobs.Count}");
                var res = await worker.Execute(job);

            }

        }

    }
}
