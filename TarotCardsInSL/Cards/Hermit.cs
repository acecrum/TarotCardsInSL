using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Hermit(Config config) : CustomCard
{
    public override string ID => "hermit";
    public override string Name => "IX | The Hermit";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomManagement;
    public override (int A, int B, int C) CardPerms => (0, 3, 2);
    public override string TechnicalDescription => "Swaps you to the opposite team.\nSCP's convert up to 2 killed players into 049-2";
    public override string Description => "May you see what life has to offer";
    public override Color GlowColor => new Color32(92, 170, 196, 255);
    public override int SpawnWeight => config.HermitSpawnWeight;


    public override void Activate(Player player)
    {
        if (player.IsSCP)
        {
            var revives = 2;
            PlayerEvents.Dying += MeWhenIRevive1;
            return;

            void MeWhenIRevive1(PlayerDyingEventArgs ev)
            {
                if (ev.Player == player)
                {
                    PlayerEvents.Dying -= MeWhenIRevive1;
                    return;
                }

                if (ev.Attacker != player) return;
                if (revives <= 0)
                {
                    PlayerEvents.Dying -= MeWhenIRevive1;
                    return;
                }

                ev.IsAllowed = false;
                if (player.Team == Team.SCPs)
                {
                    ev.Player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.Resurrected, RoleSpawnFlags.AssignInventory);
                }
                
                player.SendHitMarker();
                revives--;
            }
        }
        var existingEffects = player.ActiveEffects
            .Where(effect => effect.Intensity > 0).Select(effect => new {Type = effect.GetType(), effect.Intensity, effect.Duration}).ToList();
        switch (player.Role)
        {
            case RoleTypeId.ChaosRifleman:
                player.SetRole(RoleTypeId.NtfPrivate, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.NtfPrivate:
                player.SetRole(RoleTypeId.ChaosRifleman, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.ClassD:
                player.SetRole(RoleTypeId.Scientist, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.Scientist:
                player.SetRole(RoleTypeId.ClassD, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.NtfSpecialist:
                player.SetRole(RoleTypeId.ChaosConscript, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.ChaosConscript:
                player.SetRole(RoleTypeId.NtfSpecialist, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.NtfSergeant:
                player.SetRole(RoleTypeId.ChaosMarauder, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.NtfCaptain:
                player.SetRole(RoleTypeId.ChaosRepressor, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.Tutorial:
                if (!config.EnableTutorialInteractions) break;
                switch (UnityEngine.Random.Range(0, 1))
                {
                    case 0:
                        player.SetRole(RoleTypeId.ChaosConscript, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                        break;
                    case 1:
                        player.SetRole(RoleTypeId.NtfSpecialist, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                        break;
                }
                break;
            case RoleTypeId.FacilityGuard:
                player.SetRole(RoleTypeId.ChaosRifleman, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.ChaosMarauder:
                player.SetRole(RoleTypeId.NtfSergeant, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            case RoleTypeId.ChaosRepressor:
                player.SetRole(RoleTypeId.NtfCaptain, RoleChangeReason.ItemUsage, RoleSpawnFlags.None);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (!player.IsHuman) return;
        Timing.CallDelayed(0.2f, () =>
        {
            foreach (var savedEffect in existingEffects)
            {
                var effect = player.ReferenceHub.playerEffectsController.AllEffects
                    .FirstOrDefault(x => x.GetType() == savedEffect.Type);

                if (effect == null) continue;

                effect.ServerSetState(
                    savedEffect.Intensity,
                    savedEffect.Duration
                );
            }
        });
    }
}