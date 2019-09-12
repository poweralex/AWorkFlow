using AutoMapper;
using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;

namespace Mcs.SF.WorkFlow.Api.Models.Statuses
{
    /// <summary>
    /// data model for search status
    /// </summary>
    [MapsToStatusEntity]
    public class StatusResponse
    {
        /// <summary>
        /// target id
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// qty related to this status
        /// </summary>
        public int? Qty { get; set; }
        /// <summary>
        /// when this status posted
        /// </summary>
        public DateTime? PostTime { get; set; }
        /// <summary>
        /// who posted this status
        /// </summary>
        public string PostUser { get; set; }
    }

    /// <summary>
    /// extra mapping setting
    /// </summary>
    public class MapsToStatusEntity : MapsToAttribute
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MapsToStatusEntity() : base(typeof(StatusEntity)) { }

        /// <summary>
        /// mapping
        /// </summary>
        /// <param name="mappingExpression"></param>
        public void ConfigureMapping(IMappingExpression<StatusResponse, StatusEntity> mappingExpression)
        {
            mappingExpression
                .ForMember(d => d.CreatedAt, expression => expression.MapFrom(s => s.PostTime))
                .ForMember(d => d.CreatedBy, expression => expression.MapFrom(s => s.PostUser));
            mappingExpression.ReverseMap()
                .ForMember(d => d.PostTime, expression => expression.MapFrom(s => s.CreatedAt))
                .ForMember(d => d.PostUser, expression => expression.MapFrom(s => s.CreatedBy));

        }
    }
}
