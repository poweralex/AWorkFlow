using AWorkFlow.Core.Extensions;
using AWorkFlow.Core.Models.Jobs;
using AWorkFlow.Core.Providers;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Models
{
    public class WorkDto
    {
        public string WorkId { get; set; }
        public string WorkFlowCode { get; set; }
        public int WorkFlowVersion { get; set; }
        [IgnoreDataMember]
        public WorkFlowDto WorkFlow { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        public ArgumentsDto Arguments { get; set; }
        public string Status { get; set; }
        public List<WorkStatusDto> Statuses { get; set; }
        //public List<WorkStepGroupDto> WorkStepGroups { get; set; }
        public List<WorkStepDto> WorkSteps { get; set; }
        public List<WorkDirectionDto> WorkDirections { get; set; }

        /// <summary>
        /// post next step(s) by direction
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="nextStepDirection"></param>
        /// <returns></returns>
        public async Task<IEnumerable<JobDto>> PostStep(IJobRepository jobRepository, WorkStepDto currentStep, WorkFlowDirectionDto nextStepDirection)
        {
            ExpressionProvider expressionProvider;
            if (currentStep == null)
            {
                expressionProvider = new ExpressionProvider(new ArgumentsDto(Arguments.PublicVariables));
            }
            else
            {
                expressionProvider = new ExpressionProvider(new ArgumentsDto(currentStep.Arguments.PublicVariables));
            }
            var loopBy = await expressionProvider.Format(nextStepDirection?.LoopByExp);
            WorkFlowStepDto nextStepCfg;
            if (nextStepDirection == null)
            {
                nextStepCfg = WorkFlow.Steps.FirstOrDefault(x => x.IsBegin);
            }
            else
            {
                nextStepCfg = WorkFlow.Steps.FirstOrDefault(x => x.Code == nextStepDirection.NextStepCode);
            }
            List<WorkStepDto> nextSteps = new List<WorkStepDto>();
            if (loopBy?.IsArray == true)
            {
                var arr = loopBy.GetArray();
                foreach (var loopItem in arr)
                {
                    var step = new WorkStepDto
                    {
                        WorkStepId = Guid.NewGuid().ToString(),
                        StepCode = nextStepCfg.Code,
                        WorkFlowStep = nextStepCfg,
                        Arguments = expressionProvider.Arguments.Copy(),
                        Group = expressionProvider.Format(nextStepCfg.GroupExp).Result.ResultJson,
                        MatchQty = expressionProvider.Format(nextStepCfg.MatchQtyExp).Result.GetResult<int?>(),
                        Tags = nextStepCfg.TagExps?.Select(x => expressionProvider.Format(x).Result.ResultJson)?.ToList(),
                        TagData = expressionProvider.Format(nextStepCfg.TagDataExp).Result
                    };
                    step.Arguments.PutPublic(ReservedVariableNames.LOOP_ITEM, loopItem.ToJson());
                    nextSteps.Add(step);
                }
            }
            else
            {
                var step = new WorkStepDto
                {
                    WorkStepId = Guid.NewGuid().ToString(),
                    StepCode = nextStepCfg.Code,
                    WorkFlowStep = nextStepCfg,
                    Arguments = expressionProvider.Arguments.Copy(),
                    Group = expressionProvider.Format(nextStepCfg.GroupExp).Result.ResultJson,
                    MatchQty = expressionProvider.Format(nextStepCfg.MatchQtyExp).Result.GetResult<int?>(),
                    Tags = nextStepCfg.TagExps?.Select(x => expressionProvider.Format(x).Result.ResultJson)?.ToList(),
                    TagData = expressionProvider.Format(nextStepCfg.TagDataExp).Result
                };
                nextSteps.Add(step);
            }

            WorkSteps.AddRange(nextSteps);

            var nextJobs = nextSteps.Select(step => new StepPreActionJob
            {
                Id = Guid.NewGuid().ToString(),
                ActiveTime = DateTime.UtcNow,
                Actions = step.WorkFlowStep.Actions,
                IsManual = step.WorkFlowStep.IsManual,
                PublicVariables = step.Arguments.PublicVariables,
                Work = this,
                WorkId = WorkId
            });
            await jobRepository.InsertJobs(nextJobs);


            return nextJobs;
        }
    }

    public class WorkStatusDto
    {
        public string Status { get; set; }
        public DateTime? Time { get; set; }
    }

    public class WorkStepDto
    {
        public string WorkStepId { get; set; }
        public string StepCode { get; set; }
        [IgnoreDataMember]
        public WorkFlowStepDto WorkFlowStep { get; set; }
        [IgnoreDataMember]
        public WorkStepGroupDto WorkGroup { get; set; }
        public List<string> Tags { get; set; }
        public object TagData { get; set; }
        public string Group { get; set; }
        public int? MatchQty { get; set; }
        public ArgumentsDto Arguments { get; set; }
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public bool Fail { get; set; }

        public async Task<IEnumerable<JobDto>> UpdateStepResult(bool success, bool fail)
        {
            Completed = true;
            Success = success;
            Fail = fail;
            List<JobDto> nextJobs = new List<JobDto>();
            if (Success)
            {
                // post next step(s) by NextOn.Success
            }
            if (Fail)
            {
                // post next step(s) by NextOn.Fail
            }
            // update group result
            nextJobs.AddRange(await WorkGroup.UpdateGroupResult());

            return nextJobs;
        }

    }

    public class WorkStepGroupDto
    {
        public List<WorkStepDto> WorkSteps { get; set; }
        public bool AnySuccess { get; set; }
        public bool AnyFail { get; set; }
        public bool Completed { get; set; }
        public bool AllSuccess { get; set; }
        public bool AllFail { get; set; }

        public async Task<IEnumerable<JobDto>> UpdateGroupResult()
        {
            List<JobDto> nextJobs = new List<JobDto>();
            // post next step(s) by NextOn.Group
            return nextJobs;
        }
    }

    public class WorkDirectionDto
    {
        public string StepId { get; set; }
        public string NextStepId { get; set; }
    }

}
