using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using TarotCardsInSL.Cards;
using TarotCardsInSL.Spawning;
using TarotCardsInSL.Subclasses;
using TarotCardsInSL.UI;
using UnityEngine;
using UserSettings.ServerSpecific;
using Logger = LabApi.Features.Console.Logger;

namespace TarotCardsInSL;

public sealed class EventHandlers(CardManager cardManager, TarotDatastoring dataStoring, SubclassManager subclassManager, Config config)
{
    private readonly NaturalSpawning _cardSpawner = new(cardManager);
    private readonly Dictionary<(ReferenceHub Hub, int SettingId), bool> _keybindStates = new();

    private SSKeybindSetting? _usecard;
    private SSKeybindSetting? _dropcard;

    public void Register()
    {
        _usecard = new SSKeybindSetting( "acecrum.TarotCardsInSL.UseCard".GetStableHashCode(), "Use Tarot Card", KeyCode.Z);
        _dropcard = new SSKeybindSetting("acecrum.TarotCardsInSL.DropCard".GetStableHashCode(), "Drop Tarot Card", KeyCode.O);
        ServerSpecificSettingsSync.DefinedSettings = [_usecard, _dropcard];
        ServerSpecificSettingsSync.SendToAll();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSettingReceived;
        PlayerEvents.PickingUpItem += OnPickingUpItem;
        PlayerEvents.Death += OnPlayerDeath;
        ServerEvents.RoundRestarted += OnRoundRestarted;
        ServerEvents.RoundStarted += OnRoundStarted;
        PlayerEvents.InteractingLocker += OnInteractingLocker;
        PlayerEvents.ChangedRole += OnChangedRole;
        PlayerEvents.Joined += OnJoined;
        PlayerEvents.Left += OnLeft;
        PlayerEvents.Dying += OnDying;
        ServerEvents.WaveRespawned += OnWaveRespawned;
        Scp049Events.ResurrectedBody += On049Resurrected;
        Scp914Events.ProcessingPickup += OnProcessingPickup;
    }

    public void Unregister()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSettingReceived;
        PlayerEvents.PickingUpItem -= OnPickingUpItem;
        ServerEvents.RoundRestarted -= OnRoundRestarted;
        ServerEvents.RoundStarted -= OnRoundStarted;
        PlayerEvents.Death += OnPlayerDeath;
        PlayerEvents.InteractingLocker -= OnInteractingLocker;
        PlayerEvents.ChangedRole -= OnChangedRole;
        PlayerEvents.Joined -= OnJoined;
        PlayerEvents.Left -= OnLeft;
        ServerEvents.WaveRespawned -= OnWaveRespawned;
        Scp049Events.ResurrectedBody -= On049Resurrected;
        Scp914Events.ProcessingPickup -= OnProcessingPickup;
    }

    private void OnSettingReceived(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
        var player = Player.Get(hub);
        var data = dataStoring.GetDnt(player);
        if (setting is not SSKeybindSetting keybind) return;
        
        if (!keybind.SyncIsPressed) return;

        if (_usecard != null && keybind.SettingId == _usecard.SettingId)
        {
            Logger.Info($"{hub.nicknameSync.MyNick} pressed Use");
            TarotPlugin.CardManager.ActivateCard(player);
            data.Disabled = true;
            dataStoring.Save(player, data);
            TarotHints.HideBigDumbassConfigHint(player);
            return;
        }

        if (_dropcard == null || keybind.SettingId != _dropcard.SettingId) return;
        Logger.Info($"{hub.nicknameSync.MyNick} pressed Drop");
        if (player.Role == RoleTypeId.Scp079) return;
        TarotPlugin.CardManager.DropCard(player, player.Position);
        data.Disabled = true;
        dataStoring.Save(player, data);
        TarotHints.HideBigDumbassConfigHint(player);
    }

    private void OnPickingUpItem(PlayerPickingUpItemEventArgs ev)
    {
        var groundCard = ev.Pickup.GameObject.GetComponent<Groundcard>();
        if (groundCard == null) return;
        
        ev.IsAllowed = false;
        if (!cardManager.TryGiveCard(ev.Player, groundCard.Card)) return;
        
        groundCard.Light.Destroy();
        ev.Pickup.Destroy();
    }

    private void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
        if (!cardManager.TryGetHeldCard(ev.Player, out var card)) return;
        if (!card.KeepOnDeath)
        {
            cardManager.DropCard(ev.Player, ev.OldPosition);
        }
    }

    private void OnDying(PlayerDyingEventArgs ev)
    {
        subclassManager.TarotMasterDropCards(ev.Player);
    }

    private void OnRoundStarted()
    {
        _cardSpawner.SpawnCards();
        _cardSpawner.PrepareLockers();
    }

    private void OnRoundRestarted()
    {
        cardManager.Clear();
        subclassManager.Clear();
    }

    private void OnInteractingLocker(PlayerInteractingLockerEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        if (!ev.CanOpen) return;
        _cardSpawner.SpawnLockerCard(ev.Chamber);
    }

    private void OnChangedRole(PlayerChangedRoleEventArgs ev)
    {
        TarotHints.ScpPickUpHint(ev.Player, config);
        switch (ev.ChangeReason)
        {
            case RoleChangeReason.Escaped when subclassManager.IsTarotMaster(ev.Player):
                ev.Player.MaxHealth *= 1.5f;
                ev.Player.Health = ev.Player.MaxHealth;
                return;
            default:
                subclassManager.Remove(ev.Player);
                return;
        }
    }

    private void OnJoined(PlayerJoinedEventArgs ev)
    {
        var data = dataStoring.GetDnt(ev.Player);
        if (!data.Disabled && config.EnableReminderHint)
        {
            TarotHints.BigDumbassConfigHint(ev.Player);
        }
    }

    private void OnLeft(PlayerLeftEventArgs ev)
    {
        var keys = _keybindStates.Keys.Where(x => x.Hub == ev.Player.ReferenceHub).ToList();
        foreach (var key in keys) _keybindStates.Remove(key);
    }

    private void OnProcessingPickup(Scp914ProcessingPickupEventArgs ev)
    {
        var groundCard = ev.Pickup.GameObject.GetComponent<Groundcard>();
        if (groundCard == null) return;
        
        ev.IsAllowed = false;
    }

    private void OnWaveRespawned(WaveRespawnedEventArgs ev)
    {
        foreach (var player in ev.Players)
        {
            subclassManager.TryAssignMaster(player);
        }
    }

    private void On049Resurrected(Scp049ResurrectedBodyEventArgs ev)
    {
        subclassManager.TryAssignMaster(ev.Target);
    }
}