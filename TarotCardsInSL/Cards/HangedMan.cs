using LabApi.Features.Wrappers;
using PlayerRoles;
using TarotCardsInSL.UI;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class HangedMan(Config config) : CustomCard
{
    public override string ID => "hangedman";
    public override string Name => "XII | The Hanged Man";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomSite02;
    public override (int A, int B, int C) CardPerms => (3, 2, 1);
    public override string TechnicalDescription => "Using while looking at a corpse will bring them back on your team\nDoes not work if they're already alive";
    public override string Description => "May you find enlightenment.";
    public override Color GlowColor => new Color32(129, 152, 255, 255);
    public override int SpawnWeight => config.HangedManSpawnWeight;


    public override void Activate(Player player)
    {
        Ragdoll? StareAtRagdollLikeIts173(Player suicidalPlayer)
        {
            const float maxDistance = 2.5f;
            const float maxAngle = 15f;
            
            Ragdoll? closestRagdoll = null;
            float closestAngle = float.MaxValue;

            foreach (var ragdoll in Ragdoll.List)
            {
                var direction = ragdoll.Position - suicidalPlayer.Camera.position;
                var distance = direction.magnitude;

                if (distance > maxDistance) continue;
                
                var angle = Vector3.Angle(suicidalPlayer.Camera.forward, direction);
                if (angle > maxAngle) continue;
                if (angle >= closestAngle) continue;
                
                closestAngle = angle;
                closestRagdoll = ragdoll;
            }
            return closestRagdoll;
        }

        var ragdoll = StareAtRagdollLikeIts173(player);
        
        if (ragdoll == null)
        {
            TarotHints.CardFailHint(player);
            TarotPlugin.CardManager.RefundCard(player, this);
            return;
        }
        var deadPlayer = Player.Get(ragdoll.Base.Info.OwnerHub);
        
        if (deadPlayer.IsAlive || deadPlayer.IsOverwatchEnabled)
        {
            TarotHints.CardFailHint(player);
            TarotPlugin.CardManager.RefundCard(player, this);
            return;
        }

        deadPlayer.SetRole(player.IsSCP ? RoleTypeId.Scp0492 : player.Role, RoleChangeReason.Resurrected, RoleSpawnFlags.AssignInventory);
        deadPlayer.Position = ragdoll.Position + new Vector3(0f, 1f, 0f);
        ragdoll.Destroy();
    }
}