using AWorkFlow2.Models;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// action executor interface
    /// </summary>
    public interface IActionExecutor
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
