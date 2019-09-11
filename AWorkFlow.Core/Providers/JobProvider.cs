using AWorkFlow.Core.Extensions;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers
{
    public class JobProvider : IJobProvider
    {
        private readonly IJobRepository _jobRepository;
        private readonly IExecutorProvider _executorProvider;
        private readonly IWorkProvider _workProvider;
        private readonly IWorkFlowProvider _workFlowProvider;

        public JobProvider(IJobRepository jobRepository, IExecutorProvider executorProvider, IWorkProvider workProvider, IWorkFlowProvider workFlowProvider)
        {
            _jobRepository = jobRepository;
            _executorProvider = executorProvider;
            _workProvider = workProvider;
            _workFlowProvider = workFlowProvider;
        }

        /// <summary>
        /// execute a job, 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IEnumerable<JobDto>> Execute(JobDto job, string user)
        {
            string key = Guid.NewGuid().ToString();
            // lock job
            var lockJobResult = await LockJob(job.Id, key, TimeSpan.FromMinutes(1), user);
            if (!lockJobResult)
            {
                return new List<JobDto>();
            }
            // get job by key
            var jobToDo = await GetJob(key);
            // get action(s)
            int i = 0;
            bool success = true;
            var expressionProvider = GetJobVariables(job);
            List<ExecutionResultDto> executionResults = new List<ExecutionResultDto>();
            foreach (var action in jobToDo.Actions)
            {
                // execute Actions
                var executor = await _executorProvider.GetExecutor(action);
                var executeResult = await executor.Execute(expressionProvider, action);
                executionResults.Add(executeResult);
                expressionProvider.Arguments.PutPrivate($"result{i}", executeResult?.ExecuteResult?.ToJson());
                if (executeResult?.Success != true)
                {
                    success = false;
                    break;
                }
                i++;
            }
            // save action result
            await _jobRepository.SaveJobResult(job, executionResults, user);
            if (success)
            {
                await _jobRepository.FinishJob(job.Id);
            }
            await _jobRepository.UnLockJob(job.Id, key);
            // post next job(s)
            return await PostNextJobs(job.Id, user);
        }

        private IExpressionProvider GetJobVariables(JobDto job)
        {
            ArgumentsDto arguments = new ArgumentsDto(job?.PublicVariables, job?.PrivateVariables);
            return new ExpressionProvider(arguments);
        }

        private async Task<IEnumerable<JobDto>> PostNextJobs(string jobId, string user)
        {
            List<JobDto> nextJobs = new List<JobDto>();
            // get job
            var job = await _jobRepository.GetJob(jobId);
            if (!job.Completed)
            {
                return nextJobs;
            }
            var work = await _workProvider.GetWork(job.WorkId);
            var workflow = (await _workFlowProvider.SearchWorkFlow(string.Empty, work.WorkFlowCode, work.WorkFlowVersion))?.FirstOrDefault();
            var currentType = job.JobType;
            var expressionProvider = GetJobVariables(job);
            // if job is work.pre-action, go first-step
            if (currentType == JobTypes.WorkPreAction)
            {
                // post first_step
                var stepCfg = workflow?.Steps?.FirstOrDefault(x => x.IsBegin);
                await _workProvider.PostStep(new WorkStepDto
                {
                    WorkStepId = Guid.NewGuid().ToString(),
                    StepCode = stepCfg.Code,
                    Tags = stepCfg.TagExps?.Select(x => expressionProvider.Format(x).Result)?.ToList(),
                    Group = expressionProvider.Format(stepCfg.GroupExp).Result,
                    TagData = expressionProvider.Format(stepCfg.TagDataExp).Result,
                    MatchQty = string.IsNullOrEmpty(stepCfg.MatchQtyExp) ? null : expressionProvider.Format<int?>(stepCfg.MatchQtyExp).Result
                }, user);
            }
            // if job is work.after-action, go close work
            if (currentType == JobTypes.WorkAfterAction)
            {
                // close work
                await _workProvider.FinishWork(work, true, user);
            }
            // if job is step.pre-action, go action or wait for manual
            if (currentType == JobTypes.StepPreAction)
            {
                // post step.action job(auto/manual)
            }
            // if job is step.action, go trigger step result and after-action
            if (currentType == JobTypes.StepAction)
            {
                // trigger step result
                // post next step
                // post after-action job
            }
            // if job is step.after-action, go next step
            if (currentType == JobTypes.StepAfterAction)
            {
                // if last step, post work.after-action
            }
            return nextJobs;
        }

        public Task<JobDto> GetJob(string key)
        {
            return _jobRepository.GetJobByKey(key);
        }

        public Task<IEnumerable<JobDto>> ListJobsToDo(int? maxCount)
        {
            return _jobRepository.ListJobsToDo(maxCount);
        }

        public Task<bool> LockJob(string id, string key, TimeSpan? lockTime, string user)
        {
            // lock a job
            return _jobRepository.LockJob(id, key, lockTime ?? TimeSpan.FromMinutes(1));
        }

        public Task<bool> PostJob(JobDto job, string user)
        {
            return _jobRepository.InsertJob(job);
        }

        public Task<bool> UnLockJob(string id, string key, string user)
        {
            // unlock a job
            return _jobRepository.UnLockJob(id, key);
        }
    }
}
