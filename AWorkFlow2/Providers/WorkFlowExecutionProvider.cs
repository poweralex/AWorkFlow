using AWorkFlow2.Helps;
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
        /// <param name="duplicateReceipt"></param>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<WorkingCopy>>> StartNew(
            IEnumerable<WorkFlowConfig> workflows,
            Dictionary<string, object> input,
            string duplicateReceipt = "")

        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                DateTime now = DateTime.UtcNow;
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "input", JsonConvert.SerializeObject(input) }
                };
                ArgumentProvider argProvider = new ArgumentProvider(new WorkingArguments(args)
                {
                    UpdatedAt = now,
                    UpdatedBy = User
                });

                // pickup workflows
                var pickedWorkFlows = await PickupWorkFlows(workflows, argProvider);
                if (pickedWorkFlows?.Any() != true)
                {
                    return new OperationResult<IEnumerable<WorkingCopy>>
                    {
                        Success = true,
                        Code = Messages.WorkFlowNotExisted.Code,
                        Message = Messages.WorkFlowNotExisted.Message
                    };
                }
                // generate works
                var works = pickedWorkFlows.Select(workflow =>
                {
                    var work = new WorkingCopy
                    {
                        WorkFlowCategory = workflow.Category,
                        WorkFlowCode = workflow.Code,
                        WorkFlowVersion = workflow.Version,
                        BeginTime = now,
                        DuplicateReceipt = duplicateReceipt,
                        UpdatedAt = now,
                        UpdatedBy = User
                    };
                    work.Arguments = argProvider.WorkingArguments.Copy();
                    work.Arguments.PrivateArguments["workingCopyId"] = work.Id;
                    var results = PostBeginSteps(now, workflow, new ArgumentProvider(work.Arguments), false);
                    work.Steps.AddRange(results.Data);

                    if (string.IsNullOrEmpty(work.DuplicateReceipt))
                    {
                        work.DuplicateReceipt = work.Id;
                    }

                    SetNextExecuteTime(work);
                    return work;
                });

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
        /// <param name="workflow"></param>
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
                if (work.IsFinished || work.IsCancelled)
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
                work.IsFinished = true;
                work.UpdatedAt = now;
                work.UpdatedBy = User;

                // cancel all working steps
                CancelAllRunningSteps(work, work.Steps, now);

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Cancel@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex,
                    Data = work
                };
            }
            finally
            {
                SetNextExecuteTime(work);
            }
        }

        /// <summary>
        /// hold a work
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
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
                if (work.IsFinished || work.IsCancelled)
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
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Pause@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex,
                    Data = work
                };
            }
            finally
            {
                SetNextExecuteTime(work);
            }
        }

        /// <summary>
        /// resume a holding work
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
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
                if (work.IsFinished || work.IsCancelled)
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
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Resume@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex,
                    Data = work
                };
            }
            finally
            {
                SetNextExecuteTime(work);
            }
        }

        /// <summary>
        /// Restart a closed work
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
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
                CancelAllRunningSteps(work, work.Steps, now);
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
                var beginSteps = work.Steps.Where(x => x.IsBegin && !x.IsRetry).ToList();
                work.Steps.AddRange(beginSteps.Select(step =>
                {
                    var newStep = new WorkingCopyStep
                    {
                        Code = step.Code,
                        Name = step.Name,
                        Status = step.Status,
                        StatusScope = step.StatusScope,
                        StatusId = step.StatusId,
                        Tags = step.Tags?.ToList(),
                        IsBegin = step.IsBegin,
                        IsEnd = step.IsEnd,
                        WaitManual = step.WaitManual,
                        ByQty = step.ByQty,
                        MatchQty = step.MatchQty,
                        IsRetry = true,
                        ActiveTime = DateTime.UtcNow,
                        NextExecuteTime = now,
                        Arguments = step?.Arguments?.Copy(),
                        UpdatedAt = now,
                        UpdatedBy = User
                    };
                    newStep.Arguments.ClearKey("output");

                    return newStep;
                }));

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Restart@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex,
                    Data = work
                };
            }
            finally
            {
                SetNextExecuteTime(work);
            }
        }

        /// <summary>
        /// execute work to end(as far as possible)
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <param name="forceToExecuteNow">force execution right now ignore NextExecuteTime on step</param>
        /// <param name="workingStepId"></param>
        /// <returns></returns>
        public async Task<OperationResult<WorkingCopy>> Execute(
            WorkingCopy work, WorkFlowConfig workflow, bool forceToExecuteNow, string workingStepId = "")
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
                if (forceToExecuteNow)
                {
                    foreach (var step in work.Steps.Where(x => !x.Finished))
                    {
                        step.NextExecuteTime = null;
                    }
                }
                // loop for all steps
                while (true)
                {
                    var res = await ExecuteOneRoundImpl(work, workflow, workingStepId);
                    if (res?.Success != true)
                    {
                        // stop executing

                        break;
                    }
                    // run another round
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Execute@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex,
                    Data = work
                };
            }
            finally
            {
                SetNextExecuteTime(work);
            }
        }

        /// <summary>
        /// execute one round of steps
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <param name="forceToExecuteNow">ignore next execute time on steps</param>
        /// <param name="workingStepId">specific step id to run</param>
        /// <returns>success: can run another round</returns>
        public async Task<OperationResult<WorkingCopy>> ExecuteOneRound(
            WorkingCopy work, WorkFlowConfig workflow, bool forceToExecuteNow, string workingStepId = "")
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
                if (forceToExecuteNow)
                {
                    foreach (var step in work.Steps.Where(x => !x.Finished))
                    {
                        step.NextExecuteTime = null;
                    }
                }
                var res = await ExecuteOneRoundImpl(work, workflow, workingStepId);
                return new OperationResult<WorkingCopy>(res)
                {
                    Data = work
                };
            }
            catch (Exception ex)
            {
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"ExecuteOneRound@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Message = ex?.Message,
                    Exception = ex,
                    Data = work
                };
            }
            finally
            {
                SetNextExecuteTime(work);
            }
        }

        /// <summary>
        /// post success result on work
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workingStepId"></param>
        /// <param name="qty"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Success@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// post fail result on work
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workingStepId"></param>
        /// <param name="qty"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Fail@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// retry a step
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <param name="workingStepId"></param>
        /// <returns></returns>
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
                if (work.IsFinished)
                {
                    return new OperationResult<WorkingCopy>
                    {
                        Success = false,
                        Code = Messages.WorkingCopyAlreadyFinished.Code,
                        Message = Messages.WorkingCopyAlreadyFinished.Message
                    };
                }

                RefreshGroups(work, workflow);

                var step = work.Steps.FirstOrDefault(x => x.Id == workingStepId);
                // cancel step
                CancelOneStep(work, step, now, true);
                // cancel all steps after if activeNext
                var cancelledSteps = CancelStepsAfter(work, step);

                // post this step again with activeNext flag
                var newStep = new WorkingCopyStep
                {
                    Code = step.Code,
                    Name = step.Name,
                    Status = step.Status,
                    StatusScope = step.StatusScope,
                    StatusId = step.StatusId,
                    Tags = step.Tags?.ToList(),
                    IsBegin = step.IsBegin,
                    IsEnd = step.IsEnd,
                    WaitManual = step.WaitManual,
                    ByQty = step.ByQty,
                    MatchQty = step.MatchQty,
                    IsRetry = true,
                    ActiveTime = now,
                    NextExecuteTime = now,
                    Arguments = step?.Arguments?.Copy(),
                    UpdatedAt = now,
                    UpdatedBy = User
                };
                newStep.Arguments.ClearKey("output");

                work.Steps.Add(newStep);
                // flow this step after the original step
                var flowLeadsToStep = work.Flows.Where(x => x.ToStep.Contains(step));
                foreach (var x in flowLeadsToStep)
                {
                    x.ToStep.AddStep(newStep);
                    x.UpdatedAt = now;
                    x.UpdatedBy = User;
                };
                // refresh groups this step belongs
                foreach (var group in cancelledSteps.SelectMany(cancelledStep => work.Groups.Where(group => group.Steps.Contains(cancelledStep))))
                {
                    group.PostedNext = false;
                    group.RefreshGroup(work, workflow);
                    group.UpdatedAt = now;
                    group.UpdatedBy = User;
                }

                return new OperationResult<WorkingCopy>
                {
                    Success = true,
                    Data = work
                };
            }
            catch (Exception ex)
            {
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["ExceptionAt"] = $"Retry@{DateTime.UtcNow}";
                    work.Arguments.PrivateArguments["Exception"] = ex.ToString();
                }
                return new OperationResult<WorkingCopy>
                {
                    Success = false,
                    Exception = ex,
                    Data = work
                };
            }
            finally
            {
                SetNextExecuteTime(work);
            }
        }

        #region private
        /// <summary>
        /// pickup workflow(s) by arguments
        /// </summary>
        /// <param name="workflows"></param>
        /// <param name="argProvider"></param>
        /// <returns></returns>
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
                if (selectorResult?.Success == true && !string.Equals(bool.FalseString, selectorResult?.Data, StringComparison.CurrentCultureIgnoreCase))
                {
                    return workflow;
                }
                return null;
            }));

            return configs.Where(x => x != null);
        }
        /// <summary>
        /// cancel all running steps
        /// </summary>
        /// <param name="work"></param>
        /// <param name="steps"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private IEnumerable<WorkingCopyStep> CancelAllRunningSteps(WorkingCopy work, IEnumerable<WorkingCopyStep> steps, DateTime now)
        {
            foreach (var step in steps)
            {
                CancelOneStep(work, step, now, false);
            }
            return steps;
        }
        /// <summary>
        /// cancel steps after specific step
        /// </summary>
        /// <param name="work"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private IEnumerable<WorkingCopyStep> CancelStepsAfter(WorkingCopy work, WorkingCopyStep step)
        {
            if (step == null)
            {
                return null;
            }
            DateTime now = DateTime.UtcNow;
            var flows = work.Flows.Where(x => x.FromStep.Contains(step));
            var stepsToCancel = flows.SelectMany(x => x.ToStep?.Steps).ToList();
            return stepsToCancel.SelectMany(x =>
            {
                if (!x.Finished)
                {
                    CancelOneStep(work, x, now, false);
                }
                return CancelStepsAfter(work, x);
            }).ToList();

        }
        /// <summary>
        /// cancel one step(if not cancelled or finished)
        /// </summary>
        /// <param name="work"></param>
        /// <param name="step"></param>
        /// <param name="now"></param>
        /// <param name="force">force to cancel finished step</param>
        /// <returns></returns>
        private WorkingCopyStep CancelOneStep(WorkingCopy work, WorkingCopyStep step, DateTime now, bool force)
        {
            if (step == null)
            {
                return step;
            }
            // exit if already cancelled or (finished but not forced)
            if (step.Cancelled || (!force && step.Finished))
            {
                return step;
            }
            step.FinishedTime = now;
            step.Cancelled = true;
            step.Finished = true;
            step.NextExecuteTime = null;
            step.UpdatedAt = now;
            step.UpdatedBy = User;

            var effectGroups = work?.Groups?.Where(x => x.Steps.Any(groupStep => groupStep?.Id == step.Id));
            if (effectGroups?.Any() == true)
            {
                foreach (var group in effectGroups)
                {
                    group.Fulfilled = false;
                    group.PostedNext = false;
                    group.UpdatedAt = now;
                    group.UpdatedBy = User;
                }
            }
            return step;
        }
        /// <summary>
        /// post begin steps
        /// </summary>
        /// <param name="now"></param>
        /// <param name="workflow"></param>
        /// <param name="argProvider"></param>
        /// <param name="isRetry"></param>
        /// <returns></returns>
        private OperationResult<IEnumerable<WorkingCopyStep>> PostBeginSteps(
            DateTime now, WorkFlowConfig workflow, ArgumentProvider argProvider, bool isRetry)
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
                IsBegin = step.IsBegin,
                IsEnd = step.IsEnd,
                IsRetry = isRetry,
                Status = step.Status,
                StatusScope = step.StatusScope,
                StatusId = argProvider.Format(step.StatusId),
                UpdatedAt = now,
                UpdatedBy = User
            });

            return new OperationResult<IEnumerable<WorkingCopyStep>>
            {
                Success = true,
                Data = beginSteps
            };
        }
        /// <summary>
        /// run one round of steps
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <param name="workingStepId">specific step id to go</param>
        /// <returns></returns>
        private async Task<OperationResult> ExecuteOneRoundImpl(WorkingCopy work, WorkFlowConfig workflow, string workingStepId)
        {
            try
            {
                // get steps to go
                IEnumerable<WorkingCopyStep> stepsToGo;
                if (!string.IsNullOrEmpty(workingStepId))
                {
                    stepsToGo = work.Steps?.Where(x => !x.Finished && x.Id == workingStepId)?.ToList();
                }
                else
                {
                    stepsToGo = work.Steps?.Where(x => !x.Finished && (x.NextExecuteTime == null || x.NextExecuteTime < DateTime.UtcNow))?.ToList();
                }
                // if all finished and no end-step exists, but nothing to run, check groups
                if (stepsToGo?.Any() != true
                    && work.Steps?.All(x => x.Finished) == true
                    && work.Steps?.Any(x => x.IsEnd) != true)
                {
                    // reorganize group after steps executed
                    RefreshGroups(work, workflow);
                    if (work.Groups?.Any(x => !x.Finished) == true)
                    {
                        stepsToGo = work.Groups.FirstOrDefault(x => !x.Finished).Steps.Take(1);
                    }
                }
                if (stepsToGo?.Any() != true)
                {
                    return new OperationResult
                    {
                        Success = false
                    };
                }
                if (stepsToGo?.Count(x => !x.WaitManual) > 5)
                {
                    stepsToGo = stepsToGo.Where(x => !x.WaitManual).Take(5);
                }

                var newPosted = await ExecuteSteps(work, stepsToGo, workflow);

                return new OperationResult
                {
                    Success = newPosted
                };
            }
            catch (Exception ex)
            {
                if (work?.Arguments?.PrivateArguments != null)
                {
                    work.Arguments.PrivateArguments["Exception"] = ex.Message;
                }
                return new OperationResult
                {
                    Success = false,
                    Exception = ex
                };
            }
        }
        /// <summary>
        /// execute steps
        /// </summary>
        /// <param name="work"></param>
        /// <param name="stepsToGo"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private async Task<bool> ExecuteSteps(WorkingCopy work, IEnumerable<WorkingCopyStep> stepsToGo, WorkFlowConfig workflow)
        {
            if (work == null || stepsToGo?.Any() == null)
            {
                return false;
            }

            // execute all step
            var results = await Task.WhenAll(stepsToGo.Select(x => ExecuteOneStep(x, workflow)).ToList());

            // post next for all steps not posted
            bool newPosted = false;
            // find executionResults requires post next
            var executionsRequiresNext = work?.Steps
                ?.Where(x => x?.Cancelled != true && x?.ActionResults != null)
                ?.SelectMany(x => x?.ActionResults?.Select(ar => (step: x, result: ar)))
                ?.Where(x => !x.result.PostedNext)
                ?.ToList();
            newPosted = await PostNextForExecutions(work, executionsRequiresNext, workflow) || newPosted;
            // find steps requires post next
            var stepsRequiresNext = work?.Steps
                ?.Where(x => x?.Cancelled != true && x.ActionFinished && !x.PostedNext)
                ?.ToList();
            newPosted = await PostNextForSteps(work, stepsRequiresNext, workflow) || newPosted;

            // reorganize group after steps executed
            RefreshGroups(work, workflow);

            // find groups requires post next
            var groupsRequiresNext = work?.Groups
                ?.Where(x => !x.PostedNext)
                ?.ToList();
            newPosted = await PostNextForGroups(work, groupsRequiresNext, workflow) || newPosted;

            // process end steps to finish the work
            var endStepCodes = workflow?.Steps?.Where(x => x.IsEnd)?.Select(x => x.Code);
            var endStep = stepsToGo.FirstOrDefault(x => endStepCodes?.Contains(x.Code) == true && x.Finished);
            if (endStep != null)
            {
                work.EndTime = DateTime.UtcNow;
                work.IsFinished = true;
                work.Arguments = ProcessOutput(workflow.Output, endStep.Arguments, work.Arguments);
                return false;
            }

            return newPosted;
        }
        /// <summary>
        /// execute one step
        /// </summary>
        /// <param name="step"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private async Task<OperationResult<WorkingCopyStep>> ExecuteOneStep(WorkingCopyStep step, WorkFlowConfig workflow)
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

            step.NextExecuteTime = null;

            var workflowStep = workflow.Steps?.FirstOrDefault(x => x.Code == step.Code);
            ArgumentProvider argProviderStep = new ArgumentProvider(step.Arguments);
            argProviderStep.PutPrivate("now", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            // process pre-action
            if (!step.PreActionFinished)
            {
                ArgumentProvider argProviderExecution = new ArgumentProvider(step.Arguments?.Copy());
                argProviderExecution.WorkingArguments.ActionType = ActionTypes.StepPreAction.ToString();
                // check retry limit
                var checkLimitRes = CheckRetryLimit(
                    step.PreActionExecutedCount, workflowStep?.RetryLimit ?? 0,
                    argProviderExecution.WorkingArguments, step.LastPreActionResults?.Arguments,
                    DateTime.UtcNow);
                if (checkLimitRes?.Success == false && checkLimitRes.Data != null)
                {
                    step.PreActionFinished = true;
                    step.Success = false;
                    step.PreActionResults.Add(checkLimitRes.Data);
                }
                else
                {
                    // execute
                    if (workflowStep?.PreActions?.Any() == true)
                    {
                        var executionResult = await ExecuteActions(workflowStep.PreActions, argProviderExecution.WorkingArguments, DateTime.UtcNow);
                        step.PreActionResults.Add(executionResult.Data);
                        step.PreActionExecutedCount++;
                        if (step.LastPreActionResults.Success || step.LastPreActionResults.Failed)
                        {
                            var preActionOutputKeys = workflowStep.PreActions.Where(x => x.Output != null).SelectMany(action => action?.Output?.Keys).ToList();
                            foreach (var key in preActionOutputKeys)
                            {
                                if (executionResult?.Data?.Arguments?.PublicArguments?.ContainsKey(key) == true)
                                {
                                    argProviderStep.PutPublic(key, executionResult?.Data?.Arguments?.PublicArguments[key]);
                                }
                            }
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
            // fail the step if pre-action failed
            if (step?.LastPreActionResults?.Failed == true)
            {
                step.ActionFinished = true;
            }

            // check if pre-action is succeeded
            if (!step.PreActionFinished)
            {
                step.NextExecuteTime = DateTime.UtcNow.Add(workflowStep?.PreActionInterval ?? TimeSpan.FromMinutes(1));
                return new OperationResult<WorkingCopyStep>
                {
                    Success = true,
                    Data = step
                };
            }
            if (!step.ActionFinished && workflowStep?.Manual == true)
            {
                step.WaitManual = true;
                if (workflowStep?.ByQty == true)
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
                // check retry limit
                var checkLimitRes = CheckRetryLimit(
                    step.ActionExecutedCount, workflowStep?.RetryLimit ?? 0,
                    argProviderExecution.WorkingArguments, step.LastActionResults?.Arguments,
                    DateTime.UtcNow);
                if (checkLimitRes?.Success == false && checkLimitRes.Data != null)
                {
                    step.ActionFinished = true;
                    step.Success = false;
                    step.ActionResults.Add(checkLimitRes.Data);
                }
                else
                {
                    // execute
                    if (workflowStep?.Actions?.Any() == true)
                    {
                        var executionResult = await ExecuteActions(workflowStep.Actions, argProviderExecution.WorkingArguments, DateTime.UtcNow);
                        step.ActionResults.Add(executionResult.Data);
                        step.ActionExecutedCount++;
                        if (step?.LastActionResults?.Success == true || step?.LastActionResults?.Failed == true)
                        {
                            step.ActionFinished = true;
                            step.Success = step?.LastActionResults?.Success ?? false;
                        }
                    }
                    else
                    {
                        // no action equals all finished
                        step.ActionFinished = true;
                        step.Success = true;
                    }
                }
            }
            if (step.ActionFinished)
            {
                if (workflowStep?.Output?.Any() == true)
                {
                    var sourceArguments = step.LastActionResults != null ? step.LastActionResults.Arguments : step.Arguments;
                    // process output for step
                    ProcessOutput(workflowStep.Output, sourceArguments, step.Arguments);
                }
            }
            step.NextExecuteTime = DateTime.UtcNow.Add(workflowStep?.ActionInterval ?? TimeSpan.FromMinutes(1));
            return new OperationResult<WorkingCopyStep>
            {
                Success = true,
                Data = step
            };
        }
        /// <summary>
        /// execute actions
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="arguments"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private async Task<OperationResult<WorkingCopyStepResult>> ExecuteActions(
            IEnumerable<WorkFlowActionSetting> actions, WorkingArguments arguments, DateTime now)
        {
            try
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
                            Arguments = arguments?.Copy(),
                            UpdatedAt = now,
                            UpdatedBy = User
                        }
                    };
                }
                bool? isSuccess = true;
                var argProviderExecution = new ArgumentProvider(arguments?.Copy());
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
                        Arguments = argProviderExecution.WorkingArguments,
                        UpdatedAt = now,
                        UpdatedBy = User
                    }
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<WorkingCopyStepResult>
                {
                    Success = false,
                    Message = ex.Message,
                    Exception = ex
                };
            }
        }
        /// <summary>
        /// check whether retry limit is reached
        /// </summary>
        /// <param name="executedCount"></param>
        /// <param name="retryLimit"></param>
        /// <param name="executionArgument"></param>
        /// <param name="lastExecutionArgument"></param>
        /// <param name="now"></param>
        /// <returns></returns>
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
                        Arguments = argProvider.WorkingArguments,
                        UpdatedAt = now,
                        UpdatedBy = User
                    }
                };
            }
            return new OperationResult<WorkingCopyStepResult>
            {
                Success = true
            };
        }
        /// <summary>
        /// post next by execution result
        /// </summary>
        /// <param name="step"></param>
        /// <param name="result"></param>
        /// <param name="workflow"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private async Task<OperationResult<IEnumerable<WorkingCopyStep>>> PostNextByExecution(
            WorkingCopyStep step, WorkingCopyStepResult result, WorkFlowConfig workflow, DateTime now)
        {
            var partialFlows = workflow.Flows.Where(x => x.CurrentStepCode == step.Code
            && (x.NextOn == FlowNextType.OnPartialSuccess
            || x.NextOn == FlowNextType.OnPartialFail)).ToList();
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
                    if (selectorResult?.Success != true || string.Equals(bool.FalseString, selectorResult?.Data, StringComparison.CurrentCultureIgnoreCase))
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
        /// <summary>
        /// post next by step
        /// </summary>
        /// <param name="step"></param>
        /// <param name="workflow"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private async Task<OperationResult<IEnumerable<WorkingCopyStep>>> PostNextByStep(
            WorkingCopyStep step, WorkFlowConfig workflow, DateTime now)
        {
            var stepFlows = workflow.Flows.Where(x => x.CurrentStepCode == step.Code
            && x.NextOn != FlowNextType.OnPartialSuccess
            && x.NextOn != FlowNextType.OnPartialFail).ToList();
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
                    if (selectorResult?.Success != true || string.Equals(bool.FalseString, selectorResult?.Data, StringComparison.CurrentCultureIgnoreCase))
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
        /// <summary>
        /// post next by group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="workflow"></param>
        /// <param name="now"></param>
        /// <returns></returns>
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
                    if (group?.AllSuccess == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            WorkingArguments.Merge(
                                group.EndSteps.Select(
                                    x => x.Arguments.Copy().ExpandOutputs()
                                ), true));
                    }
                    break;
                case FlowNextType.OnGroupAllFail:
                    if (group?.AllFail == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            WorkingArguments.Merge(
                                group.EndSteps.Select(
                                    x => x.Arguments.Copy().ExpandOutputs()
                                ), true));
                    }
                    break;
                case FlowNextType.OnGroupAnySuccess:
                    if (group?.AnySuccess == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            WorkingArguments.Merge(group.SuccessSteps.Select(x => x.Arguments), false)
                            .ExpandOutputs());
                    }
                    break;
                case FlowNextType.OnGroupAnyFail:
                    if (group?.AnyFail == true)
                    {
                        matchGroup = true;
                        argumentProvider = new ArgumentProvider(
                            WorkingArguments.Merge(group.FailSteps.Select(x => x.Arguments), false)
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
                if (selectorResult?.Success != true || string.Equals(bool.FalseString, selectorResult?.Data, StringComparison.CurrentCultureIgnoreCase))
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
        /// <summary>
        /// post new step(s)
        /// </summary>
        /// <param name="stepConfig"></param>
        /// <param name="argument"></param>
        /// <param name="now"></param>
        /// <returns></returns>
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
                    Tags = stepConfig.Tags?.Select(x => argProvider.Format(x))?.ToList(),
                    ByQty = stepConfig.ByQty,
                    MatchQty = argProvider.Format(stepConfig.MatchQty).ToNullableInt(),
                    ActiveTime = now,
                    Arguments = argument.Copy().ExpandOutputs().FilterKeys(stepConfig.Input),
                    UpdatedAt = now,
                    UpdatedBy = User
                });
            }
            else
            {
                // 1 -> N
                var loopby = argProvider.Format(stepConfig.LoopBy);
                JArray array = JsonHelper.GetArray(loopby, stepConfig.LoopBy);
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
                        Tags = stepConfig.Tags?.Select(x => argProvider.Format(x))?.ToList(),
                        ByQty = stepConfig.ByQty,
                        MatchQty = loopArgProvider.Format(stepConfig.MatchQty).ToNullableInt(),
                        ActiveTime = now,
                        Arguments = loopArgProvider.WorkingArguments.FilterKeys(stepConfig.Input),
                        UpdatedAt = now,
                        UpdatedBy = User
                    });
                }

            }

            return newSteps;
        }
        /// <summary>
        /// process output by output config
        /// </summary>
        /// <param name="outputConfig"></param>
        /// <param name="sourceArgument"></param>
        /// <param name="destinationArgument"></param>
        /// <returns></returns>
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
            catch
            {
                // ignore json issues
            }


            destArgProvider.PutPublic("output", JsonConvert.SerializeObject(res));
            return destArgProvider.WorkingArguments;
        }
        /// <summary>
        /// post next for executions
        /// </summary>
        /// <param name="work"></param>
        /// <param name="executionsRequiresNext"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private async Task<bool> PostNextForExecutions(WorkingCopy work, IEnumerable<(WorkingCopyStep step, WorkingCopyStepResult result)> executionsRequiresNext, WorkFlowConfig workflow)
        {
            DateTime now = DateTime.UtcNow;
            if (executionsRequiresNext?.Any() == true)
            {
                ConcurrentBag<WorkingCopyStep> nextSteps = new ConcurrentBag<WorkingCopyStep>();
                ConcurrentBag<WorkingCopyFlow> nextFlows = new ConcurrentBag<WorkingCopyFlow>();
                var results = await Task.WhenAll(executionsRequiresNext
                    .Select(async x =>
                    {
                        try
                        {
                            var postRes = await PostNextByExecution(x.step, x.result, workflow, DateTime.UtcNow);
                            if (postRes?.Success == true)
                            {
                                x.result.PostedNext = true;
                                x.result.UpdatedAt = now;
                                x.result.UpdatedBy = User;
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
                                        ToStep = new WorkingCopyFlowSeed(postRes?.Data),
                                        UpdatedAt = now,
                                        UpdatedBy = User
                                    });
                                }
                            }
                            return new OperationResult<WorkingCopyStepResult> { Success = true, Data = x.result };
                        }
                        catch (Exception ex)
                        {
                            x.result.PostedNext = false;
                            if (x.step?.Arguments?.PrivateArguments != null)
                            {
                                x.step.Arguments.PrivateArguments["Exception"] = ex.Message;
                            }
                            return new OperationResult<WorkingCopyStepResult> { Success = false, Exception = ex, Data = x.result };
                        }
                    }));

                var failedStepResultIds = results?.Where(x => x?.Success != true)?.Select(x => x?.Data?.Id);
                var toAddNextFlows = nextFlows.Where(x => !failedStepResultIds.Contains(x.ExecutionResult.Id));
                var toAddNextSteps = toAddNextFlows.SelectMany(x => x.ToStep.Steps).Where(x => nextSteps.Contains(x));
                // add next steps and flows and groups to work
                if (toAddNextSteps?.Any() == true)
                {
                    work.Steps.AddRange(toAddNextSteps);
                }
                if (toAddNextFlows?.Any() == true)
                {
                    work.Flows.AddRange(toAddNextFlows);
                }
                return toAddNextSteps?.Any() == true;
            }
            return false;
        }
        /// <summary>
        /// post next for steps
        /// </summary>
        /// <param name="work"></param>
        /// <param name="stepRequiresNext"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private async Task<bool> PostNextForSteps(WorkingCopy work, IEnumerable<WorkingCopyStep> stepRequiresNext, WorkFlowConfig workflow)
        {
            DateTime now = DateTime.UtcNow;
            if (stepRequiresNext?.Any() == true)
            {
                ConcurrentBag<WorkingCopyStep> nextSteps = new ConcurrentBag<WorkingCopyStep>();
                ConcurrentBag<WorkingCopyFlow> nextFlows = new ConcurrentBag<WorkingCopyFlow>();
                var results = await Task.WhenAll(stepRequiresNext
                    .Select(async x =>
                    {
                        try
                        {
                            var postRes = await PostNextByStep(x, workflow, DateTime.UtcNow);
                            if (postRes?.Success == true)
                            {
                                var stepConfig = workflow.Steps.FirstOrDefault(config => config.Code == x.Code);
                                x.PostedNext = true;
                                x.Finished = true;
                                x.FinishedTime = DateTime.UtcNow;
                                x.NextExecuteTime = null;
                                if (postRes?.Data?.Any() == true)
                                {
                                    foreach (var step in postRes.Data)
                                    {
                                        nextSteps.Add(step);
                                    }
                                    nextFlows.Add(new WorkingCopyFlow
                                    {
                                        FromStep = new WorkingCopyFlowSeed(x),
                                        ToStep = new WorkingCopyFlowSeed(postRes?.Data),
                                        UpdatedAt = now,
                                        UpdatedBy = User
                                    });
                                }
                            }
                            return new OperationResult<WorkingCopyStep> { Success = true, Data = x };
                        }
                        catch (Exception ex)
                        {
                            x.PostedNext = false;
                            x.Finished = false;
                            x.FinishedTime = null;
                            x.NextExecuteTime = now.AddMinutes(1);
                            if (x?.Arguments?.PrivateArguments != null)
                            {
                                x.Arguments.PrivateArguments["Exception"] = ex.Message;
                            }

                            return new OperationResult<WorkingCopyStep> { Success = false, Exception = ex, Data = x };
                        }
                    }));

                var failedStepIds = results?.Where(x => x?.Success != true)?.Select(x => x?.Data?.Id);
                var toAddNextFlows = nextFlows.Where(x => !x.FromStep.Steps.Any(step => failedStepIds.Contains(step.Id)));
                var toAddNextSteps = toAddNextFlows.SelectMany(x => x.ToStep.Steps).Where(x => nextSteps.Contains(x));
                // add next steps and flows and groups to work
                if (toAddNextSteps?.Any() == true)
                {
                    work.Steps.AddRange(toAddNextSteps);
                }
                if (toAddNextFlows?.Any() == true)
                {
                    work.Flows.AddRange(toAddNextFlows);
                }
                return toAddNextSteps?.Any() == true;
            }
            return false;
        }
        /// <summary>
        /// post next for groups
        /// </summary>
        /// <param name="work"></param>
        /// <param name="groupsRequiresNext"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private async Task<bool> PostNextForGroups(WorkingCopy work, IEnumerable<WorkingCopyGroup> groupsRequiresNext, WorkFlowConfig workflow)
        {
            DateTime now = DateTime.UtcNow;
            if (groupsRequiresNext?.Any() == true)
            {
                ConcurrentBag<WorkingCopyStep> nextSteps = new ConcurrentBag<WorkingCopyStep>();
                ConcurrentBag<WorkingCopyFlow> nextFlows = new ConcurrentBag<WorkingCopyFlow>();
                var results = await Task.WhenAll(groupsRequiresNext
                    .Select(async x =>
                    {
                        try
                        {
                            var postRes = await PostNextByGroup(x, workflow, DateTime.UtcNow);
                            if (postRes?.Success == true)
                            {
                                x.PostedNext = true;
                                x.UpdatedAt = now;
                                x.UpdatedBy = User;
                                if (postRes?.Data?.Any() == true)
                                {
                                    foreach (var step in postRes.Data)
                                    {
                                        nextSteps.Add(step);
                                    }
                                    var fromSteps = x.EndSteps;
                                    if (x.EndSteps?.Any() != true)
                                    {
                                        fromSteps = x.BeginSteps;
                                    }
                                    nextFlows.Add(new WorkingCopyFlow
                                    {
                                        FromStep = new WorkingCopyFlowSeed(fromSteps),
                                        ToStep = new WorkingCopyFlowSeed(postRes?.Data),
                                        UpdatedAt = now,
                                        UpdatedBy = User
                                    });
                                }
                            }
                            return new OperationResult<WorkingCopyGroup> { Success = true, Data = x };
                        }
                        catch (Exception ex)
                        {
                            x.PostedNext = false;
                            if (work?.Arguments?.PrivateArguments != null)
                            {
                                work.Arguments.PrivateArguments["Exception"] = ex.Message;
                            }

                            return new OperationResult<WorkingCopyGroup> { Success = false, Exception = ex, Data = x };
                        }
                    }));
                var failedStepIds = results?.Where(x => x?.Success != true)?.SelectMany(x => x?.Data?.Steps.Select(step => step.Id));
                var toAddNextFlows = nextFlows.Where(x => !x.FromStep.Steps.Any(step => failedStepIds.Contains(step.Id)));
                var toAddNextSteps = toAddNextFlows.SelectMany(x => x.ToStep.Steps).Where(x => nextSteps.Contains(x));
                // add next steps and flows and groups to work
                if (toAddNextSteps?.Any() == true)
                {
                    work.Steps.AddRange(toAddNextSteps);
                }
                if (toAddNextFlows?.Any() == true)
                {
                    work.Flows.AddRange(toAddNextFlows);
                }
                return toAddNextSteps?.Any() == true;
            }
            return false;
        }
        /// <summary>
        /// add manual result for step
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workingStepId"></param>
        /// <param name="success"></param>
        /// <param name="qty"></param>
        /// <param name="args"></param>
        /// <param name="now"></param>
        /// <returns></returns>
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
            var argProvider = new ArgumentProvider(step.Arguments.Copy());
            argProvider.PutPublic("manualResult", JsonConvert.SerializeObject(args));
            argProvider.PutPublic("qty", $"{qty}");

            var result = new WorkingCopyStepResult
            {
                SubmitTime = now,
                Success = success,
                Qty = qty,
                Arguments = argProvider.WorkingArguments,
                UpdatedAt = now,
                UpdatedBy = User
            };
            step.ActionResults.Add(result);
            return new OperationResult<WorkingCopyStepResult>
            {
                Success = true,
                Data = result
            };
        }
        /// <summary>
        /// refresh groups
        /// </summary>
        /// <param name="work"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private WorkingCopy RefreshGroups(WorkingCopy work, WorkFlowConfig workflow)
        {
            // find workflow flow which requires group
            var flowsRequireGroup = workflow?.Flows?.Where(x => !string.IsNullOrEmpty(x?.GroupStartStepCode)).ToList();
            if (flowsRequireGroup?.Any() != true)
            {
                return work;
            }

            foreach (var flow in flowsRequireGroup)
            {
                var startStepIds = work.Steps
                    .Where(x => !x.Cancelled && string.Equals(x.Code, flow.GroupStartStepCode, StringComparison.CurrentCultureIgnoreCase))
                    .Select(x => x.Id);
                foreach (var startStepId in startStepIds)
                {
                    var existedGroup = work?.Groups?.FirstOrDefault(x => x.FLowId == flow.Id && x.Steps.Any(step => step?.Id == startStepId));
                    if (existedGroup == null)
                    {
                        // build new group
                        work?.Groups?.AddRange(WorkingCopyGroup.BuildGroup(work, workflow, flow, startStepId, User));
                    }
                    else
                    {
                        if (!existedGroup.Finished)
                        {
                            // reorganize group
                            existedGroup.RefreshGroup(work, workflow);
                        }
                    }
                }
            }
            return work;
        }

        /// <summary>
        /// set next execution time for work
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        private WorkingCopy SetNextExecuteTime(WorkingCopy work)
        {
            if (work == null)
            {
                return work;
            }
            if (work.IsCancelled || work.OnHold || work.IsFinished)
            {
                work.NextExecuteTime = null;
                return work;
            }
            work.NextExecuteTime = work.Steps?.Where(x => x.NextExecuteTime != null)?.Min(x => x.NextExecuteTime);
            if (work.NextExecuteTime == null || work.NextExecuteTime < DateTime.UtcNow)
            {
                work.NextExecuteTime = DateTime.UtcNow.AddMinutes(1);
            }

            work.IsStuck = work.Steps?.All(x => x.PostedNext) == true
                && !work.IsFinished;

            return work;
        }
        #endregion
    }

}
