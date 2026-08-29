using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Death(Config config) : CustomCard
{
    public override string ID => "death";
    public override string Name => "XIII | Death";
    public override CardType Type => CardType.Passive;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (3, 1, 0);
    public override string TechnicalDescription => "On death come back as a lower class\n<color=red>You have 50% less max HP and you cannot use healing items</color>";
    public override string Description => "Lay waste to all that oppose you.";
    public override Color GlowColor => new Color32(80, 50, 168, 255);
    public override int SpawnWeight => config.DeathSpawnWeight;

    public override void OnGiven(Player player)
    {
        var allowHealing = true;
        PlayerEvents.Dying += Dying;
        return;

        void Dying(PlayerDyingEventArgs ev)
        {
            if (ev.Player != player) return;
            ev.IsAllowed = false;
            PlayerEvents.Dying -= Dying;
            PlayerEvents.UsingItem += OnUsingItem;
            PlayerEvents.Death += DeathNotTheCardName;

            Ragdoll.SpawnRagdoll(player, ev.DamageHandler);
            player.DropAllItems();
            
            TarotPlugin.CardManager.DestroyPassive(player, this);
            if (player.IsSCP)
            {
                player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.ItemUsage);
            }
            switch (player.Role)
            {
                case RoleTypeId.ChaosRifleman:
                    player.SetRole(RoleTypeId.ChaosRifleman, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.NtfPrivate:
                    player.SetRole(RoleTypeId.FacilityGuard, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.ClassD:
                    player.SetRole(RoleTypeId.ClassD, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.Scientist:
                    player.SetRole(RoleTypeId.Scientist, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.NtfSpecialist:
                    player.SetRole(RoleTypeId.NtfPrivate, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.ChaosConscript:
                    player.SetRole(RoleTypeId.ChaosRifleman, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.NtfSergeant:
                    player.SetRole(RoleTypeId.NtfPrivate, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.NtfCaptain:
                    player.SetRole(RoleTypeId.NtfSergeant, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.Tutorial:
                    if (!config.EnableTutorialInteractions) break;
                    player.SetRole(RoleTypeId.Tutorial, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.FacilityGuard:
                    player.SetRole(RoleTypeId.Scientist, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.ChaosMarauder:
                    player.SetRole(RoleTypeId.ChaosRifleman, RoleChangeReason.ItemUsage);
                    break;
                case RoleTypeId.ChaosRepressor:
                    player.SetRole(RoleTypeId.ChaosMarauder, RoleChangeReason.ItemUsage);
                    break;
            }

            if (Warhead.IsDetonated)
            {
                var spawnRoles = new[]
                {
                    RoleTypeId.NtfPrivate,
                    RoleTypeId.ChaosRifleman
                };
                var chosenRole = spawnRoles[UnityEngine.Random.Range(0, spawnRoles.Length)];

                if (chosenRole.TryGetRandomSpawnPoint(out var spawnPosition, out _))
                {
                    player.Position = spawnPosition + Vector3.up;
                }
                return;
            }

            {
                switch (player.Role)
                {
                    case RoleTypeId.ClassD or RoleTypeId.Scientist when Decontamination.IsDecontaminating:
                    {
                        var alternativeRooms = new[]
                        {
                            RoomName.Hcz127,
                            RoomName.Hcz939,
                            RoomName.Hcz096
                        };
                        var chosenRoom = alternativeRooms[UnityEngine.Random.Range(0, alternativeRooms.Length)];
                        var room = Room.Get(chosenRoom).FirstOrDefault();

                        if (room != null)
                        {
                            player.Position = room.Position + Vector3.up;
                        }
                        return;
                    }
                    case RoleTypeId.Scp0492:
                    {
                        var alternativeRooms = new[]
                        {
                            RoomName.HczMicroHID,
                            RoomName.HczServers,
                            RoomName.Hcz049
                        };
                        var chosenRoom = alternativeRooms[UnityEngine.Random.Range(0, alternativeRooms.Length)];
                        var room = Room.Get(chosenRoom).FirstOrDefault();

                        if (room != null)
                        {
                            player.Position = room.Position + Vector3.up;
                        }
                        return;
                    }
                }
            }

            player.MaxHealth /= 2;
            player.Health = player.MaxHealth;
            allowHealing = false;
        }
        
        void OnUsingItem(PlayerUsingItemEventArgs ev)
        {
            if (ev.Player != player) return;
            if (allowHealing) return;

            ev.IsAllowed = ev.UsableItem.Type switch
            {
                ItemType.Medkit or ItemType.Painkillers or ItemType.SCP500 or ItemType.AntiSCP207
                    or ItemType.SCP207 => false,
                _ => ev.IsAllowed
            };
        }

        void DeathNotTheCardName(PlayerDeathEventArgs ev)
        {
            PlayerEvents.UsingItem -= OnUsingItem;
            PlayerEvents.Death -= DeathNotTheCardName;
        }
    }
}