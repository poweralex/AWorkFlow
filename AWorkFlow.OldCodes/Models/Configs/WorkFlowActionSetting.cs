using AutoMapper;
using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mcs.SF.WorkFlow.Api.Models.Configs
{
    /// <summary>
    /// data model for workflow action setting
    /// </summary>
    [MapsTo(typeof(WorkFlowConfigActionEntity), ReverseMap = true)]
    [MapsToWorkFlowConfigActionEntity]
    public class WorkFlowActionSetting
    {
        /// <summary>
        /// action type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType Type { get; set; }
        /// <summary>
        /// execute sequence
        /// </summary>
        public int Sequence { get; set; }
        /// <summary>
        /// action config json
        /// </summary>
        public object ActionConfig { get; set; }
    }

    /// <summary>
    /// action type
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// call web api action
        /// </summary>
        CallRestApi,
        /// <summary>
        /// variable process action
        /// </summary>
        VariableProcess
    }

    /// <summary>
    /// extra mapping setting
    /// </summary>
    public class MapsToWorkFlowConfigActionEntity : MapsToAttribute
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MapsToWorkFlowConfigActionEntity() : base(typeof(WorkFlowConfigActionEntity)) { }

        /// <summary>
        /// mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        public void ConfigureMapping(IMappingExpression<WorkFlowActionSetting, WorkFlowConfigActionEntity> mappingExpression)
        {
            mappingExpression.ForMember(d => d.ActionConfig, expression => expression.MapFrom(s => JsonConvert.SerializeObject(s.ActionConfig)));
            mappingExpression.ReverseMap().ForMember(d => d.ActionConfig, expression => expression.MapFrom(s => JsonConvert.DeserializeObject<dynamic>(s.ActionConfig)));
        }
    }

}
