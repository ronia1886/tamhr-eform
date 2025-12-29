using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public Guid[] Roles { get; set; }
    }

    public class EmployeeViewModel
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string AvatarURL { get; set; }
    }
}
