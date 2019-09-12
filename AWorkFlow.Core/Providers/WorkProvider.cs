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
    /// <summary>
    /// provides operations related to work(s)
    /// </summary>
    public class WorkProvider : IWorkProvider
    {
        private readonly IWorkRepository _workRepository;
        private readonly IWorkFlowProvider _workFlowProvider;
        private readonly IJobProvider _jobProvider;
        private readonly IExecutorProvider _executorProvider;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="workRepository"></param>
        /// <param name="workFlowProvider"></param>
        /// <param name="jobProvider"></param>
        /// <param name="executorProvider"></param>
        public WorkProvider(IWorkRepository workRepository, IWorkFlowProvider workFlowProvider, IJobProvider jobProvider, IExecutorProvider executorProvider)
        {
            _workRepository = workRepository;
            _workFlowProvider = workFlowProvider;
            _jobProvider = jobProvider;
            _executorProvider = executorProvider;
        }

        public Task FinishStep(WorkStepDto workStep, bool success, string user)
        {
            throw new NotImplementedException();
        }

        public Task FinishWork(WorkDto work, bool success, string user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// get a work by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<WorkDto> GetWork(string id)
        {
            return _workRepository.GetWork(id);
        }

        /// <summary>
        /// put a work on hold
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<WorkDto> Hold(string id, string user)
        {
            // hold the work
            await _workRepository.HoldWork(id, user);
            return await _workRepository.GetWork(id);
        }

        public Task PostStep(WorkStepDto workStep, string user)
        {
            throw new NotImplementedException();
        }

        public Task<WorkDto> Restart(string id, string user)
        {
            // restart the work
            throw new NotImplementedException();
        }

        public Task<WorkDto> Resume(string id, string user)
        {
            // release hold
            throw new NotImplementedException();
        }

        public Task<WorkDto> Retry(string id, string user)
        {
            // retry a step
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WorkDto>> Search()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<WorkDto>> Start(string category, object data, string user)
        {
            // get workflow(s) by category
            var workflows = await _workFlowProvider.GetWorkingFlows(category);
            // run selector(s) to pickup workflow(s)
            List<WorkFlowDto> workingFlows = new List<WorkFlowDto>();
            ArgumentsDto arguments = new ArgumentsDto();
            arguments.PutPublic(ReservedVariableNames.INPUT, data?.ToJson());
            IExpressionProvider expressionProvider = new ExpressionProvider(arguments);
            foreach (var workflow in workflows)
            {
                foreach (var selector in workflow.Selectors)
                {
                    var executor = await _executorProvider.GetExecutor(selector);
                    var selectorResult = await executor.Execute(expressionProvider, selector);
                    if (selectorResult?.Success == true)
                    {
                        workingFlows.Add(workflow);
                    }
                }
            }
            if (!workingFlows.Any())
            {
                return null;
            }
            List<WorkDto> works = new List<WorkDto>();
            foreach (var workflow in workingFlows)
            {
                // insert work
                var work = new WorkDto
                {
                    WorkId = Guid.NewGuid().ToString(),
                    WorkFlowCode = workflow.Code,
                    WorkFlowVersion = workflow.Version,
                    BeginTime = DateTime.UtcNow
                };
                var insertResult = await _workRepository.InsertWork(work, user);
                works.Add(work);
                // post a job for pre-action(s)
                var postJobResult = await _jobProvider.PostJob(new JobDto
                {
                    Id = Guid.NewGuid().ToString(),
                    JobType = JobTypes.WorkPreAction,
                    Actions = workflow.PreActions
                }, user);
            }

            return works;
        }

        public async Task<WorkDto> Stop(string id, string user)
        {
            // cancel the work
            var cancelResult = await _workRepository.CancelWork(id, user);
            return await _workRepository.GetWork(id);
        }
    }
}
