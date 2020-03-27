using System;
using System.Collections.Generic;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model for working step
    /// </summary>
    public class WorkingCopyStep : WorkingModelBase
    {
        /// <summary>
        /// working step id
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId { get; set; }
        /// <summary>
        /// previous working step id
        /// </summary>
        public string PreviousWorkingCopyStepId { get; set; }
        /// <summary>
        /// step code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// status scope
        /// </summary>
        public string StatusScope { get; set; }
        /// <summary>
        /// status id
        /// </summary>
        public string StatusId { get; set; }
        /// <summary>
        /// is begining step
        /// </summary>
        public bool IsBegin { get; set; }
        /// <summary>
        /// is ending step
        /// </summary>
        public bool IsEnd { get; set; }
        /// <summary>
        /// is waiting manual operation
        /// </summary>
        public bool WaitManual { get; set; }
        /// <summary>
        /// if this step goes by qty
        /// </summary>
        public bool ByQty { get; set; }
        /// <summary>
        /// target qty to match if byQty
        /// </summary>
        public int? MatchQty { get; set; }
        /// <summary>
        /// active time
        /// </summary>
        public DateTime? ActiveTime { get; set; }
        /// <summary>
        /// finish time
        /// </summary>
        public DateTime? FinishedTime { get; set; }
        /// <summary>
        /// is pre-action finished or skipped
        /// </summary>
        public bool PreActionFinished { get; set; }
        /// <summary>
        /// is action finished or skipped
        /// </summary>
        public bool ActionFinished { get; set; }
        /// <summary>
        /// have posted next steps
        /// </summary>
        public bool PostedNext { get; set; }
        /// <summary>
        /// is success
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// is normal finished
        /// </summary>
        public bool Finished { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// working arguments of the step(input/output)
        /// </summary>
        public WorkingArguments Arguments { get; set; }
        /// <summary>
        /// pre-action executed count
        /// </summary>
        public int PreActionExecutedCount { get; set; }
        /// <summary>
        /// action executed count
        /// </summary>
        public int ActionExecutedCount { get; set; }
        /// <summary>
        /// pre-action results
        /// </summary>
        public List<WorkingCopyStepResult> PreActionResults { get; set; }
        /// <summary>
        /// action results
        /// </summary>
        public List<WorkingCopyStepResult> ActionResults { get; set; }
        /// <summary>
        /// last pre-action results
        /// </summary>
        public WorkingCopyStepResult LastPreActionResults { get; set; }
        /// <summary>
        /// last action results
        /// </summary>
        public WorkingCopyStepResult LastActionResults { get; set; }
    }

    public class WorkingCopyStepIdComparer : IEqualityComparer<WorkingCopyStep>
    {
        public bool Equals(WorkingCopyStep x, WorkingCopyStep y)
        {
            return string.Equals(x.Id, y.Id);
        }

        public int GetHashCode(WorkingCopyStep obj)
        {
            return obj?.Id?.GetHashCode() ?? default(int);
        }
    }
}
