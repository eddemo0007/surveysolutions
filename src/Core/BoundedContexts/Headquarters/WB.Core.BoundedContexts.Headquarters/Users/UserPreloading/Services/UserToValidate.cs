using System;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class UserToValidate
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsArchived { get; set; }
        public Guid? SupervisorId { get; set; }
        public bool IsSupervisor { get; set; }
        public bool IsInterviewer { get; set; }
    }
}