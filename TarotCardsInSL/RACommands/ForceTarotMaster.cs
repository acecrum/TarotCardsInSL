using CommandSystem;
using LabApi.Features.Wrappers;

namespace TarotCardsInSL.RACommands;
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ForceTarotMaster : ICommand, IUsageProvider
{
    public string Command => "forcetarotmaster";
    public string[] Aliases => ["ftm"];
    public string Description => "Forces you or a specified PlayerID into a Tarot Master (Turns the executer into a Tarot Master if no PlayerID is provided)";
    public string[] Usage => ["PlayerID"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var commandsender = Player.Get(sender);
        if (!sender.CheckPermission(PlayerPermissions.ForceclassWithoutRestrictions))
        {
            switch (UnityEngine.Random.Range(0, 4))
            {
                case 0:
                    response = "Obama chuckled. You mean the Chaos Emeralds?";
                    return false;
                case 1:
                    response = "made a player Tarot Ma- nvm i lied";
                    return false;
                case 2:
                    response = "come back when you have more permissions!";
                    return false;
                case 3:
                    response = "looking at the failed command message fills you with determination.\naccess is still denied tho";
                    return false;
            }
        }
        
        switch (arguments.Count)
        {
            case 0:
                if (TarotPlugin.SubclassManager != null && commandsender != null && TarotPlugin.SubclassManager.IsTarotMaster(commandsender))
                {
                    response = "You're already a Tarot Master!";
                    return false;
                }

                if (commandsender != null) TarotPlugin.SubclassManager?.AssignMaster(commandsender);
                response = "Forced yourself into a Tarot Master.";
                return true;
            case > 1:
                response = "Only one argument is allowed";
                return false;
        }
        
        if (!int.TryParse(arguments.At(0), out var playerID))
        {
            response = "Invalid player ID.";
            return false;
        }
        var target = Player.Get(playerID);

        if (target == null)
        {
            response = "Player not found with supplied ID.";
            return false;
        }
        
        if (target != null && TarotPlugin.SubclassManager != null && TarotPlugin.SubclassManager.IsTarotMaster(target))
        {
            response = $"{target.DisplayName} is already a Tarot Master!";
            return false;
        }
        
        if (target != null) TarotPlugin.SubclassManager?.AssignMaster(target);
        response = $"Set {target?.DisplayName} as Tarot Master!";
        return true;
    }
}