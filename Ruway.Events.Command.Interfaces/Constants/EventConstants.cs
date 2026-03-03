
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

        public struct StoreEvents
        {
            public const string StoreCreated = "memos.store.events.created";
            public const string StoreUpdated = "memos.store.events.updated";
            public const string StoreDeleted = "memos.store.events.deleted";
        }

        public struct EnterpriseEvents
        {
            public const string EnterpriseCreated = "memos.enterprise.events.created";
            public const string EnterpriseUpdated = "memos.enterprise.events.updated";
            public const string EnterpriseDeleted = "memos.enterprise.events.deleted";
        }

        public struct PeopleEvents
        {
            public const string PeopleCreated = "async.people.events.created";
            public const string PeopleUpdated = "async.people.events.updated";
            public const string PeopleDeleted = "async.people.events.deleted";
        }
    }
}