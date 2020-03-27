using Autofac;
using AWorkFlow2.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// variable process type action executor
    /// </summary>
    public class VariableProcessActionExecutor : IActionExecutor
    {
        internal VariableProcessActionSetting Settings { get; set; }
        internal string ActionSetting { get; set; }

        private readonly IContainer _processorContainer;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="processorContainer">contains processors</param>
        public VariableProcessActionExecutor(IContainer processorContainer)
        {
            _processorContainer = processorContainer;
        }

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(ArgumentProvider argument)
        {
            var executor = _processorContainer.ResolveNamed<IVariableProcessor>(Settings.Method?.ToUpper());
            return executor.Execute(ActionSetting, argument);
        }

        /// <summary>
        /// initialize setting
        /// </summary>
        /// <param name="actionSetting"></param>
        public void InitializeSetting(string actionSetting)
        {
            ActionSetting = actionSetting;
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
    }
}
