using AWorkFlow2.Models.Configs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model for working step
    /// </summary>
    public class WorkingCopyStep : WorkingModelBase
    {
        private readonly object lockObj = new object();
        private string _id = Guid.NewGuid().ToString();
        /// <summary>
        /// working step id
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                if (Arguments != null)
                {
                    Arguments.WorkingStepId = _id;
                }
                PreActionResults?.SetIds(_workId, _id, ActionTypes.StepPreAction.ToString());
                ActionResults?.SetIds(_workId, _id, ActionTypes.StepAction.ToString());
            }
        }
        private string _workId = string.Empty;
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId
        {
            get
            {
                return _workId;
            }
            set
            {
                _workId = value;
                Arguments?.SetIds(_workId, _id, string.Empty, ActionTypes.StepData.ToString());
                PreActionResults?.SetIds(_workId, _id, ActionTypes.StepPreAction.ToString());
                ActionResults?.SetIds(_workId, _id, ActionTypes.StepAction.ToString());
            }
        }
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
        /// tags
        /// </summary>
        public List<string> Tags { get; set; }
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
        /// if this step is a retry step
        /// </summary>
        public bool IsRetry { get; set; }
        /// <summary>
        /// active time
        /// </summary>
        public DateTime? ActiveTime { get; set; }
        /// <summary>
        /// next execute time
        /// </summary>
        public DateTime? NextExecuteTime { get; set; }
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
        private WorkingArguments _Arguments = null;
        /// <summary>
        /// working arguments of the step(input/output)
        /// </summary>
        [IgnoreTracking]
        public WorkingArguments Arguments
        {
            get
            {
                return _Arguments;
            }
            set
            {
                _Arguments = value;
                _Arguments?.SetIds(_workId, _id, string.Empty, ActionTypes.StepData.ToString());
            }
        }
        /// <summary>
        /// pre-action executed count
        /// </summary>
        public int PreActionExecutedCount { get; set; }
        /// <summary>
        /// action executed count
        /// </summary>
        public int ActionExecutedCount { get; set; }
        private WorkingCopyStepResultCollection _PreActionResults = null;
        /// <summary>
        /// pre-action results
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyStepResultCollection PreActionResults
        {
            get
            {
                if (_PreActionResults == null)
                {
                    lock (lockObj)
                    {
                        _PreActionResults = new WorkingCopyStepResultCollection();
                        _PreActionResults.SetIds(_workId, _id, ActionTypes.StepPreAction.ToString());
                    }
                }
                return _PreActionResults;
            }
        }
        private WorkingCopyStepResultCollection _ActionResults = null;
        /// <summary>
        /// action results
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyStepResultCollection ActionResults
        {
            get
            {
                if (_ActionResults == null)
                {
                    lock (lockObj)
                    {
                        _ActionResults = new WorkingCopyStepResultCollection();
                        _ActionResults.SetIds(_workId, _id, ActionTypes.StepAction.ToString());
                    }
                }
                return _ActionResults;
            }
        }
        /// <summary>
        /// last pre-action results
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyStepResult LastPreActionResults => PreActionResults?.OrderByDescending(x => x.SubmitTime)?.FirstOrDefault();
        /// <summary>
        /// last action results
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyStepResult LastActionResults => ActionResults?.OrderByDescending(x => x.SubmitTime)?.FirstOrDefault();

        /// <summary>
        /// accept all changes include sub-items
        /// </summary>
        /// <param name="acceptAll"></param>
        public void AcceptChanges(bool acceptAll)
        {
            base.AcceptChanges();
            if (acceptAll)
            {
                // step
                Arguments?.AcceptChanges();
                // executions
                PreActionResults?.Select(x =>
                {
                    x.AcceptChanges(acceptAll);
                    return x;
                }).ToList();
                ActionResults?.Select(x =>
                {
                    x.AcceptChanges(acceptAll);
                    return x;
                }).ToList();
            }
        }
    }

    /// <summary>
    /// comparer to compare between working copy steps
    /// </summary>
    public class WorkingCopyStepIdComparer : IEqualityComparer<WorkingCopyStep>
    {
        /// <summary>
        /// equals
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(WorkingCopyStep x, WorkingCopyStep y)
        {
            return string.Equals(x.Id, y.Id);
        }

        /// <summary>
        /// get hash code
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(WorkingCopyStep obj)
        {
            return obj?.Id?.GetHashCode() ?? default(int);
        }
    }

    /// <summary>
    /// working step collection
    /// </summary>
    public class WorkingCopyStepCollection : ICollection<WorkingCopyStep>
    {
        private readonly object lockObj = new object();
        private readonly List<WorkingCopyStep> steps = new List<WorkingCopyStep>();
        private readonly bool _manageIds = true;
        private string _workId = string.Empty;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="manageIds">requires this collection to manage ids or not</param>
        public WorkingCopyStepCollection(bool manageIds = true)
        {
            _manageIds = manageIds;
        }
        /// <summary>
        /// set working copy id of all steps belong to this collection
        /// </summary>
        /// <param name="workId"></param>
        public void SetIds(string workId)
        {
            _workId = workId;
            if (_manageIds)
            {
                if (steps?.Any() == true)
                {
                    lock (lockObj)
                    {
                        foreach (var step in steps)
                        {
                            step.WorkingCopyId = _workId;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Count of steps
        /// </summary>
        public int Count => steps.Count;

        /// <summary>
        /// is readonly
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// add step
        /// </summary>
        /// <param name="item"></param>
        public void Add(WorkingCopyStep item)
        {
            if (_manageIds)
            {
                item.WorkingCopyId = _workId;
            }
            steps.Add(item);
        }

        /// <summary>
        /// add steps
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<WorkingCopyStep> items)
        {
            if (_manageIds)
            {
                steps.AddRange(items?.Select(x =>
                {
                    x.WorkingCopyId = _workId;
                    return x;
                }));
            }
            else
            {
                steps.AddRange(items);
            }
        }

        /// <summary>
        /// clear collection
        /// </summary>
        public void Clear() => steps.Clear();

        /// <summary>
        /// if contains step
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(WorkingCopyStep item) => steps.Contains(item);

        /// <summary>
        /// copy to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(WorkingCopyStep[] array, int arrayIndex) => steps.CopyTo(array, arrayIndex);

        /// <summary>
        /// get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<WorkingCopyStep> GetEnumerator() => steps.GetEnumerator();

        /// <summary>
        /// remove step
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(WorkingCopyStep item) => steps.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => steps.GetEnumerator();
    }
}
