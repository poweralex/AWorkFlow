using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AWorkFlow.Core.Models;

namespace AWorkFlow.Core.JobExecutors
{
    public interface IJobExecutor
    {
        Task<JobExecutionResultDto> Execute(JobDto job);
    }
}
