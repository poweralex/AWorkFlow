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
    /// working copy repository
    /// </summary>
    public class WorkingCopyRepository : RepositoryBase<WorkingCopyEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_working"; } }

        internal async Task<OperationResult<IEnumerable<WorkingCopyEntity>>> Search(string workingCopyId)
        {
            try
            {
                string sql = $"select * from {TableName} where 1=1 ";
                if (!string.IsNullOrEmpty(workingCopyId))
                {
                    sql += " and Id = @workingCopyId ";
                }

                var result = await Db.QueryAsync<WorkingCopyEntity>(sql, new { workingCopyId });
                return new OperationResult<IEnumerable<WorkingCopyEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopyEntity>> { Success = false, Message = ex.Message };
            }
        }
        internal async Task<OperationResult<IEnumerable<WorkingCopyEntity>>> Search(IEnumerable<string> workingCopyIds = null, string category = null, bool? finished = null)
        {
            try
            {
                string sql = $"select {TableName}.* from {TableName} " +
                    $"inner join {WorkFlowConfigRepository.TableName} on {WorkFlowConfigRepository.TableName}.{nameof(WorkFlowConfigEntity.Id)} = {TableName}.{nameof(WorkingCopyEntity.WorkFlowId)} " +
                    $"where 1=1 ";
                if (workingCopyIds?.Any(x => !string.IsNullOrEmpty(x)) == true)
                {
                    sql += $" and {TableName}.Id in @workingCopyIds ";
                }
                if (!string.IsNullOrEmpty(category))
                {
                    sql += $" and {WorkFlowConfigRepository.TableName}.category = @category ";
                }
                if (finished != null)
                {
                    if (finished.Value)
                    {
                        sql += $" and {TableName}.EndTime is not null ";
                    }
                    else
                    {
                        sql += $" and {TableName}.EndTime is null ";
                    }
                }

                var result = await Db.QueryAsync<WorkingCopyEntity>(sql, new { workingCopyIds, category, finished });
                return new OperationResult<IEnumerable<WorkingCopyEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopyEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
