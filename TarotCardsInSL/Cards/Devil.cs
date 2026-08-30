using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using RueI.API;
using RueI.API.Elements;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Devil(Config config) : CustomCard
{
    public override string ID => "devil";
    public override string Name => "XV | The Devil";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomMetalCase;
    public override (int A, int B, int C) CardPerms => (2, 0, 1);
    public override string TechnicalDescription => "On activation, starts a timer that <color=red>kills you when it reaches 0.</color>\nYou deal 1.5x damage and recieve a 35% Movement Boost\nKilling players regenerates HP and grants further buffs.";
    public override string Description => "Revel in the power of darkness";
    public override Color GlowColor => new Color32(119, 63, 73, 255);
    public override int SpawnWeight => config.DevilSpawnWeight;


    public override void Activate(Player player)
    {
        player.EnableEffect<MovementBoost>(35);
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();
        var oldSpeedIntensity = existingSpeed?.Intensity ?? 0;
        var existingDR = player.ActiveEffects.OfType<DamageReduction>().FirstOrDefault();
        var oldDRIntensity = existingDR?.Intensity ?? 0;
        var timer = 45f;
        var timerActive = true;
        
        Timing.RunCoroutine(Timer());
        
        PlayerEvents.Death += OnDeath;
        PlayerEvents.Hurting += anotherdamagemultwhocouldveguessed;
        return;

        void OnDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player == player)
            {
                PlayerEvents.Death -= OnDeath;
            }
            if (ev.Attacker != player) return;
            
            timer += 7f;
            player.EnableEffect<MovementBoost>((byte)(oldSpeedIntensity + 5));
            oldSpeedIntensity += 5;
            player.EnableEffect<DamageReduction>((byte)(oldDRIntensity + 10));
            oldDRIntensity += 10;
        }

        void anotherdamagemultwhocouldveguessed(PlayerHurtingEventArgs ev)
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
        
        IEnumerator<float> Timer()
        {
            while (timerActive && timer > 0f)
            {
                RueDisplay.Get(player).Show(new BasicElement(165, $"<align=right><color=red><b><size=24>Time left: {timer}</size></b></color></align>"), 1f);
                timer--;
                yield return Timing.WaitForSeconds(1f);
            }

            if (player.IsAlive)
            {
                player.Kill("Couldn't fulfill their promise in darkness.");
            }
        }
    }
}