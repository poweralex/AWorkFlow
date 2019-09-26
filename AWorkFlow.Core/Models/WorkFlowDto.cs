using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Models
{
    /// <summary>
    /// work flow config dto
    /// </summary>
    public class WorkFlowDto
    {
        /// <summary>
        /// workflow code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// workflow category
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// version number of the code
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// is disabled
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// selectors which will run first to determine if this workflow is good to go
        /// </summary>
        public List<ActionSettingDto> Selectors { get; set; }
        /// <summary>
        /// pre-actions to run before first step(eg. preparations, startups)
        /// </summary>
        public List<ActionSettingDto> PreActions { get; set; }
        /// <summary>
        /// after-actions to run after last step(eg. cleanup or notifications)
        /// </summary>
        public List<ActionSettingDto> AfterActions { get; set; }
        /// <summary>
        /// actions to run while any step fails if not IgnoreDefaultFailureHandlers
        /// </summary>
        public List<ActionSettingDto> DefaultFailureHandlers { get; set; }
        /// <summary>
        /// output expression
        /// </summary>
        public string OutputExp { get; set; }
        /// <summary>
        /// work flow steps
        /// </summary>
        public List<WorkFlowStepDto> Steps { get; set; }
        /// <summary>
        /// work flow directions
        /// </summary>
        public List<WorkFlowDirectionDto> Flows { get; set; }

        /// <summary>
        /// indicate by selector
        /// </summary>
        /// <param name="data"></param>
        /// <returns>if this workflow suits the data</returns>
        internal async Task<bool> Suit(object data)
        {
            if (Selectors?.Any() != true)
            {
                return true;
            }

            // TODO: execute selector(s) to check
            return true;
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// work flow step config dto
    /// </summary>
    public class WorkFlowStepDto
    {
        /// <summary>
        /// unique code to locate this step
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// tag expressions represents this step
        /// </summary>
        public List<string> TagExps { get; set; }
        /// <summary>
        /// data expression represents this step
        /// </summary>
        public string TagDataExp { get; set; }
        /// <summary>
        /// group expression
        /// </summary>
        public string GroupExp { get; set; }
        /// <summary>
        /// indicates if this step goes by qty
        /// </summary>
        public bool ByQty { get; set; }
        /// <summary>
        /// match qty expression
        /// </summary>
        public string MatchQtyExp { get; set; }
        /// <summary>
        /// max times that allow to run (if executions reach that count, this step goes fail)
        /// </summary>
        public int? MaxTimesToRun { get; set; }
        /// <summary>
        /// indicates if default failure handler will be executed while this step fails
        /// </summary>
        public bool IgnoreDefaultFailureHandlers { get; set; }
        /// <summary>
        /// indicates if this is the first step
        /// </summary>
        public bool IsBegin { get; set; }
        /// <summary>
        /// indicates if this is the last step
        /// </summary>
        public bool IsEnd { get; set; }
        /// <summary>
        /// indicates if this is the manual step(execute and submit success or fail by outside)
        /// </summary>
        public bool IsManual { get; set; }
        /// <summary>
        /// output expression
        /// </summary>
        public Dictionary<string, string> OutputExps { get; set; }
        /// <summary>
        /// pre-actions to run before actions
        /// </summary>
        public List<ActionSettingDto> PreActions { get; set; }
        /// <summary>
        /// actions of this step
        /// </summary>
        public List<ActionSettingDto> Actions { get; set; }
        /// <summary>
        /// after-actions to run after actions
        /// </summary>
        public List<ActionSettingDto> AfterActions { get; set; }
    }

    /// <summary>
    /// work flow direction config dto
    /// </summary>
    public class WorkFlowDirectionDto
    {
        /// <summary>
        /// current step code
        /// </summary>
        public string StepCode { get; set; }
        /// <summary>
        /// next step code
        /// </summary>
        public string NextStepCode { get; set; }
        /// <summary>
        /// indicates if next step will be post by loop
        /// </summary>
        public string LoopByExp { get; set; }
        /// <summary>
        /// when to post next step
        /// </summary>
        public WorkFlowNextOn NextOn { get; set; }

        internal bool IsFulfilled(WorkDto workDto)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<WorkStepDto> PostSteps(WorkDto workDto)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// post next step condition
    /// </summary>
    public enum WorkFlowNextOn
    {
        /// <summary>
        /// on step success
        /// </summary>
        OnSuccess,
        /// <summary>
        /// on step fail
        /// </summary>
        OnFail,
        /// <summary>
        /// on part(qty) of step success
        /// </summary>
        OnPartialSuccess,
        /// <summary>
        /// on part(qty) of step fail
        /// </summary>
        OnPartialFail,
        /// <summary>
        /// on all of same group success
        /// </summary>
        OnGroupAllSuccess,
        /// <summary>
        /// on all of same group fail
        /// </summary>
        OnGroupAllFail,
        /// <summary>
        /// on any of same group success
        /// </summary>
        OnGroupAnySuccess,
        /// <summary>
        /// on any of same group fail
        /// </summary>
        OnGroupAnyFail,
    }
}
