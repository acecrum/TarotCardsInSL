using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using TarotCardsInSL.Cards;
using TarotCardsInSL.UI;
using UnityEngine;
using LightSourceToy = LabApi.Features.Wrappers.LightSourceToy;

namespace TarotCardsInSL;

public class CardManager(Config config)
{
    private readonly Dictionary<string, CustomCard> _registeredCards = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CustomCard> _heldCard = new();

    public IEnumerable<CustomCard> RegisteredCards => _registeredCards.Values;
    
    public void RegisterCard(CustomCard card)
    {
        _registeredCards.Add(card.ID, card);
    }

    private void SetupCardPickup(CustomCard card, Pickup pickup)
    {
        var data = pickup.GameObject.AddComponent<Groundcard>();
        data.Card = card;
        var randomY = UnityEngine.Random.Range(-50f, 50f);
        pickup.GameObject.transform.rotation = Quaternion.Euler(0f, randomY, 0f);
        data.Card = card;
        var light = LightSourceToy.Create(pickup.GameObject.transform, networkSpawn: false);
        light.Color = card.GlowColor;
        light.Intensity = card.GlowIntensity;
        light.Range = card.GlowRange;
        if (config.EnableCardLights)
        {
            light.Spawn();
        }
        data.Light = light;
        var interactable = InteractableToy.Create(Vector3.zero, Quaternion.identity, new Vector3(0.2f, 0.03f, 0.13f), pickup.GameObject.transform);
        var interactionTime = 0f;
        if (!config.EnableScpPickUp)
        {
            interactionTime = 2.7f;
        }
        interactable.InteractionDuration = interactionTime;
        interactable.IsLocked = false;
        interactable.OnInteracted += player =>
        {
            if (!player.IsSCP) return;
            if (!TryGiveCard(player, card)) return;
            pickup.Destroy();
        };
    }
    
    public Pickup SpawnCardInLocker(CustomCard card, LockerChamber chamber)
    {
        var pickup = chamber.AddItem(card.KeycardType);
        SetupCardPickup(card, pickup);
        return pickup;
    }

    public Pickup? SpawnCard(CustomCard card, Vector3 position)
    {
        var permissions = new KeycardLevels(card.CardPerms.A, card.CardPerms.B, card.CardPerms.C);
        var host = Player.Host;
        if (host == null) return null;
        
        KeycardItem? keycard = card.KeycardType switch
        {
            ItemType.KeycardCustomSite02 =>
                KeycardItem.CreateCustomKeycardSite02(host, card.Name, "_", card.Name, permissions, card.GlowColor, card.PermissionColor, card.LabelColor, 0),
            ItemType.KeycardCustomTaskForce =>
                KeycardItem.CreateCustomKeycardTaskForce(host, card.Name, card.Name, permissions, card.GlowColor, card.PermissionColor, "0", 0),
            ItemType.KeycardCustomMetalCase =>
                KeycardItem.CreateCustomKeycardMetal(host, card.Name, "_", card.Name, permissions, card.GlowColor, card.PermissionColor, card.LabelColor, 0, "0"),
            ItemType.KeycardCustomManagement =>
                KeycardItem.CreateCustomKeycardManagement(host, card.Name, card.Name, permissions, card.GlowColor, card.PermissionColor, card.LabelColor),
            _ => null
        };
        var pickup = keycard?.DropItem();
        if (pickup == null) return null;
        
        pickup.Position = position;
        
        SetupCardPickup(card, pickup);
        return pickup;
    }
    
    public bool HasCard(Player player)
    {
        return _heldCard.ContainsKey(player.UserId);
    }

    public bool TryGiveCard(Player player, CustomCard card)
    {
        if (_heldCard.ContainsKey(player.UserId))
        {
            DropCard(player, player.Position);
        }
        _heldCard.Add(player.UserId, card);
        card.OnGiven(player);
        TarotHints.ClearPickupCardHint(player);
        TarotHints.ShowPickUpHint(player, card, config);
        TarotHints.ShowHeldCardHint(player, card);
        return true;
    }
    
    public bool TryGetCard(string name, out CustomCard card)
    {
        return _registeredCards.TryGetValue(name, out card);
    }

    public void RemoveCard(Player player, CustomCard card)
    {
        card.OnRemoved(player);
        _heldCard.Remove(player.UserId);
        TarotHints.HideHeldCardHint(player);
    }

    public bool DestroyPassive(Player player, CustomCard expectedCard)
    {
        if (!_heldCard.TryGetValue(player.UserId, out var card)) return false;
        if (card != expectedCard) return false;
        card.OnRemoved(player);
        _heldCard.Remove(player.UserId);
        TarotHints.HideHeldCardHint(player);
        return true;
    }

    public void ActivateCard(Player player)
    {
        if (!_heldCard.TryGetValue(player.UserId, out var card)) return;
        if (card.Type != CardType.Active) return;
        _heldCard.Remove(player.UserId);
        TarotHints.HideHeldCardHint(player);
        card.Activate(player);
    }

    public void DropCard(Player player, Vector3 position)
    {
        if (!_heldCard.TryGetValue(player.UserId, out var card)) return;
        if (card.KeepOnDeath) return;
        card.OnRemoved(player);
        SpawnCard(card, position);
        _heldCard.Remove(player.UserId);
        TarotHints.HideHeldCardHint(player);
    }

    public CustomCard? GetRandomCard()
    {
        var totalWeight = _registeredCards.Values.Where(card => card.SpawnWeight > 0).Sum(card => card.SpawnWeight);
        if (totalWeight <= 0) return null;

        var random = UnityEngine.Random.Range(0, totalWeight);
        foreach (var card in _registeredCards.Values.Where(card => card.SpawnWeight > 0))
        {
            if (random < card.SpawnWeight) return card;
            random -= card.SpawnWeight;
        }
        return null;
    }

    public bool TryGetHeldCard(Player player, out CustomCard card)
    {
        return _heldCard.TryGetValue(player.UserId, out card);
    }

    public bool RefundCard(Player player, CustomCard card)
    {
        if (_heldCard.ContainsKey(player.UserId)) return false;

        _heldCard[player.UserId] = card;
        TarotHints.ShowHeldCardHint(player, card);
        TarotHints.CardFailHint(player);
        return true;
    }
    
    public void Clear()
    {
        _heldCard.Clear();
    }
}