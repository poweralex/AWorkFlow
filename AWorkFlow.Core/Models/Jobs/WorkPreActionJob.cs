using AWorkFlow.Core.Repositories.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AWorkFlow.Core.Models.Jobs
{
    public class WorkPreActionJob : JobDto
    {
        [IgnoreDataMember]
        public WorkDto Work { get; set; }

        public WorkPreActionJob()
        {
            JobType = JobTypes.WorkPreAction;
        }

        internal override async Task<IEnumerable<JobDto>> AfterSuccess()
        {
            // post first step
            var nextJobs = await Work.PostStep(null, null);
            return nextJobs;
        }
    }
}
