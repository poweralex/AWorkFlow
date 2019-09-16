using Autofac;
using AWorkFlow.Core.Providers;
using AWorkFlow.Core.Providers.Interfaces;
using System;

namespace AWorkFlow.Core.Environments
{
    public class WorkFlowEnvironment
    {
        private readonly ContainerBuilder builder;
        private IContainer container;

        private WorkFlowEnvironment()
        {
            builder = new ContainerBuilder();
            builder.RegisterType<WorkFlowProvider>().As<IWorkFlowProvider>();
            builder.RegisterType<WorkProvider>().As<IWorkProvider>();
            builder.RegisterType<JobProvider>().As<IJobProvider>();
            builder.RegisterType<ExecutorProvider>().As<IExecutorProvider>();
            container = null;
        }

        public static WorkFlowEnvironment Instance { get; } = new WorkFlowEnvironment();

        public WorkFlowEnvironment RegisterAction<T>(string actionType) where T : IExecutor
        {
            builder.RegisterType<T>().Named<IExecutor>(GetActionName(actionType));
            return this;
        }

        public WorkFlowEnvironment RegisterRepository<TImp, TAs>() where TImp : TAs
        {
            builder.RegisterType<TImp>().As<TAs>();
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
