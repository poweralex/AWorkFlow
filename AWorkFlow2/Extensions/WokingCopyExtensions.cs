using AWorkFlow2.Models.Configs;
using AWorkFlow2.Models.Working;
using AWorkFlow2.Providers;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2
{
    /// <summary>
    /// working copy extensions
    /// </summary>
    public static class WokingCopyExtensions
    {
        /// <summary>
        /// start a new working copy by workflow and input
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static WorkingCopy StartNew(this WorkFlowConfig workflow, WorkFlowExecutionProvider provider, Dictionary<string, object> input, string duplicateReceipt)
        {
            var res = provider.StartNew(new List<WorkFlowConfig> { workflow }, input, duplicateReceipt);
            return res.Result?.Data?.FirstOrDefault();
        }
        /// <summary>
        /// start a batch of new working copies by workflows and input
        /// </summary>
        /// <param name="workflows"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<WorkingCopy> StartNew(this IEnumerable<WorkFlowConfig> workflows, WorkFlowExecutionProvider provider, Dictionary<string, object> input, string duplicateReceipt)
        {
            var res = provider.StartNew(workflows, input, duplicateReceipt);
            return res.Result?.Data;
        }
    }
}
