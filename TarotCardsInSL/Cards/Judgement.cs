using HarmonyLib;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Judgement(Config config) : CustomCard
{
    public override string ID => "judgement";
    public override string Name => "XX | Judgement";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 3);
    public override string TechnicalDescription => "Kills you but, causes a respawn wave to instantly respawn on your side.\nYou will respawn as the highest ranking member on your team\n<color=red>This uses a respawn token and resets respawn times.</color>\n<color=orange>If there are no Primary-wave tokens, a Mini-wave will be spawned instead.</color>\nSCP's have 2 new SCPs take their place. Both SCP's health is halved.";
    public override string Description => "Judge lest ye be judged";
    public override Color GlowColor => new Color32(247, 144, 109, 255);
    public override int SpawnWeight => config.JudgementSpawnWeight;

    public override void Activate(Player player)
    {
        var oldPlayerTeam = player.Team;
        var oldPlayerRole = player.Role;
        
        player.Kill("Brought judgement against all.");
        
        /*if (oldPlayerInfo.Team == Team.SCPs && oldPlayerInfo.Role != RoleTypeId.Scp0492)
        {
            return;
        }*/
        
        switch (oldPlayerTeam)
        {
            case Team.FoundationForces or Team.Scientists:
            {
                ServerEvents.WaveRespawning += OnRespawningWave;

                if (RespawnWaves.PrimaryMtfWave != null && RespawnWaves.PrimaryMtfWave.RespawnTokens >= 1)
                {
                    RespawnWaves.PrimaryMtfWave.InstantRespawn();
                }
                else
                {
                    RespawnWaves.MiniMtfWave?.InstantRespawn();
                }

                break;

                void OnRespawningWave(WaveRespawningEventArgs ev)
                {
                    var updateItAllBaby = new Dictionary<Player, RoleTypeId>();
                
                    foreach (var role in ev.Roles)
                    {
                        var Lookedplayer = role.Key;
                        var theirrole = role.Value;
                    
                        if (theirrole == RoleTypeId.NtfCaptain && Lookedplayer != player)
                        {
                            theirrole = RoleTypeId.NtfSergeant;
                        }

                        if (Lookedplayer == player && theirrole != RoleTypeId.NtfCaptain)
                        {
                            theirrole = RoleTypeId.NtfCaptain;
                        }
                        updateItAllBaby[Lookedplayer] = theirrole;
                    }

                    foreach (var role in updateItAllBaby)
                    {
                        ev.Roles[role.Key] = role.Value;
                    }
                
                    ServerEvents.WaveRespawning -= OnRespawningWave;
                }
            }
            case Team.ChaosInsurgency or Team.ClassD:
            {
                ServerEvents.WaveRespawning += OnRespawningWave;
            
                if (RespawnWaves.PrimaryChaosWave != null && RespawnWaves.PrimaryChaosWave.RespawnTokens >= 1)
                {
                    RespawnWaves.PrimaryChaosWave.InstantRespawn();
                }
                else
                {
                    RespawnWaves.MiniChaosWave?.InstantRespawn();
                }

                break;

                void OnRespawningWave(WaveRespawningEventArgs ev)
                {
                    var updateItAllBaby = new Dictionary<Player, RoleTypeId>();
                
                    ev.SpawningPlayers.AddItem(player);
                    
                    foreach (var role in ev.Roles)
                    {
                        var Lookedplayer = role.Key;
                        var theirrole = role.Value;
                    
                        if (theirrole == RoleTypeId.ChaosRepressor && Lookedplayer != player)
                        {
                            theirrole = RoleTypeId.ChaosMarauder;
                        }

                        if (Lookedplayer == player && theirrole != RoleTypeId.ChaosRepressor)
                        {
                            theirrole = RoleTypeId.ChaosRepressor;
                        }
                        updateItAllBaby[Lookedplayer] = theirrole;
                    }

                    foreach (var role in updateItAllBaby)
                    {
                        ev.Roles[role.Key] = role.Value;
                    }
                
                    ServerEvents.WaveRespawning -= OnRespawningWave;
                }
            }
        }
    }
}