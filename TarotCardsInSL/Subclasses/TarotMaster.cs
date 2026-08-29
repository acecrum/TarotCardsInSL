using LabApi.Features.Wrappers;
using UnityEngine;

namespace TarotCardsInSL.Subclasses;

public sealed class TarotMaster
{
    public Player Player { get; }
    public LightSourceToy? Light { get; private set; }
    public float CraftTime { get; set; }

    private readonly Config _config;
    
    public TarotMaster(Player player, Config config)
    {
        _config = config;
        Player = player;
        CreateLight();
    }

    private void CreateLight()
    {
        if (Player.GameObject == null || !_config.EnableTarotMasterLight) return;
        
        Light = LightSourceToy.Create(new Vector3(0f, -0.5f, 0f), Quaternion.identity, Player.GameObject.transform, networkSpawn: false);
        Light.Color = _config.TarotMasterLight;
        Light.Intensity = 0.5f;
        Light.Range = 1.5f;
        Light.Spawn();
    }
    

    public void Destroy()
    {
        if (Light is { IsDestroyed: false })
        {
            Light.Destroy();
        }
        Light = null;
    }

}