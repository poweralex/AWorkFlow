using AWorkFlow2.Models.Configs;
using System;
using System.Linq;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model for working copy
    /// </summary>
    public class WorkingCopy : WorkingModelBase
    {
        private readonly object lockObj = new object();
        private string _id = Guid.NewGuid().ToString();
        /// <summary>
        /// working copy id
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
                _Arguments?.SetIds(Id, string.Empty, string.Empty, ActionTypes.WorkData.ToString());
                _Steps?.SetIds(_id);
            }
        }
        /// <summary>
        /// workflow category
        /// </summary>
        public string WorkFlowCategory { get; set; }
        /// <summary>
        /// workflow code
        /// </summary>
        public string WorkFlowCode { get; set; }
        /// <summary>
        /// workflow version
        /// </summary>
        public int? WorkFlowVersion { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// next execute time
        /// </summary>
        public DateTime? NextExecuteTime { get; set; }
        /// <summary>
        /// if this work is finished
        /// </summary>
        public bool IsFinished { get; set; }
        /// <summary>
        /// if this work is cancelled
        /// </summary>
        public bool IsCancelled { get; set; }
        /// <summary>
        /// if this work is stuck
        /// </summary>
        public bool IsStuck { get; set; }
        /// <summary>
        /// is on-hold
        /// </summary>
        public bool OnHold { get; set; }
        /// <summary>
        /// on-hold time
        /// </summary>
        public DateTime? HoldTime { get; set; }
        /// <summary>
        /// release time
        /// </summary>
        public DateTime? ReleaseTime { get; set; }
        private WorkingArguments _Arguments = null;
        /// <summary>
        /// working arguments of the work(input/output)
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
                _Arguments?.SetIds(Id, string.Empty, string.Empty, ActionTypes.WorkData.ToString());
            }
        }
        private WorkingCopyStepCollection _Steps = null;
        /// <summary>
        /// steps
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyStepCollection Steps
        {
            get
            {
                if (_Steps == null)
                {
                    lock (lockObj)
                    {
                        _Steps = new WorkingCopyStepCollection();
                        _Steps?.SetIds(Id);
                    }
                }
                return _Steps;
            }
        }
        /// <summary>
        /// flows
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyFlowCollection Flows { get; } = new WorkingCopyFlowCollection();
        private WorkingCopyGroupCollection _Groups = null;
        /// <summary>
        /// groups of steps
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyGroupCollection Groups
        {
            get
            {
                if (_Groups == null)
                {
                    lock (lockObj)
                    {
                        _Groups = new WorkingCopyGroupCollection();
                        _Groups?.SetIds(Id);
                    }
                }
                return _Groups;
            }
        }
        /// <summary>
        /// receipt for duplication check
        /// (receipt + WorkFlowCode)
        /// </summary>
        public string DuplicateReceipt { get; set; }

        /// <summary>
        /// accept all changes include sub-items
        /// </summary>
        /// <param name="acceptAll"></param>
        public void AcceptChanges(bool acceptAll)
        {
            base.AcceptChanges();
            if (acceptAll)
            {
                // work
                Arguments?.AcceptChanges();
                // steps
                Steps?.Select(x =>
                {
                    x.AcceptChanges(acceptAll);
                    return x;
                })?.ToList();
                // flows
                Flows?.Select(x =>
                {
                    x.AcceptChanges(acceptAll);
                    return x;
                })?.ToList();
                // groups
                Groups?.Select(x =>
                {
                    x.AcceptChanges(acceptAll);
                    return x;
                })?.ToList();
            }
        }
    }
}