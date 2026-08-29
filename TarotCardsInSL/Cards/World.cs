using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class World(Config config) : CustomCard
{
    public override string ID => "world";
    public override string Name => "XXI | The World";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomSite02;
    public override (int A, int B, int C) CardPerms => (0, 3, 0);
    public override string TechnicalDescription => "Teleports the player to a random SCP\nIf an SCP uses the card or theres no SCP's alive,\n the player will be teleported to a random enemy\nDoesn't work with Micro H.I.D held";
    public override string Description => "Open your eyes and see.";
    public override Color GlowColor => Color.yellow;
    public override int SpawnWeight => config.WorldSpawnWeight;


    public override void Activate(Player player)
    {
        var enemiesWithExtraCheese = Player.ReadyList.Where(p => p != player && p.IsAlive && p.Team != player.Team && p.Team != Team.Dead).ToList();
        var scps = Player.ReadyList.Where(p => p != player && p.IsAlive && p.Team == Team.SCPs && p.Role != RoleTypeId.Scp079).ToList();
        var enemies = Player.ReadyList.Where(p => p != player && p.IsAlive && p.Team != player.Team && p.Team != Team.Dead && p.Role != RoleTypeId.Tutorial).ToList();
        if (config.EnableTutorialInteractions)
        {
            enemies = enemiesWithExtraCheese;
        }
        Player? target = null;

        if (player.Team == Team.SCPs)
        {
            if (enemies.Count > 0) target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
        }
        else if (scps.Count > 0)
        {
            target = scps[UnityEngine.Random.Range(0, scps.Count)];
        }
        else if (enemies.Count > 0)
        {
            target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
        }

        if (target == null)
        {
            TarotPlugin.CardManager.RefundCard(player, this);
            return;
        }

        player.Position = target.Position;
    }
}