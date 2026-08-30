using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class DebugCard : CustomCard
{
    public override string ID => "debug";
    public override string Name => "Debug Card";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 0);
    public override string TechnicalDescription => "Does nothing or something?";
    public override string Description => "Does something maybe, depends on the build tbh";
    public override Color GlowColor => Color.white;
    public override int SpawnWeight => 0;


    public override void Activate(Player player)
    {
        if (player.Team == Team.SCPs)
        {
            Timing.RunCoroutine(TimedLoop());
                    
            IEnumerator<float> TimedLoop()
            {
                var explosions = 150;

                while (explosions > 0)
                {
                    TimedGrenadeProjectile.SpawnActive(player.Position, ItemType.GrenadeHE, player);
                    explosions--;
                    yield return Timing.WaitForSeconds(0.01f);
                }
            }
        }
    }
}