using AutoMapper;
using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mcs.SF.WorkFlow.Api.Models.Configs
{
    /// <summary>
    /// data model for workflow config
    /// </summary>
    [MapsTo(typeof(WorkFlowConfigEntity), ReverseMap = true)]
    [MapsToWorkFlowConfigEntity]
    public class WorkFlowConfig
    {
        /// <summary>
        /// Code
        /// </summary>
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Desc
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Category
        /// </summary>
        [Required]
        public string Category { get; set; }
        /// <summary>
        /// WorkFlow Selector
        /// </summary>
        public WorkFlowActionSetting Selector { get; set; }
        /// <summary>
        /// WorkFlow Steps
        /// </summary>
        [MinLength(1)]
        public List<WorkFlowConfigStep> Steps { get; set; }
        /// <summary>
        /// WorkFlow Flows
        /// </summary>
        public List<WorkFlowConfigFlow> Flows { get; set; }
        /// <summary>
        /// WorkFlow Output
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }

    /// <summary>
    /// extra mapping
    /// </summary>
    public class MapsToWorkFlowConfigEntity : MapsToAttribute
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MapsToWorkFlowConfigEntity() : base(typeof(WorkFlowConfigEntity)) { }

        /// <summary>
        /// mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        public void ConfigureMapping(IMappingExpression<WorkFlowConfig, WorkFlowConfigEntity> mappingExpression)
        {
            mappingExpression
                .ForMember(d => d.Id, expression => expression.MapFrom(s => Guid.NewGuid().ToString()))
                .ForMember(d => d.Output, expression => expression.MapFrom(s => JsonConvert.SerializeObject(s.Output)));
            mappingExpression.ReverseMap()
                .ForMember(d => d.Output, expression => expression.MapFrom(s => JsonConvert.DeserializeObject<Dictionary<string, string>>(s.Output)));
        }
    }
}
