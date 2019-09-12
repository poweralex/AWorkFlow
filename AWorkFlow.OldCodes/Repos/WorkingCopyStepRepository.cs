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
    /// working step repository
    /// </summary>
    public class WorkingCopyStepRepository : RepositoryBase<WorkingCopyStepEntity>
    {
        /// <summary>
        /// table name
        /// </summary>
        internal static string TableName { get { return "wf_working_step"; } }

        internal Task<OperationResult<IEnumerable<WorkingCopyStepEntity>>> Search(string workingCopyId = null, bool? finished = null, string statusId = null)
        {
            return Search(string.IsNullOrEmpty(workingCopyId) ? null : new List<string> { workingCopyId }, finished, statusId);
        }

        internal async Task<OperationResult<IEnumerable<WorkingCopyStepEntity>>> Search(IEnumerable<string> workingCopyIds = null, bool? finished = null, string statusId = null, int? count = null)
        {
            try
            {
                string sql = $"select * from {TableName} where 1=1 ";
                if (workingCopyIds?.Any() == true)
                {
                    sql += " and WorkingCopyId in @workingCopyIds ";
                }
                if (finished != null)
                {
                    if (finished.Value)
                    {
                        sql += " and (Finished = 1 or Cancelled = 1) ";
                    }
                    else
                    {
                        sql += " and Finished = 0 and Cancelled = 0";
                    }
                }
                if (!string.IsNullOrEmpty(statusId))
                {
                    sql += " and StatusId = @statusId ";
                }
                if (count.HasValue)
                {
                    sql += $" limit {count}";
                }

                var result = await Db.QueryAsync<WorkingCopyStepEntity>(sql, new { workingCopyIds, finished, statusId });
                return new OperationResult<IEnumerable<WorkingCopyStepEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopyStepEntity>> { Success = false, Message = ex.Message };
            }
        }

        internal async Task<OperationResult<IEnumerable<WorkingCopyStepEntity>>> SearchRequireAfterSteps(string workingCopyId = "")
        {
            try
            {
                string sql = $"select wf_working_step.* from wf_working_step ";
                sql += " left join wf_working_flow on wf_working_flow.CurrentStepId = wf_working_step.id ";
                sql += " left join wf_working on wf_working.id = wf_working_step.WorkingCopyId ";
                sql += $" where wf_working_step.Finished = 1 and wf_working_flow.Id is null ";
                sql += " and wf_working.IsFinished = 0 and wf_working.IsCancelled = 0 ";
                if (!string.IsNullOrEmpty(workingCopyId))
                {
                    sql += " and wf_working_step.WorkingCopyId = @workingCopyId ";
                }

                var result = await Db.QueryAsync<WorkingCopyStepEntity>(sql, new { workingCopyId });
                return new OperationResult<IEnumerable<WorkingCopyStepEntity>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopyStepEntity>> { Success = false, Message = ex.Message };
            }
        }

        internal async Task<OperationResult<IEnumerable<WorkingCopyStepEntity>>> AllocateSteps(
            IEnumerable<string> workingCopyIds = null, 
            int? count = null, 
            int receiptExpiredTimeInSeconds = 60)
        {
            try
            {
                string allocateToken = Guid.NewGuid().ToString();
                string allocateSql = $"update wf_working_step" +
                    $" inner join (" +
                    $" select distinct wf_working_step.Id, wf_working_step.ActiveTime, working_step_results.ExecuteTimes" +
                    $" from wf_working_step" +
                    $" left join wf_working on wf_working.id = wf_working_step.WorkingCopyId" +
                    $" left join (" +
                    $" select wf_working_step.Id WorkingStepId, Count(wf_working_step_result.Id) ExecuteTimes" +
                    $" from wf_working_step" +
                    $" left join wf_working_step_result on wf_working_step_result.WorkingStepId = wf_working_step.Id" +
                    $" group by wf_working_step.Id" +
                    $" ) working_step_results on working_step_results.WorkingStepId = wf_working_step.Id" +
                    $" where wf_working.IsFinished = 0 and wf_working.IsCancelled = 0" +
                    $" and wf_working_step.Finished = 0 and wf_working_step.Cancelled = 0" +
                    $" and (AllocationTime is null or TIMESTAMPDIFF(SECOND, AllocationTime, @now) > {receiptExpiredTimeInSeconds})";
                if (workingCopyIds?.Any(x => !string.IsNullOrEmpty(x)) == true)
                {
                    allocateSql += $" and wf_working_step.WorkingCopyId in @workingCopyIds";
                }

                allocateSql += $" order by working_step_results.ExecuteTimes, wf_working_step.ActiveTime";
                if (count.HasValue)
                {
                    allocateSql += $" limit {count}";
                }
                allocateSql += $" ) conditions on conditions.Id = wf_working_step.Id" +
                    $" set AllocationToken = @token, AllocationTime = @now";

                string getStepsSql = $"select * from {TableName} where AllocationToken = @token";

                var allocateResult = await Db.ExecuteAsync(allocateSql, new { workingCopyIds, token = allocateToken, now = DateTime.UtcNow });
                if (allocateResult > 0)
                {
                    var result = await Db.QueryAsync<WorkingCopyStepEntity>(getStepsSql, new { token = allocateToken });
                    return new OperationResult<IEnumerable<WorkingCopyStepEntity>> { Success = true, Data = result };
                }
                else
                {
                    return new OperationResult<IEnumerable<WorkingCopyStepEntity>> { Success = true, Data = new List<WorkingCopyStepEntity>() };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopyStepEntity>> { Success = false, Message = ex.Message };
            }
        }
    }
}
