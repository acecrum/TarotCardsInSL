using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace TarotCardsInSL.Cards;

public sealed class Magician(Config config) : CustomCard
{
    public override string ID => "realmagician";
    public override string Name => "? | The REAL Magician";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (3, 3, 3);
    public override string TechnicalDescription => "Gives the player aimbot for 10 seconds.\n<color=red>Don't get banned</color>";
    public override string Description => "THIS MOTHERFUCKER IS RAGE HACKING";
    public override Color GlowColor => new Color32(80, 50, 168, 255);
    public override int SpawnWeight => config.MagicianSpawnWeight;
    public override float GlowIntensity => 0.75f;

    public override void Activate(Player player)
    {
        Logger.Info($"{player.DisplayName} ({player.UserId}) has begun using Magician");
        var light = LightSourceToy.Create(player.GameObject?.transform, networkSpawn: false);
        light.Color = Color.red;
        light.Intensity = 1f;
        light.Range = 1.5f;
        light.Spawn();

        // grabs the nearest enemy in a 20m range and checks if they're a valid target
        Player? GetNearPlayer(Player shooter)
        {
            Player? nearestPlayer = null;
            var nearestDistance = float.MaxValue;
            foreach (var eligible in Player.ReadyList)
            {
                if (eligible == shooter) continue;
                if (eligible.Team == shooter.Team) continue;
                if (!eligible.IsAlive) continue;
                if (eligible.IsGodModeEnabled) continue;
                if (Vector3.Distance(eligible.Position, shooter.Position) > 20f) continue;
                if (!CanThisDogshitSeeThem(shooter, eligible)) continue;
                
                var distance = Vector3.Distance(shooter.Position, eligible.Position);
                if (distance >= nearestDistance) continue;
                nearestPlayer = eligible;
                nearestDistance = distance;
            }
            return nearestPlayer;
        }
        
        void OnShooting(PlayerShootingWeaponEventArgs ev)
        {
            var target = GetNearPlayer(ev.Player);
            if (ev.Player != player) return;
            if (target == null) return;

            // all da stuff raycasts need
            var direction = (target.Position - player.Camera.position).normalized;
            var shooterRotation = Quaternion.LookRotation(direction).eulerAngles;
            var pitch = Mathf.Asin(direction.y) * Mathf.Rad2Deg;

            var newShooterRotation = new Vector2(pitch, shooterRotation.y);
            
            player.LookRotation = newShooterRotation;
        }
        
        // subscribing and unsubscribing after 10 seconds
        PlayerEvents.ShootingWeapon += OnShooting;
        Timing.CallDelayed(10f, () =>
        {
            PlayerEvents.ShootingWeapon -= OnShooting;
            light.Destroy();
            Logger.Info($"{player.DisplayName} ({player.UserId}) has finished using Magician.");
        });
        return;
        // check for if a player is actually visable
        bool CanThisDogshitSeeThem(Player shooter, Player target)
        {
            var origin = shooter.Camera.position;
            var direction = (target.Position - origin).normalized;
            var distance =  Vector3.Distance(origin, target.Position) + 1f;
            
            var hits =  Physics.RaycastAll(origin, direction, distance);
           
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var hitbox = hit.collider.GetComponentInParent<HitboxIdentity>();

                if (hitbox == null) return false;
                var hitPlayer = Player.Get(hitbox.TargetHub);
                if (hitPlayer == shooter) continue;
                return hitPlayer == target;
            }
            return false;
        }
    }
}