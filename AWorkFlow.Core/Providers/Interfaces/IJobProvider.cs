using AWorkFlow.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers.Interfaces
{
    /// <summary>
    /// provides job operations
    /// </summary>
    public interface IJobProvider
    {
        /// <summary>
        /// list job(s) to do
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        Task<IEnumerable<JobDto>> ListJobsToDo(int? maxCount);
        /// <summary>
        /// lock a job
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="lockTime"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> LockJob(string id, string key, TimeSpan? lockTime, string user);
        /// <summary>
        /// unlock a job
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> UnLockJob(string id, string key, string user);
        /// <summary>
        /// get a job by lock key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<JobDto> GetJob(string key);
        /// <summary>
        /// execute a job
        /// </summary>
        /// <param name="job"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<IEnumerable<JobDto>> Execute(JobDto job, string user);
        /// <summary>
        /// post a new job
        /// </summary>
        /// <param name="job"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> PostJob(JobDto job, string user);
    }
}
