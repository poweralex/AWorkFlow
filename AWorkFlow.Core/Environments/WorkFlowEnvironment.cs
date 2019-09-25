using System;
using System.Collections.Generic;
using Autofac;
using AWorkFlow.Core.Providers.Interfaces;
using AWorkFlow.Core.Runner;

namespace AWorkFlow.Core.Environments
{
    public class WorkFlowEnvironment
    {
        private readonly ContainerBuilder builder;
        private IContainer container;
        public List<JobExecutor> workers { get; private set; } = new List<JobExecutor>();

        internal WorkFlowEnvironment()
        {
            builder = new ContainerBuilder();

            builder.RegisterType<WorkFlowManager>();
            builder.RegisterType<WorkManager>();
            builder.RegisterType<JobManager>();
            container = null;
        }

        public WorkFlowEnvironment RegisterAction<T>(string actionType) where T : IExecutor
        {
            builder.RegisterType<T>().Named<IExecutor>(GetActionName(actionType));
            return this;
        }

        //public WorkFlowEnvironment RegisterRepository<TImp, TAs>() where TImp : TAs
        //{
        //    builder.RegisterType<TImp>().As<TAs>();
        //    return this;
        //}

        public WorkFlowEnvironment RegisterWorker(JobExecutor worker)
        {
            workers.Add(worker);
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

        public IExecutor ResolveAction(string actionType)
        {
            return container.ResolveNamed<IExecutor>(GetActionName(actionType));
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
