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
    public override (int A, int B, int C) CardPerms => (1, 2, 1);
    public override string TechnicalDescription => "On activation, starts a timer that <color=red>kills you when it reaches 0.</color>\nYou deal 1.5x damage and recieve a 35% Movement Boost\nKilling players regenerates HP and grants further buffs.";
    public override string Description => "Deal with the Devil.";
    public override Color GlowColor => new Color32(119, 63, 73, 255);
    public override int SpawnWeight => config.DevilSpawnWeight;


    public override void Activate(Player player)
    {
        player.EnableEffect<MovementBoost>(35);
        var timer = 25f;
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
                Timer().Dispose();
            }
            if (ev.Attacker != player) return;
            
            timer = Mathf.Min(timer + 7f, 60f);

            var currentSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();
            var currentDR = player.ActiveEffects.OfType<DamageReduction>().FirstOrDefault();

            var speedIntensity = currentSpeed?.Intensity ?? 0;
            var drIntensity = currentDR?.Intensity ?? 0;

            var newSpeed = Mathf.Min(speedIntensity + 5, 100);
            var newDR = Mathf.Min(drIntensity + 10, 150);

            player.EnableEffect<MovementBoost>((byte)newSpeed);
            player.EnableEffect<DamageReduction>((byte)newDR);

            if (!Mathf.Approximately(player.Health, player.MaxHealth))
            {
                player.AddRegeneration(25, 2.5f);
            }
            else
            {
                player.HumeShield += 25f;
            }
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

            if (player is { IsAlive: true, IsSCP: false })
            {
                player.Kill("Couldn't fulfill their promise in darkness.");
            }
            else if (player.IsSCP)
            {
                player.DisableEffect<MovementBoost>();
                player.DisableEffect<DamageReduction>();
                player.Health = player.MaxHealth/2;
            }
        }
    }
}