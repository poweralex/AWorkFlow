using Mcs.SF.WorkFlow.Api.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers.ActionExcutor
{
    /// <summary>
    /// variable process type action executor
    /// </summary>
    public class VariableProcessActionExecutor : IActionExecutor
    {
        internal VariableProcessActionSetting Settings { get; set; }

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(ArgumentProvider argument)
        {
            var executor = IocManager.GetByName<IVariableProcessor>(Settings.Method?.ToUpper());
            return executor.Execute(Settings, argument);
        }

        /// <summary>
        /// initialize setting
        /// </summary>
        /// <param name="actionSetting"></param>
        public void InitializeSetting(string actionSetting)
        {
            Settings = JsonConvert.DeserializeObject<VariableProcessActionSetting>(actionSetting);
        }
    }

    /// <summary>
    /// variable process action setting model
    /// </summary>
    public class VariableProcessActionSetting
    {
        /// <summary>
        /// method
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// output setting
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }
}
