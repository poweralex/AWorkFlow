using Autofac;
using AWorkFlow2.Models;
using AWorkFlow2.Models.Configs;
using AWorkFlow2.Providers.ActionExcutor;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers
{
    /// <summary>
    /// executor for all actions
    /// </summary>
    public class ActionExecutor
    {
        private readonly IContainer _actionContainer;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="actionContainer"></param>
        public ActionExecutor(IContainer actionContainer)
        {
            _actionContainer = actionContainer;
        }

        /// <summary>
        /// executes action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public async Task<ActionExecuteResult> Execute(WorkFlowActionSetting action, ArgumentProvider argument)
        {
            string actionSetting = JsonConvert.SerializeObject(action?.ActionConfig);
            var executor = _actionContainer.ResolveNamed<IActionExecutor>(action?.Type.ToString());
            executor.InitializeSetting(actionSetting);
            var actionExecuteResult = await executor.Execute(argument);
            // process output
            if (action?.Output?.Any() == true)
            {
                argument.PutPrivate("result", actionExecuteResult.Data);
                actionExecuteResult.Output = new Dictionary<string, string>();
                foreach (var kvp in action?.Output)
                {
                    actionExecuteResult.Output[kvp.Key] = argument.Format(kvp.Value);
                }
            }

            return actionExecuteResult;
        }

    }
}
