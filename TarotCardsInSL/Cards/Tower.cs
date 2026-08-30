using LabApi.Features.Wrappers;
using MapGeneration;
using TarotCardsInSL.UI;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Tower(Config config) : CustomCard
{
    public override string ID => "tower";
    public override string Name => "XVI | The Tower";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomSite02;
    public override (int A, int B, int C) CardPerms => (0, 0, 3);
    public override string TechnicalDescription => "On use, every door/gate in the room you are currently in are forced open.\nThe doors/gates that are open become broken and can no longer close.\n<color=red>SCP-079's containment is excluded from this card's effect</color>";
    public override string Description => "Catastrophe.";
    public override Color GlowColor => new Color32(197, 173, 163, 255);
    public override int SpawnWeight => config.TowerSpawnWeight;


    public override void Activate(Player player)
    {
        if (player.Room == null || player.Room.Name == RoomName.Hcz079)
        {
            TarotHints.CardFailHint(player);
            TarotPlugin.CardManager.RefundCard(player, this);
            return;
        }
        foreach (var door in player.Room.Doors)
        {
            if (door is ElevatorDoor) continue;
            door.IsOpened = true;
            door.IsLocked = true;
        }
    }
}