using AWorkFlow.ConsoleAction;
using AWorkFlow.Core.Environments;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using AWorkFlow.Core.Repositories.Interfaces;
using AWorkFlow.InMemoryRepo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello WorkFlow!");
            #region test data
            string user = "alex";
            WorkFlowDto newOrderYuePaiWorkFlowConfig = new WorkFlowDto
            {
                Code = "YuePaiNewOrder",
                Category = "NewOrder",
                Selectors = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                         ActionType = "console",
                        Indicators = new List<ResultIndicator>
                        {
                            new ResultIndicator
                            {
                                ActualExp = "{{input.salesChannel}}",
                                ExpectedExps = new List<string>{ "yuepai" },
                                IsSuccess = true
                            }
                        }
                    }
                },
                DefaultFailureHandlers = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                        ActionType = "console",
                        Settings = new ConsoleActionSetting { OutputExp = "DefaultFailureHandler: Error!" }
                    }
                },
                PreActions = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                        ActionType = "console",
                        Settings = new ConsoleActionSetting { OutputExp = "Work.PreAction: Work begins." }
                    }
                },
                AfterActions = new List<ActionSettingDto>
                {
                    new ActionSettingDto
                    {
                        ActionType = "console",
                        Settings = new ConsoleActionSetting { OutputExp = "Work.AfterAction: Work ends." }
                    }
                },
                Steps = new List<WorkFlowStepDto>
                {
                    new WorkFlowStepDto
                    {
                        Code = "prepare_record",
                        IsBegin = true,
                        OutputExps = new Dictionary<string,string>{ { "record", "{{input}}"} }
                    },
                    new WorkFlowStepDto
                    {
                        Code = "prepare_order",
                        Actions = new List<ActionSettingDto>
                        {
                            new ActionSettingDto
                            {
                                ActionType = "console",
                                Settings = new ConsoleActionSetting { OutputExp = "prepare_order executing." }
                            }
                        }
                    },
                    new WorkFlowStepDto
                    {
                        Code = "end",
                        IsEnd = true,
                        TagExps = new List<string>{ "order_completed" },
                        TagDataExp = "{{record[0].orderId}}"
                    }
                },
                Flows = new List<WorkFlowDirectionDto>
                {
                    new WorkFlowDirectionDto
                    {
                        StepCode = "prepare_record",
                        NextStepCode = "prepare_order",
                        NextOn = WorkFlowNextOn.OnSuccess
                    },
                    new WorkFlowDirectionDto
                    {
                        StepCode = "prepare_order",
                        NextStepCode = "end",
                        NextOn = WorkFlowNextOn.OnSuccess
                    }
                }
                // ...
            };
            WorkFlowDto newOrderWorkFlowConfig = JsonConvert.DeserializeObject<WorkFlowDto>(JsonConvert.SerializeObject(newOrderYuePaiWorkFlowConfig));
            newOrderWorkFlowConfig.Code = "CimpressNewOrder";
            newOrderWorkFlowConfig.Selectors = new List<ActionSettingDto>
            {
                new ActionSettingDto
                {
                    ActionType = "console",
                    Indicators = new List<ResultIndicator>
                    {
                        new ResultIndicator
                        {
                            ActualExp = "{{input.salesChannel}}",
                            ExpectedExps = new List<string>{ "Cimpress-Shanghai" },
                            IsSuccess = true
                        }
                    }
                }
            };
            object startData = new { salesChannel = "yuepai" };
            #endregion

            // init workflow.core
            var env = WorkFlowEnvironment.Instance
                .RegisterAction<ConsoleActionExecutor>("console")
                .RegisterRepository<WorkFlowInMemRepo, IWorkFlowRepository>()
                .RegisterRepository<WorkInMemRepo, IWorkRepository>()
                .RegisterRepository<JobInMemRepo, IJobRepository>()
                .Build();
            IWorkFlowProvider workFlowProvider = env.Resolve<IWorkFlowProvider>();
            IWorkProvider workProvider = env.Resolve<IWorkProvider>();
            IJobProvider jobProvider = env.Resolve<IJobProvider>();
            // config workflow
            var workflow = workFlowProvider.SetWorkFLow(newOrderYuePaiWorkFlowConfig).Result;
            Output($"workflow:{workflow.Code} of {workflow.Category}@{workflow.Version} settle.");
            workflow = workFlowProvider.SetWorkFLow(newOrderWorkFlowConfig).Result;
            Output($"workflow:{workflow.Code} of {workflow.Category}@{workflow.Version} settle.");
            // start a work
            var works = workProvider.Start("NewOrder", startData, user).Result;
            foreach (var work in works)
            {
                Output($"new work of workflow:{work.WorkFlowCode} of NewOrder@{work.WorkFlowVersion} started.");
            }
            // get job and run
            while (true)
            {
                Output("going to get jobs for executing.");
                // list job(s)
                var jobs = jobProvider.ListJobsToDo(5).Result;
                if (jobs?.Any() != true)
                {
                    Output($"get no jobs to do, wait for next run");
                    Console.WriteLine("press any key to run...");
                    Console.ReadKey();
                    continue;
                }
                // loop
                foreach (var job in jobs)
                {
                    ExecuteJob(jobProvider, job, user).Wait();
                }
                Output($"all jobs executed");
            }
            Console.WriteLine("press any key...");
            Console.ReadKey();
        }

        static async Task ExecuteJob(IJobProvider jobProvider, JobDto job, string user)
        {
            try
            {
                Output($"executing job {job.Id}");
                // lock job
                // get job
                // run
                var result = await jobProvider.Execute(job, user);
                Output($"executing job succeeded for job: {job.Id}");
                if (result?.Any() == true)
                {
                    Output($"got new jobs to do for jobs: [{string.Join(",", result.Select(x => x.Id))}]");
                    Task.WaitAll(result.Select(x => ExecuteJob(jobProvider, x, user)).ToArray());
                    Output($"all jobs executed for jobs: [{string.Join(", ", result.Select(x => x.Id))}]");
                }

                //return result;
            }
            catch (Exception ex)
            {
                Output($"execute job {job.Id} error: {ex.Message}");
            }
        }

        static void Output(string str)
        {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {str}");
        }
    }
}
