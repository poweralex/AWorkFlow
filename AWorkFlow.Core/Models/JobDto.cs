using AWorkFlow.Core.Extensions;
using AWorkFlow.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public bool Fail { get; set; }
        public List<ActionSettingDto> Actions { get; set; }
        public Dictionary<string, string> PublicVariables { get; set; }
        public Dictionary<string, string> PrivateVariables { get; set; }
        public List<JobExecutionResult> Executions { get; set; } = new List<JobExecutionResult>();

        internal virtual async Task<IEnumerable<JobDto>> Execute()
        {
            List<JobDto> nextJobs = new List<JobDto>();
            // get action(s)
            int i = 0;
            bool complete = true;
            bool success = false;
            bool fail = false;
            if (Actions?.Any() == true)
            {
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
                Executions.Add(new JobExecutionResult
                {
                    ExecuteTime = DateTime.UtcNow,
                    Results = executionResults
                });
            }
            else
            {
                complete = true;
                success = true;
            }
            if (complete)
            {
                Completed = complete;
                Success = success;
                Fail = fail;
                // post next job(s)
                if (success)
                {
                    nextJobs.AddRange(await AfterSuccess());
                }
                if (fail)
                {
                    nextJobs.AddRange(await AfterFail());
                }
            }

            return nextJobs;
        }

        internal virtual async Task<IEnumerable<JobDto>> AfterSuccess()
        {
            return new List<JobDto>();
        }

        internal virtual async Task<IEnumerable<JobDto>> AfterFail()
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

    public class JobExecutionResult
    {
        public IEnumerable<ExecutionResultDto> Results { get; set; }
        public DateTime ExecuteTime { get; set; }
    }
}
