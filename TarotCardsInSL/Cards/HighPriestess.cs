using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace TarotCardsInSL.Cards;

public sealed class HighPriestess(Config config) : CustomCard
{
    public override string ID => "priestess";
    public override string Name => "II | The High Priestess";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (1, 1, 1);
    public override string TechnicalDescription => "Recieve SCP-1344 for 8 seconds and a 35% speed boost\n when no players are around";
    public override string Description => "The O5 is watching you.";
    public override Color GlowColor => Color.cyan;
    public override int SpawnWeight => config.EmpressSpawnWeight;


    public override void Activate(Player player)
    {
        Logger.Info($"{player.DisplayName} ({player.UserId}) has begun using Hermit.");
        var enablePriestessLoop = true;
        var existingSpeed = player.ActiveEffects.OfType<MovementBoost>().FirstOrDefault();
        var oldSpeedDuration = existingSpeed?.Duration ?? 0;
        var oldSpeedIntensity = existingSpeed?.Intensity ?? 0;
        var existingGoggles = player.ActiveEffects.OfType<Scp1344>().FirstOrDefault();
        var oldGogglesDuration = existingGoggles?.Duration ?? 0;
        var oldGogglesIntensity = existingGoggles?.Intensity ?? 0;
        player.EnableEffect<Scp1344>(1, 8);
        
        Timing.RunCoroutine(HermitLoop(player));
        
        
        IEnumerator<float> HermitLoop(Player hermit)
        {
            const float radius = 10f;
            const byte speedAmount = 35;

            while (enablePriestessLoop)
            {
                var someoneNear = Player.ReadyList.Any(other => other != hermit && other.IsAlive && Vector3.Distance(hermit.Position, other.Position) <= radius);

                switch (someoneNear)
                {
                    case true:
                        player.EnableEffect<MovementBoost>(oldSpeedIntensity);
                        break;
                    case false:
                        player.EnableEffect<MovementBoost>((byte)(oldSpeedIntensity + speedAmount));
                        break;
                }
                yield return Timing.WaitForSeconds(.25f);
            }
        }
        
        Timing.CallDelayed(8f, () =>
        {
            player.EnableEffect<Scp1344>(oldGogglesIntensity, oldGogglesDuration);
        });
        
        Timing.CallDelayed(30f, () =>
        {
            enablePriestessLoop = false;
            player.EnableEffect<MovementBoost>(oldSpeedIntensity, oldSpeedDuration);
            Logger.Info($"{player.DisplayName} ({player.UserId}) has finished using Hermit.");
        });
    }
}