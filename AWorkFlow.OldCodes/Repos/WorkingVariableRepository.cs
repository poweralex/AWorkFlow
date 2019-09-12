using Dapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.DataAccessNS;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Repos
{
    /// <summary>
    /// working variable repository
    /// </summary>
    public class WorkingVariableRepository : RepositoryBase<WorkingVariableEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_working_variable"; } }

        internal Task<OperationResult<IEnumerable<WorkingVariableEntity>>> Search(string workingCopyId, string workingStepId, string key)
        {
            return Search(
                string.IsNullOrEmpty(workingCopyId) ? null : new List<string> { workingCopyId },
                string.IsNullOrEmpty(workingStepId) ? null : new List<string> { workingStepId },
                string.IsNullOrEmpty(key) ? null : new List<string> { key });
        }
        internal async Task<OperationResult<IEnumerable<WorkingVariableEntity>>> Search(IEnumerable<string> workingCopyIds = null, IEnumerable<string> workingStepIds = null, IEnumerable<string> keys = null)
        {
            try
            {
                workingCopyIds = workingCopyIds?.Where(x => !string.IsNullOrEmpty(x));
                workingStepIds = workingStepIds?.Where(x => !string.IsNullOrEmpty(x));
                keys = keys?.Where(x => !string.IsNullOrEmpty(x));
                string sql = $"select * from {TableName} where 1=1 ";
                if (workingCopyIds?.Any() == true)
                {
                    sql += " and WorkingCopyId in @workingCopyIds ";
                }
                if (workingStepIds?.Any() == true)
                {
                    sql += " and WorkingStepId in @workingStepIds ";
                }
                if (keys?.Any() == true)
                {
                    sql += " and Key in @key ";
                }

                var result = await Db.QueryAsync<WorkingVariableEntity>(sql, new { workingCopyIds, workingStepIds, keys });
                return new OperationResult<IEnumerable<WorkingVariableEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingVariableEntity>> { Success = false, Message = ex.Message };
            }
        }

        internal static Dictionary<string, string> GetVariableDictionary(IEnumerable<WorkingVariableEntity> entities, bool deserializeOutput, bool afterGroup)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            Dictionary<string, List<string>> tmp = new Dictionary<string, List<string>>();
            void addToDic(Dictionary<string, List<string>> dic, string key, string value)
            {
                if (dic == null)
                {
                    return;
                }
                if (dic.ContainsKey(key))
                {
                    dic[key].Add(value);
                }
                else
                {
                    dic[key] = new List<string> { value };
                }
            }

            // process output
            Dictionary<string, List<string>> outputs = new Dictionary<string, List<string>>();
            if (deserializeOutput)
            {
                var outputEntities = entities.Where(x => "output".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase));
                foreach (var outputEntity in outputEntities)
                {
                    var output = JsonConvert.DeserializeObject<Dictionary<string, string>>(outputEntity.Value);
                    foreach (var kvp in output)
                    {
                        addToDic(outputs, kvp.Key, kvp.Value);
                    }
                }
            }
            foreach (var kvp in outputs)
            {
                tmp[kvp.Key] = outputs[kvp.Key];
            }
            foreach (var variable in entities)
            {
                if (deserializeOutput && "output".Equals(variable.Key, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                if (deserializeOutput && outputs.ContainsKey(variable.Key))
                {
                    continue;
                }
                else
                {
                    addToDic(tmp, variable.Key, variable.Value);
                }
            }

            foreach (var kvp in tmp)
            {
                if (afterGroup)
                {
                    res.Add(kvp.Key, $"[{string.Join(",", kvp.Value)}]");
                }
                else
                {
                    res[kvp.Key] = kvp.Value.FirstOrDefault();
                }
            }

            return res;
        }
    }
}
