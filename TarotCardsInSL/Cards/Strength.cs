using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace TarotCardsInSL.Cards;

public sealed class Strength(Config config) : CustomCard
{
    public override string ID => "strength";
    public override string Name => "XI | Strength";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (3, 1, 0);
    public override string TechnicalDescription => "Increases damage by 50%, speed by 40%, and Hume by 30 for 40 seconds.";
    public override string Description => "May your power bring rage.";
    public override Color GlowColor => Color.red;
    public override int SpawnWeight => config.StrengthSpawnWeight;

    public override void Activate(Player player)
    {
        Logger.Info($"{player.DisplayName} ({player.UserId}) has begun using Strength.");
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();
        var oldSpeedIntensity = existingSpeed?.Intensity ?? 0;
        var oldSpeedDuration = existingSpeed?.TimeLeft ?? 0;
        player.EnableEffect<MovementBoost>((byte)(oldSpeedIntensity + 40), 40);
        var light = LightSourceToy.Create(player.GameObject?.transform, networkSpawn: false);
        light.Color = GlowColor;
        light.Intensity = 0.85f;
        light.Range = 0.75f;
        light.Spawn();
        
        if (player.IsSCP)
        {
            if (!Mathf.Approximately(player.HumeShield, player.MaxHumeShield))
            {
                player.HumeShield = player.MaxHumeShield;
            }
            player.HumeShield *= 1.2f;
        }
        else
        {
            player.HumeShield += 30f;
        }
        
        void OnDamaging(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker != player) return;
            if (ev.DamageHandler is not StandardDamageHandler damageHandler) return;
            switch (ev.Attacker.Role)
            {
                // for some reason 173, 049, and 106 attacks brick when modified
                case RoleTypeId.Scp173:
                    break;
                case RoleTypeId.Scp106 or RoleTypeId.Scp049 or RoleTypeId.Scp096:
                    return;
            }
            if (ev.Attacker.Role is RoleTypeId.Scp173 or RoleTypeId.Scp049 or RoleTypeId.Scp106) return;
            damageHandler.Damage *= 1.5f;
        }
        
        PlayerEvents.Hurting += OnDamaging;
        
        Timing.CallDelayed(40f, () =>
        {
            PlayerEvents.Hurting -= OnDamaging;
            light.Destroy();
            player.EnableEffect<MovementBoost>(oldSpeedIntensity, oldSpeedDuration);
            Logger.Info($"{player.DisplayName} ({player.UserId}) has finished using Strength.");
        });
    }
}