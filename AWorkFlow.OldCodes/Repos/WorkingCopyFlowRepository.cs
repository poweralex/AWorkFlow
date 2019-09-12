using Dapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.DataAccessNS;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Repos
{
    /// <summary>
    /// working flow repository
    /// </summary>
    public class WorkingCopyFlowRepository : RepositoryBase<WorkingCopyFlowEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_working_flow"; } }

        internal async Task<OperationResult<IEnumerable<WorkingCopyFlowEntity>>> Search(string workingCopyId, string workingStepId)
        {
            try
            {
                string sql = $"select * from {TableName} where 1=1 ";
                if (!string.IsNullOrEmpty(workingCopyId))
                {
                    sql += " and WorkingCopyId = @workingCopyId ";
                }
                if (!string.IsNullOrEmpty(workingCopyId))
                {
                    sql += " and (CurrentStepId = @workingStepId or NextStepId = @workingStepId) ";
                }

                var result = await Db.QueryAsync<WorkingCopyFlowEntity>(sql, new { workingCopyId, workingStepId });
                return new OperationResult<IEnumerable<WorkingCopyFlowEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopyFlowEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
