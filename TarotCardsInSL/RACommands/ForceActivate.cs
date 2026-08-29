using CommandSystem;
using LabApi.Features.Wrappers;
using TarotCardsInSL.Cards;

namespace TarotCardsInSL.RACommands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ForceActivate : ICommand, IUsageProvider
{
    public string Command => "forceactivate";
    public string[] Aliases => ["fa"];
    public string Description => "Forces a players card to be activated.";
    public string[] Usage => ["PlayerID"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.GivingItems))
        {
            switch (UnityEngine.Random.Range(0, 4))
            {
                case 0:
                    response = "weirdo.";
                    return false;
                case 1:
                    response = "weird to force people into doing things, no?";
                    return false;
                case 2:
                    response = "You can't use this command";
                    return false;
                case 3:
                    response = "tell them to press their activate key";
                    return false;
            }
        }
        switch (arguments.Count)
        {
            case 0:
                response = "forceactivate (PlayerID)";
                return false;
            case > 2:
                response = "Only one arguments is allowed";
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

        if (card.Type == CardType.Passive)
        {
            response = $"{targetPlayer.DisplayName} is holding a passive card! ({card.Name})";
            return false;
        }

        TarotPlugin.CardManager.ActivateCard(targetPlayer);
        response = $"Activated {targetPlayer.DisplayName}'s {card.Name}";
        return true;
    }
}