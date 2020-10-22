using AWorkFlow2.Models.Configs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model of working group
    /// </summary>
    public class WorkingCopyGroup : WorkingModelBase
    {
        /// <summary>
        /// Group id
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// work id
        /// </summary>
        public string WorkingCopyId { get; set; }
        /// <summary>
        /// workflow flow id
        /// </summary>
        public string FLowId { get; set; }
        /// <summary>
        /// group start step code
        /// </summary>
        public string StartStepCode { get; set; }
        /// <summary>
        /// group end step code
        /// </summary>
        public string EndStepCode { get; set; }
        /// <summary>
        /// if group fully fulfilled
        /// </summary>
        public bool Fulfilled { get; set; }
        /// <summary>
        /// if this group matchs any success
        /// (check all (not-cancelled) steps)
        /// </summary>
        [IgnoreTracking]
        public bool AnySuccess => Steps?.Where(x => x != null && !x.Cancelled)?.Any(x => x.ActionFinished && x.Success) ?? false;
        /// <summary>
        /// if this group matchs any fail
        /// (check all (not-cancelled) steps)
        /// </summary>
        [IgnoreTracking]
        public bool AnyFail => Steps?.Where(x => x != null && !x.Cancelled)?.Any(x => x.ActionFinished && !x.Success) ?? false;
        /// <summary>
        /// if this group matchs all success
        /// (check all (not-cancelled) steps)
        /// </summary>
        [IgnoreTracking]
        public bool AllSuccess => Fulfilled && (Steps?.Where(x => x != null && !x.Cancelled)?.All(x => x.ActionFinished && x.Success) ?? false);
        /// <summary>
        /// if this group matchs all fail
        /// (check all (not-cancelled) steps)
        /// </summary>
        [IgnoreTracking]
        public bool AllFail => Fulfilled && (Steps?.Where(x => x != null && !x.Cancelled)?.All(x => x.ActionFinished && !x.Success) ?? false);
        /// <summary>
        /// if this group posted next
        /// </summary>
        public bool PostedNext { get; set; }
        /// <summary>
        /// if this group is finished
        /// </summary>
        [IgnoreTracking]
        public bool Finished => Fulfilled && PostedNext;
        /// <summary>
        /// steps belong to this group
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyStepCollection Steps { get; private set; } = new WorkingCopyStepCollection(false);
        /// <summary>
        /// group end steps
        /// </summary>
        [IgnoreTracking]
        public IEnumerable<WorkingCopyStep> BeginSteps => Steps?.Where(x => x?.Cancelled != true && string.Equals(x?.Code, StartStepCode, StringComparison.CurrentCultureIgnoreCase));
        /// <summary>
        /// group end steps
        /// </summary>
        [IgnoreTracking]
        public IEnumerable<WorkingCopyStep> EndSteps => Steps?.Where(x => x?.Cancelled != true && string.Equals(x?.Code, EndStepCode, StringComparison.CurrentCultureIgnoreCase));
        /// <summary>
        /// group end steps
        /// </summary>
        [IgnoreTracking]
        public IEnumerable<WorkingCopyStep> SuccessSteps => Steps?.Where(x => x?.Cancelled != true && x?.Finished == true && x?.Success == true);
        /// <summary>
        /// group end steps
        /// </summary>
        [IgnoreTracking]
        public IEnumerable<WorkingCopyStep> FailSteps => Steps?.Where(x => x?.Cancelled != true && x?.Finished == true && x?.Success == false);
        /// <summary>
        /// groups belong to this group
        /// </summary>
        [IgnoreTracking]
        public WorkingCopyGroupCollection Groups { get; private set; } = new WorkingCopyGroupCollection(false);

        /// <summary>
        /// refresh group
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        public WorkingCopyGroup RefreshGroup(WorkingCopy work, WorkFlowConfig workflow)
        {
            var currentSteps = Steps.ToList();
            currentSteps.ForEach(startStep =>
                Steps.AddRange(
                    FindSteps(work, startStep, EndStepCode)
                    .Where(newStep => !Steps.Any(step => step.Id == newStep.Id))
                    )
                );

            // find all sub-groups(which end step belongs to this group)
            var currentGroups = Groups.ToList();
            Groups.AddRange(work.Groups
                .Where(x => !string.Equals(x.Id, Id))
                .Where(x => !currentGroups.Contains(x))
                .Where(x => !(x?.BeginSteps?.Any(x1 => BeginSteps.Contains(x1)) == true
                    && x?.EndStepCode == EndStepCode)) // except groups which beginstep and endstep are equal to this
                .Where(x =>
                    x.EndSteps.Any(y => Steps.Contains(y))
                )
            );

            Fulfilled = (Steps?.Any(x => !x.Cancelled && string.Equals(x.Code, EndStepCode, System.StringComparison.CurrentCultureIgnoreCase)) ?? false)
                && (Steps?.Where(x => !x.Cancelled).All(x => x.ActionFinished && (x.PostedNext || string.Equals(x.Code, EndStepCode, System.StringComparison.CurrentCultureIgnoreCase))) ?? false)
                && (Groups?.All(x => x.Finished) == true);

            return this;
        }

        /// <summary>
        /// build a group
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <param name="flow"></param>
        /// <param name="startStepId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<WorkingCopyGroup> BuildGroup(WorkingCopy work, WorkFlowConfig workflow, WorkFlowConfigFlow flow, string startStepId, string user)
        {
            var startSteps = work.Steps.Where(x => x.Code == flow.GroupStartStepCode
            && (string.IsNullOrEmpty(startStepId) || x.Id == startStepId))
            .ToList();
            var groups = startSteps.Select(startStep =>
            {
                var group = new WorkingCopyGroup
                {
                    FLowId = flow.Id,
                    StartStepCode = flow.GroupStartStepCode,
                    EndStepCode = flow.CurrentStepCode,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = user
                };
                group.Steps.Add(startStep);
                group.RefreshGroup(work, workflow);

                return group;
            });

            return groups;
        }

        private static IEnumerable<WorkingCopyStep> FindSteps(WorkingCopy work, WorkingCopyStep startStep, string endStepCode)
        {
            List<WorkingCopyStep> steps = new List<WorkingCopyStep>();
            if (startStep.Code == endStepCode)
            {
                return steps;
            }
            var nextSteps = work.Flows.Where(x => x.FromStep.Contains(startStep)).SelectMany(x => x.ToStep?.Steps).ToList().Distinct();

            steps.AddRange(nextSteps);
            if (nextSteps?.Any() == true)
            {
                steps.AddRange(nextSteps.SelectMany(x => FindSteps(work, x, endStepCode)));
            }
            return steps;
        }

        /// <summary>
        /// accept all changes include sub-items
        /// </summary>
        /// <param name="acceptAll"></param>
        public void AcceptChanges(bool acceptAll)
        {
            base.AcceptChanges();
        }
    }

    /// <summary>
    /// working group collection
    /// </summary>
    public class WorkingCopyGroupCollection : ICollection<WorkingCopyGroup>
    {
        private readonly object lockObj = new object();
        private readonly List<WorkingCopyGroup> groups = new List<WorkingCopyGroup>();
        private readonly bool _manageIds = true;
        private string _workId = string.Empty;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="manageIds"></param>
        public WorkingCopyGroupCollection(bool manageIds = true)
        {
            _manageIds = manageIds;
        }

        /// <summary>
        /// set ids for this collection
        /// </summary>
        /// <param name="workId"></param>
        public void SetIds(string workId)
        {
            _workId = workId;
            if (_manageIds)
            {
                if (groups?.Any() == true)
                {
                    lock (lockObj)
                    {
                        foreach (var group in groups)
                        {
                            group.WorkingCopyId = _workId;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// count
        /// </summary>
        public int Count => groups.Count;

        /// <summary>
        /// is readyonly
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// add
        /// </summary>
        /// <param name="item"></param>
        public void Add(WorkingCopyGroup item)
        {
            if (_manageIds)
            {
                item.WorkingCopyId = _workId;
            }
            groups.Add(item);
        }
        /// <summary>
        /// add range
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<WorkingCopyGroup> items)
        {
            if (_manageIds)
            {
                groups.AddRange(items.Select(x =>
                {
                    x.WorkingCopyId = _workId;
                    return x;
                }));
            }
            else
            {
                groups.AddRange(items);
            }
        }

        /// <summary>
        /// clear
        /// </summary>
        public void Clear() => groups.Clear();

        /// <summary>
        /// contains
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(WorkingCopyGroup item) => groups.Contains(item);

        /// <summary>
        /// copyto array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(WorkingCopyGroup[] array, int arrayIndex) => groups.CopyTo(array, arrayIndex);

        /// <summary>
        /// get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<WorkingCopyGroup> GetEnumerator() => groups.GetEnumerator();

        /// <summary>
        /// remove item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(WorkingCopyGroup item) => groups.Remove(item);

        /// <summary>
        /// get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => groups.GetEnumerator();
    }

}
