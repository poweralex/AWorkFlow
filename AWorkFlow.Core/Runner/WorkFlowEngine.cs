using AWorkFlow.Core.Environments;
using System;
using System.Collections.Generic;

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
            WorkFlowManager.Engine = this;
            WorkManager = Settings.Resolve<WorkManager>();
            WorkManager.Engine = this;
            WorkManager.Start();
            JobManager = Settings.Resolve<JobManager>();
            JobManager.Engine = this;
            JobManager.SetWorkers(Settings.Workers);
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
