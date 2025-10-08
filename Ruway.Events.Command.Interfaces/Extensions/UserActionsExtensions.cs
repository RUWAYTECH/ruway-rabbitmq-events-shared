using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ruway.Events.Command.Interfaces.Enums;

namespace Ruway.Events.Command.Interfaces.Extensions
{
    public static class UserActionsExtensions
{
    /// <summary>
    /// Convierte el enum UserActions a string separado por comas
    /// </summary>
    /// <param name="actions">Las acciones a convertir</param>
    /// <returns>String con las acciones separadas por comas</returns>
    public static string ToCommaSeparatedString(this UserActions actions)
    {
        if (actions == UserActions.None)
            return string.Empty;

        var actionList = new List<string>();

        if (actions.HasFlag(UserActions.Created))
            actionList.Add("Created");
        
        if (actions.HasFlag(UserActions.Updated))
            actionList.Add("Updated");
        
        if (actions.HasFlag(UserActions.Deleted))
            actionList.Add("Deleted");

        return string.Join(",", actionList);
    }

    /// <summary>
    /// Convierte un string separado por comas a UserActions enum
    /// </summary>
    /// <param name="actionsString">String con acciones separadas por comas</param>
    /// <returns>El enum UserActions correspondiente</returns>
    public static UserActions FromCommaSeparatedString(string actionsString)
    {
        if (string.IsNullOrWhiteSpace(actionsString))
            return UserActions.None;

        var actions = UserActions.None;
        var actionsList = actionsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(a => a.Trim())
                                    .ToArray();

        foreach (var action in actionsList)
        {
            if (Enum.TryParse<UserActions>(action, true, out var parsedAction))
            {
                actions |= parsedAction;
            }
        }

        return actions;
    }

    /// <summary>
    /// Convierte el enum a array de strings
    /// </summary>
    /// <param name="actions">Las acciones a convertir</param>
    /// <returns>Array de strings con las acciones</returns>
    public static string[] ToStringArray(this UserActions actions)
    {
        return actions.ToCommaSeparatedString()
                     .Split(',', StringSplitOptions.RemoveEmptyEntries)
                     .Select(a => a.Trim())
                     .ToArray();
    }
}
}