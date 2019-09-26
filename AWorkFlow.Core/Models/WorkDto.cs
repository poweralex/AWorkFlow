using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow.Core.Models
{
    public class WorkDto
    {
        public string Id { get; set; }
        public WorkFlowDto WorkFlow { get; set; }
        /// <summary>
        /// all history steps and running steps
        /// </summary>
        public List<WorkStepDto> Steps { get; set; } = new List<WorkStepDto>();
        /// <summary>
        /// all flowcharts across steps
        /// </summary>
        public List<WorkDirectionDto> FlowCharts { get; set; }
        /// <summary>
        /// input data of work
        /// </summary>
        public ArgumentsDto InputData { get; set; }
        /// <summary>
        /// output data of work
        /// </summary>
        public ArgumentsDto OutputData { get; set; }
        /// <summary>
        /// if this work completed(finished or completed)
        /// </summary>
        public bool Completed { get { return Finished || Cancelled; } }
        /// <summary>
        /// if this work finished
        /// </summary>
        public bool Finished { get; set; }
        /// <summary>
        /// if this work cancelled
        /// </summary>
        public bool Cancelled { get; set; }


        /// <summary>
        /// working steps(not completed)
        /// </summary>
        public List<WorkStepDto> WorkingSteps { get; set; } = new List<WorkStepDto>();
        /// <summary>
        /// running jobs(work jobs and step jobs)
        /// </summary>
        public List<JobDto> RunningJobs { get; set; } = new List<JobDto>();
        /// <summary>
        /// completed jobs(work jobs and step jobs)
        /// </summary>
        public List<JobResultDto> CompletedJobs = new List<JobResultDto>();
        /// <summary>
        /// all group relations of steps
        /// </summary>
        public Dictionary<string, WorkGroupDto> Groups { get; set; } = new Dictionary<string, WorkGroupDto>();

        internal void Start()
        {
            FireJobs();
        }

        /// <summary>
        /// fire jobs
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<JobDto> FireJobs()
        {
            List<JobDto> jobs = new List<JobDto>();
            // work.PreAction
            if (WorkFlow?.PreActions?.Any() == true && !RunningJobs.Any(x => x.Type == JobType.WorkPreAction) && !CompletedJobs.Any(x => x?.Job?.Type == JobType.WorkPreAction))
            {
                jobs.Add(new JobDto
                {
                    Type = JobType.WorkPreAction,
                    Work = this
                });
            }

            FireSteps();
            // step jobs
            if (WorkingSteps.Any())
            {
                foreach (var step in WorkingSteps)
                {
                    jobs.AddRange(step.FireJobs());
                }
            }

            // work.AfterAction
            if (WorkFlow?.AfterActions?.Any() == true 
                && !RunningJobs.Any(x => x.Type == JobType.WorkAfterAction) 
                && (WorkFlow?.Steps?.Any() != true || Steps.Any(x => x.WorkFlowStep.IsEnd))
                && !WorkingSteps.Any() 
                && !Finished)
            {
                jobs.Add(new JobDto
                {
                    Type = JobType.WorkAfterAction,
                    Work = this
                });
            }

            // TODO: push jobs to RunningJobs if not already exists
            RunningJobs.AddRange(jobs);
            return jobs;
        }

        /// <summary>
        /// fire next steps by current work statuses
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<WorkStepDto> FireSteps()
        {
            // find steps not handled
            var steps = Steps.Where(x => !FlowCharts.Any(fc => fc.CurrentStepId == x.Id));
            if (steps?.Any() != true)
            {
                return new List<WorkStepDto>();
            }

            // fire next step(s)
            List<WorkStepDto> nextSteps = new List<WorkStepDto>();
            foreach (var step in steps)
            {
                var flowsFromStep = WorkFlow.Flows.Where(x => x.StepCode == step.WorkFlowStep.Code);
                foreach (var flow in flowsFromStep)
                {
                    if (flow.IsFulfilled(this))
                    {
                        nextSteps.AddRange(flow.PostSteps(this));
                    }
                }
            }

            Steps.AddRange(steps);
            WorkingSteps.AddRange(steps);

            return steps;
        }
    }

    public class WorkStepDto
    {
        public string Id { get; set; }
        public WorkFlowStepDto WorkFlowStep { get; set; }
        public WorkDto Work { get; set; }
        public ArgumentsDto Data { get; set; }
        /// <summary>
        /// if this work completed(finished or completed)
        /// </summary>
        public bool Completed { get { return Finished || Cancelled; } }
        /// <summary>
        /// if this work finished
        /// </summary>
        public bool Finished { get; set; }
        /// <summary>
        /// if this work cancelled
        /// </summary>
        public bool Cancelled { get; set; }

        public List<JobDto> RunningJobs = new List<JobDto>();
        public List<JobResultDto> CompletedJobs = new List<JobResultDto>();

        internal IEnumerable<JobDto> FireJobs()
        {
            List<JobDto> jobs = new List<JobDto>();
            // step.PreAction
            if (WorkFlowStep?.PreActions?.Any() == true && !RunningJobs.Any(x => x.Type == JobType.StepPreAction) && !CompletedJobs.Any(x => x.Job.Type == JobType.StepPreAction))
            {
                jobs.Add(new JobDto
                {
                    Type = JobType.StepPreAction,
                    Work = Work,
                    Step = this
                });
            }

            // step.Action
            if (WorkFlowStep?.Actions?.Any() == true && !RunningJobs.Any(x => x.Type == JobType.StepAction) && !CompletedJobs.Any(x => x.Job.Type == JobType.StepAction))
            {
                jobs.Add(new JobDto
                {
                    Type = JobType.StepAction,
                    Work = Work,
                    Step = this
                });
            }

            // step.AfterAction
            if (WorkFlowStep?.AfterActions?.Any() == true && !RunningJobs.Any(x => x.Type == JobType.StepAfterAction) && !CompletedJobs.Any(x => x.Job.Type == JobType.StepAfterAction))
            {
                jobs.Add(new JobDto
                {
                    Type = JobType.StepAfterAction,
                    Work = Work,
                    Step = this
                });
            }

            // TODO: push jobs to RunningJobs if not already exists
            RunningJobs.AddRange(jobs);
            return jobs;
        }
    }

    public class WorkGroupDto
    {
        public string GroupKey { get; set; }
        /// <summary>
        /// if this group fully fulfilled
        /// </summary>
        public bool Fulfilled { get; set; }
        public WorkGroupDto ParentGroup { get; set; }
        public List<WorkGroupDto> RelatedGroups { get; set; } = new List<WorkGroupDto>();

        public void Join(WorkGroupDto group)
        {
            RelatedGroups.Add(group);
        }
    }

    public class WorkDirectionDto
    {
        public string CurrentStepId { get; set; }
        public string NextStepId { get; set; }
    }
}
