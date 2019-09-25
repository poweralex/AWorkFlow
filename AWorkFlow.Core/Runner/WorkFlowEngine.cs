using System;
using System.Collections.Generic;
using AWorkFlow.Core.Environments;

namespace AWorkFlow.Core.Runner
{
    public class WorkFlowEngine
    {
        private static Dictionary<string, WorkFlowEngine> namedEngines = new Dictionary<string, WorkFlowEngine>();

        public WorkFlowEnvironment Settings { get; set; } = new WorkFlowEnvironment();
        public WorkManager WorkManager { get; private set; }
        public WorkFlowManager WorkFlowManager { get; private set; }
        public JobManager JobManager { get; private set; }

        private WorkFlowEngine()
        {

        }

        /// <summary>
        /// start engine
        /// </summary>
        public void Start()
        {
            WorkFlowManager = Settings.Resolve<WorkFlowManager>();
            WorkManager = Settings.Resolve<WorkManager>();
            JobManager = Settings.Resolve<JobManager>();
        }

        /// <summary>
        /// stop engine
        /// </summary>
        public void Stop()
        {
            // persistent
        }

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
