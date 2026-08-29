using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Inventory;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;

namespace TarotCardsInSL.RACommands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AddCard : ICommand, IUsageProvider
{
    public string Command => "addcard";
    public string[] Aliases => ["ac"];
    public string Description => "Adds a Tarot card directly into a players inventory.";
    public string[] Usage => ["PlayerID","Card name"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.GivingItems))
        {
            switch (UnityEngine.Random.Range(0, 4))
            {
                case 0:
                    response = "find a card yourself.";
                    return false;
                case 1:
                    response = "Nope!";
                    return false;
                case 2:
                    response = "You can't add a Tarot Card.";
                    return false;
                case 3:
                    response = $"I'm sorry {sender.LogName}, I'm afraid I can't do that.";
                    return false;
            }
        }
        switch (arguments.Count)
        {
            case 0:
                response = "addcard (PlayerID) (CardName)\nUse 'addcard list' for a list of all cards";
                return false;
            case > 2:
                response = "Only two arguments is allowed";
                return false;
        }
        
        if (arguments.At(0).Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            var cards = TarotPlugin.CardManager.RegisteredCards;

            response = $"Registered cards:\n" + string.Join("\n", cards.Select(customCard => $"{customCard.ID}"));
            return true;
        }

        if (!int.TryParse(arguments.At(0), out var playerID))
        {
            response = "Invalid player ID.";
            return false;
        }

        if (!TarotPlugin.CardManager.TryGetCard(arguments.At(1), out var card))
        {
            response = "Not a valid card name.";
            return false;
        }
        
        var targetPlayer = Player.Get(playerID);
        if (targetPlayer == null)
        {
            response = "Player not found.";
            return false;
        }

        TarotPlugin.CardManager.TryGiveCard(targetPlayer, card);
        response = $"Added {card.Name} to {targetPlayer.DisplayName}!";
        return true;
    }
}