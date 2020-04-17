using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model for working step execute result
    /// </summary>
    public class WorkingCopyStepResult : WorkingModelBase
    {
        private string _id = Guid.NewGuid().ToString();
        /// <summary>
        /// execution result id
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
                _Arguments?.SetIds(_workId, _stepId, _id, _actionType);
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
                _Arguments?.SetIds(_workId, _stepId, _id, _actionType);
            }
        }
        private string _stepId = string.Empty;
        /// <summary>
        /// working step id
        /// </summary>
        public string WorkingStepId
        {
            get
            {
                return _stepId;
            }
            set
            {
                _stepId = value;
                _Arguments?.SetIds(_workId, _stepId, _id, _actionType);
            }
        }
        private string _actionType = string.Empty;
        /// <summary>
        /// action type
        /// </summary>
        public string ActionType
        {
            get
            {
                return _actionType;
            }
            set
            {
                _actionType = value;
                _Arguments?.SetIds(_workId, _stepId, _id, _actionType);
            }
        }
        /// <summary>
        /// submit time
        /// </summary>
        public DateTime? SubmitTime { get; set; }
        /// <summary>
        /// qty
        /// </summary>
        public int? Qty { get; set; }
        /// <summary>
        /// is success
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// is failed
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// if this result posted next
        /// </summary>
        public bool PostedNext { get; set; }
        private WorkingArguments _Arguments;
        /// <summary>
        /// working arguments of the execution(input/output)
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
                _Arguments?.SetIds(_workId, _stepId, _id, _actionType);
            }
        }

        /// <summary>
        /// accept all changes include sub-items
        /// </summary>
        /// <param name="acceptAll"></param>
        public void AcceptChanges(bool acceptAll)
        {
            base.AcceptChanges();
            if (acceptAll)
            {
                Arguments?.AcceptChanges();
            }
        }
    }

    /// <summary>
    /// working step execute result collection
    /// </summary>
    public class WorkingCopyStepResultCollection : ICollection<WorkingCopyStepResult>
    {
        private readonly object lockObj = new object();
        private readonly List<WorkingCopyStepResult> results = new List<WorkingCopyStepResult>();
        private readonly bool _manageIds = true;
        private string _workId = string.Empty;
        private string _stepId = string.Empty;
        private string _actionType = string.Empty;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="manageIds">requires this collection to manage ids or not</param>
        public WorkingCopyStepResultCollection(bool manageIds = true)
        {
            _manageIds = manageIds;
        }
        /// <summary>
        /// set ids for all results belong to this collection
        /// </summary>
        /// <param name="workId"></param>
        /// <param name="stepId"></param>
        /// <param name="actionType"></param>
        public void SetIds(string workId, string stepId, string actionType)
        {
            _workId = workId;
            _stepId = stepId;
            _actionType = actionType;
            if (_manageIds)
            {
                if (results?.Any() == true)
                {
                    lock (lockObj)
                    {
                        foreach (var result in results)
                        {
                            result.WorkingCopyId = workId;
                            result.WorkingStepId = stepId;
                            result.ActionType = _actionType;
                        }
                    }
                }
            }
        }
        public int Count => results.Count;

        public bool IsReadOnly => false;

        public void Add(WorkingCopyStepResult item)
        {
            if (_manageIds)
            {
                item.WorkingCopyId = _workId;
                item.WorkingStepId = _stepId;
                item.ActionType = _actionType;
            }
            results.Add(item);
        }

        /// <summary>
        /// add steps
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<WorkingCopyStepResult> items)
        {
            if (_manageIds)
            {
                results.AddRange(items?.Select(x =>
                {
                    x.WorkingCopyId = _workId;
                    x.WorkingStepId = _stepId;
                    x.ActionType = _actionType;
                    return x;
                }));
            }
            else
            {
                results.AddRange(items);
            }
        }

        public void Clear() => results.Clear();

        public bool Contains(WorkingCopyStepResult item) => results.Contains(item);

        public void CopyTo(WorkingCopyStepResult[] array, int arrayIndex) => results.CopyTo(array, arrayIndex);

        public IEnumerator<WorkingCopyStepResult> GetEnumerator() => results.GetEnumerator();

        public bool Remove(WorkingCopyStepResult item) => results.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => results.GetEnumerator();
    }
}
