using AWorkFlow.Core.Models;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.InMemoryRepo
{
    public class JobInMemRepo : IJobRepository
    {
        static readonly object lockObj = new object();
        static readonly List<JobDto> _jobs = new List<JobDto>();
        static readonly Dictionary<string, JobDto> lockedJobs = new Dictionary<string, JobDto>();

        public Task<bool> FinishJob(string id, bool success, bool fail)
        {
            throw new NotImplementedException();
        }

        public Task<JobDto> GetJob(string id)
        {
            return Task.FromResult(_jobs.FirstOrDefault(x => x.Id == id));
        }

        public async Task<JobDto> GetJobByKey(string key)
        {
            if (lockedJobs.ContainsKey(key))
            {
                return lockedJobs[key];
            }
            else
            {
                return null;
            }
        }

        public Task<bool> InsertJob(JobDto job)
        {
            try
            {
                if (string.IsNullOrEmpty(job?.Id))
                {
                    return Task.FromResult(false);
                }
                lock (lockObj)
                {
                    if (_jobs.Any(x => x.Id == job?.Id))
                    {
                        return Task.FromResult(false);
                    }
                    _jobs.Add(job);
                }

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> InsertJobs(IEnumerable<JobDto> jobs)
        {
            try
            {
                if (jobs?.Any(j => string.IsNullOrEmpty(j?.Id)) == true)
                {
                    return Task.FromResult(false);
                }
                lock (lockObj)
                {
                    if (_jobs.Any(x => jobs.Any(j => j.Id == x.Id)))
                    {
                        return Task.FromResult(false);
                    }
                    _jobs.AddRange(jobs);
                }

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<IEnumerable<JobDto>> ListJobsToDo(int? maxCount)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LockJob(string id, string key, TimeSpan? lockTime)
        {
            try
            {
                if (string.IsNullOrEmpty(id)
                    || string.IsNullOrEmpty(key))
                {
                    return Task.FromResult(false);
                }
                var job = GetJob(id).Result;
                if (job == null)
                {
                    return Task.FromResult(false);
                }
                lock (lockObj)
                {
                    if (lockedJobs.ContainsKey(key))
                    {
                        return Task.FromResult(false);
                    }
                    if (lockedJobs.Values.Any(x => x.Id == job.Id))
                    {
                        return Task.FromResult(false);
                    }

                    lockedJobs.Add(key, job);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> SaveJobResult(JobDto job, IEnumerable<ExecutionResultDto> results, string user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnLockJob(string id, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(id)
                    || string.IsNullOrEmpty(key))
                {
                    return Task.FromResult(false);
                }
                var job = GetJob(id).Result;
                if (job == null)
                {
                    return Task.FromResult(false);
                }
                lock (lockObj)
                {
                    if (!lockedJobs.ContainsKey(key))
                    {
                        return Task.FromResult(false);
                    }
                    if (lockedJobs[key].Id != id)
                    {
                        return Task.FromResult(false);
                    }

                    lockedJobs.Remove(key);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
