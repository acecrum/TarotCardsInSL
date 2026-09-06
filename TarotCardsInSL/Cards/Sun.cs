using CustomPlayerEffects;
using LabApi.Events;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Sun(Config config) : CustomCard
{
    public override string ID => "sun";
    public override string Name => "XIX | The Sun";
    public override CardType Type => CardType.Passive;
    public override ItemType KeycardType => ItemType.KeycardCustomManagement;
    public override (int A, int B, int C) CardPerms => (3, 3, 3);
    public override string TechnicalDescription => "<color=red>Disables attacking and using items.</color>\nIn return, creates a passive healing AOE for you and teammates,\nyou're also granted +35% speed and teammates get a 10% damage boost.";
    public override string Description => "Bring them hope.";
    public override Color GlowColor => Color.yellow;
    public override int SpawnWeight => config.SunSpawnWeight;
    
    private readonly Dictionary<string, SavedEffects> _savedEffects = new();
    private readonly Dictionary<string, LabEventHandler<PlayerChangingItemEventArgs>> _changingItemHandler = new();
    private readonly Dictionary<string, LabEventHandler<PlayerHurtingEventArgs>> _damagenulling = new();
    private readonly Dictionary<string, CoroutineHandle> _removeItemCoroutines = new();
    private readonly Dictionary<string, CoroutineHandle> _removeHealingAOE = new();
    private readonly Dictionary<string, LightSourceToy> _savedLights = new();
    private sealed class SavedEffects
    {
        public byte SpeedIntensity { get; init; }
        public float SpeedDuration { get; init; }
    }

    public override void OnGiven(Player player)
    { 
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();

        _savedEffects[player.UserId] = new SavedEffects
        {
            SpeedIntensity = existingSpeed?.Intensity ?? 0,
            SpeedDuration = existingSpeed?.TimeLeft ?? 0,
        };

        var handle = Timing.RunCoroutine(RemoveHeldItem(player));
        var healinghandle = Timing.RunCoroutine(HealNearby(player));
        _removeHealingAOE[player.UserId] = healinghandle;
        _removeItemCoroutines[player.UserId] = handle;
        _damagenulling[player.UserId] = damagenulling;
        PlayerEvents.Hurting += damagenulling;
        
        return;
        
        IEnumerator<float> RemoveHeldItem(Player player)
        {
            while (player.CurrentItem != null)
            {
                player.CurrentItem = null;

                yield return Timing.WaitForSeconds(0.1f);
            }
            
            _changingItemHandler[player.UserId] = DenyItemSwap;
            PlayerEvents.ChangingItem += DenyItemSwap;
            
            var light = LightSourceToy.Create(player.GameObject?.transform, networkSpawn: false);
            light.Color = GlowColor;
            light.Intensity = 1.5f;
            light.Range = 5f;
            light.Spawn();
            _savedLights[player.UserId] = light;
            
            player.EnableEffect<MovementBoost>(35);
            
            _removeItemCoroutines.Remove(player.UserId);
        }

        IEnumerator<float> HealNearby(Player player)
        {
            yield return Timing.WaitForSeconds(1f);
            
            while (player.IsAlive)
            {
                player.AddRegeneration(5, 1);

                var nearPlayers = GetNearPlayers(player);

                foreach (var nearPlayer in nearPlayers)
                {
                    Timing.RunCoroutine(ApplyBuff(player, nearPlayer));
                }

                yield return Timing.WaitForSeconds(1);
            }
            
            IEnumerator<float> ApplyBuff(Player player, Player nearPlayer)
            {
                PlayerEvents.Hurting += moredmgmult;
                nearPlayer.AddRegeneration(5, 1);
                
                try
                {
                    while (nearPlayer.IsAlive && Vector3.Distance(nearPlayer.Position, player.Position) <= 5f)
                    {
                        yield return Timing.WaitForSeconds(1);
                    }
                }
                finally
                {
                    PlayerEvents.Hurting -= moredmgmult;
                }

                yield break;

                void moredmgmult(PlayerHurtingEventArgs ev)
                {
                    if (ev.Attacker != nearPlayer) return;
                    if (ev.DamageHandler is not StandardDamageHandler damageHandler) return;

                    switch (ev.Attacker.Role)
                    {
                        case RoleTypeId.Scp173:
                            break;
                        case RoleTypeId.Scp106 or RoleTypeId.Scp049 or RoleTypeId.Scp096:
                            return;
                    }
                    damageHandler.Damage *= 1.1f;
                }
            }
        }
        
        void DenyItemSwap(PlayerChangingItemEventArgs ev)
        {
            if (ev.Player !=  player) return;

            ev.IsAllowed = false;
        }

        void damagenulling(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker != player) return;
            
            ev.IsAllowed = false;
        }
        
        List <Player> GetNearPlayers(Player player)
        {
            return Player.ReadyList.Where(eligible => eligible != player && eligible.Team == player.Team && eligible.IsAlive && Vector3.Distance(eligible.Position, player.Position) <= 5f).ToList();
        }
    }
    public override void OnRemoved(Player player)
    {
        if (_removeItemCoroutines.TryGetValue(player.UserId, out var coroutine))
        {
            Timing.KillCoroutines(coroutine);
            _removeItemCoroutines.Remove(player.UserId);
        }

        if (_removeHealingAOE.TryGetValue(player.UserId, out var healinghandle))
        {
            Timing.KillCoroutines(healinghandle);
            _removeHealingAOE.Remove(player.UserId);
        }

        if (_savedEffects.TryGetValue(player.UserId, out var saved))
        {
            player.EnableEffect<MovementBoost>(
                saved.SpeedIntensity,
                saved.SpeedDuration);

            _savedEffects.Remove(player.UserId);
        }

        if (_changingItemHandler.TryGetValue(player.UserId, out var handler))
        {
            PlayerEvents.ChangingItem -= handler;
            _changingItemHandler.Remove(player.UserId);
        }
        
        if (_damagenulling.TryGetValue(player.UserId, out var damagehandler))
        {
            PlayerEvents.Hurting -= damagehandler;
            _damagenulling.Remove(player.UserId);
        }

        if (!_savedLights.TryGetValue(player.UserId, out var light)) return;
        light.Destroy();
        _savedLights.Remove(player.UserId);
    }
}