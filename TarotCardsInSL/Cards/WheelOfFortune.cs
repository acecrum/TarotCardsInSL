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

public sealed class WheelOfFortune(Config config) : CustomCard
{
    public override string ID => "wheel";
    public override string Name => "X | Wheel of Fortune";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 0);
    public override string TechnicalDescription => "Spin the odds and find out...";
    public override string Description => "Spin the wheel of destiny";
    public override Color GlowColor => new Color32(227, 146, 87, 255);
    public override int SpawnWeight => config.WheelSpawnWeight;


    public override void Activate(Player player)
    {
        var display = RueDisplay.Get(player);

        switch (UnityEngine.Random.Range(0, 100))
        {
            case < 5:
                if (player.Team == Team.SCPs)
                {
                    display.Show(new BasicElement(200, "<b><size=30><color=red>Run.</color></size></b>"), 3f);
                    Timing.RunCoroutine(TimedLoop());
                    
                    IEnumerator<float> TimedLoop()
                    {
                        var explosions = 6;

                        while (explosions > 0)
                        {
                            TimedGrenadeProjectile.SpawnActive(player.Position, ItemType.GrenadeHE, player);
                            explosions--;
                            yield return Timing.WaitForSeconds(5);
                        }
                    }
                }
                else
                {
                    TimedGrenadeProjectile.SpawnActive(player.Position, ItemType.GrenadeHE, Player.Host, 0.01f);
                    player.Kill("https://www.youtube.com/watch?v=vpVFz_R3PeY");
                }
                return;
            case < 15:
                display.Show(new BasicElement(200, "<b><size=30><color=orange>Yikes.</color></size></b>"), 3f);
                if (player.Team == Team.SCPs)
                {
                    player.EnableEffect<SeveredEyes>(1, player.MaxHumeShield/10);
                }
                else
                {
                    player.MaxHealth *= 0.8f;
                    player.EnableEffect<Slowness>(40, 15);
                }
                return;
            case < 35:
                display.Show(new BasicElement(200, "<b><size=30><color=orange>Patience.</color></size></b>"), 3f);
                player.EnableEffect<Sinkhole>(1, 20);
                return;
            case < 45:
                display.Show(new BasicElement(200, "<b><size=30><color=yellow>HP down, damage up.</color></size></b>"), 3f);
                PlayerEvents.Hurting += OnDamaging;
                PlayerEvents.Dying += OnDying;
                player.MaxHealth *= 0.85f;

                return;

                void OnDying(PlayerDyingEventArgs ev)
                {
                    if (ev.Player != player) return;
                    PlayerEvents.Hurting -= OnDamaging;
                    PlayerEvents.Dying -= OnDying;
                }

                void OnDamaging(PlayerHurtingEventArgs ev)
                {
                    if (ev.Attacker != player) return;
                    if (ev.DamageHandler is not StandardDamageHandler damageHandler) return;
                    switch (ev.Attacker.Role)
                    {
                        // for some reason 173, 049, and 106 attacks brick when modified
                        case RoleTypeId.Scp173:
                            player.Heal(ev.Player.Health/6);
                            break;
                        case RoleTypeId.Scp106 or RoleTypeId.Scp049 or RoleTypeId.Scp096:
                            player.Heal(damageHandler.Damage/6);
                            return;
                    }
                    if (ev.Attacker.Role is RoleTypeId.Scp173 or RoleTypeId.Scp049 or RoleTypeId.Scp106) return;
                    damageHandler.Damage *= 1.2f;
                }
            case < 65:
                display.Show(new BasicElement(200, "<b><size=30><color=green>32.5% Damage Reduction!</color></size></b>"), 3f);
                var existingDr = player.ActiveEffects.OfType<DamageReduction>().FirstOrDefault();
                var drIntensity = existingDr?.Intensity ?? 0;
                var drDuration = existingDr?.Duration ?? 0;
                player.EnableEffect<DamageReduction>((byte)(65 + drIntensity), 20);
                Timing.CallDelayed(20.1f, () =>
                {
                    player.EnableEffect<DamageReduction>(drIntensity, drDuration);
                });
                return;
            case < 80:
                display.Show(new BasicElement(200, "<b><size=30><color=green>Regeneration!</color></size></b>"), 3f);
                player.AddRegeneration(10, 10);
                return;
            case < 95: 
                display.Show(new BasicElement(200, "<b><size=30><color=purple>Necromancer!</color></size></b>"), 3f);
                var revives = 2;
                PlayerEvents.Dying += MeWhenIRevive2;
                return;

                void MeWhenIRevive2(PlayerDyingEventArgs ev)
                {
                    if (ev.Player == player)
                    {
                        PlayerEvents.Dying -= MeWhenIRevive2;
                        return;
                    }
                    if (ev.Attacker != player) return;
                    if (revives <= 0) return;

                    ev.IsAllowed = false;
                    if (player.Team == Team.SCPs)
                    {
                        ev.Player.SetRole(RoleTypeId.Scp0492,  RoleChangeReason.Resurrected, RoleSpawnFlags.AssignInventory);
                    }
                    else
                    {
                        ev.Player.SetRole(player.Role, RoleChangeReason.Resurrected,
                            ev.Player.Inventory.UserInventory.Items.IsEmpty() ? RoleSpawnFlags.AssignInventory : RoleSpawnFlags.None);
                    }
            
                    player.SendHitMarker();
                    revives--;

                    if (revives <= 0)
                    {
                        PlayerEvents.Dying -= MeWhenIRevive2;
                    }
                }
            case < 100:
                display.Show(new BasicElement(200, "<b><size=30><color=orange>PYROMANIAC</color></size></b>"), 3f);
                var explosions = 5;
                var protectFromExplosion = false;
                PlayerEvents.Dying += ExplodingPeopleOnDying;
                PlayerEvents.Hurting += ExplosionHurtingIgnore;
                return;

                void ExplodingPeopleOnDying(PlayerDyingEventArgs ev)
                {
                    if (ev.Attacker != player) return;
                    if (explosions <= 0) return;
                    protectFromExplosion = true;
            
                    TimedGrenadeProjectile.SpawnActive(ev.Player.Position, ItemType.GrenadeHE, player, 0.01f);
                    Timing.CallDelayed(0.5f, () => {protectFromExplosion = false;});
                    explosions--;

                    if (explosions <= 0 || player == ev.Player)
                    {
                        Timing.CallDelayed(0.5f, () => {PlayerEvents.Dying -= ExplodingPeopleOnDying;
                            PlayerEvents.Hurting -= ExplosionHurtingIgnore;});
                    }
                }

                void ExplosionHurtingIgnore(PlayerHurtingEventArgs ev)
                {
                    if (!protectFromExplosion) return;
                    if (ev.Player != player) return;
                    if (ev.Attacker !=  player) return;
                    if (ev.DamageHandler is not ExplosionDamageHandler) return;
                    ev.IsAllowed = false;
                }
        }
    }
}