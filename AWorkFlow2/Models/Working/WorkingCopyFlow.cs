using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// data model for working flow
    /// </summary>
    public class WorkingCopyFlow : WorkingModelBase
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// from steps
        /// </summary>
        public WorkingCopyFlowSeed FromStep { get; set; }
        /// <summary>
        /// from execution result
        /// </summary>
        public WorkingCopyStepResult ExecutionResult { get; set; }
        /// <summary>
        /// to steps
        /// </summary>
        public WorkingCopyFlowSeed ToStep { get; set; }

        /// <summary>
        /// accept all changes include sub-items
        /// </summary>
        /// <param name="acceptAll"></param>
        public void AcceptChanges(bool acceptAll)
        {
            base.AcceptChanges();
        }

        internal override string GetContentBrief()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"flowId:{Id},");
            sb.Append($"from:({string.Join(",", FromStep?.Steps?.Select(x => x.Id))})|({ExecutionResult?.Id})");
            sb.Append($"to:({string.Join(",", ToStep?.Steps?.Select(x => x.Id))})");
            return sb.ToString();
        }
    }

    /// <summary>
    /// working flow seed
    /// </summary>
    public class WorkingCopyFlowSeed
    {
        /// <summary>
        /// constructor for one step
        /// </summary>
        /// <param name="step"></param>
        public WorkingCopyFlowSeed(WorkingCopyStep step)
        {
            Steps = new List<WorkingCopyStep> { step };
        }

        /// <summary>
        /// constructor for steps
        /// </summary>
        /// <param name="steps"></param>
        public WorkingCopyFlowSeed(IEnumerable<WorkingCopyStep> steps)
        {
            Steps = steps?.ToList();
        }
        /// <summary>
        /// if group steps
        /// </summary>
        public bool IsGroup { get { return Steps?.Count() > 1; } }
        /// <summary>
        /// step(s)
        /// </summary>
        public List<WorkingCopyStep> Steps { get; private set; }

        /// <summary>
        /// determine if this seed contains the step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public bool Contains(WorkingCopyStep step)
        {
            return Steps?.Contains(step) ?? false;
        }

        /// <summary>
        /// add step
        /// </summary>
        /// <param name="step"></param>
        public void AddStep(WorkingCopyStep step)
        {
            if (Steps == null)
            {
                Steps = new List<WorkingCopyStep>();
            }
            Steps.Add(step);
        }
    }

    /// <summary>
    /// working flow collection
    /// </summary>
    public class WorkingCopyFlowCollection : ICollection<WorkingCopyFlow>
    {
        private readonly List<WorkingCopyFlow> flows = new List<WorkingCopyFlow>();
        /// <summary>
        /// count
        /// </summary>
        public int Count => flows.Count;

        /// <summary>
        /// is readonly
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// add item
        /// </summary>
        /// <param name="item"></param>
        public void Add(WorkingCopyFlow item) => flows.Add(item);
        /// <summary>
        /// add items
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<WorkingCopyFlow> items) => flows.AddRange(items);

        /// <summary>
        /// clear
        /// </summary>
        public void Clear() => flows.Clear();
        /// <summary>
        /// contains
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(WorkingCopyFlow item) => flows.Contains(item);

        /// <summary>
        /// copy to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(WorkingCopyFlow[] array, int arrayIndex) => flows.CopyTo(array, arrayIndex);
        /// <summary>
        /// get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<WorkingCopyFlow> GetEnumerator() => flows.GetEnumerator();

        /// <summary>
        /// remove item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(WorkingCopyFlow item) => flows.Remove(item);

        /// <summary>
        /// get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => flows.GetEnumerator();
    }
}
