using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using RueI.API;
using RueI.API.Elements;
using TarotCardsInSL.UI;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Lovers(Config config) : CustomCard
{
    public override string ID => "lovers";
    public override string Name => "VI | The Lovers";
    public override CardType Type =>  CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomManagement;
    public override (int A, int B, int C) CardPerms => (2, 0, 1);
    public override string TechnicalDescription => "Use on a nearby teammate to link with them and get buffs\nHealth and effects are shared between the linked players\nSCP's are linked for 1 minute and health is not shared\n<color=red>When a linked player dies, you die as well</color>";
    public override string Description => "May you find your true love.";
    public override Color GlowColor => new Color32(133, 255, 135, 255);
    public override int SpawnWeight => config.LoversSpawnWeight;


    public override void Activate(Player player) // holy shit this was awful to do
    {
        var playerB = OoooShiTheyInLove(player);
        if (playerB == null || playerB.Team !=  player.Team)
        {
            TarotHints.CardFailHint(player);
            TarotPlugin.CardManager.RefundCard(player, this);
            return;
        }

        var linkActive = true;
        var syncingEffects = false;
        var syncingHP = false;
        var HPA = player.ReferenceHub.playerStats.GetModule<HealthStat>();
        var HPB = playerB.ReferenceHub.playerStats.GetModule<HealthStat>();

        HPA.OnStatChange += HPAChanged;
        HPB.OnStatChange += HPBChanged;
        PlayerEvents.Dying += ohshittheydied;

        var displayA = RueDisplay.Get(player);
        {displayA.Show(new BasicElement(200, $"<b><size=30><color={GlowColor.ToHex()}>You are now linked with {playerB.DisplayName}!</color></size></b>"), 3f);}
        player.EnableEffect<MovementBoost>(15);
        MergeStartingEffects();
        PlayerEvents.UpdatedEffect += updatedeffect;
        if (player.IsSCP)
        {
            player.Health = player.MaxHealth *= 1.1f;
            playerB.Health = player.MaxHealth *= 1.1f;
            Timing.CallDelayed(60f, endlink);
        }
        else
        {
            player.Health = player.MaxHealth;
            playerB.Health = playerB.MaxHealth;
        }
        return;

        void HPAChanged(float oldHP, float newHP)
        {
            if (!linkActive || syncingHP) return;
            
            var difference = newHP - oldHP;
            if (Mathf.Approximately(difference, 0)) return;
            
            syncingHP = true;
            
            playerB.Health = Mathf.Clamp(playerB.Health + difference, 0f, playerB.MaxHealth);
            syncingHP = false;
        }
        
        void HPBChanged(float oldHP, float newHP)
        {
            if (!linkActive || syncingHP) return;
            
            var difference = newHP - oldHP;
            if (Mathf.Approximately(difference, 0)) return;
            
            syncingHP = true;
            
            player.Health = Mathf.Clamp(player.Health + difference, 0f, player.MaxHealth);
            syncingHP = false;
        }
        
        void ohshittheydied(PlayerDyingEventArgs ev)
        {
            if (!linkActive) return;
            if (ev.Player != player && ev.Player != playerB) return;
            if (player.Role == RoleTypeId.Scp0492 && playerB.Role != RoleTypeId.Scp0492|| playerB.Role == RoleTypeId.Scp0492 && playerB.Role != RoleTypeId.Scp0492) return;
            
            var other = ev.Player == player ? playerB : player;
            linkActive = false;
            if (other.IsAlive)
            {
                other.Kill($"Heartbreak from {ev.Player.DisplayName}'s Death");
            }
            endlink();
        }

        void updatedeffect(PlayerEffectUpdatedEventArgs ev)
        {
            if (!linkActive || syncingEffects) return;
            if (ev.Player != player && ev.Player != playerB) return;

            var source = ev.Player;
            var other = source == player ? playerB : player;

            var sourceEffect = ev.Effect;
            var effectName = sourceEffect.GetType().Name;
            var intensity = ev.Intensity;

            Timing.CallDelayed(0.1f, () =>
            {
                if (!linkActive) return;
                if (!other.TryGetEffect(effectName, out StatusEffectBase? otherEffect)) return;
                
                var duration = sourceEffect.Duration;
                syncingEffects = true;

                try
                {
                    otherEffect.ServerSetState(intensity, duration);
                }
                finally
                {
                    syncingEffects = false;
                } 
            });
        }

        void MergeStartingEffects()
        {
            var effectsA = player.ActiveEffects.ToList();
            var effectsB = playerB.ActiveEffects.ToList();
            
            foreach (var effectA in effectsA)
            {
                var effectB =
                    playerB.ReferenceHub.playerEffectsController.AllEffects.FirstOrDefault(e =>
                        e.GetType() == effectA.GetType());

                if (effectB == null) continue;

                if (effectB.Intensity == 0)
                {
                    effectB.ServerSetState(effectA.Intensity, effectA.Duration);
                    
                    continue;
                }
                MergeEffects(effectA, effectB);
            }
            
            foreach (var effectB in effectsB)
            {
                var effectA =
                    player.ReferenceHub.playerEffectsController.AllEffects.FirstOrDefault(e =>
                        e.GetType() == effectB.GetType());

                if (effectA == null) continue;

                if (effectA.Intensity == 0)
                {
                    effectA.ServerSetState(effectB.Intensity, effectB.Duration);
                }
            }
        }

        void MergeEffects(StatusEffectBase effectA, StatusEffectBase effectB)
        {
            var aPerma = effectA.Duration <= 0f;
            var bPerma = effectB.Duration <= 0f;

            byte intensity;
            float duration;

            if (aPerma && !bPerma)
            {
                intensity = effectA.Intensity;
                duration = 0f;
            }
            else if (bPerma && !aPerma)
            {
                intensity = effectB.Intensity;
                duration = 0f;
            }
            else
            {
                intensity = (byte)Mathf.Clamp(effectA.Intensity + effectB.Intensity, 0, 255);
                duration = aPerma && bPerma ? 0f : effectA.Duration + effectB.Duration;
            }
            effectA.ServerSetState(intensity, duration);
            effectB.ServerSetState(intensity, duration);
        }

        Player? OoooShiTheyInLove(Player playerA)
        {
            const float maxDistance = 5f;
            const float maxAngle = 15f;
            
            Player? closestPlayer = null;
            var closestAngle = float.MaxValue;

            foreach (var b in Player.ReadyList)
            {
                var direction = b.Position - playerA.Camera.position;
                var distance = direction.magnitude;

                if (distance > maxDistance) continue;
                
                var angle = Vector3.Angle(playerA.Camera.forward, direction);
                if (angle > maxAngle) continue;
                if (angle >= closestAngle) continue;
                
                closestAngle = angle;
                closestPlayer = b;
            }
            return closestPlayer;
        }

        void endlink()
        {
            linkActive = false;
            
            PlayerEvents.Dying -= ohshittheydied;
            PlayerEvents.UpdatedEffect -= updatedeffect;
            HPA.OnStatChange -= HPAChanged;
            HPB.OnStatChange -= HPBChanged;
        }
    }
}