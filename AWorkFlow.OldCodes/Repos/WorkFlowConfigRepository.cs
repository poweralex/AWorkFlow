using Dapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.DataAccessNS;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Repos
{
    internal class WorkFlowConfigRepository : RepositoryBase<WorkFlowConfigEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_workflow"; } }

        internal async Task<OperationResult<IEnumerable<WorkFlowConfigEntity>>> Search(bool fuzzy, string category = "", string code = "", int? version = null)
        {
            try
            {
                string sql = $"select * from {TableName} where 1=1 ";
                if (!string.IsNullOrEmpty(category))
                {
                    if (fuzzy)
                    {
                        sql += " and category like @category";
                        category = $"%{category}%";
                    }
                    else
                    {
                        sql += " and category = @category";
                    }
                }
                if (!string.IsNullOrEmpty(code))
                {
                    if (fuzzy)
                    {
                        sql += " and code like @code";
                        code = $"%{code}%";
                    }
                    else
                    {
                        sql += " and code = @code";
                    }
                }
                if (version != null)
                {
                    sql += " and version = @version";
                }

                var result = await Db.QueryAsync<WorkFlowConfigEntity>(sql, new { category, code, version });
                return new OperationResult<IEnumerable<WorkFlowConfigEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkFlowConfigEntity>> { Success = false, Message = ex.Message };
            }
        }
        internal async Task<OperationResult<IEnumerable<WorkFlowConfigEntity>>> Search(IEnumerable<string> ids)
        {
            try
            {
                string sql = $"select * from {TableName} where Id in @ids ";

                var result = await Db.QueryAsync<WorkFlowConfigEntity>(sql, new { ids });
                return new OperationResult<IEnumerable<WorkFlowConfigEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkFlowConfigEntity>> { Success = false, Message = ex.Message };
            }
        }

        internal async Task<OperationResult<IEnumerable<string>>> GetUsingCategories()
        {
            try
            {
                string sql = $"select distinct category from {TableName}";

                var result = await Db.QueryAsync<string>(sql);
                return new OperationResult<IEnumerable<string>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<string>> { Success = false, Message = ex.Message };
            }
        }
    }
}
