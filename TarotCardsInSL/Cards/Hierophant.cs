using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Hierophant(Config config) : CustomCard
{
    public override string ID => "hierophant";
    public override string Name => "V | The Hierophant";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomManagement;
    public override (int A, int B, int C) CardPerms => (1, 0, 2);
    public override string TechnicalDescription => "Adds 50 Hume shield to a human and grants an adrenaline\nSCP's hume shield is fully restored on use";
    public override string Description => "Two prayers for the lost.";
    public override Color GlowColor => new Color32(133, 235, 255, 255);
    public override int SpawnWeight => config.HierophantSpawnWeight;


    public override void Activate(Player player)
    {
        if (player.Team == Team.SCPs)
        {
            player.HumeShield = player.MaxHumeShield;
        }
        else
        {
            player.HumeShield += 50;
            if (!player.IsInventoryFull)
            {
                player.AddItem(ItemType.Adrenaline);
                return;
            }
            Pickup.Create(ItemType.Adrenaline, player.Position);
        }
    }
}