using System;
using System.Collections.Generic;
using AWorkFlow.Core.Environments;

namespace AWorkFlow.Core.Runner
{
    public class WorkFlowEngine
    {
        private static Dictionary<string, WorkFlowEngine> namedEngines = new Dictionary<string, WorkFlowEngine>();

        public WorkFlowEnvironment Settings { get; set; } = new WorkFlowEnvironment();
        public WorkManager Worker { get; set; } = new WorkManager();
        public WorkFlowManager WorkFlow { get; set; } = new WorkFlowManager();

        public static WorkFlowEngine Create(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (namedEngines.ContainsKey(name))
            {
                throw new Exception($"Name {name} already created.");
            }

            var engine = new WorkFlowEngine();
            namedEngines.Add(name, engine);

            return engine;
        }

        public static WorkFlowEngine Engine(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return namedEngines[name];
        }
    }
}
