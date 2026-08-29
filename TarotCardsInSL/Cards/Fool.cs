using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Fool(Config config) : CustomCard
{
    public override string ID => "fool";
    public override string Name => "0 | The Fool";
    public override CardType Type => CardType.Passive;
    public override ItemType KeycardType => ItemType.KeycardCustomSite02;
    public override (int A, int B, int C) CardPerms => (0, 0, 0);
    public override string TechnicalDescription => "Grants the player 15% Movement Boost and 20% Damage Reduction\nwhile held";
    public override string Description => "Where journey begins.";
    public override Color GlowColor => Color.green;
    public override int SpawnWeight => config.FoolSpawnWeight;
    
    private readonly Dictionary<string, SavedEffects> _savedEffects = new();
    private sealed class SavedEffects
    {
        public byte SpeedIntensity { get; init; }
        public float SpeedDuration { get; init; }

        public byte DrIntensity { get; init; }
        public float DrDuration { get; init; }
    }

    public override void OnGiven(Player player)
    { 
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault(); 
        var existingDr = player.ActiveEffects.OfType<DamageReduction>().FirstOrDefault();
        
        _savedEffects[player.UserId] = new SavedEffects
        {
            SpeedIntensity = existingSpeed?.Intensity ?? 0,
            SpeedDuration = existingSpeed?.TimeLeft ?? 0,

            DrIntensity = existingDr?.Intensity ?? 0,
            DrDuration = existingDr?.TimeLeft ?? 0
        };
        
        player.EnableEffect<MovementBoost>(15);
        player.EnableEffect<DamageReduction>(40);
    }
    public override void OnRemoved(Player player)
    {
        if (!_savedEffects.TryGetValue(player.UserId, out var saved))
            return;
        
        player.EnableEffect<MovementBoost>(saved.SpeedIntensity, saved.SpeedDuration);
        player.EnableEffect<DamageReduction>(saved.DrIntensity, saved.DrDuration);

        _savedEffects.Remove(player.UserId);
    }
}