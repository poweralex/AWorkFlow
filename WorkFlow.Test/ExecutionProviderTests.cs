using Autofac;
using AWorkFlow2.Models.Configs;
using AWorkFlow2.Providers;
using AWorkFlow2.Providers.ActionExcutor;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkFlow.Test
{
    class ExecutionProviderTests
    {
        private WorkFlowExecutionProvider _executionProvider;
        private List<WorkFlowConfig> _workflows;
        private dynamic _data;
        private Dictionary<string, object> _input;

        [SetUp]
        public void Setup()
        {
            var actionContainerBuilder = new ContainerBuilder();
            var processorContainerBuilder = new ContainerBuilder();

            actionContainerBuilder.RegisterType<WebApiActionExecutor>().Named<IActionExecutor>(ActionType.CallRestApi.ToString());
            actionContainerBuilder.RegisterType<VariableProcessActionExecutor>().Named<IActionExecutor>(ActionType.VariableProcess.ToString());

            processorContainerBuilder.RegisterType<SetValueProcessor>().Named<IVariableProcessor>(SetValueProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<MapListProcessor>().Named<IVariableProcessor>(MapListProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<ExpandProcessor>().Named<IVariableProcessor>(ExpandProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<CompareNumberProcessor>().Named<IVariableProcessor>(CompareNumberProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<CompareProcessor>().Named<IVariableProcessor>(CompareProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<GroupListProcessor>().Named<IVariableProcessor>(GroupListProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<AggregateListProcessor>().Named<IVariableProcessor>(AggregateListProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<PeekListProcessor>().Named<IVariableProcessor>(PeekListProcessor.Method.ToUpper());
            processorContainerBuilder.RegisterType<UnitTestActionProcessor>().Named<IVariableProcessor>(UnitTestActionProcessor.Method.ToUpper());
            var processorContainer = processorContainerBuilder.Build();
            actionContainerBuilder.RegisterInstance(processorContainer);
            var actionContainer = actionContainerBuilder.Build();
            ActionExecutor actionExecutor = new ActionExecutor(actionContainer);
            _executionProvider = new WorkFlowExecutionProvider(actionExecutor);
            _executionProvider.User = "unittest";

            _workflows = new List<WorkFlowConfig>();
            var workflow1 = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess
                    }
                },
                Selector = new WorkFlowActionSetting
                {
                    Type = ActionType.VariableProcess,
                    ActionConfig = new
                    {
                        Method = CompareProcessor.Method,
                        Comparer = "equal",
                        Arg1 = "{{input.orderType}}",
                        Arg2 = "A",
                        IgnoreCase = false
                    }
                }
            };
            _workflows.Add(workflow1);

            var workflow2 = JsonConvert.DeserializeObject<WorkFlowConfig>(
                JsonConvert.SerializeObject(workflow1)
                );
            workflow2.Code = "TestWorkFlow2";
            workflow2.Desc = "TestWorkFlow2";
            workflow2.Name = "TestWorkFlow2";
            workflow2.Selector = new WorkFlowActionSetting
            {
                Type = ActionType.VariableProcess,
                ActionConfig = new
                {
                    Method = CompareProcessor.Method,
                    Comparer = "equal",
                    Arg1 = "{{input.orderType}}",
                    Arg2 = "B",
                    IgnoreCase = false
                }
            };
            _workflows.Add(workflow2);

            _data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A"
            };
            _input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(_data)
            );

        }

        /// <summary>
        /// test target:
        /// 1. select one workflow and create new work
        /// 2. verify initial work with workflow
        /// 3. verify work argument(input)
        /// 4. verify 1 step(Begin) with argument(input)
        /// 5. verify 0 flow
        /// </summary>
        [Test]
        public void TestCreateNewWork_SelectOne()
        {
            var workRes = _executionProvider.StartNew(_workflows, _input, string.Empty).Result;
            Common.ValidateOperationResultWithData(workRes);
            var works = workRes.Data;
            /// 1. select one workflow and create new work
            Assert.AreEqual(1, works.Count());

            /// 2. verify initial work with workflow
            var work = works.First();
            var targetWorkflow = _workflows.FirstOrDefault(x => x.Code == "TestWorkFlow1");
            Assert.IsNotNull(work);
            Assert.IsTrue(!string.IsNullOrEmpty(work.Id));
            Assert.AreEqual(targetWorkflow.Category, work.WorkFlowCategory);
            Assert.AreEqual(targetWorkflow.Code, work.WorkFlowCode);
            Assert.AreEqual(targetWorkflow.Version, work.WorkFlowVersion);
            Assert.IsNotNull(work.BeginTime);
            Assert.IsNull(work.EndTime);
            Assert.IsFalse(work.IsFinished);
            Assert.IsFalse(work.IsCancelled);
            Assert.IsFalse(work.OnHold);
            Assert.IsNull(work.HoldTime);
            Assert.IsNull(work.ReleaseTime);
            Assert.IsNotNull(work.NextExecuteTime);

            /// 3. verify work argument(input)
            Assert.IsNotNull(work.Arguments);
            Assert.AreEqual(work.Id, work.Arguments?.WorkingCopyId);
            Assert.AreEqual(1, work.Arguments.PublicArguments.Count);

            /// 4. verify 1 step(Begin) with argument(input)
            Assert.IsNotNull(work.Steps);
            var targetSteps = targetWorkflow.Steps.Where(x => x.IsBegin);
            Assert.AreEqual(1, targetSteps.Count());
            Assert.AreEqual(targetSteps.Count(), work.Steps.Count);
            var targetStep = targetSteps.First();
            var step = work.Steps.FirstOrDefault(x => x.Code == targetStep.Code);
            Assert.IsNotNull(step);
            Assert.IsTrue(!string.IsNullOrEmpty(step.Id));
            Assert.AreEqual(work.Id, step.WorkingCopyId);
            Assert.AreEqual(targetStep.Code, step.Code);
            Assert.AreEqual(targetStep.Name, step.Name);
            Assert.AreEqual(targetStep.Status, step.Status);
            Assert.AreEqual(targetStep.StatusScope, step.StatusScope);
            Assert.AreEqual(_data.merchantOrderId, step.StatusId);
            Assert.AreEqual(targetStep.ByQty, step.ByQty);
            Assert.IsNotNull(step.ActiveTime);
            Assert.IsNull(step.FinishedTime);
            Assert.IsTrue(step.IsBegin);
            Assert.IsFalse(step.IsEnd);
            Assert.IsFalse(step.IsRetry);
            Assert.IsFalse(step.PreActionFinished);
            Assert.IsFalse(step.Success);
            Assert.IsFalse(step.Finished);
            Assert.IsFalse(step.Cancelled);
            Assert.IsTrue(step.PreActionResults?.Any() != true);
            Assert.IsTrue(step.ActionResults?.Any() != true);
            Assert.IsTrue(work.Inserted);
            Assert.IsNotNull(work.UpdatedAt);
            Assert.IsNotNull(work.UpdatedBy);
            Assert.IsTrue(step.Inserted);
            Assert.IsNotNull(step.UpdatedAt);
            Assert.IsNotNull(step.UpdatedBy);

            // argument
            Assert.IsNotNull(step.Arguments);
            Assert.AreEqual(1, step.Arguments.PublicArguments.Count);
            Assert.AreEqual(work.Id, step.Arguments.WorkingCopyId);
            Assert.AreEqual(step.Id, step.Arguments.WorkingStepId);
            Assert.IsNotNull(step.Arguments.PrivateArguments);
            Assert.IsTrue(step.Arguments.PrivateArguments.ContainsKey("workingStepId"));
            Assert.AreEqual(step.Id, step.Arguments.PrivateArguments["workingStepId"]);


            /// 5. verify 0 flow
            Assert.IsNotNull(work.Flows);
            Assert.AreEqual(0, work.Flows.Count());
        }

        /// <summary>
        /// test target:
        /// 1. select no workflow and create no work
        /// </summary>
        [Test]
        public void TestCreateNewWork_SelectNo()
        {
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "C"
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            var workRes = _executionProvider.StartNew(_workflows, input).Result;
            Common.ValidateOperationResult(workRes);
            /// 1. select no workflow and create no work
            Assert.IsNull(workRes.Data);
        }

        /// <summary>
        /// test target:
        /// 1. select two workflow and create two new works for each workflow
        /// </summary>
        [Test]
        public void TestCreateNewWork_SelectMulti()
        {
            var workflows = new List<WorkFlowConfig>();
            var workflow1 = JsonConvert.DeserializeObject<WorkFlowConfig>(
                JsonConvert.SerializeObject(_workflows[0])
                );
            workflow1.Selector = null;
            var workflow2 = JsonConvert.DeserializeObject<WorkFlowConfig>(
                JsonConvert.SerializeObject(_workflows[1])
                );
            workflow2.Selector = null;
            workflows.Add(workflow1);
            workflows.Add(workflow2);

            var workRes = _executionProvider.StartNew(workflows, _input).Result;
            Common.ValidateOperationResultWithData(workRes);
            /// 1. select two workflow and create two new works for each workflow
            Assert.AreEqual(2, workRes.Data.Count());
            Assert.IsTrue(workRes.Data.Any(x => x.WorkFlowCode == workflow1.Code));
            Assert.IsTrue(workRes.Data.Any(x => x.WorkFlowCode == workflow2.Code));
        }

        /// <summary>
        /// test target:
        /// 1. select no workflow and create no work
        /// </summary>
        [Test]
        public void TestCreateNewWork_CategoryNotExist()
        {
            var workRes = _executionProvider.StartNew(new List<WorkFlowConfig>(), _input).Result;
            Assert.IsNotNull(workRes);
            Assert.IsTrue(workRes.Success);
            /// 1. select no workflow and create no work
            Assert.IsNull(workRes.Data);
        }

        [Test]
        public void TestCancelWork()
        {
            var workRes = _executionProvider.StartNew(_workflows, _input).Result;
            var work = workRes?.Data?.FirstOrDefault();
            var targetWorkflow = _workflows.FirstOrDefault(x => x.Code == "TestWorkFlow1");
            Common.ValidateOperationResultWithData(workRes);
            Assert.IsNotNull(work);
            var cancelRes = _executionProvider.Cancel(work, targetWorkflow).Result;
            Common.ValidateOperationResultWithData(cancelRes);
            work = cancelRes.Data;
            Assert.IsTrue(work.IsCancelled);
            Assert.IsNotNull(work.EndTime);
            Assert.IsNotNull(work.UpdatedAt);
            Assert.IsNotNull(work.UpdatedBy);
            foreach (var step in work.Steps)
            {
                Assert.IsTrue(step.Cancelled || step.Finished);
                if (step.Cancelled)
                {
                    Assert.IsNotNull(step.UpdatedAt);
                    Assert.IsNotNull(step.UpdatedBy);
                }
            }
        }

        [Test]
        public void TestPauseWork()
        {
            var workRes = _executionProvider.StartNew(_workflows, _input).Result;
            var work = workRes?.Data?.FirstOrDefault();
            var targetWorkflow = _workflows.FirstOrDefault(x => x.Code == work.WorkFlowCode && x.Version == work.WorkFlowVersion);
            Common.ValidateOperationResultWithData(workRes);
            Assert.IsNotNull(work);
            var pauseRes = _executionProvider.Pause(work, targetWorkflow).Result;
            Common.ValidateOperationResultWithData(pauseRes);
            work = pauseRes.Data;
            Assert.IsTrue(work.OnHold);
            Assert.IsNotNull(work.HoldTime);
            Assert.IsNull(work.ReleaseTime);
            Assert.IsNotNull(work.UpdatedAt);
            Assert.IsNotNull(work.UpdatedBy);
            foreach (var step in work.Steps)
            {
                Assert.IsFalse(step.Cancelled || step.Finished);
            }
        }

        [Test]
        public void TestReleaseWork()
        {
            var workRes = _executionProvider.StartNew(_workflows, _input).Result;
            var work = workRes?.Data?.FirstOrDefault();
            var targetWorkflow = _workflows.FirstOrDefault(x => x.Code == work.WorkFlowCode && x.Version == work.WorkFlowVersion);
            Common.ValidateOperationResultWithData(workRes);
            Assert.IsNotNull(work);
            var pauseRes = _executionProvider.Pause(work, targetWorkflow).Result;
            Common.ValidateOperationResultWithData(pauseRes);
            work = pauseRes.Data;
            Assert.IsTrue(work.OnHold);
            Assert.IsNotNull(work.HoldTime);
            Assert.IsNull(work.ReleaseTime);
            Assert.IsNotNull(work.UpdatedAt);
            Assert.IsNotNull(work.UpdatedBy);
            foreach (var step in work.Steps)
            {
                Assert.IsFalse(step.Cancelled || step.Finished);
            }
            var resumeRes = _executionProvider.Resume(work, targetWorkflow).Result;
            Common.ValidateOperationResultWithData(resumeRes);
            Assert.IsFalse(work.OnHold);
            Assert.IsNotNull(work.HoldTime);
            Assert.IsNotNull(work.ReleaseTime);
            Assert.IsNotNull(work.UpdatedAt);
            Assert.IsNotNull(work.UpdatedBy);
            foreach (var step in work.Steps)
            {
                Assert.IsFalse(step.Cancelled || step.Finished);
            }
        }

        [Test]
        public void TestRestartWork()
        {
            var workRes = _executionProvider.StartNew(_workflows, _input).Result;
            Common.ValidateOperationResultWithData(workRes);
            var works = workRes.Data;
            Assert.AreEqual(1, works.Count());
            var work = works.First();
            var targetWorkflow = _workflows.FirstOrDefault(x => x.Code == work.WorkFlowCode && x.Version == work.WorkFlowVersion);

            var stepCount = work.Steps.Count;
            var restartRes = _executionProvider.Restart(work, targetWorkflow).Result;
            Common.ValidateOperationResultWithData(restartRes);
            Assert.AreEqual(stepCount * 2, work.Steps.Count);
            Assert.AreEqual(stepCount, work.Steps.Count(x => x.Cancelled));
            Assert.AreEqual(stepCount, work.Steps.Count(x => !x.Cancelled));
            Assert.IsNotNull(work.UpdatedAt);
            Assert.IsNotNull(work.UpdatedBy);
        }

        /// <summary>
        ///            Begin
        ///              |
        ///             End
        /// test target: 
        /// 1. verify completed work
        /// 2. verify work argument(input, output)
        /// 3. verify 2 working steps(Begin, End)
        /// 4. verify step:Begin with arguments(input, output)
        /// 5. verify step:End with arguments(input, record, output)
        /// </summary>
        [Test]
        public void TestExecuteWork_BeginEnd()
        {
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess
                    }
                },
                Output = new Dictionary<string, string> {
                    { "orderId", "{{input.orderId}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A"
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );

            var workRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, _input).Result;
            Common.ValidateOperationResultWithData(workRes);
            var works = workRes.Data;
            Assert.AreEqual(1, works.Count());
            var work = works.First();
            Assert.IsNotNull(work);
            Assert.IsNotNull(work.Steps);
            Assert.IsNotNull(work.Flows);
            var executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            var resultWork = executionResult.Data;

            /// 1. verify completed work
            Assert.AreEqual(resultWork.WorkFlowCategory, workflow.Category);
            Assert.AreEqual(resultWork.WorkFlowCode, workflow.Code);
            Assert.AreEqual(resultWork.WorkFlowVersion, workflow.Version);
            Assert.IsNotNull(resultWork.BeginTime);
            Assert.IsNotNull(resultWork.EndTime);
            Assert.IsNull(resultWork.NextExecuteTime);
            Assert.IsTrue(resultWork.IsFinished);
            Assert.IsFalse(resultWork.IsCancelled);
            Assert.IsFalse(resultWork.OnHold);
            Assert.IsNull(resultWork.HoldTime);
            Assert.IsNull(resultWork.ReleaseTime);
            Assert.IsNotNull(resultWork.UpdatedAt);
            Assert.IsNotNull(resultWork.UpdatedBy);

            /// 2. verify work argument(input, output)
            Assert.IsNotNull(resultWork.Arguments);
            Assert.AreEqual(2, resultWork.Arguments.PublicArguments.Count);
            Assert.IsTrue(resultWork.Arguments.PublicArguments.Any(x => x.Key == "input"));
            Assert.IsTrue(resultWork.Arguments.PublicArguments.Any(x => x.Key == "output"));

            /// 3. verify 2 working steps(Begin, End)
            Assert.IsNotNull(resultWork.Steps);
            Assert.AreEqual(2, resultWork.Steps.Count);
            /// 4. verify step:Begin with arguments(input, output)
            var step1 = resultWork.Steps.FirstOrDefault(x => x.Code == "Begin");
            var stepConfig1 = workflow.Steps.FirstOrDefault(x => x.Code == step1.Code);
            Assert.IsNotNull(step1);
            Assert.IsNotNull(stepConfig1);
            Assert.AreEqual(work.Id, step1.WorkingCopyId);
            Assert.AreEqual(stepConfig1.Code, step1.Code);
            Assert.AreEqual(stepConfig1.Name, step1.Name);
            Assert.AreEqual(stepConfig1.Status, step1.Status);
            Assert.AreEqual(stepConfig1.StatusScope, step1.StatusScope);
            Assert.AreEqual(data.merchantOrderId, step1.StatusId);
            Assert.AreEqual(stepConfig1.ByQty, step1.ByQty);
            Assert.AreEqual(stepConfig1.MatchQty, step1.MatchQty);
            Assert.IsNotNull(step1.ActiveTime);
            Assert.IsNotNull(step1.FinishedTime);
            Assert.IsTrue(step1.PreActionFinished);
            Assert.IsTrue(step1.ActionFinished);
            Assert.IsTrue(step1.PostedNext);
            Assert.IsTrue(step1.Success);
            Assert.IsTrue(step1.Finished);
            Assert.IsFalse(step1.Cancelled);
            Assert.AreEqual(0, step1.PreActionExecutedCount);
            Assert.AreEqual(0, step1.PreActionExecutedCount);

            // step argument
            Assert.IsNotNull(step1.Arguments);
            Assert.AreEqual(work.Id, step1.Arguments.WorkingCopyId);
            Assert.AreEqual(step1.Id, step1.Arguments.WorkingStepId);
            Assert.AreEqual(2, step1.Arguments.PublicArguments.Count);
            Assert.IsTrue(step1.Arguments.PublicArguments.Any(x => x.Key == "input"));
            Assert.IsTrue(step1.Arguments.PublicArguments.Any(x => x.Key == "output"));
            Assert.IsNotNull(step1.Arguments.PrivateArguments);
            Assert.IsTrue(step1.Arguments.PrivateArguments.ContainsKey("now"));
            Assert.IsTrue(step1.Arguments.PrivateArguments.ContainsKey("workingStepId"));
            Assert.AreEqual(step1.Id, step1.Arguments.PrivateArguments["workingStepId"]);

            /// 5. verify step:End with arguments(input, record)
            var step2 = resultWork.Steps.FirstOrDefault(x => x.Code == "End");
            var stepConfig2 = workflow.Steps.FirstOrDefault(x => x.Code == step2.Code);
            Assert.IsNotNull(step2);
            Assert.IsNotNull(stepConfig2);
            Assert.AreEqual(work.Id, step2.WorkingCopyId);
            Assert.AreEqual(stepConfig2.Code, step2.Code);
            Assert.AreEqual(stepConfig2.Name, step2.Name);
            Assert.AreEqual(stepConfig2.Status, step2.Status);
            Assert.AreEqual(stepConfig2.StatusScope, step2.StatusScope);
            Assert.AreEqual(data.merchantOrderId, step2.StatusId);
            Assert.AreEqual(stepConfig2.ByQty, step2.ByQty);
            Assert.AreEqual(stepConfig2.MatchQty, step2.MatchQty);
            Assert.IsNotNull(step2.ActiveTime);
            Assert.IsNotNull(step2.FinishedTime);
            Assert.IsTrue(step2.PreActionFinished);
            Assert.IsTrue(step2.ActionFinished);
            Assert.IsTrue(step2.PostedNext);
            Assert.IsTrue(step2.Success);
            Assert.IsTrue(step2.Finished);
            Assert.IsFalse(step2.Cancelled);
            Assert.AreEqual(0, step2.PreActionExecutedCount);
            Assert.AreEqual(0, step2.PreActionExecutedCount);

            // step argument
            Assert.IsNotNull(step2.Arguments);
            Assert.AreEqual(work.Id, step2.WorkingCopyId);
            Assert.AreEqual(step2.Id, step2.Arguments.WorkingStepId);
            Assert.AreEqual(2, step2.Arguments.PublicArguments.Count);
            Assert.IsTrue(step2.Arguments.PublicArguments.Any(x => x.Key == "input"));
            Assert.IsTrue(step2.Arguments.PublicArguments.Any(x => x.Key == "record"));
            Assert.IsNotNull(step2.Arguments.PrivateArguments);
            Assert.IsTrue(step2.Arguments.PrivateArguments.ContainsKey("now"));
            Assert.IsTrue(step2.Arguments.PrivateArguments.ContainsKey("workingStepId"));
            Assert.AreEqual(step2.Id, step2.Arguments.PrivateArguments["workingStepId"]);
        }

        /// <summary>
        ///            Begin
        ///              |
        ///            process
        ///              |
        ///             End
        /// test target: 
        /// 1. verify completed work
        /// 2. verify 3 working steps(Begin, process, End)
        /// 3. verify step:process with arguments(input, output)
        /// </summary>
        [Test]
        public void TestExecuteWork_BeginStepEnd()
        {
            /*
                    Begin
                      |
                    process
                      |
                     End
             */
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code="process",
                        Actions = new List<WorkFlowActionSetting>
                        {
                            new WorkFlowActionSetting
                            {
                                Type = ActionType.VariableProcess,
                                Sequence = 0,
                                ActionConfig = new {
                                    Method = MapListProcessor.Method,
                                    Source = "{{record.items}}",
                                    Output = new Dictionary<string,string>
                                    {
                                        { "itemKey", "{{mapItem.itemId}}" },
                                        { "quantity", "{{mapItem.qty}}"}
                                    }
                                },
                                Output = new Dictionary<string, string>
                                {
                                    { "binItems", "{{result}}"}
                                }
                            }
                        },
                        Output = new Dictionary<string, string>
                        {
                            { "binItems", "{{binItems}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "process",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "process",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess
                    }
                },
                Output = new Dictionary<string, string> {
                    { "orderId", "{{input.orderId}}"},
                    { "binItems", "{{binItems}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic> {
                    new {
                        itemId = "item1",
                        qty = 10
                    },
                    new {
                        itemId = "item2",
                        qty = 8
                    }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );

            var workRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(workRes);
            var works = workRes.Data;
            Assert.AreEqual(1, works.Count());
            var work = works.First();
            Assert.IsNotNull(work);
            Assert.IsNotNull(work.Steps);
            Assert.IsNotNull(work.Flows);
            var executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            var resultWork = executionResult.Data;

            /// 1. verify completed work
            Assert.IsTrue(resultWork.IsFinished);

            /// 2. verify 3 working steps(Begin, process, End)
            Assert.IsNotNull(resultWork.Steps);
            Assert.AreEqual(3, resultWork.Steps.Count);

            /// 3. verify step:process with arguments(input, output)
            var step2 = resultWork.Steps.FirstOrDefault(x => x.Code == "process");
            Assert.IsNotNull(step2);
            var stepConfig2 = workflow.Steps.FirstOrDefault(x => x.Code == step2.Code);
            Assert.IsNotNull(stepConfig2);
            Assert.AreEqual(stepConfig2.Code, step2.Code);
            Assert.AreEqual(stepConfig2.Name, step2.Name);
            Assert.AreEqual(stepConfig2.Status, step2.Status);
            Assert.AreEqual(stepConfig2.StatusScope, step2.StatusScope);
            Assert.AreEqual(string.Empty, step2.StatusId);
            Assert.AreEqual(stepConfig2.ByQty, step2.ByQty);
            Assert.AreEqual(stepConfig2.MatchQty, step2.MatchQty);
            Assert.IsNotNull(step2.ActiveTime);
            Assert.IsNotNull(step2.FinishedTime);
            Assert.IsTrue(step2.PreActionFinished);
            Assert.IsTrue(step2.ActionFinished);
            Assert.IsTrue(step2.PostedNext);
            Assert.IsTrue(step2.Success);
            Assert.IsTrue(step2.Finished);
            Assert.IsFalse(step2.Cancelled);
            Assert.AreEqual(0, step2.PreActionExecutedCount);
            // step argument
            Assert.IsNotNull(step2.Arguments);
            Assert.AreEqual(3, step2.Arguments.PublicArguments.Count);
            Assert.IsTrue(step2.Arguments.PublicArguments.Any(x => x.Key == "input"));
            Assert.IsTrue(step2.Arguments.PublicArguments.Any(x => x.Key == "record"));
            Assert.IsTrue(step2.Arguments.PublicArguments.Any(x => x.Key == "output"));
        }

        /// <summary>
        ///                         Begin
        ///                         /    \
        ///               processItem1   processItem2
        ///               |          \   /          |
        ///      afterProcessItem1   group     afterProcessItem2
        ///                          /    \
        ///           afterProcessItemAny  \
        ///                                End
        /// test target: 
        /// 1. verify 2 processItem generated by "loopby"
        /// 2. verify 7 working steps
        /// 3. verify OnSuccess/OnGroupAnySuccess/OnGroupAllSuccess
        /// 4. verify 2 working groups for each workflow-flow config
        /// 5. verify steps without next
        /// 6. verify work.Groups
        /// </summary>
        [Test]
        public void TestExecuteWork_BeginGroupEnd()
        {
            #region data
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "processItem",
                        Status = "processing_item",
                        StatusScope = "item",
                        StatusId = "{{loopItem.itemId}}",
                        LoopBy = "{{record.items}}",
                        Output = new Dictionary<string, string>
                        {
                            { "item","{{loopItem}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "afterProcessItem",
                        Status = "processed_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}"
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "afterProcessItemAny",
                        Status = "processed_item_any",
                        StatusScope = "order",
                        StatusId = "{{record[0].merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{record[0]}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{record[0]}}"}
                        }
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "processItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "afterProcessItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "afterProcessItemAny",
                        NextOn = FlowNextType.OnGroupAnySuccess,
                        GroupStartStepCode = "Begin"
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnGroupAllSuccess,
                        GroupStartStepCode = "Begin"
                    }
                },
                Output = new Dictionary<string, string> {
                    { "completedOrderId", "{{record.orderId}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic>
                {
                    new { itemId = "item1", qty = 1 },
                    new { itemId = "item2", qty = 2 }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            #endregion

            #region start
            var workRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(workRes);
            var works = workRes.Data;
            Assert.AreEqual(1, works.Count());
            var work = works.First();
            #endregion

            #region execute
            var executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            var resultWork = executionResult.Data;
            #endregion

            Assert.IsNotNull(resultWork);
            Assert.IsNotNull(resultWork.Steps);
            Assert.IsNotNull(resultWork.Flows);
            Assert.IsNotNull(resultWork.Groups);
            Assert.IsTrue(resultWork.IsFinished);

            var processItem1 = resultWork.Steps.FirstOrDefault(x => x.Code == "processItem" && x.StatusId == "item1");
            var processItem2 = resultWork.Steps.FirstOrDefault(x => x.Code == "processItem" && x.StatusId == "item2");
            var afterProcessItem1 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item1");
            var afterProcessItem2 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item2");
            var afterProcessItemAnys = resultWork.Steps.Where(x => x.Code == "afterProcessItemAny");
            Assert.IsNotNull(afterProcessItemAnys);
            Assert.AreEqual(1, afterProcessItemAnys.Count());
            var afterProcessItemAny = afterProcessItemAnys.FirstOrDefault();
            /// 1. verify 2 processItem generated by "loopby"
            Assert.IsNotNull(processItem1);
            Assert.IsNotNull(processItem2);
            /// 2. verify 7 working steps
            Assert.AreEqual(7, resultWork.Steps.Count);
            Assert.IsTrue(resultWork.Steps.All(x => x.Finished));
            /// 3. verify OnSuccess/OnGroupAnySuccess/OnGroupAllSuccess
            Assert.AreEqual(5, resultWork.Flows.Count);
            /// 4. verify 2 working groups for each workflow-flow config
            Assert.AreEqual(2, resultWork.Groups.Count);
            var flow1 = workflow.Flows.FirstOrDefault(x => x.NextOn == FlowNextType.OnGroupAnySuccess);
            var flow2 = workflow.Flows.FirstOrDefault(x => x.NextOn == FlowNextType.OnGroupAllSuccess);
            var group1 = work.Groups.FirstOrDefault(x => x.FLowId == flow1.Id);
            var group2 = work.Groups.FirstOrDefault(x => x.FLowId == flow2.Id);
            Assert.IsNotNull(group1);
            Assert.IsNotNull(group2);
            /// 5. verify steps without next
            Assert.IsTrue(afterProcessItem1.Finished);
            Assert.IsTrue(afterProcessItem1.PostedNext);
            Assert.IsTrue(afterProcessItem2.Finished);
            Assert.IsTrue(afterProcessItem2.PostedNext);
            Assert.IsTrue(afterProcessItemAny.Finished);
            Assert.IsTrue(afterProcessItemAny.PostedNext);
            /// 6. verify work.Groups
            Assert.IsTrue(resultWork.Groups.All(x => x.Finished));
        }

        /// <summary>
        ///                         Begin
        ///                         /    \
        ///               processItem1   processItem2
        ///               |                         |
        ///      afterProcessItem1             afterProcessItem2
        ///               |manual                   |manual
        ///       afterManualItem1              afterManualItem2
        ///                      \             /
        ///                            End
        /// test target: 
        /// 1. verify 2 part to execute
        /// 2. verify 8 working steps(5 before second run)
        /// 3. verify 6 working flows(3 before second run)
        /// 4. verify OnSuccess(on manual post)
        /// 5. verify OnGroupAllSuccess
        /// 6. verify 1 group
        /// </summary>
        [Test]
        public void TestExecuteWork_BeginManualEnd()
        {
            #region data
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "processItem",
                        Status = "processing_item",
                        StatusScope = "item",
                        StatusId = "{{loopItem.itemId}}",
                        LoopBy = "{{record.items}}",
                        Output = new Dictionary<string, string>
                        {
                            { "item","{{loopItem}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "afterProcessItem",
                        Status = "processed_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}",
                        Manual = true
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "afterManualItem",
                        Status = "after_manual_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}"
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{record[0]}}"}
                        }
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "processItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "afterProcessItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "afterProcessItem",
                        NextStepCode = "afterManualItem",
                        NextOn = FlowNextType.OnSuccess,
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "afterManualItem",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnGroupAllSuccess,
                        GroupStartStepCode = "Begin"
                    }
                },
                Output = new Dictionary<string, string> {
                    { "completedOrderId", "{{record.orderId}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic>
                {
                    new { itemId = "item1", qty = 1 },
                    new { itemId = "item2", qty = 2 }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            #endregion

            #region start
            var workRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(workRes);
            var works = workRes.Data;
            Assert.AreEqual(1, works.Count());
            var work = works.First();
            #endregion

            /// 1. verify 2 part to execute
            #region execute 1
            var executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            var resultWork = executionResult.Data;
            #endregion

            Assert.IsNotNull(resultWork);
            Assert.IsNotNull(resultWork.Steps);
            Assert.IsNotNull(resultWork.Flows);
            Assert.IsNotNull(resultWork.Groups);
            Assert.IsFalse(resultWork.IsFinished);
            Assert.IsNotNull(resultWork.NextExecuteTime);
            Assert.AreEqual(5, resultWork.Steps.Count);
            Assert.AreEqual(3, resultWork.Flows.Count);

            /// 4. verify OnSuccess(on manual post)
            var step1 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item1");
            var step2 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item2");
            Assert.IsNotNull(step1);
            Assert.IsNotNull(step2);
            Assert.IsFalse(step1.Finished);
            Assert.IsTrue(step1.WaitManual);
            Assert.IsFalse(step2.Finished);
            Assert.IsTrue(step2.WaitManual);
            var post1Result = _executionProvider.Success(resultWork, step1.Id, 10, new Dictionary<string, object> { { "success", "true" } }).Result;
            var post2Result = _executionProvider.Success(resultWork, step2.Id, 9, new Dictionary<string, object> { { "success", "false" } }).Result;
            Common.ValidateOperationResultWithData(post1Result);
            Assert.IsNotNull(post1Result.Data);
            Assert.AreEqual(work.Id, post1Result.Data.WorkingCopyId);
            Assert.AreEqual(step1.Id, post1Result.Data.WorkingStepId);
            Assert.IsNotNull(post1Result.Data.Arguments);
            Assert.AreEqual(work.Id, post1Result.Data.Arguments.WorkingCopyId);
            Assert.AreEqual(step1.Id, post1Result.Data.Arguments.WorkingStepId);
            Assert.AreEqual(post1Result.Data.Id, post1Result.Data.Arguments.WorkingStepResultId);
            Assert.AreEqual(ActionTypes.StepAction.ToString(), post1Result.Data.Arguments.ActionType);
            step1.ActionResults.Add(post1Result.Data);
            step2.ActionResults.Add(post2Result.Data);

            Assert.IsNotNull(resultWork);
            Assert.IsNotNull(resultWork.Steps);
            Assert.IsNotNull(resultWork.Flows);
            Assert.IsNotNull(resultWork.Groups);
            Assert.IsFalse(resultWork.IsFinished);
            Assert.IsNotNull(resultWork.NextExecuteTime);
            /// 2. verify 8 working steps(5 before second run)
            Assert.AreEqual(5, resultWork.Steps.Count);
            /// 3. verify 7 working flows(3 before second run)
            Assert.AreEqual(3, resultWork.Flows.Count);

            /// 1. verify 2 part to execute
            #region execute 2
            executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            resultWork = executionResult.Data;
            #endregion

            Assert.IsNotNull(resultWork);
            Assert.IsNotNull(resultWork.Steps);
            Assert.IsNotNull(resultWork.Flows);
            Assert.IsNotNull(resultWork.Groups);
            Assert.IsTrue(resultWork.IsFinished);
            Assert.IsNull(resultWork.NextExecuteTime);
            /// 2. verify 8 working steps(5 before second run)
            Assert.AreEqual(8, resultWork.Steps.Count);
            /// 3. verify 7 working flows(3 before second run)
            Assert.AreEqual(6, resultWork.Flows.Count);
            /// 4. verify OnSuccess(on manual post)
            step1 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item1");
            step2 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item2");
            Assert.IsNotNull(step1);
            Assert.IsNotNull(step2);
            Assert.IsTrue(step1.Finished);
            Assert.IsTrue(step2.Finished);
            /// 6. verify 1 group
            Assert.AreEqual(1, work.Groups.Count);
            /// 5. verify OnGroupAllSuccess
            var group = work.Groups.First();
            Assert.IsTrue(group.Fulfilled);
            Assert.IsTrue(group.AllSuccess);
            Assert.IsTrue(group.Finished);
        }

        /// <summary>
        ///             Begin
        ///               |
        ///          processItem1
        ///               |
        ///      afterProcessItem1
        ///               |manual(partial)        |manual(partial)     |
        ///       afterManualItem1        afterManualItem2             |
        ///                                                            |
        ///                                                           End
        /// test target: 
        /// 1. verify execute to afterProcessItem1(3 steps and 2 flows)
        /// 2. post success(6)
        /// 3. verify execute to afterManualItem1(4 steps and 3 flows)
        /// 4. post success(4)
        /// 5. verify execute to Finish(6 steps and 5 flows)
        /// </summary>
        [Test]
        public void TestExecuteWork_BeginPartialEnd()
        {
            #region data
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "processItem",
                        Status = "processing_item",
                        StatusScope = "item",
                        StatusId = "{{loopItem.itemId}}",
                        LoopBy = "{{record.items}}",
                        Output = new Dictionary<string, string>
                        {
                            { "item","{{loopItem}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "afterProcessItem",
                        Status = "processed_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}",
                        Manual = true,
                        ByQty = true,
                        MatchQty = "{{item.qty}}"
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "afterManualItem",
                        Status = "after_manual_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}"
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{record[0]}}"}
                        }
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "processItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "afterProcessItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "afterProcessItem",
                        NextStepCode = "afterManualItem",
                        NextOn = FlowNextType.OnPartialSuccess,
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "afterProcessItem",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess
                    }
                },
                Output = new Dictionary<string, string> {
                    { "completedOrderId", "{{record.orderId}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic>
                {
                    new { itemId = "item1", qty = 10 }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            #endregion

            #region start
            var workRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(workRes);
            var works = workRes.Data;
            Assert.AreEqual(1, works.Count());
            var work = works.First();
            Assert.AreEqual(1, work.Steps.Count);
            #endregion

            /// 1. verify execute to afterProcessItem1(3 steps and 2 flows)
            #region execute 1
            var executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            var resultWork = executionResult.Data;
            #endregion

            Assert.IsNotNull(resultWork);
            Assert.IsNotNull(resultWork.Steps);
            Assert.IsNotNull(resultWork.Flows);
            Assert.IsNotNull(resultWork.Groups);
            Assert.IsFalse(resultWork.IsFinished);
            Assert.IsNotNull(resultWork.NextExecuteTime);
            Assert.AreEqual(3, resultWork.Steps.Count);
            Assert.AreEqual(2, resultWork.Flows.Count);
            Assert.AreEqual(0, resultWork.Groups.Count);

            /// 2. post success(6)
            var step1 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item1");
            Assert.IsNotNull(step1);
            Assert.IsFalse(step1.Finished);
            Assert.IsTrue(step1.WaitManual);
            var postResult = _executionProvider.Success(resultWork, step1.Id, 6, new Dictionary<string, object> { { "success", "true" } }).Result;
            Common.ValidateOperationResultWithData(postResult);
            executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            resultWork = executionResult.Data;

            /// 3. verify execute to afterManualItem1(4 steps and 3 flows)
            Assert.IsNotNull(resultWork);
            Assert.IsNotNull(resultWork.Steps);
            Assert.IsNotNull(resultWork.Flows);
            Assert.IsNotNull(resultWork.Groups);
            Assert.IsFalse(resultWork.IsFinished);
            Assert.IsNotNull(resultWork.NextExecuteTime);
            Assert.AreEqual(4, resultWork.Steps.Count);
            Assert.AreEqual(3, resultWork.Flows.Count);

            /// 4. post success(4)
            step1 = resultWork.Steps.FirstOrDefault(x => x.Code == "afterProcessItem" && x.StatusId == "item1");
            Assert.IsNotNull(step1);
            Assert.IsFalse(step1.Finished);
            Assert.IsTrue(step1.WaitManual);
            postResult = _executionProvider.Success(resultWork, step1.Id, 4, new Dictionary<string, object> { { "success", "true" } }).Result;
            Common.ValidateOperationResultWithData(postResult);
            executionResult = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionResult);
            resultWork = executionResult.Data;

            /// 5. verify execute to Finish(6 steps and 5 flows)
            Assert.IsNotNull(resultWork);
            Assert.IsNotNull(resultWork.Steps);
            Assert.IsNotNull(resultWork.Flows);
            Assert.IsNotNull(resultWork.Groups);
            Assert.IsTrue(resultWork.IsFinished);
            Assert.IsNull(resultWork.NextExecuteTime);
            Assert.AreEqual(6, resultWork.Steps.Count);
            Assert.AreEqual(5, resultWork.Flows.Count);
        }

        /// <summary>
        ///            Begin
        ///              |                |retry
        ///            process1        process2
        ///              |                |
        ///             End1(cancel)     End2
        ///                           
        ///                           
        /// test target: 
        /// 1. start a work and execute process1 and post end1
        /// 2. retry process1, post process2 and cancel end1
        /// 3. check process2
        /// 4. execute to end
        /// 5. verify 5 working steps(Begin, process1, End1, process2, end2)
        /// 6. verify 3 working flows
        /// </summary>
        [Test]
        public void TestRetryStep()
        {
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code="process",
                        Actions = new List<WorkFlowActionSetting>
                        {
                            new WorkFlowActionSetting
                            {
                                Type = ActionType.VariableProcess,
                                Sequence = 0,
                                ActionConfig = new {
                                    Method = MapListProcessor.Method,
                                    Source = "{{record.items}}",
                                    Output = new Dictionary<string,string>
                                    {
                                        { "itemKey", "{{mapItem.itemId}}" },
                                        { "quantity", "{{mapItem.qty}}"}
                                    }
                                },
                                Output = new Dictionary<string, string>
                                {
                                    { "binItems", "{{result}}"}
                                }
                            }
                        },
                        Output = new Dictionary<string, string>
                        {
                            { "binItems", "{{binItems}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "process",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "process",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess
                    }
                },
                Output = new Dictionary<string, string> {
                    { "orderId", "{{input.orderId}}"},
                    { "binItems", "{{binItems}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic> {
                    new {
                        itemId = "item1",
                        qty = 10
                    },
                    new {
                        itemId = "item2",
                        qty = 8
                    }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );

            /// 1. start a work and execute process1 and post end1
            var workRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(workRes);
            var work = workRes.Data.First();
            var stepBegin = work.Steps.First();
            var executeRes = _executionProvider.Execute(work, workflow, true, stepBegin.Id).Result;
            Common.ValidateOperationResultWithData(executeRes);
            work = executeRes.Data;
            Assert.AreEqual(2, work.Steps.Count);

            var stepProcess1 = work.Steps.First(x => x.Code == "process");
            executeRes = _executionProvider.Execute(work, workflow, true, stepProcess1.Id).Result;
            Common.ValidateOperationResultWithData(executeRes);
            work = executeRes.Data;
            Assert.AreEqual(3, work.Steps.Count);
            var stepEnd1 = work.Steps.FirstOrDefault(x => x.Code == "End");
            Assert.IsNotNull(stepEnd1);
            Assert.IsFalse(stepEnd1.Cancelled);

            /// 2. retry process1, post process2 and cancel end1
            var retryRes = _executionProvider.Retry(work, workflow, stepProcess1.Id).Result;
            Common.ValidateOperationResultWithData(retryRes);
            work = executeRes.Data;
            Assert.AreEqual(4, work.Steps.Count);
            Assert.IsTrue(stepEnd1.Cancelled);
            Assert.IsNotNull(stepEnd1.FinishedTime);
            /// 3. check process2
            var stepProcess2 = work.Steps.First(x => x.Code == "process" && x.Id != stepProcess1.Id);
            Assert.IsNotNull(stepProcess2);
            var stepConfigProcess = workflow.Steps.FirstOrDefault(x => x.Code == "process");
            Assert.IsNotNull(stepConfigProcess);
            Assert.AreEqual(stepConfigProcess.Code, stepProcess2.Code);
            Assert.AreEqual(stepConfigProcess.Name, stepProcess2.Name);
            Assert.AreEqual(stepConfigProcess.Status, stepProcess2.Status);
            Assert.AreEqual(stepConfigProcess.StatusScope, stepProcess2.StatusScope);
            Assert.AreEqual(string.Empty, stepProcess2.StatusId);
            Assert.AreEqual(stepConfigProcess.ByQty, stepProcess2.ByQty);
            Assert.AreEqual(stepConfigProcess.MatchQty, stepProcess2.MatchQty);
            Assert.IsNotNull(stepProcess2.ActiveTime);
            Assert.IsNull(stepProcess2.FinishedTime);
            Assert.IsFalse(stepProcess2.PreActionFinished);
            Assert.IsFalse(stepProcess2.ActionFinished);
            Assert.IsFalse(stepProcess2.PostedNext);
            Assert.IsFalse(stepProcess2.Success);
            Assert.IsFalse(stepProcess2.Finished);
            Assert.IsFalse(stepProcess2.Cancelled);
            Assert.AreEqual(0, stepProcess2.PreActionExecutedCount);
            Assert.AreEqual(0, stepProcess2.ActionExecutedCount);
            // step argument
            Assert.IsNotNull(stepProcess2.Arguments);
            Assert.AreEqual(2, stepProcess2.Arguments.PublicArguments.Count);
            Assert.IsTrue(stepProcess2.Arguments.PublicArguments.Any(x => x.Key == "input"));
            Assert.IsTrue(stepProcess2.Arguments.PublicArguments.Any(x => x.Key == "record"));
            Assert.IsNotNull(stepProcess2.Arguments.PrivateArguments);
            Assert.IsTrue(stepProcess2.Arguments.PrivateArguments.ContainsKey("workingStepId"));
            Assert.AreEqual(stepProcess2.Id, stepProcess2.Arguments.PrivateArguments["workingStepId"]);

            /// 4. execute to end
            executeRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executeRes);
            work = executeRes.Data;
            Assert.IsTrue(work.IsFinished);
            /// 5. verify 5 working steps(Begin, process1, End1, process2, end2)
            Assert.IsNotNull(work.Steps);
            Assert.AreEqual(5, work.Steps.Count);
            /// 6. verify 3 working flows
            Assert.IsNotNull(work.Flows);
            Assert.AreEqual(3, work.Flows.Count);
            var flowBeginToProcess = work.Flows.FirstOrDefault(x => x.FromStep?.Steps?.FirstOrDefault()?.Code == "Begin");
            var flowProcess1ToEnd1 = work.Flows.FirstOrDefault(x => x.FromStep?.Steps?.FirstOrDefault()?.Id == stepProcess1.Id);
            var flowProcess2ToEnd2 = work.Flows.FirstOrDefault(x => x.FromStep?.Steps?.FirstOrDefault()?.Id == stepProcess2.Id);
            Assert.IsNotNull(flowBeginToProcess);
            Assert.AreEqual(1, flowBeginToProcess.FromStep.Steps.Count);
            Assert.AreEqual(2, flowBeginToProcess.ToStep.Steps.Count);
            Assert.IsNotNull(flowProcess1ToEnd1);
            Assert.AreEqual(1, flowProcess1ToEnd1.FromStep.Steps.Count);
            Assert.AreEqual(1, flowProcess1ToEnd1.ToStep.Steps.Count);
            Assert.IsNotNull(flowProcess2ToEnd2);
            Assert.AreEqual(1, flowProcess2ToEnd2.FromStep.Steps.Count);
            Assert.AreEqual(1, flowProcess2ToEnd2.ToStep.Steps.Count);
        }

        [Test]
        public void TestPreActionShouldNotTriggerOnSuccess()
        {
            #region data
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "processItem",
                        Status = "processed_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}",
                        PreActions = new List<WorkFlowActionSetting>
                        {
                            new WorkFlowActionSetting
                            {
                                 Type = ActionType.VariableProcess,
                                  ActionConfig = new
                                  {
                                      Method = UnitTestActionProcessor.Method,
                                      Key = Guid.NewGuid(),
                                      ResultSequence = new List<bool>{ true }
                                  }
                            }
                        },
                        Manual = true
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{record[0]}}"}
                        }
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "processItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess,
                        GroupStartStepCode = "Begin"
                    }
                },
                Output = new Dictionary<string, string> {
                    { "completedOrderId", "{{record.orderId}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic>
                {
                    new { itemId = "item1", qty = 1 },
                    new { itemId = "item2", qty = 2 }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            #endregion

            var startWorkRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(startWorkRes);

            var work = startWorkRes.Data?.FirstOrDefault();
            Assert.IsNotNull(work);
            var executionRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionRes);
            work = executionRes.Data;
            // work should not be finished, only 2steps existed: begin -> processItem
            Assert.IsNotNull(work);
            Assert.IsFalse(work.IsFinished);
            Assert.AreEqual(2, work.Steps.Count);
            var stepProcessItem = work.Steps.FirstOrDefault(x => x.Code == "processItem");
            Assert.IsTrue(stepProcessItem.PreActionFinished);
            Assert.IsFalse(stepProcessItem.ActionFinished);
            Assert.IsFalse(stepProcessItem.Finished);
            Assert.IsFalse(stepProcessItem.Success);
        }

        [Test]
        public void TestPreActionReachRetryLimit()
        {
            #region data
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "processItem",
                        Status = "processed_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}",
                        RetryLimit = 2,
                        PreActions = new List<WorkFlowActionSetting>
                        {
                            new WorkFlowActionSetting
                            {
                                 Type = ActionType.VariableProcess,
                                  ActionConfig = new
                                  {
                                      Method = UnitTestActionProcessor.Method,
                                      Key = Guid.NewGuid(),
                                      ResultSequence = new List<bool>{ false, false, false, false, false,false,false,false  }
                                  }
                            }
                        },
                        Manual = true
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "onProcessItemFailed",
                        Status = "onProcessItemFailed",
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{record[0]}}"}
                        }
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "processItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess,
                        GroupStartStepCode = "Begin"
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "onProcessItemFailed",
                        NextOn = FlowNextType.OnFail
                    }
                },
                Output = new Dictionary<string, string> {
                    { "completedOrderId", "{{record.orderId}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic>
                {
                    new { itemId = "item1", qty = 1 },
                    new { itemId = "item2", qty = 2 }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            #endregion

            var startWorkRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(startWorkRes);

            var work = startWorkRes.Data?.FirstOrDefault();
            Assert.IsNotNull(work);

            // fail 1
            var executionRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionRes);
            work = executionRes.Data;
            // work should not be finished, only 2steps existed: begin -> processItem
            Assert.IsNotNull(work);
            Assert.IsFalse(work.IsFinished);
            Assert.AreEqual(2, work.Steps.Count);
            var stepProcessItem = work.Steps.FirstOrDefault(x => x.Code == "processItem");
            Assert.IsFalse(stepProcessItem.PreActionFinished);
            Assert.IsFalse(stepProcessItem.ActionFinished);
            Assert.IsFalse(stepProcessItem.Finished);
            Assert.IsFalse(stepProcessItem.Success);
            Assert.AreEqual(1, stepProcessItem.PreActionExecutedCount);

            // fail 2
            executionRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionRes);
            work = executionRes.Data;
            // work should not be finished, only 2steps existed: begin -> processItem
            Assert.IsNotNull(work);
            Assert.IsFalse(work.IsFinished);
            Assert.AreEqual(2, work.Steps.Count);
            stepProcessItem = work.Steps.FirstOrDefault(x => x.Code == "processItem");
            Assert.IsFalse(stepProcessItem.PreActionFinished);
            Assert.IsFalse(stepProcessItem.ActionFinished);
            Assert.IsFalse(stepProcessItem.Finished);
            Assert.IsFalse(stepProcessItem.Success);
            Assert.AreEqual(2, stepProcessItem.PreActionExecutedCount);

            // reach rety limit
            executionRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionRes);
            work = executionRes.Data;
            // work should not be finished, only 2steps existed: begin -> processItem -> onProcessItemFailed
            Assert.IsNotNull(work);
            Assert.IsFalse(work.IsFinished);
            Assert.AreEqual(3, work.Steps.Count);
            stepProcessItem = work.Steps.FirstOrDefault(x => x.Code == "processItem");
            Assert.IsTrue(stepProcessItem.PreActionFinished);
            Assert.IsTrue(stepProcessItem.ActionFinished);
            Assert.IsTrue(stepProcessItem.Finished);
            Assert.IsFalse(stepProcessItem.Success);
            Assert.AreEqual(2, stepProcessItem.PreActionExecutedCount);
            Assert.IsTrue(work.Steps.Any(x => x.Code == "onProcessItemFailed"));
        }

        [Test]
        public void TestPreActionFailed()
        {
            #region data
            var workflow = new WorkFlowConfig
            {
                Category = "testing",
                Code = "TestWorkFlow1",
                Desc = "TestWorkFlow1",
                Name = "TestWorkFlow1",
                Version = 1,
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "Begin",
                        IsBegin = true,
                        Status = "record_incoming",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{input}}"}
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "processItem",
                        Status = "processed_item",
                        StatusScope = "item",
                        StatusId = "{{item.itemId}}",
                        RetryLimit = 2,
                        PreActions = new List<WorkFlowActionSetting>
                        {
                            new WorkFlowActionSetting
                            {
                                Type = ActionType.VariableProcess,
                                ActionConfig = new
                                {
                                    Method = UnitTestActionProcessor.Method,
                                    Key = Guid.NewGuid(),
                                    ResultSequence = new List<bool>{ false, false, false, false, false,false,false,false  },
                                    DirectFail = true
                                }
                            }
                        },
                        Manual = true
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "onProcessItemFailed",
                        Status = "onProcessItemFailed",
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "End",
                        IsEnd = true,
                        Status = "order_completed",
                        StatusScope = "order",
                        StatusId = "{{input.merchantOrderId}}",
                        Output = new Dictionary<string, string>
                        {
                            { "record","{{record[0]}}"}
                        }
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "Begin",
                        NextStepCode = "processItem",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess,
                        GroupStartStepCode = "Begin"
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "processItem",
                        NextStepCode = "onProcessItemFailed",
                        NextOn = FlowNextType.OnFail
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "onProcessItemFailed",
                        NextStepCode = "End",
                        NextOn = FlowNextType.OnSuccess
                    }
                },
                Output = new Dictionary<string, string> {
                    { "completedOrderId", "{{record.orderId}}"}
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic>
                {
                    new { itemId = "item1", qty = 1 },
                    new { itemId = "item2", qty = 2 }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            #endregion

            var startWorkRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(startWorkRes);

            var work = startWorkRes.Data?.FirstOrDefault();
            Assert.IsNotNull(work);

            // fail 1
            var executionRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executionRes);
            work = executionRes.Data;
            // work should not be finished, only 2steps existed: begin -> processItem
            Assert.IsNotNull(work);
            Assert.IsTrue(work.IsFinished);
            Assert.AreEqual(4, work.Steps.Count);
            var stepProcessItem = work.Steps.FirstOrDefault(x => x.Code == "processItem");
            Assert.IsTrue(stepProcessItem.PreActionFinished);
            Assert.IsTrue(stepProcessItem.ActionFinished);
            Assert.IsTrue(stepProcessItem.Finished);
            Assert.IsFalse(stepProcessItem.Success);
            Assert.AreEqual(1, stepProcessItem.PreActionExecutedCount);

            Assert.IsTrue(work.Steps.Any(x => x.Code == "onProcessItemFailed"));
        }

        [Test]
        public void TestAfterGroupOutput()
        {
            #region data
            var workflow = new WorkFlowConfig
            {
                Steps = new List<WorkFlowConfigStep>
                {
                    new WorkFlowConfigStep
                    {
                        Code = "start",
                        IsBegin = true
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "item_process",
                        LoopBy = "{{input.items}}"
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "after_group",
                        Output = new Dictionary<string, string>
                        {
                            { "input","{{input[0]}}"}
                        },
                        Manual = true,
                        PreActions = new List<WorkFlowActionSetting>
                        {
                            new WorkFlowActionSetting
                            {
                                Type = ActionType.VariableProcess,
                                ActionConfig = new
                                {
                                    Method = UnitTestActionProcessor.Method,
                                    Key = Guid.NewGuid(),
                                    ResultSequence = new List<bool>{ true  }
                                }
                            }
                        }
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "after_group_2"
                    },
                    new WorkFlowConfigStep
                    {
                        Code = "end",
                        IsEnd = true
                    }
                },
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "start",
                        NextStepCode = "item_process",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "item_process",
                        NextStepCode = "after_group",
                        NextOn = FlowNextType.OnGroupAllSuccess,
                        GroupStartStepCode = "start"
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "after_group",
                        NextStepCode = "after_group_2",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "after_group_2",
                        NextStepCode = "end",
                        NextOn = FlowNextType.OnSuccess
                    }
                }
            };
            var data = new
            {
                orderId = "123",
                merchantOrderId = "m123",
                orderType = "A",
                items = new List<dynamic>
                {
                    new { itemId = "item1", qty = 1 },
                    new { itemId = "item2", qty = 2 }
                }
            };
            var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(data)
            );
            #endregion

            var startRes = _executionProvider.StartNew(new List<WorkFlowConfig> { workflow }, input).Result;
            Common.ValidateOperationResultWithData(startRes);
            var work = startRes.Data.FirstOrDefault();
            Assert.IsNotNull(work);

            var executeRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executeRes);
            work = executeRes.Data;
            Assert.IsNotNull(work);

            var afterGroupStep = work.Steps.FirstOrDefault(x => x.Code == "after_group");
            var successRes = _executionProvider.Success(work, afterGroupStep.Id, 0, new Dictionary<string, object> { }).Result;
            Common.ValidateOperationResultWithData(successRes);

            executeRes = _executionProvider.Execute(work, workflow, true).Result;
            Common.ValidateOperationResultWithData(executeRes);
            work = executeRes.Data;
            Assert.IsNotNull(work);

            var startStep = work.Steps.FirstOrDefault(x => x.Code == "start");
            afterGroupStep = work.Steps.FirstOrDefault(x => x.Code == "after_group");
            var afterGroup2Step = work.Steps.FirstOrDefault(x => x.Code == "after_group_2");

            Assert.IsNotNull(startStep?.Arguments?.PublicArguments);
            Assert.IsNotNull(afterGroupStep?.Arguments?.PublicArguments);
            Assert.IsNotNull(afterGroup2Step?.Arguments?.PublicArguments);

            //Assert.AreEqual($"[{startStep.Arguments.PublicArguments["input"]}]", afterGroupStep.Arguments.PublicArguments["input"]);
            Assert.AreEqual(startStep.Arguments.PublicArguments["input"], afterGroup2Step.Arguments.PublicArguments["input"].Replace("\r", "").Replace("\n", ""));
        }
    }
}
