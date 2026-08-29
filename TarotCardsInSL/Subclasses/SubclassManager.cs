using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using TarotCardsInSL.UI;
using UnityEngine;

namespace TarotCardsInSL.Subclasses;

public sealed class SubclassManager(CardManager cardManager, Config config)
{
    private readonly Dictionary<string, TarotMaster>  _master = new();
    
    // checks if the user is a tarot master
    public bool IsTarotMaster(Player player)
    {
        return _master.ContainsKey(player.UserId);
    }

    // removed on any role change that isn't escaping
    public void Remove(Player player)
    {
        if (!_master.TryGetValue(player.UserId, out var master)) return;
        master.Destroy();
        player.CustomInfo = string.Empty;
        player.InfoArea &= ~PlayerInfoArea.CustomInfo;
        _master.Remove(player.UserId);
    }
    
    // clears all keys once a round restarts 
    public void Clear()
    {
        _master.Clear();
    }

    private bool CanBecomeMaster(Player player)
    {
        return player.Role switch
        {

        // checks if a player is a blacklisted role (all main scp's, admin roles, and dead roles)
            RoleTypeId.Scp0492 => true,
            RoleTypeId.Tutorial => config.EnableTutorialInteractions,
            RoleTypeId.Spectator or RoleTypeId.Overwatch or RoleTypeId.None or RoleTypeId.Filmmaker or RoleTypeId.Destroyed => false, _ => player.Role.GetTeam() != Team.SCPs 
        };
    }

    // used in normal gameplay
    public bool TryAssignMaster(Player player)
    {
        if (_master.Count > config.TarotMasterSpawnCap) return false;
        if (!CanBecomeMaster(player)) return false;
        if (UnityEngine.Random.value > config.TarotMasterSpawnChance) return false;
        AssignMaster(player);
        return true;
    }

    // used in normal gameplay and in ra to skip valid role checks
    public void AssignMaster(Player player) // todo: use tophat instead of light
    {
        _master[player.UserId] = new TarotMaster(player, config);
        TarotHints.TarotMasterHint(player);
        player.CustomInfo = "Tarot Master";
        player.InfoArea |= PlayerInfoArea.CustomInfo;
        player.MaxHealth *= 1.5f;
        player.Health = player.MaxHealth;
        StartCrafting(player);
    }
    
    // the timer where the crafting cooldown occurs
    private IEnumerator<float> CraftLoop(Player player)
    {
        while (IsTarotMaster(player))
        {
            if (!_master.TryGetValue(player.UserId, out var tarotMaster)) yield break;
            var delay = UnityEngine.Random.Range(config.MininumCardCraftingTime, config.MaximumCardCraftingTime);
            tarotMaster.CraftTime = Time.time + delay;
            if (!IsTarotMaster(player)) yield break;
                
            var remaining = Mathf.CeilToInt(tarotMaster.CraftTime - Time.time);
            while (remaining > 0)
            {
                // dont judge me
                if (GetNearbyTeammate(player) != null)
                {
                    remaining -= 1;
                    TarotHints.MasterTimer(player, remaining);
                }
                else
                {
                    TarotHints.InactiveMasterTimer(player, remaining);
                }
                yield return Timing.WaitForSeconds(1f);
                if (!IsTarotMaster(player)) break;
            }
            TryCraftCard(player);
        }
    }
    
    private void StartCrafting(Player player)
    {
        Timing.RunCoroutine(CraftLoop(player));
    }

    // returns the role of the tarot master and a player near them and if they're the same team
    private bool IsTeammate(Player a, Player b)
    {
        return a.Role.GetTeam() == b.Role.GetTeam();
    }
    
    private List<Player> GetEligibleNearbyTeammate(Player master)
    {
        return Player.ReadyList.Where(player => player != master && IsTeammate(master, player) && !cardManager.HasCard(player) && Vector3.Distance(master.Position, player.Position) < 7.5f).ToList();
    }

    private Player? GetNearbyTeammate(Player master)
    {
        var teammates =  GetEligibleNearbyTeammate(master);
        
        return teammates.Count == 0 ? null : teammates[UnityEngine.Random.Range(0, teammates.Count)];
    }
    
    private void TryCraftCard(Player master)
    {
        var target = GetNearbyTeammate(master);
        if (target == null) return;

        var card = cardManager.GetRandomCard();
        if (card == null) return;
        if (!cardManager.TryGiveCard(target, card)) return;
        TarotHints.GiveHint(master, card, target);
        TarotHints.RecieveHint(target, card, master);
    }

    public void TarotMasterDropCards(Player master)
    {
        if (!IsTarotMaster(master)) return;
        for (var i = 0; i < 3; i++)
        {
            var cards = cardManager.GetRandomCard();
            if (cards == null) return;
            TarotPlugin.CardManager.SpawnCard(cards, master.Position);
        }
    }
}