using System;
using System.Collections.Generic;
using AWorkFlow.ConsoleAction;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Runner;

namespace AWorkFlow.Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello workflow!");
            #region init data
            // set workflow
            var workflow = new WorkFlowDto
            {
                Category = "NewOrder",
                Code = "TestOrder",
                OutputExp = "",
                Selectors = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                        Sequence = 1,
                        Indicators = new List<ResultIndicator>
                        {
                            new ResultIndicator
                            {
                                ActualExp = "{{input.IsTest}}",
                                ExpectedExps = new List<string> { "true" },
                                IsSuccess = true
                            }
                        }
                    }
                },
                PreActions = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                        Sequence = 1,
                        ActionType = "console",
                        Settings = new ConsoleActionSetting
                        {
                            OutputExp = "TestOrder({{input.OrderId}}) begins."
                        }
                    }
                },
                AfterActions = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                        ActionType = "console",
                        Settings = new ConsoleActionSetting
                        {
                            OutputExp = "TestOrder({{input.OrderId}}) completed."
                        }
                    }
                },
                DefaultFailureHandlers = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                        ActionType = "console",
                        Settings = new ConsoleActionSetting
                        {
                            OutputExp = "TestOrder({{input.OrderId}}) failed: {{exception.Message}}."
                        }
                    }
                },
                Steps = new List<WorkFlowStepDto>
                {
                    new WorkFlowStepDto
                    {
                        Code = "prepare_record",
                        IsBegin = true,
                        OutputExps = new Dictionary<string, string>
                        {
                            { "record", "{{input}}" }
                        },
                        Actions = new List<ActionSettingDto>
                        {
                            new ActionSettingDto
                            {
                                Sequence = 1,
                                ActionType = "console",
                                Settings = new ConsoleActionSetting
                                {
                                    OutputExp = "preparing record."
                                }
                            }
                        }
                    },
                    new WorkFlowStepDto
                    {
                        Code = "prepare_order",
                        Actions = new List<ActionSettingDto>
                        {
                            new ActionSettingDto
                            {
                                Sequence = 1,
                                ActionType = "console",
                                Settings = new ConsoleActionSetting
                                {
                                    OutputExp = "preparing order {{record.OrderId}}."
                                }
                            }
                        }
                    },
                    new WorkFlowStepDto
                    {
                        Code = "prepare_item",
                        Actions = new List<ActionSettingDto>
                        {
                            new ActionSettingDto
                            {
                                Sequence = 1,
                                ActionType = "console",
                                Settings = new ConsoleActionSetting
                                {
                                    OutputExp = "preparing item {{loopItem.ItemId}}"
                                }
                            }
                        }
                    },
                    new WorkFlowStepDto
                    {
                        Code ="collapse_items",
                        OutputExps = new Dictionary<string, string>
                        {
                            { "record", "record[0]" }
                        },
                        Actions = new List<ActionSettingDto>
                        {
                            new ActionSettingDto
                            {
                                Sequence = 1,
                                ActionType = "console",
                                Settings = new ConsoleActionSetting
                                {
                                    OutputExp = "collapsing items to record."
                                }
                            }
                        }
                    },
                    new WorkFlowStepDto
                    {
                        Code = "end",
                        OutputExps = new Dictionary<string, string>
                        {
                            { "record", "record" }
                        },
                        Actions = new List<ActionSettingDto>
                        {
                            new ActionSettingDto
                            {
                                Sequence = 1,
                                ActionType = "console",
                                Settings = new ConsoleActionSetting
                                {
                                    OutputExp = "TestOrder {{record.OrderId}} ends."
                                }
                            }
                        }
                    }
                },
                Flows = new List<WorkFlowDirectionDto>
                {
                    // begin => 1 (1:1)
                    new WorkFlowDirectionDto
                    {
                       StepCode = "prepare_record",
                       NextStepCode = "prepare_order",
                       NextOn = WorkFlowNextOn.OnSuccess
                    },
                    // 1 => 2a + 2b (1:N)
                    new WorkFlowDirectionDto
                    {
                        StepCode = "prepare_order",
                        NextStepCode = "prepare_item",
                        NextOn = WorkFlowNextOn.OnSuccess,
                        LoopByExp = "record.Items"
                    },
                    // 2a + 2b => 3 (N:1)
                    new WorkFlowDirectionDto
                    {
                        StepCode = "prepare_item",
                        NextStepCode = "collapse_items",
                        NextOn = WorkFlowNextOn.OnGroupAllSuccess
                    },
                    // 3 => end (1:1)
                    new WorkFlowDirectionDto
                    {
                        StepCode = "collapse_items",
                        NextStepCode = "end",
                        NextOn = WorkFlowNextOn.OnSuccess
                    }
                },
            };
            // test data
            var orderData = new
            {
                OrderId = "order001",
                IsTest = true,
                Items = new List<dynamic>
                {
                    new
                    {
                        ItemId = "item001",
                        Sku = "skuA",
                        Qty = 10
                    },
                    new
                    {
                        ItemId = "item002",
                        Sku = "skuB",
                        Qty = 19
                    }
                }
            };
            #endregion
            // set environments
            var engine = WorkFlowEngine.Create("workflow");
            engine.Settings
                .RegisterAction<ConsoleActionExecutor>("console")
                .Build();
            engine.WorkFlow.Add(workflow);

            // start a new work with data
            var work = engine.Worker.StartWork("NewOrder", orderData);

            Console.WriteLine("press any key to exit...");
            Console.ReadKey();
        }
    }
}
