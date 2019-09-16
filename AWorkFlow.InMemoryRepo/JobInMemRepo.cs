using AWorkFlow.Core.Models;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.InMemoryRepo
{
    public class JobInMemRepo : RepoBase<JobDto>, IJobRepository
    {
        public Task<bool> FinishJob(string id, bool success, bool fail)
        {
            throw new NotImplementedException();
        }

        public Task<JobDto> GetJob(string id)
        {
            throw new NotImplementedException();
        }

        public Task<JobDto> GetJobByKey(string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertJob(JobDto job)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertJobs(IEnumerable<JobDto> jobs)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<JobDto>> ListJobsToDo(int? maxCount)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LockJob(string id, string key, TimeSpan? lockTime)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveJobResult(JobDto job, IEnumerable<ExecutionResultDto> results, string user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnLockJob(string id, string key)
        {
            throw new NotImplementedException();
        }
    }
}
