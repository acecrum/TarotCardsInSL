using CustomPlayerEffects;
using LabApi.Events;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerStatsSystem;
using RueI.API;
using RueI.API.Elements;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Chariot(Config config) : CustomCard
{
    public override string ID => "chariot";
    public override string Name => "VII | The Chariot";
    public override CardType Type => CardType.Passive;
    public override ItemType KeycardType => ItemType.KeycardCustomMetalCase;
    public override (int A, int B, int C) CardPerms => (0, 0, 2);
    public override Color PermissionColor => Color.black;
    public override Color LabelColor => Color.black;
    public override string TechnicalDescription => "Grants a second chance at life as long as you kill the person who killed you.\n<color=red>You cannot deal damage to other players except your killer\nand you do not take damage.</color>";
    public override string Description => "May nothing stand before you.";
    public override Color GlowColor => new Color32(255, 217, 0, 255);
    public override int SpawnWeight => config.ChariotSpawnWeight;
    private readonly Dictionary<string, LabEventHandler<PlayerDyingEventArgs>> _dyingHandlers = new();
    
    public override void OnGiven(Player player)
    { 
        Player? savedAttacker;
        LightSourceToy? savedLight;
        var revengeSucceeded = false;
        
        _dyingHandlers[player.UserId] = OnDying;
        PlayerEvents.Dying += OnDying;
        return;

        void OnDying(PlayerDyingEventArgs ev)
        {
            if (ev.Player != player) return;
            if (ev.Attacker == null) return;
            
            savedAttacker = ev.Attacker;
            TarotPlugin.CardManager.DestroyPassive(player, this);
            ev.IsAllowed = false;
            
            var light = LightSourceToy.Create(player.GameObject?.transform, networkSpawn: false);
            light.Color = GlowColor;
            light.Intensity = 1f;
            light.Range = 1.5f;
            light.Spawn();
            savedLight = light;
            
            player.EnableEffect<MovementBoost>(50, 10);
            PlayerEvents.Dying -= OnDying;
            PlayerEvents.Hurting += DamageIgnoring;
            PlayerEvents.Death += OnDeath;
            
            Timing.RunCoroutine(RevengeTimer());
        }

        void DamageIgnoring(PlayerHurtingEventArgs DamageEv)
        {
            if (DamageEv.Player == player)
            {
                DamageEv.IsAllowed = false;
                return;
            }
            
            if (DamageEv.Attacker != player || DamageEv.DamageHandler is not StandardDamageHandler handler)
            {
                return;
            }

            if (DamageEv.Player != savedAttacker)
            {
                handler.Damage = 0f;
            }
        }

        void OnDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player != savedAttacker) return;
            if (ev.Attacker != player) return;

            revengeSucceeded = true;

            if (player.IsSCP)
            {
                player.Health = player.MaxHealth/5;
            }
            else
            {
                player.Health = player.MaxHealth;
            }
            
            PlayerEvents.Hurting -= DamageIgnoring;
            PlayerEvents.Death -= OnDeath;
            
            savedLight?.Destroy();
            player.DisableEffect<MovementBoost>();
        }

        IEnumerator<float> RevengeTimer()
        {
            var remaining = 10;
            if (savedAttacker.IsSCP)
            {
                remaining /= 2;
            }

            while (remaining > 0 && !revengeSucceeded)
            {
                if (savedAttacker == null) yield break;
                
                RueDisplay.Get(savedAttacker).Show(new BasicElement(250, $"<b><size=24><color=red>{player.DisplayName} is hunting you!\n{remaining}</color></size></b>"), 1f);
                
                RueDisplay.Get(player).Show(new BasicElement(250, $"<b><size=24><color=red>Kill {savedAttacker.DisplayName}!\n{remaining}</color></size></b>"), 1f);
                
                yield return Timing.WaitForSeconds(1f);
                remaining--;
            }
            if (revengeSucceeded) yield break;
            
            PlayerEvents.Hurting -= DamageIgnoring;
            PlayerEvents.Death -= OnDeath;
            savedLight?.Destroy();

            if (savedAttacker != null)
            {
                player.Kill($"Couldn't kill {savedAttacker.DisplayName} in time.");
            }
        }
    }
    public override void OnRemoved(Player player)
    {
        if (!_dyingHandlers.TryGetValue(player.UserId, out var handler)) return;
        PlayerEvents.Dying -= handler;
        _dyingHandlers.Remove(player.UserId);
    }
}