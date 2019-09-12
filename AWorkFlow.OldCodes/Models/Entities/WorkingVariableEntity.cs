using Dapper.Contrib.Extensions;
using Mcs.SF.Common.ServiceProviders.CommonModel;

namespace Mcs.SF.WorkFlow.Api.Models.Entities
{
    /// <summary>
    /// entity model for working variable(s)
    /// </summary>
    [Table("wf_working_variable")]
    public class WorkingVariableEntity : GuidEntity
    {
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId { get; set; }
        /// <summary>
        /// working step id
        /// </summary>
        public string WorkingStepId { get; set; }
        /// <summary>
        /// working step result id
        /// </summary>
        public string WorkingStepResultId { get; set; }
        /// <summary>
        /// variable name
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// variable value
        /// </summary>
        public string Value { get; set; }
    }
}
