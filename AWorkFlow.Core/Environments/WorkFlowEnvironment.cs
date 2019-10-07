using System;
using System.Collections.Generic;
using Autofac;
using AWorkFlow.Core.ActionExecutors;
using AWorkFlow.Core.Distributes;
using AWorkFlow.Core.Runner;

namespace AWorkFlow.Core.Environments
{
    public class WorkFlowEnvironment
    {
        private readonly ContainerBuilder builder;
        private IContainer container;
        public List<IJobDistribute> Workers { get; private set; } = new List<IJobDistribute>();

        internal WorkFlowEnvironment()
        {
            builder = new ContainerBuilder();

            builder.RegisterType<WorkFlowManager>();
            builder.RegisterType<WorkManager>();
            builder.RegisterType<JobManager>();
            container = null;
        }

        public WorkFlowEnvironment RegisterAction<T>(string actionType) where T : IActionExecutor
        {
            builder.RegisterType<T>().Named<IActionExecutor>(GetActionName(actionType));
            return this;
        }

        //public WorkFlowEnvironment RegisterRepository<TImp, TAs>() where TImp : TAs
        //{
        //    builder.RegisterType<TImp>().As<TAs>();
        //    return this;
        //}

        public WorkFlowEnvironment RegisterWorker(IJobDistribute worker)
        {
            Workers.Add(worker);
            return this;
        }

        public WorkFlowEnvironment Build()
        {
            if (container == null)
            {
                container = builder.Build();
                return this;
            }
            else
            {
                throw new InvalidOperationException("Environment already settled");
            }
        }

        public IActionExecutor ResolveAction(string actionType)
        {
            return container.ResolveNamed<IActionExecutor>(GetActionName(actionType));
        }

        public T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        private string GetActionName(string actionType)
        {
            return $"Action_Type_{actionType}";
        }
    }
}
