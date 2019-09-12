using Mcs.SF.WorkFlow.Api.Models;
using Mcs.SF.WorkFlow.Api.Models.Configs;
using Mcs.SF.WorkFlow.Api.Providers.ActionExcutor;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers
{
    internal class ActionExecutor
    {
        internal static Task<ActionExecuteResult> Execute(ActionType type, string actionSetting, ArgumentProvider argument)
        {
            var executor = IocManager.GetByName<IActionExecutor>(type.ToString());
            executor.InitializeSetting(actionSetting);
            return executor.Execute(argument);
        }

    }
}
