using CommandSystem;
using LabApi.Features.Wrappers;
using TarotCardsInSL.Cards;
using UnityEngine;

namespace TarotCardsInSL.RACommands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SpawnCard : ICommand, IUsageProvider
{
    public string Command => "spawncard";
    public string[] Aliases => ["sp"];
    public string Description => "Spawns a Tarot Card";
    public string[] Usage => ["CardName"];

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var commandsender = Player.Get(sender);
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
                    response = "You can't spawn a Tarot Card.";
                    return false;
                case 3:
                    response = $"I'm sorry {sender.LogName}, I'm afraid I can't do that.";
                    return false;
            }
        }

        switch (arguments.Count)
        {
            case 0:
                response = "spawncard list/Cardname";
                return false;
            case > 1:
                response = "Only one argument is allowed";
                return false;
        }

        if (arguments.At(0).Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            var cards = TarotPlugin.CardManager.RegisteredCards;

            response = $"Registered cards:\n" + string.Join("\n", cards.Select(customCard => $"{customCard.ID}"));
            return true;
        }
        
        if (arguments.At(0).Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var cards = TarotPlugin.CardManager.RegisteredCards.ToList();

            if (cards.Count == 0)
            {
                response = "no. (no cards are registered)";
                return false;
            }
            const float radius = 1.5f;
            var angleStep = 360f/cards.Count;

            for (var i = 0; i < cards.Count; i++)
            {
                var angle = angleStep * i * Mathf.Deg2Rad;
                var offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle)  * radius);

                if (commandsender != null)
                {
                    var spawnPosition = commandsender.Position + offset;
                
                    TarotPlugin.CardManager.SpawnCard(cards[i], spawnPosition);
                }
            }
            
            response = "you're making a mistake";
            return true;
        }

        if (!TarotPlugin.CardManager.TryGetCard(arguments.At(0), out CustomCard card))
        {
            response = "Not a valid card name.";
            return false;
        }

        if (commandsender != null) TarotPlugin.CardManager.SpawnCard(card, commandsender.Position);
        response = $"Spawned {card.Name}!";
        return true;
    }
}