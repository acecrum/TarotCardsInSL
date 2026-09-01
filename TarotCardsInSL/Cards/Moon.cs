using LabApi.Features.Wrappers;
using MapGeneration;
using TarotCardsInSL.UI;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Moon(Config config) : CustomCard
{
    public override string ID => "moon";
    public override string Name => "XVIII | The Moon";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 3);
    public override string TechnicalDescription => "On death, spectate a player and use the card to torment them for 1 minute.\nTormented players cannot pick up items, equip different items,\nand blacks out the current room they are in.\n<color=red>SCP's cannot be tormented</color>";
    public override string Description => "Bring them terror.";
    public override Color GlowColor => new Color32(123, 126, 186, 255);
    public override int SpawnWeight => config.MoonSpawnWeight;
    public override bool KeepOnDeath => true;


    public override void Activate(Player player)
    {
        
    }
}