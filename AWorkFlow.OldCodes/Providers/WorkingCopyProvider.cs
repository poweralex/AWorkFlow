using AutoMapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.UnitOfWorkNS;
using Mcs.SF.WorkFlow.Api.Models;
using Mcs.SF.WorkFlow.Api.Models.Configs;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Mcs.SF.WorkFlow.Api.Models.Working;
using Mcs.SF.WorkFlow.Api.Repos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers
{
    /// <summary>
    /// provides operations for workingCopy
    /// </summary>
    public class WorkingCopyProvider
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWorkProvider _unitOfWorkProvider;

        /// <summary>
        /// operating user
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="unitOfWorkProvider"></param>
        public WorkingCopyProvider(IMapper mapper, UnitOfWorkProvider unitOfWorkProvider)
        {
            _mapper = mapper;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        /// <summary>
        /// list working steps require to execute
        /// </summary>
        /// <param name="workingCopyId"></param>
        /// <param name="finished"></param>
        /// <param name="count">max count of this batch</param>
        /// <param name="allocate">allocate to be exclusive or not</param>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<WorkingCopyStep>>> ListWorkingSteps(string workingCopyId, bool? finished, int count, bool allocate)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                List<string> workingCopyIds = null;
                if (!string.IsNullOrEmpty(workingCopyId))
                {
                    workingCopyIds = new List<string> { workingCopyId };
                }
                OperationResult<IEnumerable<WorkingCopyStepEntity>> getWorkingStepsResult;
                if (allocate)
                {
                    getWorkingStepsResult = await uow.Repo<WorkingCopyStepRepository>().AllocateSteps(workingCopyIds, count);
                }
                else
                {
                    getWorkingStepsResult = await uow.Repo<WorkingCopyStepRepository>().Search(workingCopyIds, finished, count: count);
                }

                return new OperationResult<IEnumerable<WorkingCopyStep>>(getWorkingStepsResult) { Data = _mapper.Map<List<WorkingCopyStep>>(getWorkingStepsResult?.Data) };
            }
        }

        internal async Task<OperationResult<WorkingCopyStatus>> GetWorkingCopy(string workingCopyId)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var getWorkingCopyResult = await uow.Repo<WorkingCopyRepository>().Search(workingCopyId);
                var getWorkingStepsResult = await uow.Repo<WorkingCopyStepRepository>().Search(new List<string> { workingCopyId });
                var getWorkingStepResultsResult = await uow.Repo<WorkingCopyStepResultRepository>().Search(workingCopyId, string.Empty);
                var getWorkingVariablesResult = await uow.Repo<WorkingVariableRepository>().Search(workingCopyId, string.Empty, string.Empty);
                var getWorkFlowConfigResult = await uow.Repo<WorkFlowConfigRepository>().Search(getWorkingCopyResult?.Data?.Select(x => x.WorkFlowId));
                var workingCopyOutput = getWorkingVariablesResult?.Data?.FirstOrDefault(x => string.IsNullOrEmpty(x.WorkingStepId) && "output".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase));

                var res = AssembleWorkingCopyStatus(getWorkingCopyResult?.Data, getWorkingStepsResult?.Data, getWorkingStepResultsResult?.Data, getWorkingVariablesResult?.Data, getWorkFlowConfigResult?.Data);

                return new OperationResult<WorkingCopyStatus>(getWorkingStepsResult) { Data = res?.FirstOrDefault() };
            }
        }

        internal async Task<OperationResult<IEnumerable<WorkingCopyStatus>>> SearchWorkingCopy(string category, string id, bool? finished)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var searchResult = await uow.Repo<WorkingCopyStepRepository>().Search(string.Empty, null, id);
                if (searchResult?.Data?.Any() != true)
                {
                    return new OperationResult<IEnumerable<WorkingCopyStatus>> { Success = true, Data = new List<WorkingCopyStatus>() };
                }
                var getWorkingCopyResult = await uow.Repo<WorkingCopyRepository>().Search(searchResult?.Data?.Select(x => x.WorkingCopyId), category, finished);
                if (getWorkingCopyResult?.Data?.Any() != true)
                {
                    return new OperationResult<IEnumerable<WorkingCopyStatus>> { Success = true, Data = new List<WorkingCopyStatus>() };
                }
                var workingCopyIds = getWorkingCopyResult?.Data?.Select(x => x.Id);
                var getWorkingStepsResult = await uow.Repo<WorkingCopyStepRepository>().Search(workingCopyIds);
                var getWorkingStepResultsResult = await uow.Repo<WorkingCopyStepResultRepository>().Search(workingCopyIds);
                var getWorkingVariablesResult = await uow.Repo<WorkingVariableRepository>().Search(workingCopyIds);
                var getWorkFlowConfigResult = await uow.Repo<WorkFlowConfigRepository>().Search(getWorkingCopyResult?.Data?.Select(x => x.WorkFlowId));
                var workingCopyOutput = getWorkingVariablesResult?.Data?.FirstOrDefault(x => string.IsNullOrEmpty(x.WorkingStepId) && "output".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase));

                var res = AssembleWorkingCopyStatus(getWorkingCopyResult?.Data, getWorkingStepsResult?.Data, getWorkingStepResultsResult?.Data, getWorkingVariablesResult?.Data, getWorkFlowConfigResult?.Data);

                return new OperationResult<IEnumerable<WorkingCopyStatus>>(getWorkingStepsResult) { Data = res };
            }
        }

        private IEnumerable<WorkingCopyStatus> AssembleWorkingCopyStatus(IEnumerable<WorkingCopyEntity> workingCopies, IEnumerable<WorkingCopyStepEntity> workingSteps, IEnumerable<WorkingCopyStepResultEntity> workingStepResults, IEnumerable<WorkingVariableEntity> workingVariables, IEnumerable<WorkFlowConfigEntity> workflows)
        {
            List<WorkingCopyStatus> result = new List<WorkingCopyStatus>();
            foreach (var workingCopy in workingCopies)
            {
                var currentSteps = workingSteps?.Where(x => x.WorkingCopyId == workingCopy.Id);
                var workflow = workflows.FirstOrDefault(x => x.Id == workingCopy.WorkFlowId);
                var workingCopyOutput = workingVariables?.FirstOrDefault(x => string.IsNullOrEmpty(x.WorkingStepId) && "output".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase));
                result.Add(new WorkingCopyStatus
                {
                    WorkingCopyId = workingCopy?.Id,
                    WorkFlowCode = workflow?.Code,
                    WorkFlowVersion = workflow?.Version,
                    BeginTime = workingCopy?.BeginTime,
                    EndTime = workingCopy?.EndTime,
                    IsFinished = workingCopy?.IsFinished ?? false,
                    IsCancelled = workingCopy?.IsCancelled ?? false,
                    Output = workingCopyOutput?.Value?.ToJsonObject(),
                    Steps = currentSteps?.Select(workingStep =>
                    {
                        var currentWorkingStepResults = workingStepResults?.Where(x => x.WorkingStepId == workingStep.Id);
                        var lastExecuteResultId = currentWorkingStepResults?.OrderByDescending(x => x.CreatedAt)?.FirstOrDefault()?.Id;
                        return new WorkingCopyStepStatus
                        {
                            WorkingStepId = workingStep.Id,
                            PreviousWorkingStepId = workingStep.PreviousWorkingCopyStepId,
                            StepCode = workingStep.Code,
                            BeginTime = workingStep.ActiveTime,
                            EndTime = workingStep.FinishedTime,
                            IsFinished = workingStep.Finished,
                            IsCancelled = workingStep.Cancelled,
                            IsSuccess = workingStep.Success,
                            Output = workingVariables?.FirstOrDefault(x => x.WorkingStepId == workingStep.Id && "output".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase))?.Value?.ToJsonObject(),
                            ExecuteCount = currentWorkingStepResults?.Count() ?? 0,
                            LastExecuteResults = string.IsNullOrEmpty(lastExecuteResultId) ?
                                new Dictionary<string, string>() :
                                workingVariables?.Where(x => x.WorkingStepResultId == lastExecuteResultId)
                                .ToDictionary(x => x.Key, x => x.Value)
                        };
                    }).ToList()
                });
            }
            return result;
        }

        /// <summary>
        /// start a new work
        /// </summary>
        /// <param name="category"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<WorkingCopyStatus>>> StartNew(string category, Dictionary<string, object> input)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                // get flow by category
                var configEntities = await uow.Repo<WorkFlowConfigRepository>().Search(false, category);

                // choose workflows (pick list of max version of each code)
                Dictionary<string, WorkFlowConfigEntity> availableWorkflows = new Dictionary<string, WorkFlowConfigEntity>();
                foreach (var entity in configEntities.Data)
                {
                    if (availableWorkflows.ContainsKey(entity.Code))
                    {
                        if (availableWorkflows[entity.Code].Version < entity.Version)
                        {
                            availableWorkflows[entity.Code] = entity;
                        }
                    }
                    else
                    {
                        availableWorkflows[entity.Code] = entity;
                    }
                }
                // run workflow selector
                List<WorkFlowConfigEntity> configs = new List<WorkFlowConfigEntity>();
                var selectors = await uow.Repo<WorkFlowConfigActionRepository>().Search(availableWorkflows.Values.Select(x => x.Id));
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "input", JsonConvert.SerializeObject(input) }
                };
                ArgumentProvider argProvider = new ArgumentProvider(args);
                foreach (var entity in availableWorkflows.Values)
                {
                    var selector = selectors.Data.FirstOrDefault(x => x.RefId == entity.Id && WorkFlowConfigProvider.WorkFlowSelector.Equals(x.Code, StringComparison.CurrentCultureIgnoreCase));
                    if (selector == null)
                    {
                        // if a workflow has no selector, run directly
                        configs.Add(entity);
                    }
                    else
                    {
                        // if a workflow has a selector, run selector and only the success goes
                        var selectorResult = await ActionExecutor.Execute(Enum.Parse<ActionType>(selector.Type), selector.ActionConfig, argProvider);
                        if (selectorResult?.Success == true)
                        {
                            configs.Add(entity);
                        }
                    }
                }
                var stepEntities = await uow.Repo<WorkFlowConfigStepRepository>().Search(configs.Select(x => x.Id));

                //if (config == null || stepEntities?.Success != true || flowEntities?.Success != true || actionEntities?.Success != true)
                if (configs?.Any() != true || stepEntities?.Success != true)
                {
                    return new OperationResult<IEnumerable<WorkingCopyStatus>> { Success = false, Code = Messages.WorkFlowNotExisted.Code, Message = Messages.WorkFlowNotExisted.Message };
                }
                // create working copy
                var workingCopys = configs.Select(config => new WorkingCopyEntity
                {
                    WorkFlowId = config.Id,
                    BeginTime = now,
                    CreatedAt = now,
                    CreatedBy = User,
                    UpdatedAt = now,
                    UpdatedBy = User
                }).ToList();
                var tasks = new List<Task<OperationResult>>
                {
                    uow.Repo<WorkingCopyRepository>().BatchInsert(workingCopys)
                };
                // post begin step
                tasks.AddRange(workingCopys.SelectMany(workingCopy =>
                    PostFirstStep(uow, stepEntities.Data.Where(x => x.WorkFlowId == workingCopy.WorkFlowId), workingCopy.Id, null, now, JsonConvert.SerializeObject(input))
                ));

                var results = await Task.WhenAll(tasks);
                if (results?.All(x => x.Success) == true)
                {
                    uow.Commit();
                    return new OperationResult<IEnumerable<WorkingCopyStatus>>
                    {
                        Success = true,
                        Data = workingCopys.Select(x =>
                            new WorkingCopyStatus
                            {
                                WorkingCopyId = x.Id,
                                WorkFlowCode = configs?.FirstOrDefault(wf => wf.Id == x.WorkFlowId)?.Code,
                                WorkFlowVersion = configs?.FirstOrDefault(wf => wf.Id == x.WorkFlowId)?.Version,
                                BeginTime = x.BeginTime,
                                EndTime = x.EndTime,
                                IsCancelled = x.IsCancelled,
                                IsFinished = x.IsFinished
                            }
                        )
                    };
                }
                else
                {
                    return new OperationResult<IEnumerable<WorkingCopyStatus>> { Success = false, Code = Messages.StartNewWorkFailed.Code, Message = Messages.StartNewWorkFailed.Message };
                }
            }
        }

        /// <summary>
        /// cancel a work
        /// </summary>
        /// <param name="workingId"></param>
        /// <returns></returns>
        public async Task<OperationResult> Stop(string workingId)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                var getWorkingCopyResult = await uow.Repo<WorkingCopyRepository>().Get(workingId);
                if (getWorkingCopyResult?.Success != true || getWorkingCopyResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyNotExisted.Code, Message = Messages.WorkingCopyNotExisted.Message };
                }
                var workingCopy = getWorkingCopyResult.Data;
                if (workingCopy.EndTime != null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyAlreadyFinished.Code, Message = Messages.WorkingCopyAlreadyFinished.Message };
                }
                // cancel all steps
                var getStepsResult = await uow.Repo<WorkingCopyStepRepository>().Search(workingId, false);
                if (getStepsResult?.Data?.Any() == true)
                {
                    var steps = getStepsResult.Data;
                    foreach (var step in steps)
                    {
                        step.Cancelled = true;
                        step.FinishedTime = now;
                        step.UpdatedAt = now;
                        step.UpdatedBy = User;
                    }
                    var updateStepsResult = await uow.Repo<WorkingCopyStepRepository>().BatchUpdate(steps);
                    if (updateStepsResult?.Success != true)
                    {
                        return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                    }
                }
                // finish working copy
                workingCopy.IsCancelled = true;
                workingCopy.EndTime = now;
                workingCopy.UpdatedAt = now;
                workingCopy.UpdatedBy = User;
                var result = await uow.Repo<WorkingCopyRepository>().Update(workingCopy);
                if (result?.Success == true)
                {
                    uow.Commit();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                }
            }
        }

        /// <summary>
        /// hold a work
        /// </summary>
        /// <param name="workingId"></param>
        /// <returns></returns>
        public async Task<OperationResult> Hold(string workingId)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                var getWorkingCopyResult = await uow.Repo<WorkingCopyRepository>().Get(workingId);
                if (getWorkingCopyResult?.Success != true || getWorkingCopyResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyNotExisted.Code, Message = Messages.StartNewWorkFailed.Message };
                }
                var workingCopy = getWorkingCopyResult.Data;
                // hold working copy
                workingCopy.OnHold = true;
                workingCopy.HoldTime = now;
                workingCopy.ReleaseTime = null;
                workingCopy.UpdatedAt = now;
                workingCopy.UpdatedBy = User;
                var result = await uow.Repo<WorkingCopyRepository>().Update(workingCopy);
                if (result?.Success == true)
                {
                    uow.Commit();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                }
            }
        }

        /// <summary>
        /// resume a holding work
        /// </summary>
        /// <param name="workingId"></param>
        /// <returns></returns>
        public async Task<OperationResult> Resume(string workingId)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                var getWorkingCopyResult = await uow.Repo<WorkingCopyRepository>().Get(workingId);
                if (getWorkingCopyResult?.Success != true || getWorkingCopyResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyNotExisted.Code, Message = Messages.StartNewWorkFailed.Message };
                }
                var workingCopy = getWorkingCopyResult.Data;
                // resume working copy
                workingCopy.OnHold = false;
                workingCopy.ReleaseTime = now;
                workingCopy.UpdatedAt = now;
                workingCopy.UpdatedBy = User;
                var result = await uow.Repo<WorkingCopyRepository>().Update(workingCopy);
                if (result?.Success == true)
                {
                    uow.Commit();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                }
            }
        }

        /// <summary>
        /// restart a holding work
        /// </summary>
        /// <param name="workingId"></param>
        /// <returns></returns>
        public async Task<OperationResult> Restart(string workingId)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                var getWorkingCopyResult = await uow.Repo<WorkingCopyRepository>().Get(workingId);
                if (getWorkingCopyResult?.Success != true || getWorkingCopyResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyNotExisted.Code, Message = Messages.StartNewWorkFailed.Message };
                }
                var workingCopy = getWorkingCopyResult.Data;
                if (workingCopy.EndTime != null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyAlreadyFinished.Code, Message = Messages.WorkingCopyAlreadyFinished.Message };
                }
                // cancel all steps
                var getStepsResult = await uow.Repo<WorkingCopyStepRepository>().Search(workingId, false);
                if (getStepsResult?.Data?.Any() == true)
                {
                    var steps = getStepsResult.Data;
                    foreach (var step in steps)
                    {
                        step.Cancelled = true;
                        step.FinishedTime = now;
                        step.UpdatedAt = now;
                        step.UpdatedBy = User;
                    }
                    var updateStepsResult = await uow.Repo<WorkingCopyStepRepository>().BatchUpdate(steps);
                    if (updateStepsResult?.Success != true)
                    {
                        return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                    }
                }
                var tasks = new List<Task<OperationResult>>();
                // resume working copy
                workingCopy.OnHold = false;
                workingCopy.ReleaseTime = now;
                workingCopy.UpdatedAt = now;
                workingCopy.UpdatedBy = User;
                tasks.Add(uow.Repo<WorkingCopyRepository>().Update(workingCopy));
                // post begin step
                var inputVariables = await uow.Repo<WorkingVariableRepository>().Search(workingCopy.Id, string.Empty, "input");
                var stepEntities = await uow.Repo<WorkFlowConfigStepRepository>().Search(workingCopy.Id);
                tasks.AddRange(PostFirstStep(uow, stepEntities.Data, workingCopy.Id, getStepsResult?.Data?.Max(x => x.Id), now, inputVariables?.Data?.FirstOrDefault()?.Value));

                var results = await Task.WhenAll(tasks);
                if (results?.All(x => x.Success) == true)
                {
                    uow.Commit();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                }
            }
        }

        /// <summary>
        /// execute a working step
        /// </summary>
        /// <param name="workingStepId"></param>
        /// <returns></returns>
        public async Task<OperationResult<WorkingCopyStep>> Execute(string workingStepId)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                // prepare data and check status
                var getWorkingStepResult = await uow.Repo<WorkingCopyStepRepository>().Get(workingStepId);
                var workingStep = getWorkingStepResult?.Data;
                if (workingStep == null)
                {
                    return new OperationResult<WorkingCopyStep>
                    {
                        Success = false,
                        Code = Messages.WorkingStepNotExisted.Code,
                        Message = Messages.WorkingStepNotExisted.Message
                    };
                }
                var getWorkingCopy = await uow.Repo<WorkingCopyRepository>().Get(workingStep.WorkingCopyId);
                var workingCopy = getWorkingCopy?.Data;
                if (workingCopy == null)
                {
                    return new OperationResult<WorkingCopyStep>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyNotExisted.Code,
                        Message = Messages.WorkingCopyNotExisted.Message
                    };
                }
                if (workingCopy.IsFinished)
                {
                    return new OperationResult<WorkingCopyStep>
                    {
                        Success = true,
                        Code = Messages.WorkingCopyAlreadyFinished.Code,
                        Message = Messages.WorkingCopyAlreadyFinished.Message
                    };
                }
                if (workingCopy.IsCancelled)
                {
                    return new OperationResult<WorkingCopyStep>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyAlreadyCancelled.Code,
                        Message = Messages.WorkingCopyAlreadyCancelled.Message
                    };
                }
                if (workingStep.Finished)
                {
                    ProcessAfterStep().Wait();
                    return new OperationResult<WorkingCopyStep>
                    {
                        Success = true,
                        Code = Messages.WorkingStepAlreadyFinished.Code,
                        Message = Messages.WorkingStepAlreadyFinished.Message,
                        Data = _mapper.Map<WorkingCopyStep>(workingStep)
                    };
                }
                if (workingStep.Cancelled)
                {
                    return new OperationResult<WorkingCopyStep>
                    {
                        Success = false,
                        Code = Messages.WorkingStepAlreadyCancelled.Code,
                        Message = Messages.WorkingStepAlreadyCancelled.Message,
                        Data = _mapper.Map<WorkingCopyStep>(workingStep)
                    };
                }
                var getWorkFlowConfigSteps = await uow.Repo<WorkFlowConfigStepRepository>().Search(getWorkingCopy.Data.WorkFlowId);
                var getWorkFlowConfigFlows = await uow.Repo<WorkFlowConfigFlowRepository>().Search(getWorkingCopy.Data.WorkFlowId);
                var currentConfigStep = getWorkFlowConfigSteps.Data.FirstOrDefault(x => string.Equals(x.Code, workingStep.Code));
                var getActions = await uow.Repo<WorkFlowConfigActionRepository>().Search(currentConfigStep.Id);
                var actions = getActions.Data.OrderBy(x => x.Sequence);
                var getVariables = await uow.Repo<WorkingVariableRepository>().Search(getWorkingCopy.Data.Id, workingStep.Id, string.Empty);
                Dictionary<string, string> args = WorkingVariableRepository.GetVariableDictionary(getVariables.Data, false, false);
                ArgumentProvider argProvider = new ArgumentProvider(args);
                // execute step
                bool? isSuccess = true;
                int i = 0;
                Dictionary<string, string> results = new Dictionary<string, string>();
                foreach (var action in actions)
                {
                    var actionExecuteResult = await ActionExecutor.Execute(Enum.Parse<ActionType>(action.Type), action.ActionConfig, argProvider);
                    results.Add($"result{i}", actionExecuteResult.Data);
                    if (actionExecuteResult?.Success == true)
                    {
                        if (actionExecuteResult.Output?.Any() == true)
                        {
                            foreach (var kvp in actionExecuteResult.Output)
                            {
                                args[kvp.Key] = kvp.Value;
                            }
                        }
                        argProvider.Put($"result{i}", actionExecuteResult.Data);
                        i++;
                        continue;
                    }
                    if (actionExecuteResult?.Fail == true)
                    {
                        isSuccess = false;
                        break;
                    }
                    isSuccess = null;
                    break;
                }
                // store result
                var res = await SaveStepResult(uow, isSuccess, now, workingStep, args, results);
                if (res?.Success == true)
                {
                    uow.Commit();
                }
                else
                {
                    return new OperationResult<WorkingCopyStep>
                    {
                        Success = false,
                        Code = Messages.UpdateWorkingCopyFailed.Code,
                        Message = Messages.UpdateWorkingCopyFailed.Message,
                        Data = _mapper.Map<WorkingCopyStep>(workingStep)
                    };
                }

                if (isSuccess != null)
                {
                    // process after step(s)
                    ProcessAfterStep().Wait();
                    if (isSuccess.Value)
                    {
                        return new OperationResult<WorkingCopyStep>
                        {
                            Success = isSuccess.Value,
                            Code = Messages.ExecuteActionSucceed.Code,
                            Message = Messages.ExecuteActionSucceed.Message,
                            Data = _mapper.Map<WorkingCopyStep>(workingStep)
                        };
                    }
                    else
                    {
                        return new OperationResult<WorkingCopyStep>
                        {
                            Success = isSuccess.Value,
                            Code = Messages.ExecuteActionFailed.Code,
                            Message = Messages.ExecuteActionFailed.Message,
                            Data = _mapper.Map<WorkingCopyStep>(workingStep)
                        };
                    }
                }

                // pending next run
                return new OperationResult<WorkingCopyStep>
                {
                    Success = true,
                    Code = Messages.PendingNextRun.Code,
                    Message = Messages.PendingNextRun.Message,
                    Data = _mapper.Map<WorkingCopyStep>(workingStep)
                };
            }
        }

        /// <summary>
        /// mark a working step as success
        /// </summary>
        /// <param name="workingStepId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<OperationResult> Success(string workingStepId, Dictionary<string, string> args)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                var getWorkingStepResult = await uow.Repo<WorkingCopyStepRepository>().Get(workingStepId);
                if (getWorkingStepResult?.Success != true || getWorkingStepResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyStepNotExisted.Code, Message = Messages.WorkingCopyStepNotExisted.Message };
                }
                var workingStep = getWorkingStepResult.Data;

                var res = await SaveStepResult(uow, true, now, workingStep, args, args);
                if (res?.Success == true)
                {
                    uow.Commit();
                    ProcessAfterStep().Wait();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                }
            }
        }

        /// <summary>
        /// save step result and update step if finished
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="success">is step success</param>
        /// <param name="now"></param>
        /// <param name="workingStep">working step</param>
        /// <param name="args">output base</param>
        /// <param name="results">execute results</param>
        /// <returns></returns>
        private async Task<OperationResult> SaveStepResult(IUnitOfWork uow, bool? success, DateTime now, WorkingCopyStepEntity workingStep, Dictionary<string, string> args, Dictionary<string, string> results)
        {
            List<Task<OperationResult>> tasks = new List<Task<OperationResult>>();
            // post step result
            var stepResult = new WorkingCopyStepResultEntity
            {
                WorkingStepId = workingStep.Id,
                Success = success ?? false,
                Failed = success.HasValue ? !success.Value : false,
                SubmitTime = now,
                CreatedAt = now,
                CreatedBy = User,
                UpdatedAt = now,
                UpdatedBy = User
            };
            tasks.Add(uow.Repo<WorkingCopyStepResultRepository>().Insert(stepResult));

            if (results?.Any() == true)
            {
                tasks.Add(uow.Repo<WorkingVariableRepository>().BatchInsert(
                    results.Select(resultKvp =>
                    new WorkingVariableEntity
                    {
                        WorkingCopyId = workingStep.WorkingCopyId,
                        WorkingStepId = workingStep.Id,
                        WorkingStepResultId = stepResult.Id,
                        Key = resultKvp.Key,
                        Value = resultKvp.Value,
                        CreatedAt = now,
                        CreatedBy = User,
                        UpdatedAt = now,
                        UpdatedBy = User
                    }))
                );
            }

            // update step status
            if (success != null)
            {
                workingStep.Success = success.Value;
                workingStep.Finished = true;
                workingStep.FinishedTime = now;
                workingStep.UpdatedAt = now;
                workingStep.UpdatedBy = User;
                tasks.Add(uow.Repo<WorkingCopyStepRepository>().Update(workingStep));

                var getWorkingCopy = await uow.Repo<WorkingCopyRepository>().Get(workingStep.WorkingCopyId);
                var getSteps = await uow.Repo<WorkFlowConfigStepRepository>().Search(getWorkingCopy.Data?.WorkFlowId);
                var currentStep = getSteps?.Data?.FirstOrDefault(x => string.Equals(x.Code, workingStep.Code));

                if (!string.IsNullOrEmpty(currentStep.Output))
                {
                    var stepOutput = GetWorkingCopyOutput(currentStep.Output, args);
                    // store step output
                    tasks.Add(uow.Repo<WorkingVariableRepository>().Insert(new WorkingVariableEntity
                    {
                        WorkingCopyId = workingStep.WorkingCopyId,
                        WorkingStepId = workingStep.Id,
                        Key = "output",
                        Value = JsonConvert.SerializeObject(stepOutput),
                        CreatedAt = now,
                        CreatedBy = User,
                        UpdatedAt = now,
                        UpdatedBy = User
                    }));

                    foreach (var kvp in stepOutput)
                    {
                        args[kvp.Key] = kvp.Value;
                    }
                }
            }

            var dbResults = await Task.WhenAll(tasks);
            if (dbResults?.All(x => x.Success) == true)
            {
                return new OperationResult { Success = true };
            }
            else
            {
                return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
            }

        }

        private async Task ProcessAfterStep()
        {
            List<WorkingCopyStepEntity> workingSteps = null;
            DateTime now = DateTime.UtcNow;
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var getWorkingSteps = await uow.Repo<WorkingCopyStepRepository>().SearchRequireAfterSteps();
                workingSteps = getWorkingSteps?.Data?.ToList();
            }
            if (workingSteps?.Any() != true)
            {
                return;
            }
            foreach (var workingStep in workingSteps)
            {
                using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
                {
                    uow.BeginTransaction();
                    var getWorkingFlow = await uow.Repo<WorkingCopyFlowRepository>().Search(workingStep.WorkingCopyId, workingStep.Id);
                    if (getWorkingFlow?.Data?.Any(x => x.CurrentStepId == workingStep.Id) == true)
                    {
                        // processed
                        continue;
                    }
                    var getWorkingCopy = await uow.Repo<WorkingCopyRepository>().Get(workingStep.WorkingCopyId);
                    var getWorkingSteps = await uow.Repo<WorkingCopyStepRepository>().Search(workingStep.WorkingCopyId, null);
                    var getWorkingVariables = await uow.Repo<WorkingVariableRepository>().Search(workingStep.WorkingCopyId, workingStep.Id, string.Empty);
                    var workingCopy = getWorkingCopy.Data;
                    var workflowId = workingCopy.WorkFlowId;
                    var getConfig = await uow.Repo<WorkFlowConfigRepository>().Get(workflowId);
                    var getSteps = await uow.Repo<WorkFlowConfigStepRepository>().Search(workflowId);
                    var getFlows = await uow.Repo<WorkFlowConfigFlowRepository>().Search(workflowId);
                    var stepConfigs = getSteps?.Data;
                    var currentStep = stepConfigs?.FirstOrDefault(x => string.Equals(x.Code, workingStep.Code));
                    Dictionary<string, string> args = WorkingVariableRepository.GetVariableDictionary(getWorkingVariables.Data, true, false);

                    List<Task<OperationResult>> tasks = new List<Task<OperationResult>>();
                    // after process(finish working copy or post next steps)
                    if (currentStep.IsEnd)
                    {
                        // finish working copy
                        workingCopy.IsFinished = true;
                        workingCopy.EndTime = now;
                        workingCopy.UpdatedAt = now;
                        workingCopy.UpdatedBy = User;
                        tasks.Add(uow.Repo<WorkingCopyRepository>().Update(workingCopy));

                        // store output
                        var workflowOutput = getConfig.Data.Output;
                        if (!string.IsNullOrEmpty(workflowOutput))
                        {
                            tasks.Add(uow.Repo<WorkingVariableRepository>().Insert(new WorkingVariableEntity
                            {
                                WorkingCopyId = workingStep.WorkingCopyId,
                                Key = "output",
                                Value = JsonConvert.SerializeObject(GetWorkingCopyOutput(workflowOutput, args)),
                                CreatedAt = now,
                                CreatedBy = User,
                                UpdatedAt = now,
                                UpdatedBy = User
                            }));
                        }
                    }
                    else
                    {
                        // post next step according to work flow
                        tasks.AddRange(PostNextStep(uow, workingStep, getWorkingSteps.Data, getSteps?.Data, getFlows?.Data, now));
                    }

                    var results = await Task.WhenAll(tasks);
                    if (results.All(x => x.Success))
                    {
                        uow.Commit();
                    }
                    else
                    {
                        uow.Rollback();
                    }
                }

            }
        }

        private Dictionary<string, string> GetWorkingCopyOutput(string outputSetting, Dictionary<string, string> args)
        {
            if (string.IsNullOrEmpty(outputSetting) || args?.Any() != true)
            {
                return null;
            }

            Dictionary<string, string> res = new Dictionary<string, string>();
            Dictionary<string, string> output = null;
            try
            {
                output = JsonConvert.DeserializeObject<Dictionary<string, string>>(outputSetting);
            }
            catch { }
            var argumentProvider = new ArgumentProvider(args);
            foreach (var kvp in output)
            {
                res[kvp.Key] = argumentProvider.Format(kvp.Value);
            }

            return res;
        }

        /// <summary>
        /// mark a working step as fail
        /// </summary>
        /// <param name="workingStepId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<OperationResult> Fail(string workingStepId, Dictionary<string, string> args)
        {
            // update step status to fail
            // post next step according to work flow
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                var getWorkingStepResult = await uow.Repo<WorkingCopyStepRepository>().Get(workingStepId);
                if (getWorkingStepResult?.Success != true || getWorkingStepResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyStepNotExisted.Code, Message = Messages.WorkingCopyStepNotExisted.Message };
                }
                var workingStep = getWorkingStepResult.Data;

                var res = await SaveStepResult(uow, false, now, workingStep, args, args);
                if (res?.Success == true)
                {
                    uow.Commit();
                    ProcessAfterStep().Wait();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                }

            }

        }

        /// <summary>
        /// retry a specific working step
        /// </summary>
        /// <param name="workingStepId"></param>
        /// <param name="activeNext"></param>
        /// <returns></returns>
        public async Task<OperationResult> Retry(string workingStepId, bool activeNext)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                DateTime now = DateTime.UtcNow;
                uow.BeginTransaction();
                var getWorkingStepResult = await uow.Repo<WorkingCopyStepRepository>().Get(workingStepId);
                if (getWorkingStepResult?.Success != true || getWorkingStepResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyStepNotExisted.Code, Message = Messages.WorkingCopyStepNotExisted.Message };
                }
                var workingStep = getWorkingStepResult.Data;
                var getWorkingCopyResult = await uow.Repo<WorkingCopyRepository>().Get(workingStep.WorkingCopyId);
                if (getWorkingCopyResult?.Success != true || getWorkingCopyResult?.Data == null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyNotExisted.Code, Message = Messages.StartNewWorkFailed.Message };
                }
                var workingCopy = getWorkingCopyResult.Data;
                if (workingCopy.EndTime != null)
                {
                    return new OperationResult { Success = false, Code = Messages.WorkingCopyAlreadyFinished.Code, Message = Messages.WorkingCopyAlreadyFinished.Message };
                }
                // cancel all steps after if activeNext
                var getStepsResult = await uow.Repo<WorkingCopyStepRepository>().Search(workingStep.WorkingCopyId, false);
                List<string> toCancelStepIds = new List<string>
                {
                    workingStepId
                };
                if (activeNext)
                {
                    var getFlowsResult = await uow.Repo<WorkingCopyFlowRepository>().Search(workingStep.WorkingCopyId, string.Empty);
                    while (true)
                    {
                        var flowsAfter = getFlowsResult?.Data?.Where(x => toCancelStepIds.Contains(x.CurrentStepId));
                        if (flowsAfter?.Any() != true)
                        {
                            break;
                        }
                        var newFlowsAfter = flowsAfter.Where(x => !toCancelStepIds.Contains(x.NextStepId));
                        if (newFlowsAfter?.Any() != true)
                        {
                            break;
                        }
                        toCancelStepIds.AddRange(newFlowsAfter.Select(x => x.NextStepId));
                    }
                }
                toCancelStepIds = toCancelStepIds.Distinct().ToList();

                if (toCancelStepIds?.Any() == true)
                {
                    var steps = getStepsResult.Data.Where(x => toCancelStepIds.Contains(x.Id));
                    foreach (var step in steps)
                    {
                        step.Cancelled = true;
                        step.FinishedTime = now;
                        step.UpdatedAt = now;
                        step.UpdatedBy = User;
                    }
                    var updateStepsResult = await uow.Repo<WorkingCopyStepRepository>().BatchUpdate(steps);
                    if (updateStepsResult?.Success != true)
                    {
                        return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                    }
                }
                // post this step again with activeNext flag
                // post begin step
                var inputVariables = await uow.Repo<WorkingVariableRepository>().Search(workingCopy.Id, workingStep.Id, string.Empty);
                Dictionary<string, string> args = WorkingVariableRepository.GetVariableDictionary(inputVariables.Data, false, false);
                var result = await uow.Repo<WorkingCopyStepRepository>().Insert(new WorkingCopyStepEntity
                {
                    WorkingCopyId = workingStep.WorkingCopyId,
                    PreviousWorkingCopyStepId = getStepsResult.Data.Max(x => x.Id),
                    Code = workingStep.Code,
                    Name = workingStep.Name,
                    Status = workingStep.Status,
                    StatusScope = workingStep.StatusScope,
                    StatusId = workingStep.StatusId,
                    Tags = workingStep.Tags,
                    Group = workingStep.Group,
                    ActiveNext = activeNext,
                    ActiveTime = now,
                    CreatedAt = now,
                    CreatedBy = User,
                    UpdatedAt = now,
                    UpdatedBy = User
                });
                if (result?.Success == true)
                {
                    uow.Commit();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false, Code = Messages.UpdateWorkingCopyFailed.Code, Message = Messages.UpdateWorkingCopyFailed.Message };
                }
            }
        }

        private List<Task<OperationResult>> PostFirstStep(IUnitOfWork uow, IEnumerable<WorkFlowConfigStepEntity> steps, string workingCopyId, string previousWrokingStepId, DateTime now, string input)
        {
            var firstSteps = steps.Where(x => x.IsBegin);
            ArgumentProvider argumentProvider = new ArgumentProvider(new Dictionary<string, string> { { "input", input } });
            var nextWorkingSteps = firstSteps.Select(firstStep => new WorkingCopyStepEntity
            {
                WorkingCopyId = workingCopyId,
                PreviousWorkingCopyStepId = previousWrokingStepId,
                ActiveNext = true,
                ActiveTime = now,
                Code = firstStep.Code,
                Name = firstStep.Name,
                Status = firstStep.Status,
                StatusScope = firstStep.StatusScope,
                StatusId = argumentProvider.Format(firstStep.StatusId),
                Tags = firstStep.Tags,
                CreatedAt = now,
                CreatedBy = User,
                UpdatedAt = now,
                UpdatedBy = User
            }).ToList();
            // store input variable
            var variables = nextWorkingSteps.Select(step => new WorkingVariableEntity
            {
                WorkingCopyId = workingCopyId,
                WorkingStepId = step.Id,
                Key = "input",
                Value = input,
                CreatedAt = now,
                CreatedBy = User,
                UpdatedAt = now,
                UpdatedBy = User
            }).ToList();

            List<Task<OperationResult>> tasks = new List<Task<OperationResult>>
            {
                uow.Repo<WorkingCopyStepRepository>().BatchInsert(nextWorkingSteps),
                uow.Repo<WorkingVariableRepository>().BatchInsert(variables)
            };
            return tasks;
        }

        private List<Task<OperationResult>> PostNextStep(IUnitOfWork uow,
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
    }
}
