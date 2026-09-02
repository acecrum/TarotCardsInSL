using CustomPlayerEffects;
using LabApi.Events;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Sun(Config config) : CustomCard
{
    public override string ID => "sun";
    public override string Name => "XIX | The Sun";
    public override CardType Type => CardType.Passive;
    public override ItemType KeycardType => ItemType.KeycardCustomManagement;
    public override (int A, int B, int C) CardPerms => (3, 3, 3);
    public override string TechnicalDescription => "<color=red>Disables attacking and using items.</color>\nIn return, creates a passive healing AOE for you and teammates,\nyou're also granted +35% speed and teammates get a 10% damage boost.";
    public override string Description => "Bring them hope.";
    public override Color GlowColor => Color.yellow;
    public override int SpawnWeight => config.FoolSpawnWeight;
    
    private readonly Dictionary<string, SavedEffects> _savedEffects = new();
    private readonly Dictionary<string, LabEventHandler<PlayerChangingItemEventArgs>> _changingItemHandler = new();
    private LightSourceToy? savedLight;
    private sealed class SavedEffects
    {
        public byte SpeedIntensity { get; init; }
        public float SpeedDuration { get; init; }
    }

    public override void OnGiven(Player player)
    { 
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();
        
        _savedEffects[player.UserId] = new SavedEffects
        {
            SpeedIntensity = existingSpeed?.Intensity ?? 0,
            SpeedDuration = existingSpeed?.TimeLeft ?? 0,
        };
        
        Timing.RunCoroutine(RemoveHeldItem(player));
        
        return;
        
        IEnumerator<float> RemoveHeldItem(Player player)
        {
            while (player.CurrentItem != null)
            {
                player.CurrentItem = null;

                yield return Timing.WaitForSeconds(0.1f);
            }
            
            _changingItemHandler[player.UserId] = DenyItemSwap;
            PlayerEvents.ChangingItem += DenyItemSwap;
            
            var light = LightSourceToy.Create(player.GameObject?.transform, networkSpawn: false);
            light.Color = GlowColor;
            light.Intensity = 1.5f;
            light.Range = 5f;
            light.Spawn();
            savedLight = light;
            
            player.EnableEffect<MovementBoost>(35);
        }
        
        void DenyItemSwap(PlayerChangingItemEventArgs ev)
        {
            if (ev.Player !=  player) return;

            ev.IsAllowed = false;
        }
    }
    public override void OnRemoved(Player player)
    {
        if (!_savedEffects.TryGetValue(player.UserId, out var saved)) return;
        if (!_changingItemHandler.TryGetValue(player.UserId, out var handler)) return;
        player.EnableEffect<MovementBoost>(saved.SpeedIntensity, saved.SpeedDuration);
        _savedEffects.Remove(player.UserId);
        
        PlayerEvents.ChangingItem -= handler;
        savedLight?.Destroy();
    }
}