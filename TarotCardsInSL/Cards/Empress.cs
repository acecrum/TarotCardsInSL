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

public sealed class Empress(Config config) : CustomCard
{
    public override string ID => "empress";
    public override string Name => "III | The Empress";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomManagement;
    public override (int A, int B, int C) CardPerms => (3, 1, 0);
    public override string TechnicalDescription => "Increases damage by 20% and speed by 20% for 15 seconds.\nGrants 25% lifesteal.";
    public override string Description => "May your rage bring power.";
    public override Color GlowColor => new Color32(255, 89, 89, 255);
    public override int SpawnWeight => config.PriestessSpawnWeight;


    public override void Activate(Player player)
    {
        Logger.Info($"{player.DisplayName} ({player.UserId}) has begun using Empress.");
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();
        var oldSpeedIntensity = existingSpeed?.Intensity ?? 0;
        var oldSpeedDuration = existingSpeed?.TimeLeft ?? 0;
        player.EnableEffect<MovementBoost>((byte)(oldSpeedIntensity + 20), 150);


        // subscribing and unsubscribing after 15 seconds
        PlayerEvents.Hurting += OnDamaging;
        
        Timing.CallDelayed(15f, () =>
        {
            PlayerEvents.Hurting -= OnDamaging;
            player.EnableEffect<MovementBoost>(oldSpeedIntensity, oldSpeedDuration);
            Logger.Info($"{player.DisplayName} ({player.UserId}) has finished using Empress.");
        });
        return;

        void OnDamaging(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker != player) return;
            if (ev.DamageHandler is not StandardDamageHandler damageHandler) return;
            switch (ev.Attacker.Role)
            {
                // for some reason 173, 049, and 106 attacks brick when modified
                case RoleTypeId.Scp173:
                    player.Heal(ev.Player.Health/4);
                    break;
                case RoleTypeId.Scp106 or RoleTypeId.Scp049:
                    player.Heal(damageHandler.Damage/4);
                    break;
            }
            if (ev.Attacker.Role is RoleTypeId.Scp173 or RoleTypeId.Scp049 or RoleTypeId.Scp106) return;
            damageHandler.Damage *= 1.2f;
            player.Heal(damageHandler.Damage/4);
        }
    }
}