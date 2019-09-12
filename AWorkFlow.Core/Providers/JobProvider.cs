using AWorkFlow.Core.Extensions;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using AWorkFlow.Core.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Providers
{
    public class JobProvider : IJobProvider
    {
        private readonly IJobRepository _jobRepository;
        private readonly IExecutorProvider _executorProvider;
        private readonly IWorkProvider _workProvider;
        private readonly IWorkFlowProvider _workFlowProvider;

        public JobProvider(IJobRepository jobRepository, IExecutorProvider executorProvider, IWorkProvider workProvider, IWorkFlowProvider workFlowProvider)
        {
            _jobRepository = jobRepository;
            _executorProvider = executorProvider;
            _workProvider = workProvider;
            _workFlowProvider = workFlowProvider;
        }

        /// <summary>
        /// execute a job, 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IEnumerable<JobDto>> Execute(JobDto job, string user)
        {
            string key = Guid.NewGuid().ToString();
            // lock job
            var lockJobResult = await LockJob(job.Id, key, TimeSpan.FromMinutes(1), user);
            if (!lockJobResult)
            {
                return new List<JobDto>();
            }
            // get job by key
            var jobToDo = await GetJob(key);
            // get action(s)
            int i = 0;
            bool success = true;
            var expressionProvider = GetJobVariables(job);
            List<ExecutionResultDto> executionResults = new List<ExecutionResultDto>();
            foreach (var action in jobToDo.Actions)
            {
                // execute Actions
                var executor = await _executorProvider.GetExecutor(action);
                var executeResult = await executor.Execute(expressionProvider, action);
                executionResults.Add(executeResult);
                expressionProvider.Arguments.PutPrivate($"result{i}", executeResult?.ExecuteResult?.ToJson());
                if (executeResult?.Success != true)
                {
                    success = false;
                    break;
                }
                i++;
            }
            // save action result
            await _jobRepository.SaveJobResult(job, executionResults, user);
            if (success)
            {
                await _jobRepository.FinishJob(job.Id);
            }
            await _jobRepository.UnLockJob(job.Id, key);
            // post next job(s)
            return await PostNextJobs(job.Id, user);
        }

        private IExpressionProvider GetJobVariables(JobDto job)
        {
            ArgumentsDto arguments = new ArgumentsDto(job?.PublicVariables, job?.PrivateVariables);
            return new ExpressionProvider(arguments);
        }

        private async Task<IEnumerable<JobDto>> PostNextJobs(string jobId, string user)
        {
            List<JobDto> nextJobs = new List<JobDto>();
            // get job
            var job = await _jobRepository.GetJob(jobId);
            var work = await _workProvider.GetWork(job.WorkId);
            var currentStep = work.WorkSteps.FirstOrDefault(x => x.WorkStepId == job.WorkStepId);
            var workflow = (await _workFlowProvider.SearchWorkFlow(string.Empty, work.WorkFlowCode, work.WorkFlowVersion))?.FirstOrDefault();
            var currentType = job.JobType;
            var expressionProvider = GetJobVariables(job);
            // if job is work.pre-action, go first-step
            if (currentType == JobTypes.WorkPreAction)
            {
                if (job.Success)
                {
                    // post first_step
                    var stepCfg = workflow?.Steps?.FirstOrDefault(x => x.IsBegin);
                    var firstStep = new WorkStepDto
                    {
                        WorkStepId = Guid.NewGuid().ToString(),
                        StepCode = stepCfg.Code,
                        Tags = stepCfg.TagExps?.Select(x => expressionProvider.Format(x).Result)?.ToList(),
                        Group = expressionProvider.Format(stepCfg.GroupExp).Result,
                        TagData = expressionProvider.Format(stepCfg.TagDataExp).Result,
                        MatchQty = string.IsNullOrEmpty(stepCfg.MatchQtyExp) ? null : expressionProvider.Format<int?>(stepCfg.MatchQtyExp).Result,
                        Arguments = new ArgumentsDto(expressionProvider.Arguments.PublicVariables)
                    };
                    await _workProvider.PostStep(firstStep, user);
                    if (stepCfg?.PreActions?.Any() == true)
                    {
                        var nextJob = new JobDto
                        {
                            Id = Guid.NewGuid().ToString(),
                            WorkId = work.WorkId,
                            WorkStepId = job.WorkStepId,
                            JobType = JobTypes.StepPreAction,
                            Actions = stepCfg.PreActions,
                            ActiveTime = DateTime.UtcNow,
                            PublicVariables = expressionProvider.Arguments.PublicVariables
                        };
                        await _jobRepository.InsertJob(nextJob);
                        nextJobs.Add(nextJob);
                    }
                    else
                    {
                        // post next
                        currentType = JobTypes.StepPreAction;
                        currentStep = firstStep;
                    }
                }
            }
            // if job is work.after-action, go close work
            if (currentType == JobTypes.WorkAfterAction)
            {
                if (job.Success)
                {
                    // close work
                    await _workProvider.FinishWork(work, job.Success, user);
                }
            }
            // if job is step.pre-action, go action or wait for manual
            if (currentType == JobTypes.StepPreAction)
            {
                if (job.Success)
                {
                    // post step.action job(auto/manual)
                    var stepCfg = workflow?.Steps?.FirstOrDefault(x => x.Code == currentStep.StepCode);
                    await _jobRepository.InsertJob(new JobDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        WorkId = work.WorkId,
                        WorkStepId = job.WorkStepId,
                        JobType = JobTypes.StepAction,
                        IsManual = stepCfg.IsManual,
                        MatchQty = currentStep.MatchQty,
                        Actions = stepCfg.Actions,
                        ActiveTime = DateTime.UtcNow,
                        PublicVariables = expressionProvider.Arguments.PublicVariables
                    });
                }
            }
            // if job is step.action, go trigger step result and after-action
            if (currentType == JobTypes.StepAction)
            {
                // trigger step result
                await _workProvider.FinishStep(currentStep, job.Success, user);
                // post next step
                await PostNextStep();
                //var nextStep = new WorkStepDto
                //{
                //    WorkStepId = Guid.NewGuid().ToString(),
                //    Arguments = new ArgumentsDto(expressionProvider.Arguments.PublicVariables),
                //    StepCode = stepCfg.Code,
                //    Tags = stepCfg.TagExps?.Select(x => expressionProvider.Format(x).Result)?.ToList(),
                //    Group = expressionProvider.Format(stepCfg.GroupExp).Result,
                //    TagData = expressionProvider.Format(stepCfg.TagDataExp).Result,
                //    MatchQty = string.IsNullOrEmpty(stepCfg.MatchQtyExp) ? null : expressionProvider.Format<int?>(stepCfg.MatchQtyExp).Result

                //};
                //await _workProvider.PostStep(nextStep, user);
                // post after-action job
            }
            // if job is step.after-action, go next step
            if (currentType == JobTypes.StepAfterAction)
            {
                // if last step, post work.after-action
            }
            return nextJobs;
        }

        private Task PostNextStep(
            WorkingCopyStepEntity currentWorkingStep, IEnumerable<WorkingCopyStepEntity> workingSteps,
            IEnumerable<WorkFlowConfigStepEntity> configSteps, IEnumerable<WorkFlowConfigFlowEntity> configFlows,
            DateTime now)
        {
            // if not finished, exit
            if (currentWorkingStep?.Finished != true)
            {
                return new List<Task<OperationResult>>();
            }

            // find next steps by flow route
            List<WorkingCopyStepEntity> nextWorkingSteps = new List<WorkingCopyStepEntity>();
            List<WorkingCopyFlowEntity> nextWorkingFlows = new List<WorkingCopyFlowEntity>();
            List<WorkingVariableEntity> variables = new List<WorkingVariableEntity>();
            var nextFlows = configFlows.Where(x => string.Equals(x.CurrentStepCode, currentWorkingStep.Code));

            IEnumerable<WorkingCopyStepEntity> groupSteps;
            if (string.IsNullOrEmpty(currentWorkingStep.Group))
            {
                groupSteps = workingSteps.Where(x => string.Equals(x.Id, currentWorkingStep.Id, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                groupSteps = workingSteps.Where(x => string.Equals(x.Group, currentWorkingStep.Group, StringComparison.CurrentCultureIgnoreCase) && !x.Cancelled);
            }
            var getGroupVariables = uow.Repo<WorkingVariableRepository>().Search(null, groupSteps?.Select(x => x.Id), null).Result;
            var groupArgs = WorkingVariableRepository.GetVariableDictionary(getGroupVariables.Data, true, true);
            ArgumentProvider groupArgumentProvider = new ArgumentProvider(groupArgs);
            var singleArgs = WorkingVariableRepository.GetVariableDictionary(getGroupVariables.Data?.Where(x => x.WorkingStepId == currentWorkingStep.Id), true, false);
            ArgumentProvider singleArgumentProvider = new ArgumentProvider(singleArgs);

            foreach (var flow in nextFlows)
            {
                // check NextOn condition
                var nextOn = Enum.Parse<FlowNextType>(flow.NextOn);
                var nextStep = configSteps.FirstOrDefault(x => string.Equals(x.Code, flow.NextStepCode));
                bool matchOne = false;
                bool matchGroup = false;
                ArgumentProvider argumentProvider = null;
                switch (nextOn)
                {
                    case FlowNextType.OnSuccess:
                        if (currentWorkingStep.Success)
                        {
                            matchOne = true;
                            argumentProvider = singleArgumentProvider;
                        }
                        break;
                    case FlowNextType.OnFail:
                        if (!currentWorkingStep.Success)
                        {
                            matchOne = true;
                            argumentProvider = singleArgumentProvider;
                        }
                        break;
                    case FlowNextType.OnGroupAllSuccess:
                        if (groupSteps.All(x => x.Success))
                        {
                            matchGroup = true;
                            argumentProvider = groupArgumentProvider;
                        }
                        break;
                    case FlowNextType.OnGroupAllFail:
                        if (groupSteps.All(x => !x.Success))
                        {
                            matchGroup = true;
                            argumentProvider = groupArgumentProvider;
                        }
                        break;
                    case FlowNextType.OnGroupAnySuccess:
                        if (groupSteps.Any(x => x.Success))
                        {
                            matchGroup = true;
                            argumentProvider = groupArgumentProvider;
                        }
                        break;
                    case FlowNextType.OnGroupAnyFail:
                        if (groupSteps.Any(x => !x.Success))
                        {
                            matchGroup = true;
                            argumentProvider = groupArgumentProvider;
                        }
                        break;
                }
                if (!matchOne && !matchGroup)
                {
                    continue;
                }
                // collect next step(s)
                List<WorkingCopyStepEntity> nextWorkingStep = new List<WorkingCopyStepEntity>();
                if (string.IsNullOrEmpty(nextStep.LoopBy))
                {
                    // 1 -> 1
                    nextWorkingStep.Add(new WorkingCopyStepEntity
                    {
                        WorkingCopyId = currentWorkingStep.WorkingCopyId,
                        PreviousWorkingCopyStepId = currentWorkingStep.Id,
                        ActiveNext = true,
                        ActiveTime = now,
                        Code = nextStep.Code,
                        Name = nextStep.Name,
                        Status = nextStep.Status,
                        StatusScope = nextStep.StatusScope,
                        StatusId = argumentProvider.Format(nextStep.StatusId),
                        Tags = nextStep.Tags,
                        Group = argumentProvider.Format(nextStep.Group),
                        CreatedAt = now,
                        CreatedBy = User,
                        UpdatedAt = now,
                        UpdatedBy = User
                    });
                }
                else
                {
                    // 1 -> N
                    var loopby = argumentProvider.Format(nextStep.LoopBy);
                    JArray array = JArray.Parse(loopby);
                    int index = 0;
                    foreach (var loopItem in array)
                    {
                        var arguments = new ArgumentProvider(new Dictionary<string, string>(argumentProvider.Arguments));
                        arguments.Put("loopItem", JsonConvert.SerializeObject(loopItem));
                        arguments.Put("index", index.ToString());
                        var s = new WorkingCopyStepEntity
                        {
                            WorkingCopyId = currentWorkingStep.WorkingCopyId,
                            PreviousWorkingCopyStepId = currentWorkingStep.Id,
                            ActiveNext = true,
                            ActiveTime = now,
                            Code = nextStep.Code,
                            Name = nextStep.Name,
                            Status = nextStep.Status,
                            StatusScope = nextStep.StatusScope,
                            StatusId = arguments.Format(nextStep.StatusId),
                            Tags = nextStep.Tags,
                            Group = arguments.Format(nextStep.Group),
                            CreatedAt = now,
                            CreatedBy = User,
                            UpdatedAt = now,
                            UpdatedBy = User
                        };
                        nextWorkingStep.Add(s);
                        variables.Add(new WorkingVariableEntity
                        {
                            WorkingCopyId = currentWorkingStep.WorkingCopyId,
                            WorkingStepId = s.Id,
                            Key = "loopItem",
                            Value = JsonConvert.SerializeObject(loopItem),
                            CreatedAt = now,
                            CreatedBy = User,
                            UpdatedAt = now,
                            UpdatedBy = User
                        });
                        variables.Add(new WorkingVariableEntity
                        {
                            WorkingCopyId = currentWorkingStep.WorkingCopyId,
                            WorkingStepId = s.Id,
                            Key = "index",
                            Value = index.ToString(),
                            CreatedAt = now,
                            CreatedBy = User,
                            UpdatedAt = now,
                            UpdatedBy = User
                        });
                        index++;
                    }

                }
                if (matchOne)
                {
                    // 1 -> 1
                    nextWorkingSteps.AddRange(nextWorkingStep);
                    nextWorkingFlows.AddRange(nextWorkingStep.Select(x => new WorkingCopyFlowEntity
                    {
                        WorkingCopyId = currentWorkingStep.WorkingCopyId,
                        CurrentStepId = currentWorkingStep.Id,
                        NextStepId = x.Id,
                        CreatedAt = now,
                        CreatedBy = User,
                        UpdatedAt = now,
                        UpdatedBy = User
                    }));
                }
                if (matchGroup)
                {
                    // N -> 1
                    nextWorkingSteps.AddRange(nextWorkingStep);
                    nextWorkingFlows.AddRange(groupSteps.SelectMany(groupStep =>
                    nextWorkingStep.Select(x => new WorkingCopyFlowEntity
                    {
                        WorkingCopyId = currentWorkingStep.WorkingCopyId,
                        CurrentStepId = groupStep.Id,
                        NextStepId = x.Id,
                        CreatedAt = now,
                        CreatedBy = User,
                        UpdatedAt = now,
                        UpdatedBy = User
                    })));
                }
                // store input variable
                variables.AddRange(nextWorkingStep.SelectMany(step =>
                   argumentProvider.Arguments?.Where(kvp => !variables.Any(v => string.Equals(v.Key, kvp.Key))).Select(kvp => new WorkingVariableEntity
                   {
                       WorkingCopyId = currentWorkingStep.WorkingCopyId,
                       WorkingStepId = step.Id,
                       Key = kvp.Key,
                       Value = kvp.Value,
                       CreatedAt = now,
                       CreatedBy = User,
                       UpdatedAt = now,
                       UpdatedBy = User
                   })));
            }

            List<Task<OperationResult>> tasks = new List<Task<OperationResult>>();
            if (nextWorkingSteps?.Any() == true)
            {
                tasks.Add(uow.Repo<WorkingCopyStepRepository>().BatchInsert(nextWorkingSteps));
            }
            if (nextWorkingFlows?.Any() == true)
            {
                tasks.Add(uow.Repo<WorkingCopyFlowRepository>().BatchInsert(nextWorkingFlows));
            }
            if (variables?.Any() == true)
            {
                tasks.Add(uow.Repo<WorkingVariableRepository>().BatchInsert(variables));
            }
            return tasks;
        }

        private Task TriggerNextSteps()
        { }

        private Task TriggerNextJobs()
        { }

        public Task<JobDto> GetJob(string key)
        {
            return _jobRepository.GetJobByKey(key);
        }

        public Task<IEnumerable<JobDto>> ListJobsToDo(int? maxCount)
        {
            return _jobRepository.ListJobsToDo(maxCount);
        }

        public Task<bool> LockJob(string id, string key, TimeSpan? lockTime, string user)
        {
            // lock a job
            return _jobRepository.LockJob(id, key, lockTime ?? TimeSpan.FromMinutes(1));
        }

        public Task<bool> PostJob(JobDto job, string user)
        {
            return _jobRepository.InsertJob(job);
        }

        public Task<bool> UnLockJob(string id, string key, string user)
        {
            // unlock a job
            return _jobRepository.UnLockJob(id, key);
        }
    }
}
