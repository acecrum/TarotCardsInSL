using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Tower(Config config) : CustomCard
{
    public override string ID => "tower";
    public override string Name => "XVI | The Tower";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomSite02;
    public override (int A, int B, int C) CardPerms => (0, 3, 0);
    public override string TechnicalDescription => "TBD";
    public override string Description => "TBD";
    public override Color GlowColor => new Color32(197, 173, 163, 255);
    public override int SpawnWeight => config.TowerSpawnWeight;


    public override void Activate(Player player)
    {
        
    }
}