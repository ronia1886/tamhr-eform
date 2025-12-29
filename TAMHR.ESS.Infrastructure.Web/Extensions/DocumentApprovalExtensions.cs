using TAMHR.ESS.Infrastructure.Web.Authorization;

namespace TAMHR.ESS.Infrastructure.Web.Extensions
{
    public static class DocumentApprovalExtensions
    {
        public static bool CanViewApprovalHistories(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.ViewApprovalHistories", false);
        }

        public static bool CanCreateConversation(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.CreateConversation", false);
        }

        public static bool CanViewConversation(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.ViewConversation", false);
        }

        public static bool CanViewAction(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.ViewAction", false);
        }

        public static bool CanEdit(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.Edit", false);
        }

        public static bool CanApprove(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.Approve", false);
        }

        public static bool CanReject(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.Reject", false);
        }

        public static bool CanRevise(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.Revise", false);
        }

        public static bool CanUpload(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.Upload", false);
        }

        public static bool CanSubmit(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.Submit", false);
        }

        public static bool CanCancel(this AclHelper aclHelper)
        {
            return aclHelper.HasPermission("Core.Approval.Cancel", false);
        }
    }
}
