using AWorkFlow2.Models;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// interface of variable processor
    /// </summary>
    public interface IVariableProcessor
    {
        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument);
    }
}
