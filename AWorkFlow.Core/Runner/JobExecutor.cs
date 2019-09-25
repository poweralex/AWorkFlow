using System.Threading.Tasks;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.Runner
{
    public class JobExecutor
    {
        public Task Execute(JobDto job)
        {
            return job.Execute();
        }
    }
}
