using Dapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.DataAccessNS;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Repos
{
    internal class WorkFlowConfigActionRepository : RepositoryBase<WorkFlowConfigActionEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_workflow_action"; } }

        internal Task<OperationResult<IEnumerable<WorkFlowConfigActionEntity>>> Search(params string[] refIds)
        {
            return Search((IEnumerable<string>)refIds);
        }
        internal async Task<OperationResult<IEnumerable<WorkFlowConfigActionEntity>>> Search(IEnumerable<string> refIds)
        {
            try
            {
                string sql = $"select * from {TableName} where RefId in @refIds ";

                var result = await Db.QueryAsync<WorkFlowConfigActionEntity>(sql, new { refIds });
                return new OperationResult<IEnumerable<WorkFlowConfigActionEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkFlowConfigActionEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
