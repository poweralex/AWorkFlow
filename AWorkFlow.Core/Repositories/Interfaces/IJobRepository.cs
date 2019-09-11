using AWorkFlow.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Repositories.Interfaces
{
    public interface IJobRepository
    {
        Task<bool> InsertJob(JobDto job);
        Task<bool> SaveJobResult(JobDto job, IEnumerable<ExecutionResultDto> results, string user);
        Task<bool> FinishJob(string id);
        Task<bool> LockJob(string id, string key, TimeSpan? lockTime);
        Task<bool> UnLockJob(string id, string key);
        Task<JobDto> GetJob(string id);
        Task<JobDto> GetJobByKey(string key);
        Task<IEnumerable<JobDto>> ListJobsToDo(int? maxCount);
    }
}
