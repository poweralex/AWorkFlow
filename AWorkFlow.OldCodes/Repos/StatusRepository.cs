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
    /// repository of workflow status
    /// </summary>
    internal class StatusRepository : RepositoryBase<StatusEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_status"; } }

        /// <summary>
        /// search by key
        /// </summary>
        /// <param name="targetIds">target ids</param>
        /// <param name="statuses">statuses</param>
        /// <returns></returns>
        internal async Task<OperationResult<IEnumerable<StatusEntity>>> Search(IEnumerable<string> targetIds = null, IEnumerable<string> statuses = null)
        {
            try
            {
                string sql = $"select * from {TableName} where 1=1";
                if (targetIds?.Any() == true)
                {
                    sql += " and TargetId in @targetIds";
                }
                if (statuses?.Any() == true)
                {
                    sql += " and Status in @statuses";
                }

                var result = await Db.QueryAsync<StatusEntity>(sql, new { targetIds, statuses });
                return new OperationResult<IEnumerable<StatusEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<StatusEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
