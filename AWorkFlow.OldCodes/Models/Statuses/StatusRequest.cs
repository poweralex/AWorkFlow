using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Mcs.SF.WorkFlow.Api.Models.Statuses
{
    /// <summary>
    /// data model for status
    /// </summary>
    [MapsTo(typeof(StatusEntity), ReverseMap = true)]
    public class StatusRequest
    {
        /// <summary>
        /// TargetId
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TargetId { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Status { get; set; }
        /// <summary>
        /// Qty
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Qty { get; set; }
    }
}
