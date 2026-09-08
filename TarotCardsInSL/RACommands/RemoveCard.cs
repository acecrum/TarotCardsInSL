using CommandSystem;
using LabApi.Features.Wrappers;

namespace TarotCardsInSL.RACommands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class RemoveCard : ICommand, IUsageProvider
{
    public string Command => "removecard";
    public string[] Aliases => ["rmc"];
    public string Description => "Removes a player's Tarot Card if they have one";
    public string[] Usage => ["PlayerID"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.GivingItems))
        {
            switch (UnityEngine.Random.Range(0, 4))
            {
                case 0:
                    response = "ask someone to drop their card";
                    return false;
                case 1:
                    response = "thats rude to do";
                    return false;
                case 2:
                    response = "You can't remove a Tarot Card.";
                    return false;
                case 3:
                    response = $"I'm sorry {sender.LogName}, I'm afraid I can't do that.";
                    return false;
            }
        }
        switch (arguments.Count)
        {
            case 0:
                response = "removecard (PlayerID)";
                return false;
            case > 1:
                response = "Only one argument is allowed";
                return false;
        }

        if (!int.TryParse(arguments.At(0), out var playerID))
        {
            response = "Invalid player ID.";
            return false;
        }
        
        var targetPlayer = Player.Get(playerID);
        if (targetPlayer == null)
        {
            response = "Player not found.";
            return false;
        }
        
        if (!TarotPlugin.CardManager.TryGetHeldCard(targetPlayer, out var card))
        {
            response = $"{targetPlayer.DisplayName} is not holding a card!";
            return false;
        }

        TarotPlugin.CardManager.RemoveCard(targetPlayer, card);
        response = $"Removed {card.Name} from {targetPlayer.DisplayName}";
        return true;
    }
}