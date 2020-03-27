using AWorkFlow2.Models;
using AWorkFlow2.Models.Configs;
using AWorkFlow2.Models.Working;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers
{
    /// <summary>
    /// working logic, no repo related
    /// </summary>
    public class WorkFlowExecutionProvider
    {
        private readonly ActionExecutor _actionExecutor;

        /// <summary>
        /// operating user
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="actionExecutor"></param>
        public WorkFlowExecutionProvider(
            ActionExecutor actionExecutor)
        {
            _actionExecutor = actionExecutor;
        }

        /// <summary>
        /// start a batch of working copies
        /// </summary>
        /// <param name="workflows"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<WorkingCopy>>> StartNew(
            IEnumerable<WorkFlowConfig> workflows,
            Dictionary<string, object> input)

        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                DateTime now = DateTime.UtcNow;
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "input", JsonConvert.SerializeObject(input) }
                };
                ArgumentProvider argProvider = new ArgumentProvider(new WorkingArguments(args));

                var pickedWorkFlows = await PickupWorkFlows(workflows, argProvider);
                if (pickedWorkFlows?.Any() != true)
                {
                    return new OperationResult<IEnumerable<WorkingCopy>>
                    {
                        Success = false,
                        Code = Messages.WorkFlowNotExisted.Code,
                        Message = Messages.WorkFlowNotExisted.Message
                    };
                }
                var works = await Task.WhenAll(pickedWorkFlows.Select(async workflow =>
                {
                    var work = new WorkingCopy
                    {
                        WorkFlowCode = workflow.Code,
                        WorkFlowVersion = workflow.Version,
                        BeginTime = now,
                    };
                    work.Arguments = argProvider.WorkingArguments.Copy();
                    var results = await PostBeginSteps(now, workflow, new ArgumentProvider(work.Arguments));
                    work.Steps = results.Data.ToList();
                    work.Flows = new List<WorkingCopyFlow>();
                    work.Groups = new List<WorkingCopyGroup>();
                    return work;
                }));

                return new OperationResult<IEnumerable<WorkingCopy>>
                {
                    Success = true,
                    Data = works
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<IEnumerable<WorkingCopy>>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// cancel a work
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        public async Task<OperationResult<WorkingCopy>> Cancel(
            WorkingCopy work, WorkFlowConfig workflow)
        {
            var correlationId = work?.Id;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                DateTime now = DateTime.UtcNow;
                if (work == null)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyNotExisted.Code,
                        Message = Messages.WorkingCopyNotExisted.Message
                    };
                }
                if (work.IsFinished || work.IsCancelled || work.EndTime.HasValue)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Data = work,
                        Code = Messages.WorkingCopyAlreadyFinished.Code,
                        Message = Messages.WorkingCopyAlreadyFinished.Message
                    };
                }
                // cancel work
                work.EndTime = now;
                work.IsCancelled = true;
                work.UpdatedAt = now;
                work.UpdatedBy = User;

                // cancel all working steps
                CancelAllRunningSteps(work.Steps, now);

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// hold a work
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        public async Task<OperationResult<WorkingCopy>> Pause(
            WorkingCopy work, WorkFlowConfig workflow)
        {
            var correlationId = work?.Id;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                DateTime now = DateTime.UtcNow;
                if (work == null)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyNotExisted.Code,
                        Message = Messages.WorkingCopyNotExisted.Message
                    };
                }
                if (work.IsFinished || work.IsCancelled || work.EndTime.HasValue)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Data = work,
                        Code = Messages.WorkingCopyAlreadyFinished.Code,
                        Message = Messages.WorkingCopyAlreadyFinished.Message
                    };
                }
                if (work.OnHold)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = true,
                        Data = work
                    };
                }
                // hold working copy
                work.OnHold = true;
                work.HoldTime = now;
                work.ReleaseTime = null;
                work.UpdatedAt = now;
                work.UpdatedBy = User;

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// resume a holding work
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        public async Task<OperationResult<WorkingCopy>> Resume(
            WorkingCopy work, WorkFlowConfig workflow)
        {
            var correlationId = work?.Id;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                DateTime now = DateTime.UtcNow;
                if (work == null)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyNotExisted.Code,
                        Message = Messages.WorkingCopyNotExisted.Message
                    };
                }
                if (work.IsFinished || work.IsCancelled || work.EndTime.HasValue)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Data = work,
                        Code = Messages.WorkingCopyAlreadyFinished.Code,
                        Message = Messages.WorkingCopyAlreadyFinished.Message
                    };
                }
                if (!work.OnHold)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = true,
                        Data = work
                    };
                }
                // hold working copy
                work.OnHold = false;
                work.ReleaseTime = now;
                work.UpdatedAt = now;
                work.UpdatedBy = User;

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        public async Task<OperationResult<WorkingCopy>> Restart(
            WorkingCopy work, WorkFlowConfig workflow)
        {
            var correlationId = work?.Id;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                DateTime now = DateTime.UtcNow;
                if (work == null)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyNotExisted.Code,
                        Message = Messages.WorkingCopyNotExisted.Message
                    };
                }
                CancelAllRunningSteps(work.Steps, now);
                // init work
                work.EndTime = null;
                work.IsCancelled = false;
                work.IsFinished = false;
                work.OnHold = false;
                work.HoldTime = null;
                work.ReleaseTime = null;
                work.UpdatedAt = now;
                work.UpdatedBy = User;

                // post first step
                var beginSteps = work.Steps.Where(x => work.Flows?.Any(flow => flow?.ToStep?.Contains(x) == true) != true).ToList();
                work.Steps.AddRange(beginSteps.Select(x =>
                {
                    var step = CopyObject(x);
                    step.Arguments.ClearKey("output");
                    step.ActiveTime = now;
                    step.Cancelled = false;
                    step.Finished = false;
                    step.FinishedTime = null;
                    step.PreActionFinished = false;
                    step.LastPreActionResults = null;
                    step.ActionFinished = false;
                    step.PostedNext = false;
                    step.LastActionResults = null;
                    step.Success = false;
                    step.UpdatedAt = now;
                    step.UpdatedBy = User;
                    return step;
                }));

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        public async Task<OperationResult<WorkingCopy>> Execute(
            WorkingCopy work, WorkFlowConfig workflow, string workingStepId = "")
        {
            var correlationId = work?.Id;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                // verify work
                if (work == null)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyNotExisted.Code,
                        Message = Messages.WorkingCopyNotExisted.Message,
                        Data = work
                    };
                }
                if (work.IsFinished)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = true,
                        Code = Messages.WorkingCopyAlreadyFinished.Code,
                        Message = Messages.WorkingCopyAlreadyFinished.Message,
                        Data = work
                    };
                }
                if (work.IsCancelled)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyAlreadyCancelled.Code,
                        Message = Messages.WorkingCopyAlreadyCancelled.Message,
                        Data = work
                    };
                }
                // loop for all steps
                while (true)
                {
                    work.NextExecuteTime = null;
                    // get steps to go
                    IEnumerable<WorkingCopyStep> stepsToGo;
                    if (!string.IsNullOrEmpty(workingStepId))
                    {
                        stepsToGo = work.Steps?.Where(x => !x.Finished && x.Id == workingStepId)?.ToList();
                    }
                    else
                    {
                        stepsToGo = work.Steps?.Where(x => !x.Finished)?.ToList();
                    }
                    if (stepsToGo?.Any() != true)
                    {
                        return new OperationResult<WorkingCopy>
                        {
                            Success = true,
                            Code = Messages.GotNothingToGo.Code,
                            Message = Messages.GotNothingToGo.Code,
                            Data = work
                        };
                    }

                    var newPosted = await ExecuteSteps(work, stepsToGo, workflow);

                    // if nothing new, set next run time
                    if (!newPosted && !work.IsFinished)
                    {
                        work.NextExecuteTime = DateTime.Now.AddMinutes(1);
                    }

                    // return if not going to run another round
                    if (!work.NextExecuteTime.HasValue || DateTime.Now >= work.NextExecuteTime)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        public async Task<OperationResult<WorkingCopyStepResult>> Success(
            WorkingCopy work, string workingStepId, int? qty, Dictionary<string, object> args)
        {
            var correlationId = workingStepId;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                DateTime now = DateTime.UtcNow;
                var res = AddManualExecutionResult(work, workingStepId, true, qty, args, now);
                return res;
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        public async Task<OperationResult<WorkingCopyStepResult>> Fail(
            WorkingCopy work, string workingStepId, int? qty, Dictionary<string, object> args)
        {
            var correlationId = workingStepId;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                DateTime now = DateTime.UtcNow;
                var res = AddManualExecutionResult(work, workingStepId, false, qty, args, now);
                return res;
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        public async Task<OperationResult<WorkingCopy>> Retry(
            WorkingCopy work, WorkFlowConfig workflow, string workingStepId)
        {
            var correlationId = workingStepId;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            try
            {
                DateTime now = DateTime.UtcNow;
                if (work == null)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyNotExisted.Code,
                        Message = Messages.WorkingCopyNotExisted.Message
                    };
                }
                if (work.EndTime != null)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyAlreadyFinished.Code,
                        Message = Messages.WorkingCopyAlreadyFinished.Message
                    };
                }
                var step = work.Steps.FirstOrDefault(x => x.Id == workingStepId);
                // cancel step
                CancelOneStep(step);
                // cancel all steps after if activeNext
                var cancelledSteps = CancelStepsAfter(work, step);

                // post this step again with activeNext flag
                var newStep = CopyObject(step);
                newStep.Id = Guid.NewGuid().ToString();
                newStep.Cancelled = false;
                newStep.Success = false;
                newStep.Finished = false;
                newStep.FinishedTime = null;
                newStep.PreActionFinished = false;
                newStep.ActionFinished = false;
                newStep.PostedNext = false;
                newStep.ActiveTime = DateTime.UtcNow;
                newStep.Arguments.ClearKey("output");
                newStep.UpdatedAt = DateTime.UtcNow;
                newStep.UpdatedBy = User;
                work.Steps.Add(newStep);
                // flow this step after the original step
                var flowLeadsToStep = work.Flows.Where(x => x.ToStep.Contains(step));
                flowLeadsToStep.Select(x =>
                {
                    x.ToStep.AddStep(newStep);
                    return x;
                }).ToList();
                // refresh groups this step belongs
                cancelledSteps.SelectMany(cancelledStep => work.Groups.Where(group => group.Steps.Contains(cancelledStep)))
                    .Select(group =>
                    {
                        group.PostedNext = false;
                        group.ReorganizeGroup(work, workflow);
                        return group;
                    }).ToList();

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        #region private
        private async Task<IEnumerable<WorkFlowConfig>> PickupWorkFlows(IEnumerable<WorkFlowConfig> workflows, ArgumentProvider argProvider)
        {
            // run workflow selector
            var configs = await Task.WhenAll(workflows.Select(async workflow =>
            {
                // if a workflow has no selector, just go
                if (workflow.Selector == null)
                {
                    return workflow;
                }
                // if a workflow has a selector, run selector and only the success goes
                var argumentProvider = new ArgumentProvider(argProvider.WorkingArguments.Copy());
                var selectorResult = await _actionExecutor.Execute(workflow.Selector, argumentProvider);
                if (selectorResult?.Success == true)
                {
                    return workflow;
                }
                return null;
            }));

            return configs.Where(x => x != null);
        }
        private IEnumerable<WorkingCopyStep> CancelAllRunningSteps(IEnumerable<WorkingCopyStep> steps, DateTime now)
        {
            foreach (var step in steps)
            {
                if (step.Finished || step.Cancelled || step.FinishedTime.HasValue)
                {
                    continue;
                }
                step.FinishedTime = now;
                step.Cancelled = true;
                step.UpdatedAt = now;
                step.UpdatedBy = User;
            }
            return steps;
        }
        private IEnumerable<WorkingCopyStep> CancelStepsAfter(WorkingCopy work, WorkingCopyStep step)
        {
            if (step == null)
            {
                return null;
            }
            var flows = work.Flows.Where(x => x.FromStep.Contains(step));
            var stepsToCancel = flows.SelectMany(x => x.ToStep.GetSteps()).ToList();
            return stepsToCancel.SelectMany(x =>
            {
                if (!x.Finished)
                {
                    CancelOneStep(x);
                }
                return CancelStepsAfter(work, x);
            }).ToList();

        }
        private WorkingCopyStep CancelOneStep(WorkingCopyStep step)
        {
            if (step == null)
            {
                return step;
            }
            step.Cancelled = true;
            step.Finished = true;
            step.FinishedTime = DateTime.UtcNow;
            return step;
        }
        private async Task<OperationResult<IEnumerable<WorkingCopyStep>>> PostBeginSteps(
            DateTime now, WorkFlowConfig workflow, ArgumentProvider argProvider)
        {
            var beginStepConfigs = workflow.Steps.Where(x => x.IsBegin);
            var beginSteps = beginStepConfigs.Select(step => new WorkingCopyStep
            {
                Code = step.Code,
                Name = step.Name,
                ActiveTime = now,
                Arguments = argProvider.WorkingArguments.Copy(),
                ByQty = step.ByQty,
                MatchQty = argProvider.Format(step.MatchQty).ToNullableInt(),
                Status = step.Status,
                StatusScope = step.StatusScope,
                StatusId = argProvider.Format(step.StatusId)
            });

            //// TODO: store flow if restart?
            //if (!string.IsNullOrEmpty(previousWrokingStepId))
            //{
            //    var flows = nextWorkingSteps.Select(step =>
            //    new WorkingCopyFlowEntity
            //    {
            //        CurrentStepId = previousWrokingStepId,
            //        NextStepId = step.Id,
            //        WorkingCopyId = step.WorkingCopyId,
            //        WorkFlowFlowId = "restart",
            //        CreatedAt = now,
            //        CreatedBy = User,
            //        UpdatedAt = now,
            //        UpdatedBy = User
            //    }
            //    );
            //    tasks.Add(uow.Repo<WorkingCopyFlowRepository>().BatchInsert(flows));
            //}

            return new OperationResult<IEnumerable<WorkingCopyStep>>
            {
                Success = true,
                Data = beginSteps
            };
        }
        private async Task<bool> ExecuteSteps(WorkingCopy work, IEnumerable<WorkingCopyStep> stepsToGo, WorkFlowConfig workflow)
        {
            // execute all step
            var results = await Task.WhenAll(stepsToGo.Select(x => ExecuteOneStep(x, workflow, DateTime.UtcNow)));

            // reorganize group after steps executed
            await ReorganizeGroups(work, workflow);

            // post next for all steps not posted
            bool newPosted = false;
            // find executionResults requires post next
            var executionsRequiresNext = work?.Steps
                ?.Where(x => x?.ActionResults != null)
                ?.SelectMany(x => x?.ActionResults?.Select(ar => (step: x, result: ar)))
                ?.Where(x => !x.result.PostedNext)
                ?.ToList();
            newPosted = await PostNextForExecutions(work, executionsRequiresNext, workflow) || newPosted;
            // find steps requires post next
            var stepsRequiresNext = work?.Steps
                ?.Where(x => x.ActionFinished && !x.PostedNext)
                ?.ToList();
            newPosted = await PostNextForSteps(work, stepsRequiresNext, workflow) || newPosted;
            // find groups requires post next
            var groupsRequiresNext = work?.Groups
                ?.Where(x => x.Fulfilled && !x.PostedNext)
                ?.ToList();
            newPosted = await PostNextForGroups(work, groupsRequiresNext, workflow) || newPosted;

            // process end steps to finish the work
            var endStepCodes = workflow?.Steps?.Where(x => x.IsEnd)?.Select(x => x.Code);
            var endStep = stepsToGo.FirstOrDefault(x => endStepCodes?.Contains(x.Code) == true && x.Finished);
            if (endStep != null && work.Steps.All(x => x.Finished))
            {
                work.EndTime = DateTime.UtcNow;
                work.IsFinished = true;
                work.Arguments = ProcessOutput(workflow.Output, endStep.Arguments, work.Arguments);
                return false;
            }

            return newPosted;
        }
        private async Task<OperationResult<WorkingCopyStep>> ExecuteOneStep(WorkingCopyStep step, WorkFlowConfig workflow, DateTime now)
        {
            if (step.Finished)
            {
                return new OperationResult<WorkingCopyStep>
                {
                    Success = true,
                    Code = Messages.WorkingStepAlreadyFinished.Code,
                    Message = Messages.WorkingStepAlreadyFinished.Message,
                    Data = step
                };
            }
            if (step.Cancelled)
            {
                return new OperationResult<WorkingCopyStep>
                {
                    Success = false,
                    Code = Messages.WorkingStepAlreadyCancelled.Code,
                    Message = Messages.WorkingStepAlreadyCancelled.Message,
                    Data = step
                };
            }

            var workflowStep = workflow.Steps?.FirstOrDefault(x => x.Code == step.Code);
            ArgumentProvider argProviderStep = new ArgumentProvider(step.Arguments);

            // process pre-action
            if (!step.PreActionFinished)
            {
                ArgumentProvider argProviderExecution = new ArgumentProvider(step.Arguments?.Copy());
                argProviderExecution.WorkingArguments.ActionType = ActionTypes.StepPreAction.ToString();
                argProviderExecution.PutPrivate("now", DateTime.UtcNow.ToString("yyyy/MM/ddThh:mm:ss.fffZ"));
                // check retry limit
                var checkLimitRes = CheckRetryLimit(
                    step.PreActionExecutedCount, workflowStep.RetryLimit,
                    argProviderExecution.WorkingArguments, step.LastPreActionResults?.Arguments,
                    now);
                if (checkLimitRes?.Success == false && checkLimitRes.Data != null)
                {
                    step.PreActionFinished = true;
                    step.Success = false;
                    step.Finished = true;
                    step.LastPreActionResults = checkLimitRes.Data;
                }
                else
                {
                    // execute
                    if (workflowStep.PreActions?.Any() == true)
                    {
                        var executionResult = await ExecuteActions(workflowStep.PreActions, argProviderExecution.WorkingArguments, now);
                        step.LastPreActionResults = executionResult.Data;
                        if (step.LastPreActionResults.Success)
                        {
                            step.PreActionFinished = true;
                        }
                    }
                    else
                    {
                        // no action equals all finished
                        step.PreActionFinished = true;
                    }
                }
            }
            // check if pre-action is succeeded
            if (!step.PreActionFinished)
            {
                return new OperationResult<WorkingCopyStep>
                {
                    Success = true,
                    Data = step
                };
            }
            if (workflowStep.Manual)
            {
                step.WaitManual = true;
                if (workflowStep.ByQty)
                {
                    var successQty = step?.ActionResults?.Where(x => x.Success)?.Sum(x => x.Qty);
                    var failedQty = step?.ActionResults?.Where(x => x.Failed)?.Sum(x => x.Qty);
                    step.ActionFinished = (successQty - failedQty >= step?.MatchQty);
                    step.Success = step.ActionFinished;
                }
                else
                {
                    step.ActionFinished = step?.ActionResults?.Any() == true;
                    step.Success = step?.ActionResults?.Any(x => x.Success) == true;
                }
            }
            // process action
            if (!step.ActionFinished && !step.WaitManual)
            {
                var argProviderExecution = new ArgumentProvider(argProviderStep.WorkingArguments?.Copy());
                argProviderExecution.WorkingArguments.ActionType = ActionTypes.StepAction.ToString();
                argProviderExecution.PutPrivate("now", DateTime.UtcNow.ToString("yyyy/MM/ddThh:mm:ss.fffZ"));
                // check retry limit
                var checkLimitRes = CheckRetryLimit(
                    step.ActionExecutedCount, workflowStep.RetryLimit,
                    argProviderExecution.WorkingArguments, step.LastActionResults?.Arguments,
                    now);
                if (checkLimitRes?.Success == false && checkLimitRes.Data != null)
                {
                    step.ActionFinished = true;
                    step.Success = false;
                    step.Finished = true;
                    step.LastActionResults = checkLimitRes.Data;
                }
                // execute
                if (workflowStep.Actions?.Any() == true)
                {
                    var executionResult = await ExecuteActions(workflowStep.Actions, argProviderExecution.WorkingArguments, now);
                    step.LastActionResults = executionResult.Data;
                    if (step.LastActionResults.Success == true)
                    {
                        step.ActionFinished = true;
                        step.Success = true;
                    }
                }
                else
                {
                    // no action equals all finished
                    step.ActionFinished = true;
                    step.Success = true;
                }
                if (step.ActionFinished)
                {
                    var sourceArguments = step.LastActionResults != null ? step.LastActionResults.Arguments : step.Arguments;
                    if (workflowStep.Output?.Any() == true)
                    {
                        ProcessOutput(workflowStep.Output, sourceArguments, step.Arguments);
                    }
                }
            }
            return new OperationResult<WorkingCopyStep>
            {
                Success = true,
                Data = step
            };
        }
        private async Task<OperationResult<WorkingCopyStepResult>> ExecuteActions(
            IEnumerable<WorkFlowActionSetting> actions, WorkingArguments arguments, DateTime now)
        {
            if (actions?.Any() != true)
            {
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = true,
                    Data = new WorkingCopyStepResult
                    {
                        Success = true,
                        Failed = false,
                        SubmitTime = now,
                        Arguments = arguments
                    }
                };
            }
            bool? isSuccess = true;
            var argProviderExecution = new ArgumentProvider(arguments);
            foreach (var action in actions)
            {
                var actionExecuteResult = await _actionExecutor.Execute(action, argProviderExecution);
                argProviderExecution.PutPrivate($"result{action.Sequence}", actionExecuteResult.Data);
                if (actionExecuteResult.Output?.Any() == true)
                {
                    foreach (var kvp in actionExecuteResult.Output)
                    {
                        argProviderExecution.PutPublic(kvp.Key, kvp.Value);
                    }
                }
                if (actionExecuteResult?.Success == true)
                {
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
            return new OperationResult<WorkingCopyStepResult>
            {
                Success = true,
                Data = new WorkingCopyStepResult
                {
                    Success = isSuccess == true,
                    Failed = isSuccess == false,
                    SubmitTime = now,
                    Arguments = arguments
                }
            };
        }
        private OperationResult<WorkingCopyStepResult> CheckRetryLimit(
            int executedCount, int retryLimit, WorkingArguments executionArgument, WorkingArguments lastExecutionArgument, DateTime now)
        {
            if (retryLimit > 0 && executedCount >= retryLimit)
            {
                var argProvider = new ArgumentProvider(executionArgument);
                argProvider.PutPublic("message", $"execution failed for reaching retry limit: {retryLimit}");
                if (lastExecutionArgument?.PrivateArguments != null)
                {
                    foreach (var kvp in lastExecutionArgument.PrivateArguments)
                    {
                        if (!argProvider.WorkingArguments.PrivateArguments.ContainsKey(kvp.Key))
                        {
                            argProvider.PutPrivate(kvp.Key, kvp.Value);
                        }
                    }
                }
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Data = new WorkingCopyStepResult
                    {
                        Success = false,
                        Failed = true,
                        SubmitTime = now,
                        Arguments = argProvider.WorkingArguments
                    }
                };
            }
            return new OperationResult<WorkingCopyStepResult>
            {
                Success = true
            };
        }
        private async Task<OperationResult<WorkingCopy>> ProcessAfterStep(WorkingCopy work, WorkFlowConfig workflow, DateTime now)
        {
            var executionsRequiresToProcess = work.Steps.SelectMany(x => x.ActionResults.Select(ar => new { step = x, result = ar }))
                .Where(x => !x.result.PostedNext);
            // post next by execution
            var executionResult = await Task.WhenAll(executionsRequiresToProcess.Select(
                x => PostNextByExecution(x.step, x.result, workflow, now)
                ));
            if (executionResult?.All(x => x.Success) != true)
            {
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Data = work
                };
            }
            var stepsRequiresToProcess = work.Steps.Where(x => x.ActionFinished && !x.Finished);
            // post next by step
            var stepResults = await Task.WhenAll(stepsRequiresToProcess.Select(
                x => PostNextByStep(x, workflow, now)
                ));
            // if the end step, finish the work
            var endStepCodes = workflow.Steps.Where(x => x.IsEnd).Select(x => x.Code);
            var endSteps = work.Steps.Where(x => endStepCodes.Contains(x.Code) && x.ActionFinished);
            if (endSteps?.Any() == true)
            {
                work.IsFinished = true;
                work.EndTime = now;
                work.UpdatedAt = now;
                work.UpdatedBy = User;
            }
            return new OperationResult<WorkingCopy>
            {
                Success = true,
                Data = work
            };
        }
        private async Task<OperationResult<IEnumerable<WorkingCopyStep>>> PostNextByExecution(
            WorkingCopyStep step, WorkingCopyStepResult result, WorkFlowConfig workflow, DateTime now)
        {
            var partialFlows = workflow.Flows.Where(x => x.CurrentStepCode == step.Code
            && (x.NextOn == FlowNextType.OnPartialSuccess
            || x.NextOn == FlowNextType.OnPartialFail));
            if (partialFlows?.Any() != true)
            {
                return new OperationResult<IEnumerable<WorkingCopyStep>>
                {
                    Success = true
                };
            }
            List<WorkingCopyStep> newSteps = new List<WorkingCopyStep>();
            foreach (var flow in partialFlows)
            {
                // check NextOn condition
                var nextStep = workflow.Steps.FirstOrDefault(x => x.Code == flow.NextStepCode);
                bool matchOne = false;
                switch (flow.NextOn)
                {
                    case FlowNextType.OnPartialSuccess:
                        if (result.Success)
                        {
                            matchOne = true;
                        }
                        break;
                    case FlowNextType.OnPartialFail:
                        if (result.Failed)
                        {
                            matchOne = true;
                        }
                        break;
                }
                if (!matchOne)
                {
                    continue;
                }

                if (flow.Selector != null)
                {
                    var argProvider = new ArgumentProvider(result.Arguments);
                    // if a workflow flow has a selector, run selector and only the success goes
                    var selectorResult = _actionExecutor.Execute(flow.Selector, argProvider).Result;
                    if (selectorResult?.Success != true)
                    {
                        continue;
                    }
                }

                // collect next step(s)
                newSteps.AddRange(PostNewStep(nextStep, result.Arguments, now));
            }
            return new OperationResult<IEnumerable<WorkingCopyStep>>
            {
                Success = true,
                Data = newSteps
            };
        }
        private async Task<OperationResult<IEnumerable<WorkingCopyStep>>> PostNextByStep(
            WorkingCopyStep step, WorkFlowConfig workflow, DateTime now)
        {
            var stepFlows = workflow.Flows.Where(x => x.CurrentStepCode == step.Code
            && x.NextOn != FlowNextType.OnPartialSuccess
            && x.NextOn != FlowNextType.OnPartialFail);
            if (stepFlows?.Any() != true)
            {
                return new OperationResult<IEnumerable<WorkingCopyStep>>
                {
                    Success = true
                };
            }
            List<WorkingCopyStep> newSteps = new List<WorkingCopyStep>();
            foreach (var flow in stepFlows)
            {
                // check NextOn condition
                var nextStep = workflow.Steps.FirstOrDefault(x => x.Code == flow.NextStepCode);
                bool matchOne = false;
                ArgumentProvider argumentProvider = null;
                switch (flow.NextOn)
                {
                    case FlowNextType.OnSuccess:
                        if (step.Success)
                        {
                            matchOne = true;
                        }
                        break;
                    case FlowNextType.OnFail:
                        if (!step.Success)
                        {
                            matchOne = true;
                        }
                        break;
                }
                if (!matchOne)
                {
                    continue;
                }
                argumentProvider = new ArgumentProvider(step.Arguments.Copy().ExpandOutputs());

                if (flow.Selector != null)
                {
                    // if a workflow flow has a selector, run selector and only the success goes
                    var selectorResult = _actionExecutor.Execute(flow.Selector, argumentProvider).Result;
                    if (selectorResult?.Success != true)
                    {
                        continue;
                    }
                }

                newSteps.AddRange(PostNewStep(nextStep, argumentProvider.WorkingArguments, now));
            }
            return new OperationResult<IEnumerable<WorkingCopyStep>>
            {
                Success = true,
                Data = newSteps

            };
        }
        private async Task<OperationResult<IEnumerable<WorkingCopyStep>>> PostNextByGroup(
            WorkingCopyGroup group, WorkFlowConfig workflow, DateTime now)
        {
            var groupFlow = workflow.Flows.FirstOrDefault(x => x.Id == group.FLowId
            && (x.NextOn == FlowNextType.OnGroupAllSuccess
            || x.NextOn == FlowNextType.OnGroupAllFail
            || x.NextOn == FlowNextType.OnGroupAnySuccess
            || x.NextOn == FlowNextType.OnGroupAnyFail));
            if (groupFlow == null)
            {
                return new OperationResult<IEnumerable<WorkingCopyStep>>
                {
                    Success = false,
                    Code = Messages.WorkFlowNotExisted.Code,
                    Message = Messages.WorkFlowNotExisted.Message
                };
            }
            List<WorkingCopyStep> newSteps = new List<WorkingCopyStep>();
            var flow = groupFlow;
            // check NextOn condition
            var nextStep = workflow.Steps.FirstOrDefault(x => x.Code == flow.NextStepCode);
            bool matchGroup = false;
            ArgumentProvider argumentProvider = null;
            switch (flow.NextOn)
            {
                case FlowNextType.OnGroupAllSuccess:
                    if (group?.Steps?.All(x => x.ActionFinished && x.Success) == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            WorkingArguments.Merge(
                                group.Steps.Select(
                                    x => x.Arguments.Copy().ExpandOutputs()
                                ), true));
                    }
                    break;
                case FlowNextType.OnGroupAllFail:
                    if (group?.Steps?.All(x => x.ActionFinished && !x.Success) == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            WorkingArguments.Merge(
                                group.Steps.Select(
                                    x => x.Arguments.Copy().ExpandOutputs()
                                ), true));
                    }
                    break;
                case FlowNextType.OnGroupAnySuccess:
                    if (group?.Steps?.Any(x => x.ActionFinished && x.Success) == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            group.Steps.FirstOrDefault(x => x.ActionFinished && x.Success)
                            .Arguments.Copy()
                            .ExpandOutputs());
                    }
                    break;
                case FlowNextType.OnGroupAnyFail:
                    if (group?.Steps?.Any(x => x.ActionFinished && !x.Success) == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            group.Steps.FirstOrDefault(x => x.ActionFinished && !x.Success)
                            .Arguments.Copy()
                            .ExpandOutputs());
                    }
                    break;
            }
            if (!matchGroup)
            {
                return new OperationResult<IEnumerable<WorkingCopyStep>>
                {
                    Success = false
                };
            }

            if (flow.Selector != null)
            {
                // if a workflow flow has a selector, run selector and only the success goes
                var selectorResult = _actionExecutor.Execute(flow.Selector, argumentProvider).Result;
                if (selectorResult?.Success != true)
                {
                    return new OperationResult<IEnumerable<WorkingCopyStep>>
                    {
                        Success = true
                    };
                }
            }

            newSteps.AddRange(PostNewStep(nextStep, argumentProvider.WorkingArguments, now));
            return new OperationResult<IEnumerable<WorkingCopyStep>>
            {
                Success = true,
                Data = newSteps
            };

        }

        private IEnumerable<WorkingCopyStep> PostNewStep(
            WorkFlowConfigStep stepConfig, WorkingArguments argument, DateTime now)
        {
            List<WorkingCopyStep> newSteps = new List<WorkingCopyStep>();
            var argProvider = new ArgumentProvider(argument);
            if (string.IsNullOrEmpty(stepConfig.LoopBy))
            {
                // 1 -> 1
                newSteps.Add(new WorkingCopyStep
                {
                    Code = stepConfig.Code,
                    Name = stepConfig.Name,
                    Status = stepConfig.Status,
                    StatusScope = stepConfig.StatusScope,
                    StatusId = argProvider.Format(stepConfig.StatusId),
                    ByQty = stepConfig.ByQty,
                    MatchQty = argProvider.Format(stepConfig.MatchQty).ToNullableInt(),
                    ActiveTime = now,
                    Arguments = argument.Copy().ExpandOutputs()
                });
            }
            else
            {
                // 1 -> N
                var loopby = argProvider.Format(stepConfig.LoopBy);
                JArray array = JArray.Parse(loopby);
                int index = 0;
                foreach (var loopItem in array)
                {
                    var loopArgProvider = new ArgumentProvider(argument.Copy().ExpandOutputs());
                    if (loopItem.Type == JTokenType.String)
                    {
                        loopArgProvider.PutPublic("loopItem", loopItem.ToString());
                    }
                    else
                    {
                        loopArgProvider.PutPublic("loopItem", JsonConvert.SerializeObject(loopItem));
                    }
                    loopArgProvider.PutPublic("index", index.ToString());
                    newSteps.Add(new WorkingCopyStep
                    {
                        Code = stepConfig.Code,
                        Name = stepConfig.Name,
                        Status = stepConfig.Status,
                        StatusScope = stepConfig.StatusScope,
                        StatusId = loopArgProvider.Format(stepConfig.StatusId),
                        ByQty = stepConfig.ByQty,
                        MatchQty = loopArgProvider.Format(stepConfig.MatchQty).ToNullableInt(),
                        ActiveTime = now,
                        Arguments = loopArgProvider.WorkingArguments
                    });
                }

            }

            return newSteps;
        }

        private WorkingArguments ProcessOutput(Dictionary<string, string> outputConfig, WorkingArguments sourceArgument, WorkingArguments destinationArgument)
        {
            if (outputConfig?.Any() != true || sourceArgument == null || destinationArgument == null)
            {
                return destinationArgument;
            }
            Dictionary<string, string> res = new Dictionary<string, string>();
            ArgumentProvider destArgProvider = new ArgumentProvider(destinationArgument);
            ArgumentProvider sourceArgProvider = new ArgumentProvider(sourceArgument);
            try
            {
                foreach (var kvp in outputConfig)
                {
                    res[kvp.Key] = sourceArgProvider.Format(kvp.Value);
                }
            }
            catch { }


            destArgProvider.PutPublic("output", JsonConvert.SerializeObject(res));
            return destArgProvider.WorkingArguments;
        }
        private async Task<bool> PostNextForExecutions(WorkingCopy work, IEnumerable<(WorkingCopyStep step, WorkingCopyStepResult result)> executionsRequiresNext, WorkFlowConfig workflow)
        {
            if (executionsRequiresNext?.Any() == true)
            {
                ConcurrentBag<WorkingCopyStep> nextSteps = new ConcurrentBag<WorkingCopyStep>();
                ConcurrentBag<WorkingCopyFlow> nextFlows = new ConcurrentBag<WorkingCopyFlow>();
                await Task.WhenAll(executionsRequiresNext
                    .Select(async x =>
                    {
                        var postRes = await PostNextByExecution(x.step, x.result, workflow, DateTime.UtcNow);
                        if (postRes?.Success == true && postRes?.Data?.Any() == true)
                        {
                            x.result.PostedNext = true;
                            if (postRes?.Data?.Any() == true)
                            {
                                foreach (var step in postRes.Data)
                                {
                                    nextSteps.Add(step);
                                }
                                nextFlows.Add(new WorkingCopyFlow
                                {
                                    FromStep = new WorkingCopyFlowSeed(x.step),
                                    ExecutionResult = x.result,
                                    ToStep = new WorkingCopyFlowSeed(postRes?.Data)
                                });
                            }
                        }
                    }));

                // add next steps and flows and groups to work
                if (nextSteps?.Any() == true)
                {
                    if (work.Steps == null)
                    {
                        work.Steps = new List<WorkingCopyStep>();
                    }
                    work.Steps.AddRange(nextSteps);
                }
                if (nextFlows?.Any() == true)
                {
                    if (work.Flows == null)
                    {
                        work.Flows = new List<WorkingCopyFlow>();
                    }
                    work.Flows.AddRange(nextFlows);
                }
                return true;
            }
            return false;
        }
        private async Task<bool> PostNextForSteps(WorkingCopy work, IEnumerable<WorkingCopyStep> stepRequiresNext, WorkFlowConfig workflow)
        {
            if (stepRequiresNext?.Any() == true)
            {
                ConcurrentBag<WorkingCopyStep> nextSteps = new ConcurrentBag<WorkingCopyStep>();
                ConcurrentBag<WorkingCopyFlow> nextFlows = new ConcurrentBag<WorkingCopyFlow>();
                await Task.WhenAll(stepRequiresNext
                    .Select(async x =>
                    {
                        var postRes = await PostNextByStep(x, workflow, DateTime.UtcNow);
                        if (postRes?.Success == true)
                        {
                            var stepConfig = workflow.Steps.FirstOrDefault(config => config.Code == x.Code);
                            x.PostedNext = true;
                            x.Finished = true;
                            x.FinishedTime = DateTime.UtcNow;
                            if (postRes?.Data?.Any() == true)
                            {
                                foreach (var step in postRes.Data)
                                {
                                    nextSteps.Add(step);
                                }
                                nextFlows.Add(new WorkingCopyFlow
                                {
                                    FromStep = new WorkingCopyFlowSeed(x),
                                    ToStep = new WorkingCopyFlowSeed(postRes?.Data)
                                });
                            }
                        }
                    }));

                // add next steps and flows and groups to work
                if (nextSteps?.Any() == true)
                {
                    if (work.Steps == null)
                    {
                        work.Steps = new List<WorkingCopyStep>();
                    }
                    work.Steps.AddRange(nextSteps);
                }
                if (nextFlows?.Any() == true)
                {
                    if (work.Flows == null)
                    {
                        work.Flows = new List<WorkingCopyFlow>();
                    }
                    work.Flows.AddRange(nextFlows);
                }
                return true;
            }
            return false;
        }
        private async Task<bool> PostNextForGroups(WorkingCopy work, IEnumerable<WorkingCopyGroup> groupsRequiresNext, WorkFlowConfig workflow)
        {
            if (groupsRequiresNext?.Any() == true)
            {
                ConcurrentBag<WorkingCopyStep> nextSteps = new ConcurrentBag<WorkingCopyStep>();
                ConcurrentBag<WorkingCopyFlow> nextFlows = new ConcurrentBag<WorkingCopyFlow>();
                await Task.WhenAll(groupsRequiresNext
                    .Select(async x =>
                    {
                        var postRes = await PostNextByGroup(x, workflow, DateTime.UtcNow);
                        if (postRes?.Success == true && postRes?.Data?.Any() == true)
                        {
                            x.PostedNext = true;
                            if (postRes?.Data?.Any() == true)
                            {
                                foreach (var step in postRes.Data)
                                {
                                    nextSteps.Add(step);
                                }
                                nextFlows.Add(new WorkingCopyFlow
                                {
                                    FromStep = new WorkingCopyFlowSeed(x.Steps),
                                    ToStep = new WorkingCopyFlowSeed(postRes?.Data)
                                });
                            }
                        }
                    }));
                // add next steps and flows and groups to work
                if (nextSteps?.Any() == true)
                {
                    if (work.Steps == null)
                    {
                        work.Steps = new List<WorkingCopyStep>();
                    }
                    work.Steps.AddRange(nextSteps);
                }
                if (nextFlows?.Any() == true)
                {
                    if (work.Flows == null)
                    {
                        work.Flows = new List<WorkingCopyFlow>();
                    }
                    work.Flows.AddRange(nextFlows);
                }
                return true;
            }
            return false;
        }
        private OperationResult<WorkingCopyStepResult> AddManualExecutionResult(WorkingCopy work, string workingStepId, bool success, int? qty, Dictionary<string, object> args, DateTime now)
        {
            if (work == null)
            {
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Code = Messages.WorkingCopyNotExisted.Code,
                    Message = Messages.WorkingCopyNotExisted.Message
                };
            }
            var step = work.Steps.FirstOrDefault(x => x.Id == workingStepId);
            if (step == null)
            {
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Code = Messages.WorkingCopyStepNotExisted.Code,
                    Message = Messages.WorkingStepNotExisted.Message
                };
            }
            // store manual result on step, wait next run to process this result
            var argProvider = new ArgumentProvider(step.Arguments);
            argProvider.PutPublic("manualResult", JsonConvert.SerializeObject(args));

            var result = new WorkingCopyStepResult
            {
                SubmitTime = now,
                Success = success,
                Qty = qty,
                Arguments = argProvider.WorkingArguments
            };
            return new OperationResult<WorkingCopyStepResult>
            {
                Success = true,
                Data = result
            };
        }
        private async Task ReorganizeGroups(WorkingCopy work, WorkFlowConfig workflow)
        {
            // find workflow flow which requires group
            var flowsRequireGroup = workflow?.Flows?.Where(x => !string.IsNullOrEmpty(x?.GroupStartStepCode)).ToList();
            if (flowsRequireGroup?.Any() != true)
            {
                return;
            }

            foreach (var flow in flowsRequireGroup)
            {
                var existedGroup = work?.Groups?.FirstOrDefault(x => x.FLowId == flow.Id);
                if (existedGroup == null)
                {
                    if (work.Groups == null)
                    {
                        work.Groups = new List<WorkingCopyGroup>();
                    }
                    // build new group
                    work.Groups.AddRange(WorkingCopyGroup.BuildGroup(work, workflow, flow));
                }
                else
                {
                    if (!existedGroup.Finished)
                    {
                        // reorganize group
                        existedGroup.ReorganizeGroup(work, workflow);
                    }
                }
            }
        }

        private T CopyObject<T>(T o)
        {
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(o)
                );
        }
        #endregion
    }

}
