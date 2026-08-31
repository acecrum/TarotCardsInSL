using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp106Events;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.Arguments.Scp3114Events;
using LabApi.Events.Arguments.Scp939Events;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Stars(Config config) : CustomCard
{
    public override string ID => "stars";
    public override string Name => "XVII | The Stars";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 3);
    public override string TechnicalDescription => "<color=red>Removes every item from the players inventory and disables picking up.</color>\nAfter a minute, replaces them with direct upgrades.\nSCP's abilities and attacking is disabled for 90 seconds instead\nSCP's recieve a buff tailored to each SCP";
    public override string Description => "Let go and find what you need.";
    public override Color GlowColor => new Color32(251, 225, 114, 255);
    public override int SpawnWeight => config.StarsSpawnWeight;


    public override void Activate(Player player)
    {
        switch (player.Role)
        {
            case RoleTypeId.Scp049:
            {
                Scp049Events.UsingSense += DisableGoodSense;
                Scp049Events.UsingDoctorsCall += DisableDoctorCall;
                Scp049Events.StartingResurrection += DisableRes;
                
                Timing.CallDelayed(90f, () =>
                {
                    Scp049Events.UsingSense -= DisableGoodSense;
                    Scp049Events.UsingDoctorsCall -= DisableDoctorCall;
                    Scp049Events.StartingResurrection -= DisableRes;
                });
                break;

                void DisableGoodSense(Scp049UsingSenseEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }

                void DisableDoctorCall(Scp049UsingDoctorsCallEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }

                void DisableRes(Scp049StartingResurrectionEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }
            }
            case RoleTypeId.Scp106:
            {
                Scp106Events.UsingHunterAtlas += DisableAtlas;
                Scp106Events.ChangingStalkMode += DisableStalk;
                
                Timing.CallDelayed(90f, () =>
                {
                    Scp106Events.UsingHunterAtlas -= DisableAtlas;
                    Scp106Events.ChangingStalkMode -= DisableStalk;
                });
                break;
                
                void DisableAtlas(Scp106UsingHunterAtlasEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }

                void DisableStalk(Scp106ChangingStalkModeEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    ev.IsAllowed  = false;
                }
            }
            case RoleTypeId.Scp939:
            {
                Scp939Events.CreatingAmnesticCloud += DisableCloud;
                Scp939Events.MimickingEnvironment += DisableSpeaking;
                
                Timing.CallDelayed(90f, () =>
                {
                    Scp939Events.CreatingAmnesticCloud -= DisableCloud;
                    Scp939Events.MimickingEnvironment -= DisableSpeaking;
                });
                
                break;
                void DisableCloud(Scp939CreatingAmnesticCloudEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }

                void DisableSpeaking(Scp939MimickingEnvironmentEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }
            }
            case RoleTypeId.Scp173:
            {
                Scp173Events.CreatingTantrum += DisableShitting;
                
                Timing.CallDelayed(90f, () =>
                {
                    Scp173Events.CreatingTantrum -= DisableShitting;
                });
                
                break;
                void DisableShitting(Scp173CreatingTantrumEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }
            }
            case RoleTypeId.Scp096:
            {
                Scp096Events.Enraging += DisableScopophobia;
                
                Timing.CallDelayed(90f, () =>
                {
                    Scp096Events.Enraging -= DisableScopophobia;
                });
                
                break;
                void DisableScopophobia(Scp096EnragingEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }
            }
            case RoleTypeId.Scp3114:
            {
                Scp3114Events.Disguising += DisableDisguise;
                Scp3114Events.StrangleStarting += DisableKinkyStrangling;
                
                Timing.CallDelayed(90f, () =>
                {
                    Scp3114Events.Disguising -= DisableDisguise;
                    Scp3114Events.StrangleStarting -= DisableKinkyStrangling;
                });
                break;
                void DisableDisguise(Scp3114DisguisingEventArgs ev)
                {
                    if (ev.Player !=  player) return;

                    ev.IsAllowed = false;
                }

                void DisableKinkyStrangling(Scp3114StrangleStartingEventArgs ev)
                {
                    if (ev.Player !=  player) return;
                    
                    ev.IsAllowed  = false;
                }
            }
        }

        var delay = 60f;
        if (player.IsSCP)
        {
            delay = 90f;
        }
        
        PlayerEvents.Hurting += Idkwhattonamethisshitanymoreman;
        
        Timing.CallDelayed(delay, () =>
        {
            PlayerEvents.Hurting -= Idkwhattonamethisshitanymoreman;
        });

        var savedInventory = player.Inventory;
        
        if (player.IsHuman)
        {
            PlayerEvents.PickingUpItem += Denypickup;
            player.ClearInventory();

            void Denypickup(PlayerPickingUpItemEventArgs ev)
            {
                if  (ev.Player !=  player) return;
                ev.IsAllowed = false;
            }
            
            Timing.CallDelayed(60f, () =>
            {
                PlayerEvents.PickingUpItem -= Denypickup;
            });
        }
        
        return;
        
        void Idkwhattonamethisshitanymoreman(PlayerHurtingEventArgs ev) 
        {
            if (ev.Attacker !=  player) return;
            ev.IsAllowed  = false;
        }


    }
}