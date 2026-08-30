using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public sealed class Devil(Config config) : CustomCard
{
    public override string ID => "devil";
    public override string Name => "XV | The Devil";
    public override CardType Type => CardType.Active;
    public override ItemType KeycardType => ItemType.KeycardCustomMetalCase;
    public override (int A, int B, int C) CardPerms => (2, 0, 1);
    public override string TechnicalDescription => "On activation, starts a timer that <color=red>kills you when it reaches 0.</color>\nYou deal 1.5x damage and recieve a 35% Movement Boost\nKilling players regenerates HP and grants further buffs.";
    public override string Description => "Revel in the power of darkness";
    public override Color GlowColor => new Color32(119, 63, 73, 255);
    public override int SpawnWeight => config.DevilSpawnWeight;


    public override void Activate(Player player)
    {
        if (player.Team == Team.SCPs)
        {
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    player.MaxHealth *= 1.05f;
                    player.Heal(player.MaxHealth/20);
                    break;
                case 1:
                    player.MaxHumeShield  *= 1.15f;
                    break;
                case 2:
                    if (player.Role == RoleTypeId.Scp106)
                    {
                        player.HumeShieldRegenRate *= 1.15f;
                    }
                    player.HumeShieldRegenCooldown -= 1.5f;
                    break;
            }
        }
        else 
        {
            var grenadePool = new[]
            { 
                ItemType.GrenadeHE, ItemType.GrenadeFlash
            };
            var keycardPool = new[]
            {
                ItemType.KeycardJanitor, ItemType.KeycardScientist, ItemType.KeycardResearchCoordinator, ItemType.KeycardZoneManager, ItemType.KeycardGuard, ItemType.KeycardMTFPrivate
            };
            var medicalPool = new[]
            {
                ItemType.Medkit, ItemType.Painkillers, ItemType.Adrenaline
            };
            var scpItemPool = new[]
            {
                ItemType.SCP207, ItemType.AntiSCP207, ItemType.SCP1853, ItemType.SCP500
            };
            var randomGrenade = grenadePool[UnityEngine.Random.Range(0, grenadePool.Length)];
            var randomKeycard = keycardPool[UnityEngine.Random.Range(0, keycardPool.Length)];
            var randomMedical = medicalPool[UnityEngine.Random.Range(0, medicalPool.Length)];
            var randomScpItem = scpItemPool[UnityEngine.Random.Range(0, scpItemPool.Length)];

            var items = new[]
            {
                randomGrenade, randomKeycard, randomMedical, randomScpItem
            };
            
            const float radius = 1.5f;
            var angleStep = 360f/items.Length;

            for (var i = 0; i < items.Length; i++)
            {
                var angle = angleStep * i * Mathf.Deg2Rad;
                var offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle)  * radius);

                if (player == null) continue;
                var spawnPosition = player.Position + offset;

                Pickup.Create(items[i], spawnPosition);
            }
        }
    }
}