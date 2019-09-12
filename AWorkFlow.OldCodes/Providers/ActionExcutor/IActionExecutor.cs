using Mcs.SF.WorkFlow.Api.Models;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers.ActionExcutor
{
    /// <summary>
    /// action executor interface
    /// </summary>
    interface IActionExecutor
    {
        /// <summary>
        /// execute
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        Task<ActionExecuteResult> Execute(ArgumentProvider argument);
        /// <summary>
        /// initialize executor setting
        /// </summary>
        /// <param name="actionSetting"></param>
        void InitializeSetting(string actionSetting);
    }
}
