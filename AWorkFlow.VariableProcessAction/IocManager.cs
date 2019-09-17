using Autofac;
using AWorkFlow.VariableProcessAction.Processor;

namespace AWorkFlow.VariableProcessAction
{
    class IocManager
    {
        private static IContainer _container;
        public static IContainer Initialize()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SetValueProcessor>().Named<IVariableProcessor>("setValue".ToLower());

            _container = builder.Build();
            return _container;
        }

        public static T Get<T>(string name)
        {
            return _container.ResolveNamed<T>(name?.ToLower());
        }
    }
}
