using CommandSystem;
using LabApi.Features.Wrappers;

namespace TarotCardsInSL.RACommands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class HasCard : ICommand, IUsageProvider
{
    public string Command => "hascard";
    public string[] Aliases => ["hc"];
    public string Description => "Gets what card a player has if they have one";
    public string[] Usage => ["PlayerID"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.PlayerSensitiveDataAccess))
        {
            switch (UnityEngine.Random.Range(0, 4))
            {
                case 0:
                    response = "it's rude to stare, y'know?";
                    return false;
                case 1:
                    response = "nosy, arentcha?";
                    return false;
                case 2:
                    response = "they might have a card, they might not idk";
                    return false;
                case 3:
                    response = "you dont have permission to use this command";
                    return false;
            }
            
        }
        switch (arguments.Count)
        {
            case 0:
                response = "hascard [PlayerID]";
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
            response = "Player with supplied ID not found.";
            return false;
        }

        if (!TarotPlugin.CardManager.TryGetHeldCard(targetPlayer, out var card))
        {
            response = $"{targetPlayer.DisplayName} is not holding a card!";
            return false;
        }
        
        response = $"{targetPlayer.DisplayName} has {card.Name}!";
        return true;
    }
}