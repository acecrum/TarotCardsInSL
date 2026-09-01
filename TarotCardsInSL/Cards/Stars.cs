using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp106Events;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.Arguments.Scp3114Events;
using LabApi.Events.Arguments.Scp939Events;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using Scp914;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Stars(Config config) : CustomCard
{
    public override string ID => "stars";
    public override string Name => "XVII | The Stars";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomTaskForce;
    public override (int A, int B, int C) CardPerms => (0, 0, 3);
    public override string TechnicalDescription => "<color=red>Removes every item from the players inventory and disables picking up.</color>\nAfter a minute, replaces them with direct upgrades.\nSCP's abilities and attacking is disabled for 90 seconds instead\nSCP's recieve either Damage Reduction or Speed";
    public override string Description => "Let go and find what you need.";
    public override Color GlowColor => new Color32(251, 225, 114, 255);
    public override int SpawnWeight => config.StarsSpawnWeight;


    public override void Activate(Player player)
    {
        var duration = player.IsSCP ? 90f : 60f;
        var savedItems = player.IsHuman ? player.Items.Select(item => item.Type).ToList() : new List<ItemType>();
        var totalAmmo = player.Ammo.Values.Sum(x => (int)x);

        var cleanedUp = false;
        CoroutineHandle timer = default;

        PlayerEvents.Death += OnDeath;
        PlayerEvents.Hurting += BlockOutgoingDamage;
        
        if (player.IsHuman)
        {
            PlayerEvents.PickingUpItem += DenyPickup;
            player.ClearInventory();
        }
        else
        {
            SubscribeScpEvents();
        }
        
        timer = Timing.RunCoroutine(EffectTimer());
        
        return;

        ItemType? GetAmmoType(ItemType gun)
        {
            return gun switch
            {
                ItemType.GunCOM15 or
                    ItemType.GunCOM18 or
                    ItemType.GunCom45 or
                    ItemType.GunCrossvec or
                    ItemType.GunFSP9
                    => ItemType.Ammo9x19,

                ItemType.GunE11SR or
                    ItemType.GunFRMG0
                    => ItemType.Ammo556x45,

                ItemType.GunAK or
                    ItemType.GunA7 or
                    ItemType.GunLogicer
                    => ItemType.Ammo762x39,

                ItemType.GunShotgun
                    => ItemType.Ammo12gauge,

                ItemType.GunRevolver
                    => ItemType.Ammo44cal,

                _ => null
            };
        }

        void OnDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player != player) return;
            Cleanup();
        }    
        
        void BlockOutgoingDamage(PlayerHurtingEventArgs ev)
        {
            if (ev.Attacker != player)
                return;

            ev.IsAllowed = false;
        }

        void DenyPickup(PlayerPickingUpItemEventArgs ev)
        {
            if (ev.Player != player)
                return;

            ev.IsAllowed = false;
        }
        
        IEnumerator<float> EffectTimer()
        {
            yield return Timing.WaitForSeconds(duration);

            if (player.IsHuman)
            {
                RestoreInventory();
            }
            else
            {
                SCPBuffs();
            }

            cleanedUp = false;
        }
                    
            void Cleanup(bool killTimer = true)
            {
                if (cleanedUp)
                    return;

                cleanedUp = true;

                if (killTimer && timer.IsValid)
                    Timing.KillCoroutines(timer);

                PlayerEvents.Death -= OnDeath;
                PlayerEvents.Hurting -= BlockOutgoingDamage;
                PlayerEvents.PickingUpItem -= DenyPickup;

                Scp049Events.UsingSense -= DisableGoodSense;
                Scp049Events.UsingDoctorsCall -= DisableDoctorCall;
                Scp049Events.StartingResurrection -= DisableRes;

                Scp106Events.UsingHunterAtlas -= DisableAtlas;
                Scp106Events.ChangingStalkMode -= DisableStalk;

                Scp939Events.CreatingAmnesticCloud -= DisableCloud;
                Scp939Events.MimickingEnvironment -= DisableSpeaking;

                Scp173Events.CreatingTantrum -= DisableShitting;

                Scp096Events.Enraging -= DisableScopophobia;

                Scp3114Events.Disguising -= DisableDisguise;
                Scp3114Events.StrangleStarting -= DisableStrangle;
            }
            
            void RestoreInventory()
            {
                PlayerEvents.PickingUpItem -= DenyPickup;

                foreach (var itemType in savedItems)
                {
                    var processor = LabApi.Features.Wrappers.Scp914.GetItemProcessor(itemType);
                    
                    if (processor == null)
                    {
                        player.AddItem(itemType);
                        continue;
                    }
                    var item = player.AddItem(itemType);

                    if (item == null) continue;

                    processor.UpgradeItem(Scp914KnobSetting.Fine, item);

                    var gunsByAmmo = player.Items.Select(item => GetAmmoType(item.Type)).OfType<ItemType>().GroupBy(ammo => ammo)
                        .ToDictionary(group => group.Key, group => group.Count());
                    if (gunsByAmmo.Count <= 0) continue;
                    var ammoPerType = totalAmmo / gunsByAmmo.Count;

                    foreach (var ammoType in gunsByAmmo)
                    {
                        player.SetAmmo(ammoType.Key,(ushort)ammoPerType);
                    }
                }
            }
            
            void SubscribeScpEvents()
            {
                switch (player.Role)
                {
                    case RoleTypeId.Scp049:
                        Scp049Events.UsingSense += DisableGoodSense;
                        Scp049Events.UsingDoctorsCall += DisableDoctorCall;
                        Scp049Events.StartingResurrection += DisableRes;
                        break;

                    case RoleTypeId.Scp106:
                        Scp106Events.UsingHunterAtlas += DisableAtlas;
                        Scp106Events.ChangingStalkMode += DisableStalk;
                        break;

                    case RoleTypeId.Scp939:
                        Scp939Events.CreatingAmnesticCloud += DisableCloud;
                        Scp939Events.MimickingEnvironment += DisableSpeaking;
                        break;

                    case RoleTypeId.Scp173:
                        Scp173Events.CreatingTantrum += DisableShitting;
                        break;

                    case RoleTypeId.Scp096:
                        Scp096Events.Enraging += DisableScopophobia;
                        break;

                    case RoleTypeId.Scp3114:
                        Scp3114Events.Disguising += DisableDisguise;
                        Scp3114Events.StrangleStarting += DisableStrangle;
                        break;
                }
            }

            void SCPBuffs()
            {
                switch (UnityEngine.Random.Range(0, 2))
                {
                    case 0:
                        player.EnableEffect<MovementBoost>(20);
                        RueDisplay.Get(player).Show(new BasicElement(200, $"<b><size=30><color={GlowColor.ToHex()}>20% Speed boost</color></size></b>"), 3f);
                        break;
                    case 1:
                        player.EnableEffect<DamageReduction>(50);
                        RueDisplay.Get(player).Show(new BasicElement(200, $"<b><size=30><color={GlowColor.ToHex()}>25% Damage Reduction</color></size></b>"), 3f);
                        break;
                }
            }
            
            void DisableGoodSense(Scp049UsingSenseEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableDoctorCall(Scp049UsingDoctorsCallEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableRes(Scp049StartingResurrectionEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableAtlas(Scp106UsingHunterAtlasEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableStalk(Scp106ChangingStalkModeEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableCloud(Scp939CreatingAmnesticCloudEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableSpeaking(Scp939MimickingEnvironmentEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableShitting(Scp173CreatingTantrumEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }

            void DisableScopophobia(Scp096EnragingEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableDisguise(Scp3114DisguisingEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
            void DisableStrangle(Scp3114StrangleStartingEventArgs ev)
            {
                if (ev.Player == player)
                    ev.IsAllowed = false;
            }
    }
}