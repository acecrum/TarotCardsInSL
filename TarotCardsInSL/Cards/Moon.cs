using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using RueI.API;
using RueI.API.Elements;
using TarotCardsInSL.UI;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Moon(Config config) : CustomCard
{
    public override string ID => "moon";
    public override string Name => "XVIII | The Moon";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 0);
    public override string TechnicalDescription => "On death, spectate a player and use the card to torment them for 1 minute.\nTormented players cannot pick up items, equip different items,\nand blacks out the current room they are in.\n<color=red>SCP's cannot be tormented</color>";
    public override string Description => "Bring them terror.";
    public override Color GlowColor => new Color32(123, 126, 186, 255);
    public override int SpawnWeight => config.MoonSpawnWeight;
    public override bool KeepOnDeath => true;

    public override void Activate(Player player)
    {
        if (player.IsAlive || player.CurrentlySpectating == null || player.CurrentlySpectating.IsSCP ||
            player.CurrentlySpectating.IsTutorial && !config.EnableTutorialInteractions)
        {
            TarotHints.CardFailHint(player);
            TarotPlugin.CardManager.RefundCard(player, this);
        }
        var savedTarget = player.CurrentlySpectating;
        
        PlayerEvents.ChangingItem += DenyChanging;
        PlayerEvents.PickingUpItem += DenyPickingUp;

        var room = savedTarget?.Room;

        room?.LightController?.FlickerLights(25f);
        
        Timing.RunCoroutine(Timer());

        Timing.CallDelayed(60f, () =>
        {
            PlayerEvents.ChangingItem -= DenyChanging;
            PlayerEvents.PickingUpItem -= DenyPickingUp;
        });
        return;

        IEnumerator<float> Timer()
        {
            var timer = 60;
            while (timer > 0)
            {
                if (savedTarget != null)
                {
                    RueDisplay.Get(savedTarget).Show(new BasicElement(250, $"<b><size=24><color={GlowColor.ToHex()}>{player.DisplayName} is tormenting you!\n{timer}</color></size></b>"), 1f);

                    RueDisplay.Get(player).Show(new BasicElement(85, $"<b><size=24><color={GlowColor.ToHex()}><align=right>Tormenting {savedTarget.DisplayName}: {timer}</align></color></size></b>"), 1f);
                }
                yield return Timing.WaitForSeconds(1f);
                timer--;
            }
        }

        void DenyChanging(PlayerChangingItemEventArgs ev)
        {
            if (ev.Player != savedTarget) return;
            ev.IsAllowed = false;
        }

        void DenyPickingUp(PlayerPickingUpItemEventArgs ev)
        {
            if (ev.Player != savedTarget) return;
            ev.IsAllowed = false;
        }
    }
}