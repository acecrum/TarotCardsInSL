using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Emperor(Config config) : CustomCard
{
    public override string ID => "emperor";
    public override string Name => "IV | The Emperor";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 3, 0);
    public override string TechnicalDescription => "Increases defense massively but slows you down for 10 seconds.";
    public override string Description => "Challenge me!";
    public override Color GlowColor => Color.yellow;
    public override int SpawnWeight => config.EmperorSpawnWeight;

    public override void Activate(Player player)
    {
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();
        var oldSpeedIntensity = existingSpeed?.Intensity ?? 0;
        var oldSpeedDuration = existingSpeed?.TimeLeft ?? 0;
        var existingDr = player.ActiveEffects.OfType<DamageReduction>().FirstOrDefault();
        var oldDrIntensity = existingDr?.Intensity ?? 0;
        var oldDrDuration = existingDr?.TimeLeft ?? 0;
        player.DisableEffect<MovementBoost>();
        player.EnableEffect<DamageReduction>(175, 10);
        player.EnableEffect<Slowness>(65, 10);
        
        Timing.CallDelayed(10f, () =>
        {
            player.EnableEffect<MovementBoost>(oldSpeedIntensity, oldSpeedDuration);
            player.EnableEffect<DamageReduction>(oldDrIntensity, oldDrDuration);
        });
    }
}