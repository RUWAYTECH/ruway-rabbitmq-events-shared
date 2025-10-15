using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruway.Events.Command.Interfaces.Constants
{
    public class EventConstants
    {
         public struct UserEvents
        {
            public const string UserCreated = "security.user.events.created";
            public const string UserUpdated = "security.user.events.updated";
            public const string UserDeleted = "security.user.events.deleted";
            public const string UserRoleAssigned = "security.user.events.role_assigned";
        }
        public struct EmployeeEvents
        {
            public const string EmployeeCreated = "memos.employee.events.created";
            public const string EmployeeUpdated = "memos.employee.events.updated";
            public const string EmployeeDeleted = "memos.employee.events.deleted";
        }
    }
}