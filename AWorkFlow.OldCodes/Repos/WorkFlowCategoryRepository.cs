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
    /// repository of workflow category
    /// </summary>
    internal class WorkFlowCategoryRepository : RepositoryBase<WorkFlowCategoryEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_category"; } }

        /// <summary>
        /// search by key
        /// </summary>
        /// <param name="fuzzy">is fuzzy search</param>
        /// <param name="key">search key</param>
        /// <returns></returns>
        internal async Task<OperationResult<IEnumerable<WorkFlowCategoryEntity>>> Search(bool fuzzy = false, string key = "")
        {
            try
            {
                string sql = $"select * from {TableName}";
                if (!string.IsNullOrEmpty(key))
                {
                    if (fuzzy)
                    {
                        sql += " where category like @key";
                        key = $"%{key}%";
                    }
                    else
                    {
                        sql += " where category = @key";
                    }
                }

                var result = await Db.QueryAsync<WorkFlowCategoryEntity>(sql, new { key });
                return new OperationResult<IEnumerable<WorkFlowCategoryEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkFlowCategoryEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
