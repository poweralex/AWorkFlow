using Dapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.DataAccessNS;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Repos
{
    /// <summary>
    /// working step result repository
    /// </summary>
    public class WorkingCopyStepResultRepository : RepositoryBase<WorkingCopyStepResultEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_working_step_result"; } }

        internal Task<OperationResult<IEnumerable<WorkingCopyStepResultEntity>>> Search(string workingCopyId, string workingStepId)
        {
            return Search(
                string.IsNullOrEmpty(workingCopyId) ? null : new List<string> { workingCopyId },
                string.IsNullOrEmpty(workingStepId) ? null : new List<string> { workingStepId });
        }

        internal async Task<OperationResult<IEnumerable<WorkingCopyStepResultEntity>>> Search(IEnumerable<string> workingCopyIds = null, IEnumerable<string> workingStepIds = null)
        {
            try
            {
                string sql = $"select distinct {TableName}.* from {TableName} " +
                    $"inner join {WorkingCopyStepRepository.TableName} on {WorkingCopyStepRepository.TableName}.id = {TableName}.{nameof(WorkingCopyStepResultEntity.WorkingStepId)} " +
                    $"where 1=1 ";
                if (workingCopyIds?.Any() == true)
                {
                    sql += $" and {WorkingCopyStepRepository.TableName}.{nameof(WorkingCopyStepEntity.WorkingCopyId)} in @workingCopyIds ";
                }
                if (workingStepIds?.Any() == true)
                {
                    sql += $" and {TableName}.WorkingStepId in @workingStepIds ";
                }

                var result = await Db.QueryAsync<WorkingCopyStepResultEntity>(sql, new { workingCopyIds, workingStepIds });
                return new OperationResult<IEnumerable<WorkingCopyStepResultEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopyStepResultEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
