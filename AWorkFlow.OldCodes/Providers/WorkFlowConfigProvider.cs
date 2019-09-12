using AutoMapper;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.Common.ServiceProviders.UnitOfWorkNS;
using Mcs.SF.WorkFlow.Api.Models;
using Mcs.SF.WorkFlow.Api.Models.Configs;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Mcs.SF.WorkFlow.Api.Repos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers
{
    /// <summary>
    /// provides operations of workflow
    /// </summary>
    public class WorkFlowConfigProvider
    {
        /// <summary>
        /// workflow selector keyword
        /// </summary>
        public static string WorkFlowSelector { get { return "WorkFlowSelector"; } }
        /// <summary>
        /// step selector keyword
        /// </summary>
        public static string StepSelector { get { return "StepSelector"; } }
        /// <summary>
        /// step action keyword
        /// </summary>
        public static string StepAction { get { return "StepAction"; } }

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
        public WorkFlowConfigProvider(IMapper mapper, UnitOfWorkProvider unitOfWorkProvider)
        {
            _mapper = mapper;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        /// <summary>
        /// validate workflow
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public Task<OperationResult<IEnumerable<SFMessage>>> ValidateWorkFlow(WorkFlowConfig config)
        {
            List<SFMessage> results = new List<SFMessage>();
            // begin/end steps
            var beginStepCount = config?.Steps?.Count(x => x.IsBegin);
            if (beginStepCount < 1)
            {
                results.Add(Messages.BeginStepNotFound);
            }
            if (beginStepCount > 1)
            {
                results.Add(Messages.MultipleBeginStepFound);
            }
            if (config?.Steps?.Any(x => x.IsEnd) != true)
            {
                results.Add(Messages.EndStepNotFound);
            }

            // action sequences
            bool actionSequenceInvalid = false;
            HashSet<string> actionSequences = new HashSet<string>();
            foreach (var step in config?.Steps)
            {
                if (step?.Actions?.Any() == true)
                {
                    foreach (var action in step?.Actions)
                    {
                        var key = $"{step.Code}-{action.Sequence}";
                        if (actionSequences.Contains(key))
                        {
                            results.Add(Messages.InvalidActionSequences);
                            actionSequenceInvalid = true;
                            break;
                        }
                        else
                        {
                            actionSequences.Add(key);
                        }
                    }
                }
                if (actionSequenceInvalid)
                {
                    break;
                }
            }

            // flow connected

            // dead loop

            return Task.FromResult(new OperationResult<IEnumerable<SFMessage>> { Success = !results.Any(), Data = results });
        }

        /// <summary>
        /// create workflow
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<OperationResult> CreateWorkFlow(WorkFlowConfig config)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                uow.BeginTransaction();
                DateTime now = DateTime.UtcNow;
                // look for workflow for same category and same code
                var currentConfigs = await uow.Repo<WorkFlowConfigRepository>().Search(false, config.Category, config.Code);
                var configEntity = _mapper.Map<WorkFlowConfigEntity>(config);
                var stepEntities = new List<WorkFlowConfigStepEntity>();
                var flowEntities = new List<WorkFlowConfigFlowEntity>();
                List<WorkFlowConfigActionEntity> actionEntities = new List<WorkFlowConfigActionEntity>();
                // version should be max + 1, initialize from 1
                if (currentConfigs?.Data?.Any() != true)
                {
                    configEntity.Version = 1;
                }
                else
                {
                    configEntity.Version = currentConfigs.Data.Max(x => x.Version) + 1;
                }
                configEntity.Active = true;
                configEntity.CreatedAt = now;
                configEntity.CreatedBy = User;
                configEntity.UpdatedAt = now;
                configEntity.UpdatedBy = User;
                // set workflow selector
                if (config.Selector != null)
                {
                    actionEntities.Add(new WorkFlowConfigActionEntity
                    {
                        RefId = configEntity.Id,
                        Code = WorkFlowSelector,
                        Type = config.Selector.Type.ToString(),
                        Sequence = config.Selector.Sequence,
                        ActionConfig = JsonConvert.SerializeObject(config.Selector.ActionConfig),
                        CreatedAt = now,
                        CreatedBy = User,
                        UpdatedAt = now,
                        UpdatedBy = User
                    });
                }
                // steps
                foreach (var step in config.Steps)
                {
                    var stepEntity = _mapper.Map<WorkFlowConfigStepEntity>(step);
                    stepEntity.WorkFlowId = configEntity.Id;
                    stepEntity.CreatedAt = now;
                    stepEntity.CreatedBy = User;
                    stepEntity.UpdatedAt = now;
                    stepEntity.UpdatedBy = User;
                    stepEntities.Add(stepEntity);
                    if (step.Actions?.Any() == true)
                    {
                        foreach (var action in step.Actions)
                        {
                            actionEntities.Add(new WorkFlowConfigActionEntity
                            {
                                RefId = stepEntity.Id,
                                Code = StepAction,
                                Type = action.Type.ToString(),
                                Sequence = action.Sequence,
                                ActionConfig = JsonConvert.SerializeObject(action.ActionConfig),
                                CreatedAt = now,
                                CreatedBy = User,
                                UpdatedAt = now,
                                UpdatedBy = User
                            });
                        }
                    }
                }
                // flows
                if (config.Flows?.Any() == true)
                {
                    foreach (var flow in config.Flows)
                    {
                        var flowEntity = _mapper.Map<WorkFlowConfigFlowEntity>(flow);
                        flowEntity.WorkFlowId = configEntity.Id;
                        flowEntity.CreatedAt = now;
                        flowEntity.CreatedBy = User;
                        flowEntity.UpdatedAt = now;
                        flowEntity.UpdatedBy = User;
                        flowEntities.Add(flowEntity);
                    }
                }

                List<Task<OperationResult>> tasks = new List<Task<OperationResult>>
                {
                    uow.Repo<WorkFlowConfigRepository>().Insert(configEntity),
                    uow.Repo<WorkFlowConfigStepRepository>().BatchInsert(stepEntities)
                };
                if (flowEntities?.Any() == true)
                {
                    tasks.Add(uow.Repo<WorkFlowConfigFlowRepository>().BatchInsert(flowEntities));
                }
                if (actionEntities?.Any() == true)
                {
                    tasks.Add(uow.Repo<WorkFlowConfigActionRepository>().BatchInsert(actionEntities));
                }
                var results = await Task.WhenAll(tasks);

                if (results.All(x => x.Success))
                {
                    uow.Commit();
                    return new OperationResult { Success = true };
                }
                else
                {
                    return new OperationResult { Success = false };
                }
            }
        }

        /// <summary>
        /// remove workflow
        /// </summary>
        /// <param name="code"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async Task<OperationResult> RemoveWorkFlow(string code, int? version = null)
        {
            DateTime now = DateTime.UtcNow;
            string user = "user";
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                uow.BeginTransaction();
                // looking for workflow with specific code and version
                var currentEntities = await uow.Repo<WorkFlowConfigRepository>().Search(false, string.Empty, code, version);
                if (currentEntities?.Data?.Any() == true)
                {
                    var entities = currentEntities.Data;
                    foreach (var entity in entities)
                    {
                        // in-active those workflow(s)
                        entity.Active = false;
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = user;
                    }
                    var res = await uow.Repo<WorkFlowConfigRepository>().BatchUpdate(entities.ToList());
                    if (res?.Success == true)
                    {
                        uow.Commit();
                        return new OperationResult { Success = true };
                    }
                    else
                    {
                        return new OperationResult { Success = false };
                    }
                }
            }
            return new OperationResult();
        }

        /// <summary>
        /// search workflow(s)
        /// </summary>
        /// <param name="category"></param>
        /// <param name="code"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<WorkFlowConfig>>> GetWorkFlows(string category, string code = null, int? version = null)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var configEntities = await uow.Repo<WorkFlowConfigRepository>().Search(false, category, code, version);
                var configIds = configEntities?.Data?.Select(x => x.Id);
                var stepEntities = await uow.Repo<WorkFlowConfigStepRepository>().Search(configIds);
                var flowEntities = await uow.Repo<WorkFlowConfigFlowRepository>().Search(configIds);
                List<string> refIds = new List<string>();
                refIds.AddRange(configIds);
                refIds.AddRange(stepEntities.Data.Select(x => x.Id));
                var actionEntities = await uow.Repo<WorkFlowConfigActionRepository>().Search(refIds);
                var workflows = AssembleWorkFlowConfig(configEntities?.Data, stepEntities?.Data, flowEntities?.Data, actionEntities?.Data);
                return new OperationResult<IEnumerable<WorkFlowConfig>> { Success = true, Data = workflows };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<WorkFlowConfig>>> ListWorkFlowCategory()
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var entity = await uow.Repo<WorkFlowCategoryRepository>().Search();
                if (entity?.Success == true)
                {
                    return new OperationResult<IEnumerable<WorkFlowConfig>> { Success = true, Data = _mapper.Map<List<WorkFlowConfig>>(entity?.Data) };
                }
                else
                {
                    return new OperationResult<IEnumerable<WorkFlowConfig>>(entity);
                }
            }
        }

        /// <summary>
        /// create workflow category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<OperationResult> CreateCategory(WorkFlowCategory category)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var entity = _mapper.Map<WorkFlowCategoryEntity>(category);
                var now = DateTime.UtcNow;
                entity.CreatedAt = now;
                entity.CreatedBy = User;
                entity.UpdatedAt = now;
                entity.UpdatedBy = User;
                return await uow.Repo<WorkFlowCategoryRepository>().Insert(entity);
            }
        }

        /// <summary>
        /// remove workflow category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<OperationResult> RemoveCategory(string category)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var entity = await uow.Repo<WorkFlowCategoryRepository>().Search(false, category);
                if (entity?.Success != true || entity?.Data?.Any() != true)
                {
                    return new OperationResult { Success = false, Message = $"category {category} not existed" };
                }
                return await uow.Repo<WorkFlowCategoryRepository>().Delete(entity.Data.First());
            }
        }

        /// <summary>
        /// fuzzy search workflow category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<OperationResult<IEnumerable<WorkFlowCategory>>> GetCategory(string category)
        {
            using (var uow = _unitOfWorkProvider.CreateUnitOfWork())
            {
                var entity = await uow.Repo<WorkFlowCategoryRepository>().Search(true, category);
                return new OperationResult<IEnumerable<WorkFlowCategory>> { Success = entity.Success, Data = _mapper.Map<List<WorkFlowCategory>>(entity?.Data) };
            }
        }

        private IEnumerable<WorkFlowConfig> AssembleWorkFlowConfig(
            IEnumerable<WorkFlowConfigEntity> configEntities,
            IEnumerable<WorkFlowConfigStepEntity> stepEntities,
            IEnumerable<WorkFlowConfigFlowEntity> flowEntities,
            IEnumerable<WorkFlowConfigActionEntity> actionEntities)
        {
            List<WorkFlowConfig> results = new List<WorkFlowConfig>();
            foreach (var configE in configEntities)
            {
                var config = _mapper.Map<WorkFlowConfig>(configE);
                var stepEs = stepEntities.Where(x => x.WorkFlowId == configE.Id);
                var flowEs = flowEntities.Where(x => x.WorkFlowId == configE.Id);
                var selectorAction = actionEntities.FirstOrDefault(x => x.RefId == configE.Id && WorkFlowSelector.Equals(x.Code, StringComparison.CurrentCultureIgnoreCase));
                if (selectorAction != null)
                {
                    config.Selector = _mapper.Map<WorkFlowActionSetting>(selectorAction);
                }
                config.Flows = _mapper.Map<List<WorkFlowConfigFlow>>(flowEs);
                List<WorkFlowConfigStep> steps = new List<WorkFlowConfigStep>();
                foreach (var stepE in stepEs)
                {
                    var step = _mapper.Map<WorkFlowConfigStep>(stepE);
                    var actionEs = actionEntities.Where(x => x.RefId == stepE.Id && StepAction.Equals(x.Code, StringComparison.CurrentCultureIgnoreCase));
                    step.Actions = _mapper.Map<List<WorkFlowActionSetting>>(actionEs);
                    steps.Add(step);
                }
                config.Steps = steps;
                results.Add(config);
            }

            return results;
        }
    }
}
