using AutoMapper;
using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Mcs.SF.WorkFlow.Api.Models.Working
{
    /// <summary>
    /// data model for working step
    /// </summary>
    [MapsTo(typeof(WorkingCopyStepEntity), ReverseMap = true)]
    [MapsToWorkingCopyStepEntity]
    public class WorkingCopyStep
    {
        /// <summary>
        /// working step id
        /// </summary>
        [MapsFromProperty(typeof(string), "Id")]
        public string Id { get; set; }
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId { get; set; }
        /// <summary>
        /// previous working step id
        /// </summary>
        public string PreviousWorkingCopyStepId { get; set; }
        /// <summary>
        /// step code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// status scope
        /// </summary>
        public string StatusScope { get; set; }
        /// <summary>
        /// status id
        /// </summary>
        public string StatusId { get; set; }
        /// <summary>
        /// tags
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// group
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// active time
        /// </summary>
        public DateTime? ActiveTime { get; set; }
        /// <summary>
        /// finish time
        /// </summary>
        public DateTime? FinishedTime { get; set; }
        /// <summary>
        /// is success
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// is normal finished
        /// </summary>
        public bool Finished { get; set; }
        /// <summary>
        /// is cancelled
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// variables related to this step
        /// </summary>
        public WorkingVariables Variables { get; private set; } = new WorkingVariables();
        /// <summary>
        /// execute results
        /// </summary>
        public List<WorkingCopyStepResult> Results { get; set; }
    }

    /// <summary>
    /// extra mapping settings
    /// </summary>
    public class MapsToWorkingCopyStepEntity : MapsToAttribute
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MapsToWorkingCopyStepEntity() : base(typeof(WorkingCopyStepEntity)) { }

        /// <summary>
        /// mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        public void ConfigureMapping(IMappingExpression<WorkingCopyStep, WorkingCopyStepEntity> mappingExpression)
        {
            mappingExpression
                .ForMember(d => d.Tags, expression => expression.MapFrom(s => JsonConvert.SerializeObject(s.Tags)));
            mappingExpression.ReverseMap()
                .ForMember(d => d.Tags, expression => expression.MapFrom(s => JsonConvert.DeserializeObject<List<string>>(s.Tags)));

        }
    }

}
