using AWorkFlow2.Models;
using AWorkFlow2.Providers;
using AWorkFlow2.Providers.ActionExcutor;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkFlow.Test
{
    class UnitTestActionProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "UTAction";

        static readonly Dictionary<string, int> executionCounts = new Dictionary<string, int>();

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public async Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<UnitTestActionSetting>(actionSetting);
            var key = setting.Key;
            var currentCount = 0;
            if (executionCounts.ContainsKey(key))
            {
                currentCount = executionCounts[key] + 1;
            }
            executionCounts[key] = currentCount;
            bool result = true;
            if (setting?.ResultSequence?.Any() != true)
            {
                result = true;
            }
            if (setting.ResultSequence.Count < currentCount)
            {
                result = true;
            }
            else
            {
                result = setting.ResultSequence[currentCount];
            }
            var res = new ActionExecuteResult
            {
                Output = new Dictionary<string, string> { { "result", $"{result}" } },
                Data = $"{result}"
            };
            if (result || setting.DirectFail)
            {
                res.Success = result;
                res.Fail = !result;
            }

            return res;
        }
    }

    class UnitTestActionSetting
    {
        public string Key { get; set; }
        public List<bool> ResultSequence { get; set; }
        public bool DirectFail { get; set; }
    }
}
