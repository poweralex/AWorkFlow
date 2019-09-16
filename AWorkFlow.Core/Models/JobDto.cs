using AWorkFlow.Core.Extensions;
using AWorkFlow.Core.Providers;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Models
{
    /// <summary>
    /// job dto
    /// </summary>
    [Serializable]
    public class JobDto : ISerializable
    {
        /// <summary>
        /// job unique id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// job type
        /// </summary>
        public JobTypes JobType { get; internal set; }
        public string WorkId { get; set; }
        public string WorkStepId { get; set; }
        public DateTime ActiveTime { get; set; }
        public bool IsManual { get; set; }
        public int? MatchQty { get; set; }
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public bool Fail { get; set; }
        public List<ActionSettingDto> Actions { get; set; }
        public Dictionary<string, string> PublicVariables { get; set; }
        public Dictionary<string, string> PrivateVariables { get; set; }

        internal virtual async Task<IEnumerable<JobDto>> Execute(IJobRepository jobRepository, string user)
        {
            List<JobDto> nextJobs = new List<JobDto>();
            // get action(s)
            int i = 0;
            bool complete = true;
            bool success = false;
            bool fail = false;
            ArgumentsDto arguments = new ArgumentsDto(PublicVariables, PrivateVariables);
            var expressionProvider = new ExpressionProvider(arguments);
            List<ExecutionResultDto> executionResults = new List<ExecutionResultDto>();
            foreach (var action in Actions)
            {
                // execute Actions
                ExecutorProvider executorProvider = new ExecutorProvider();
                var executor = await executorProvider.GetExecutor(action);
                var executeResult = await executor.Execute(expressionProvider, action);
                executionResults.Add(executeResult);
                expressionProvider.Arguments.PutPrivate($"result{i}", executeResult?.ExecuteResult?.ToJson());
                if (executeResult?.Success == true)
                {
                    success = true;
                    complete = true;
                    break;
                }
                if (executeResult?.Fail == true)
                {
                    fail = true;
                    complete = true;
                    break;
                }
                if (executeResult?.Completed != true)
                {
                    complete = false;
                    break;
                }
                i++;
            }
            // save action result
            await jobRepository.SaveJobResult(this, executionResults, user);
            if (complete)
            {
                await jobRepository.FinishJob(Id, success, fail);
                // post next job(s)
                if (success)
                {
                    nextJobs.AddRange(await AfterSuccess(jobRepository, user));
                }
                if (fail)
                {
                    nextJobs.AddRange(await AfterFail(jobRepository, user));
                }
            }

            return nextJobs;
        }

        internal virtual async Task<IEnumerable<JobDto>> AfterSuccess(IJobRepository jobRepository, string user)
        {
            return new List<JobDto>();
        }

        internal virtual async Task<IEnumerable<JobDto>> AfterFail(IJobRepository jobRepository, string user)
        {
            return new List<JobDto>();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// job types
    /// </summary>
    public enum JobTypes
    {
        /// <summary>
        /// PreAction of a work
        /// </summary>
        WorkPreAction,
        /// <summary>
        /// AfterAction of a work
        /// </summary>
        WorkAfterAction,
        /// <summary>
        /// PreAction of a step
        /// </summary>
        StepPreAction,
        /// <summary>
        /// Action of a step
        /// </summary>
        StepAction,
        /// <summary>
        /// AfterAction of a step
        /// </summary>
        StepAfterAction
    }
}
