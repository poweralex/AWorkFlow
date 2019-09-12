namespace Mcs.SF.WorkFlow.Api.Models
{
    /// <summary>
    /// messages
    /// </summary>
    internal static class Messages
    {
        // list controller
        internal static readonly SFMessage ListCategoryFailed = new SFMessage { Code = "5200140001", Message = "List all categories failed." };

        // config controller
        internal static readonly SFMessage InvalidWorkFlowCategory = new SFMessage { Code = "4000140001", Message = "Invalid workflow category request." };
        internal static readonly SFMessage WorkFlowCategoryInUse = new SFMessage { Code = "4000140004", Message = "WorkFlow category is in use.." };
        internal static readonly SFMessage CreateWorkFlowCategoryFailed = new SFMessage { Code = "5200140002", Message = "Create workflow category failed." };
        internal static readonly SFMessage RemoveWorkFlowCategoryFailed = new SFMessage { Code = "5200140003", Message = "Remove workflow category failed." };
        internal static readonly SFMessage SearchWorkFlowCategoryFailed = new SFMessage { Code = "5200140004", Message = "Search workflow category failed." };
        internal static readonly SFMessage InvalidWorkFlow = new SFMessage { Code = "4000140002", Message = "Invalid workflow category." };
        internal static readonly SFMessage CreateWorkFlowFailed = new SFMessage { Code = "5200140005", Message = "Create workflow failed." };
        internal static readonly SFMessage RemoveWorkFlowFailed = new SFMessage { Code = "5200140006", Message = "Remove workflow failed." };
        internal static readonly SFMessage SearchWorkFlowFailed = new SFMessage { Code = "5200140007", Message = "Search workflow failed." };
        internal static readonly SFMessage InvalidGetWorkFlowRequest = new SFMessage { Code = "4000140003", Message = "Invalid get workflow request." };
        internal static readonly SFMessage WorkFlowNotExisted = new SFMessage { Code = "5200140008", Message = "WorkFlow not existed." };

        // working controller
        internal static readonly SFMessage StartNewWorkFailed = new SFMessage { Code = "5200140009", Message = "Start new work failed." };
        internal static readonly SFMessage WorkingCopyNotExisted = new SFMessage { Code = "5200140010", Message = "WorkingCopy not existed." };
        internal static readonly SFMessage WorkingCopyStepNotExisted = new SFMessage { Code = "5200140011", Message = "Working step not existed." };
        internal static readonly SFMessage WorkingCopyStepAlreadyFinished = new SFMessage { Code = "5200140012", Message = "Working step already finished." };
        internal static readonly SFMessage WorkingCopyAlreadyFinished = new SFMessage { Code = "5200140013", Message = "Working copy already finished." };
        internal static readonly SFMessage WorkingCopyAlreadyCancelled = new SFMessage { Code = "5200140023", Message = "Working copy already cancelled." };
        internal static readonly SFMessage UpdateWorkingCopyFailed = new SFMessage { Code = "5200140014", Message = "Update workingcopy failed." };
        internal static readonly SFMessage ExecuteActionFailed = new SFMessage { Code = "5200140015", Message = "Execute action failed." };
        internal static readonly SFMessage BeginStepNotFound = new SFMessage { Code = "5200140016", Message = "Begin step not found." };
        internal static readonly SFMessage MultipleBeginStepFound = new SFMessage { Code = "5200140017", Message = "Multiple begin step found." };
        internal static readonly SFMessage EndStepNotFound = new SFMessage { Code = "5200140018", Message = "End step not found." };
        internal static readonly SFMessage InvalidActionSequences = new SFMessage { Code = "5200140019", Message = "Some of action sequences invalid." };
        internal static readonly SFMessage WorkingStepAlreadyFinished = new SFMessage { Code = "5200140020", Message = "The working step request to execute is already finished." };
        internal static readonly SFMessage WorkingStepAlreadyCancelled = new SFMessage { Code = "5200140021", Message = "The working step request to execute is already cancelled." };
        internal static readonly SFMessage WorkingStepNotExisted = new SFMessage { Code = "5200140022", Message = "The working step request to execute is not existed." };
        internal static readonly SFMessage PendingNextRun = new SFMessage { Code = "5200140024", Message = "Execute completed, but not finished successfully, pending next run." };
        internal static readonly SFMessage ExecuteActionSucceed = new SFMessage { Code = "5200140025", Message = "Execute action succeeded." };
        internal static readonly SFMessage PostMessageFailed = new SFMessage { Code = "5200140026", Message = "Post status failed." };
        internal static readonly SFMessage SearchConditionRequired = new SFMessage { Code = "4000140005", Message = "Search condition required." };
        internal static readonly SFMessage SearchMessageFailed = new SFMessage { Code = "5200140027", Message = "Search status failed." };
    }

    /// <summary>
    /// SmartFactory message struct
    /// </summary>
    public struct SFMessage
    {
        /// <summary>
        /// message code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// message body
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// returns "{Code}: {Message}"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Code}: {Message}";
        }
    }
}
