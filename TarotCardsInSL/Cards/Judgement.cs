using LabApi.Features.Wrappers;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Judgement(Config config) : CustomCard
{
    public override string ID => "judgement";
    public override string Name => "XX | Judgement";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 3);
    public override string TechnicalDescription => "Kills you but, causes a respawn wave to instantly respawn on your side.\nYou will respawn as the highest ranking member on your team\nSCP's have 2 new SCPs take their place. Both SCP's health is halved.";
    public override string Description => "Judge lest ye be judged";
    public override Color GlowColor => new Color32(247, 144, 109, 255);
    public override int SpawnWeight => config.JudgementSpawnWeight;

    public override void Activate(Player player)
    {
        
    }
}