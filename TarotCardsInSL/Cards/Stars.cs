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
    public override string TechnicalDescription => "<color=red>Removes every item from the players inventory and disables picking up.</color>\nAfter a minute, replaces them with direct upgrades.\nSCP's abilities and attacking is disabled for 90 seconds instead\nSCP's recieve a buff tailored to each SCP";
    public override string Description => "Let go and find what you need.";
    public override Color GlowColor => new Color32(251, 225, 114, 255);
    public override int SpawnWeight => config.StarsSpawnWeight;


    public override void Activate(Player player)
    {
        var duration = player.IsSCP ? 90f : 60f;
        var savedItems = player.IsHuman ? player.Items.Select(item => item.Type).ToList() : new List<ItemType>();
        
        bool cleanedUp = false;
        CoroutineHandle timer = default;

        PlayerEvents.Death += OnDeath;
        PlayerEvents.Hurting += BlockOutgoingDamage;
        
        SubscribeScpEvents();
        
        if (player.IsHuman)
        {
            PlayerEvents.PickingUpItem += DenyPickup;
            player.ClearInventory();
        }
        
        timer = Timing.RunCoroutine(EffectTimer());
        
        return;
        
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