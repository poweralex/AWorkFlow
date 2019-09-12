using AutoMapper.Attributes;
using Mcs.SF.WorkFlow.Api.Models.Entities;
using System;
using System.Collections.Generic;

namespace Mcs.SF.WorkFlow.Api.Models.Working
{
    /// <summary>
    /// data model for working copy
    /// </summary>
    [MapsTo(typeof(WorkingCopyEntity), ReverseMap = true)]
    public class WorkingCopy
    {
        /// <summary>
        /// working copy id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// workflow code
        /// </summary>
        public string WorkFlowCode { get; set; }
        /// <summary>
        /// workflow version
        /// </summary>
        public int? WorkFlowVersion { get; set; }
        /// <summary>
        /// begin time
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// is on-hold
        /// </summary>
        public bool OnHold { get; set; }
        /// <summary>
        /// on-hold time
        /// </summary>
        public DateTime? HoldTime { get; set; }
        /// <summary>
        /// release time
        /// </summary>
        public DateTime? ReleaseTime { get; set; }
        /// <summary>
        /// steps
        /// </summary>
        public List<WorkingCopyStep> Steps { get; set; }
    }
}