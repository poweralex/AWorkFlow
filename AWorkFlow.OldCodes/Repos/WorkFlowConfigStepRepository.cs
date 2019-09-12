using Dapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.DataAccessNS;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Repos
{
    internal class WorkFlowConfigStepRepository : RepositoryBase<WorkFlowConfigStepEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_workflow_step"; } }

        internal Task<OperationResult<IEnumerable<WorkFlowConfigStepEntity>>> Search(params string[] workflowIds)
        {
            return Search((IEnumerable<string>)workflowIds);
        }
        internal async Task<OperationResult<IEnumerable<WorkFlowConfigStepEntity>>> Search(IEnumerable<string> workflowIds)
        {
            try
            {
                string sql = $"select * from {TableName} where WorkFlowId in @workflowIds ";

                var result = await Db.QueryAsync<WorkFlowConfigStepEntity>(sql, new { workflowIds });
                return new OperationResult<IEnumerable<WorkFlowConfigStepEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkFlowConfigStepEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
