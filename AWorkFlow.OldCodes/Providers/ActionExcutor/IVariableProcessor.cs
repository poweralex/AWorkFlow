using Mcs.SF.WorkFlow.Api.Models;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers.ActionExcutor
{
    interface IVariableProcessor
    {
        Task<ActionExecuteResult> Execute(VariableProcessActionSetting setting, ArgumentProvider argument);
    }
}
