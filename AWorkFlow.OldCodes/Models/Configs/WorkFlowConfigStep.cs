using AutoMapper;
using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mcs.SF.WorkFlow.Api.Models.Configs
{
    /// <summary>
    /// workflow step config
    /// </summary>
    [MapsTo(typeof(WorkFlowConfigStepEntity), ReverseMap = true)]
    [MapsToWorkFlowConfigStepEntity]
    public class WorkFlowConfigStep
    {
        /// <summary>
        /// step code
        /// </summary>
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// desc
        /// </summary>
        public string Desc { get; set; }
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
        /// loop by
        /// </summary>
        public string LoopBy { get; set; }
        /// <summary>
        /// is manual step
        /// </summary>
        public bool Manual { get; set; }
        /// <summary>
        /// is begin step
        /// </summary>
        public bool IsBegin { get; set; }
        /// <summary>
        /// is end step
        /// </summary>
        public bool IsEnd { get; set; }
        /// <summary>
        /// step output(s)
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
        /// <summary>
        /// actions
        /// </summary>
        public List<WorkFlowActionSetting> Actions { get; set; }
    }

    /// <summary>
    /// extra mapping setting
    /// </summary>
    public class MapsToWorkFlowConfigStepEntity : MapsToAttribute
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MapsToWorkFlowConfigStepEntity() : base(typeof(WorkFlowConfigStepEntity)) { }

        /// <summary>
        /// mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        public void ConfigureMapping(IMappingExpression<WorkFlowConfigStep, WorkFlowConfigStepEntity> mappingExpression)
        {
            mappingExpression
                .ForMember(d => d.Output, expression => expression.MapFrom(s => (s.Output == null || s.Output.Any() == false) ? string.Empty : JsonConvert.SerializeObject(s.Output)))
                .ForMember(d => d.Tags, expression => expression.MapFrom(s => (s.Tags == null || s.Tags.Any() == false) ? string.Empty : JsonConvert.SerializeObject(s.Tags)));
            mappingExpression.ReverseMap()
                .ForMember(d => d.Output, expression => expression.MapFrom(s => string.IsNullOrEmpty(s.Output) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(s.Output)))
                .ForMember(d => d.Tags, expression => expression.MapFrom(s => string.IsNullOrEmpty(s.Tags) ? null : JsonConvert.DeserializeObject<List<string>>(s.Tags)));

        }
    }

}
