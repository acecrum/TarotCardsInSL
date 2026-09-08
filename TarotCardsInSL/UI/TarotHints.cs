using RueI.API;
using RueI.API.Elements;
using LabApi.Features.Wrappers;
using PlayerRoles;
using Respawning.Objectives;
using TarotCardsInSL.Cards;

namespace TarotCardsInSL.UI;

public class TarotHints
{

    private static readonly Tag HeldCardTag = new("tarot-held-card");
    private static readonly Tag PickupCardTag = new("tarot-pickup-card");
    private static readonly Tag ConfigTag = new("tarot-config");
    
    public static void ShowHeldCardHint(Player player, CustomCard card)
    {
        var Type = card.Type == CardType.Passive ? "| P" : "| A";
        RueDisplay.Get(player).Show(HeldCardTag, new BasicElement(85, $"<align=right><b><size=24><color={card.GlowColor.ToHex()}>{card.Name} {Type}</color></size></b></align>"));
    }

    public static void ShowPickUpHint(Player player, CustomCard card, Config config)
    {
        var description = card.Description;
        var duration = 2.5f;
        if (config.EnableTechnicalHints)
        {
            description = card.TechnicalDescription;
            duration += 7.5f;
        }
        RueDisplay.Get(player).Show(PickupCardTag, new BasicElement(850, $"<b><color={card.GlowColor.ToHex()}>{card.Name}</color></b>\n{description}"), duration);
    }

    public static void ScpPickUpHint(Player player, Config config)
    {
        if (!config.EnableScpPickUp) return;
        var scpPlayer = player.RoleBase.Team == Team.SCPs && player.RoleBase.RoleTypeId != RoleTypeId.Scp079;
        if (!scpPlayer) return;
        RueDisplay.Get(player).Show(new BasicElement(200, "<b><size=30>As an <color=red>SCP</color> you can pick up and use <color=yellow>Tarot Cards.</color></size></b>"), 7f);
    }
    
    public static void Scp079Hint(Player player, Config config)
    {
        if (!config.EnableScpPickUp) return;
        var scp079Player = player.RoleBase.RoleTypeId == RoleTypeId.Scp079;
        if (!scp079Player) return;
        RueDisplay.Get(player).Show(new BasicElement(200, "<b><size=30>As <color=#6c6cff>SCP-079</color> you cannot grab <color=yellow>Tarot cards</color> however,\n you can craft a <color=yellow>Tarot card</color> that assists you every 3 minutes."), 10f);
        
    }

    public static void HideHeldCardHint(Player player)
    {
        RueDisplay.Get(player).Remove(HeldCardTag);
    }

    public static void BigDumbassConfigHint(Player player)
    {
        RueDisplay.Get(player).Show(ConfigTag, new BasicElement(250, "<b><color=red>HEY!</color></b>\n <size=24>You haven't set up your keybinds to use/drop Tarot Cards.\n Setup keybinds in Server Specific settings and press your Use or Drop key to close this message.\n<color=yellow> if you have DNT (Do Not Track) on, this message will always appear on joining.</color></size>"));
    }

    public static void HideBigDumbassConfigHint(Player player)
    {
        RueDisplay.Get(player).Remove(ConfigTag);
    }
    
    public static void TarotMasterHint(Player player)
    {
        RueDisplay.Get(player).Show(new BasicElement(200, $"<size=30><b>You are the Tarot Master,\n you are able to craft <color=yellow>Tarot Cards</color> for your <color={player.Role.GetRoleColor().ToHex()}>teammates</color> to use! \n You also recieve buffs to make you stronger.<b></size>"), 10f);
    }
    
    public static void RecieveHint(Player player, CustomCard card, Player master)
    {
        RueDisplay.Get(player).Show(new BasicElement(250, $"You recieved <color={card.GlowColor.ToHex()}>{card.Name}</color> from <color={master.Role.GetRoleColor().ToHex()}>{master.DisplayName}</color>!"), 3f);
    }

    public static void GiveHint(Player player, CustomCard card, Player target)
    {
        RueDisplay.Get(player).Show(new BasicElement(250, $"You gave <color={card.GlowColor.ToHex()}>{card.Name}</color> to <color={target.Role.GetRoleColor().ToHex()}>{target.DisplayName}</color>!"), 3f);
    }

    public static void MasterTimer(Player player, int seconds)
    {
        var minutes = seconds / 60;
        var remainingSeconds = seconds % 60;
        var timer = $"{minutes}:{remainingSeconds:00}";
        RueDisplay.Get(player).Show(new BasicElement(125, $"<align=right><b><size=24>Next card in {timer}</size></b></align>"), 1f);
    }
    
    public static void InactiveMasterTimer(Player player, int seconds)
    {
        var minutes = seconds / 60;
        var remainingSeconds = seconds % 60;
        var timer = $"{minutes}:{remainingSeconds:00}";
        RueDisplay.Get(player).Show(new BasicElement(125, $"<align=right><b><size=24>Next card in <color=#808080>{timer}</color></size></b></align>"), 1.1f);
    }
    
    public static void CardFailHint(Player player)
    {
        RueDisplay.Get(player).Show(new BasicElement(165, "<align=right><b><size=24><color=red>Nope!</color></size></b></align>"), 0.5f);
    }
    
    public static void ClearPickupCardHint(Player player)
    {
        RueDisplay.Get(player).Remove(PickupCardTag);
    }

}