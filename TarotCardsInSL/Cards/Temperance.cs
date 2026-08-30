using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using RueI.API;
using RueI.API.Elements;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Temperance(Config config) : CustomCard
{
    public override string ID => "temperance";
    public override string Name => "XIV | Temperance";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 3, 0);
    public override string TechnicalDescription => "Decrease one stat to boost another.";
    public override string Description => "Trade with your body.";
    public override Color GlowColor => Color.green;
    public override int SpawnWeight => config.TemperanceSpawnWeight;

    public override void Activate(Player player) // todo: add more effects but these are good imo
    {
        var badEffectString = "";
        var goodEffectString = "";
        
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                player.EnableEffect<Slowness>(20);
                badEffectString = "Speed";
                break;
            case 1:
                PlayerEvents.Hurting += DamageTakenIncreased;
                PlayerEvents.Dying += DeleteThisShitBrah;
                badEffectString = "Damage Reduction";
                break;

                void DamageTakenIncreased(PlayerHurtingEventArgs ev)
                {
                    if (ev.Player != player) return;
                    if (ev.DamageHandler is not StandardDamageHandler damageHandler) return;
                    damageHandler.Damage *= 1.25f;
                }

                void DeleteThisShitBrah(PlayerDyingEventArgs ev)
                {
                    PlayerEvents.Hurting -= DamageTakenIncreased;
                    PlayerEvents.Dying -= DeleteThisShitBrah;
                }
            case 2:
                if (player.IsSCP)
                {
                    player.MaxHealth *= 0.8f;
                    player.Heal(player.MaxHealth/5);
                }
                else if (player.IsHuman)
                {
                    player.MaxHealth *= 0.65f;
                }
                badEffectString = "Max HP";
                break;
        }

        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0:
                player.EnableEffect<MovementBoost>(20);
                goodEffectString = "Speed";
                break;
            case 1:
                player.EnableEffect<DamageReduction>(45);
                goodEffectString = "Damage Reduction";
                break;
            case 2:
                if (player.IsSCP)
                {
                    player.MaxHealth *= 1.2f;
                    player.Heal(player.MaxHealth/5);
                }
                else if (player.IsHuman)
                {
                    player.MaxHealth *= 1.35f;
                }
                badEffectString = "Max HP";
                break;
            
        }
        
        RueDisplay.Get(player).Show(new BasicElement(200, $"<b><size=30><color=red>{badEffectString} down, </color><color=green>{goodEffectString} up.</color></size></b>"), 3f);
    }
}