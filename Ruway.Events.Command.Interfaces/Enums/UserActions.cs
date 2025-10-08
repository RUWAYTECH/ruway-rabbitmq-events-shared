namespace Ruway.Events.Command.Interfaces.Enums
{
    public enum UserActions
    {
         None = 0,
        Created = 1,
        Updated = 2,
        Deleted = 4,
        All = Created | Updated | Deleted
    }
}