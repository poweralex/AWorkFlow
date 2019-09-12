using Dapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.DataAccessNS;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Repos
{
    internal class WorkFlowConfigFlowRepository : RepositoryBase<WorkFlowConfigFlowEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_workflow_flow"; } }

        internal Task<OperationResult<IEnumerable<WorkFlowConfigFlowEntity>>> Search(params string[] workflowIds)
        {
            return Search((IEnumerable<string>)workflowIds);
        }
        internal async Task<OperationResult<IEnumerable<WorkFlowConfigFlowEntity>>> Search(IEnumerable<string> workflowIds)
        {
            try
            {
                string sql = $"select * from {TableName} where WorkFlowId in @workflowIds ";

                var result = await Db.QueryAsync<WorkFlowConfigFlowEntity>(sql, new { workflowIds });
                return new OperationResult<IEnumerable<WorkFlowConfigFlowEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkFlowConfigFlowEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
