using AWorkFlow2.Models.Configs;
using AWorkFlow2.Models.Working;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace WorkFlow.Test
{
    class WorkingCopyGroupTests
    {
        [Test]
        public void TestBuildGroup()
        {
            var startStep = new WorkingCopyStep
            {
                Code = "start",
                StatusId = "all",
                ActionFinished = true,
                Success = true,
                PostedNext = true,
                Finished = true,
            };
            var a1Step1Step = new WorkingCopyStep
            {
                Code = "step1",
                StatusId = "a1",
                ActionFinished = true,
                Success = true
            };
            var a2Step1Step = new WorkingCopyStep
            {
                Code = "step1",
                StatusId = "a2",
                ActionFinished = false,
                Success = true
            };

            var work = new WorkingCopy
            {
                Steps = new List<WorkingCopyStep>
                {
                    startStep,
                    a1Step1Step,
                    a2Step1Step,
                },
                Flows = new List<WorkingCopyFlow>
                {
                    new WorkingCopyFlow
                    {
                        FromStep = new WorkingCopyFlowSeed(startStep),
                        ToStep = new WorkingCopyFlowSeed(a1Step1Step)
                    },
                    new WorkingCopyFlow
                    {
                        FromStep = new WorkingCopyFlowSeed(startStep),
                        ToStep = new WorkingCopyFlowSeed(a2Step1Step)
                    },
                }
            };
            var workflow = new WorkFlowConfig
            {
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "start",
                        NextStepCode = "step1",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "step1",
                        NextStepCode = "step2",
                        NextOn = FlowNextType.OnGroupAnySuccess,
                        GroupStartStepCode = "start"
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "step1",
                        NextStepCode = "step3",
                        NextOn = FlowNextType.OnGroupAllSuccess,
                        GroupStartStepCode = "start"
                    },
                }
            };
            var flow = workflow.Flows.FirstOrDefault(x => x.CurrentStepCode == "step1" && x.NextStepCode == "step2");
            var groups = WorkingCopyGroup.BuildGroup(work, workflow, flow);
            Assert.IsNotNull(groups);
            Assert.AreEqual(1, groups.Count());
            var group = groups.First();
            Assert.IsNotNull(group);
            Assert.AreEqual(flow.Id, group.FLowId);
            Assert.AreEqual(flow.GroupStartStepCode, group.StartStepCode);
            Assert.AreEqual(flow.CurrentStepCode, group.EndStepCode);
            Assert.IsFalse(group.Fulfilled);
            Assert.IsTrue(group.AnySuccess);
            Assert.IsFalse(group.AnyFail);
            Assert.IsFalse(group.AllSuccess);
            Assert.IsFalse(group.AllFail);
            Assert.IsFalse(group.PostedNext);
            Assert.IsNotNull(group.Steps);
            Assert.AreEqual(3, group.Steps.Count);
        }

        [Test]
        public void TestBuildGroup_NotFulfilled()
        {
            var startStep = new WorkingCopyStep
            {
                Code = "start",
                StatusId = "all",
                ActionFinished = true,
                Success = true,
                PostedNext = false
            };
            var work = new WorkingCopy
            {
                Steps = new List<WorkingCopyStep>
                {
                    startStep,
                },
                Flows = new List<WorkingCopyFlow>
                {
                }
            };
            var workflow = new WorkFlowConfig
            {
                Flows = new List<WorkFlowConfigFlow>
                {
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "start",
                        NextStepCode = "step1",
                        NextOn = FlowNextType.OnSuccess
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "step1",
                        NextStepCode = "step2",
                        NextOn = FlowNextType.OnGroupAnySuccess,
                        GroupStartStepCode = "start"
                    },
                    new WorkFlowConfigFlow
                    {
                        CurrentStepCode = "step1",
                        NextStepCode = "step3",
                        NextOn = FlowNextType.OnGroupAllSuccess,
                        GroupStartStepCode = "start"
                    },
                }
            };
            var flow = workflow.Flows.FirstOrDefault(x => x.CurrentStepCode == "step1" && x.NextStepCode == "step2");
            var groups = WorkingCopyGroup.BuildGroup(work, workflow, flow);
            Assert.IsNotNull(groups);
            Assert.AreEqual(1, groups.Count());
            var group = groups.First();
            Assert.IsNotNull(group);
            Assert.AreEqual(flow.Id, group.FLowId);
            Assert.AreEqual(flow.GroupStartStepCode, group.StartStepCode);
            Assert.AreEqual(flow.CurrentStepCode, group.EndStepCode);
            Assert.IsFalse(group.Fulfilled);
            Assert.IsFalse(group.AnySuccess);
            Assert.IsFalse(group.AnyFail);
            Assert.IsFalse(group.AllSuccess);
            Assert.IsFalse(group.AllFail);
            Assert.IsFalse(group.PostedNext);
            Assert.IsNotNull(group.Steps);
            Assert.AreEqual(1, group.Steps.Count);
        }

    }
}
