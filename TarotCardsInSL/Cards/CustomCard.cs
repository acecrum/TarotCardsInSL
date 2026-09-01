using LabApi.Features.Wrappers;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public enum CardType
{
    Active,
    Passive
}

public abstract class CustomCard
{
    public abstract string ID { get; }
    public abstract string Name { get; }
    public abstract CardType Type { get; }
    public abstract ItemType KeycardType { get; }
    public abstract (int A, int B, int C) CardPerms { get; }
    public virtual Color PermissionColor => Color.black;
    public virtual Color LabelColor => Color.black;
    public virtual int Wear => 0;
    public abstract string TechnicalDescription { get; }
    public abstract string Description { get; }
    public abstract Color GlowColor { get; }
    public virtual float GlowRange => .2f;
    public virtual float GlowIntensity => .25f;
    public virtual int SpawnWeight => 100;
    public virtual bool KeepOnDeath => false;

    public virtual void Activate(Player player)
    {
    }
    public virtual void OnGiven(Player player)
    {
    }
    public virtual void OnRemoved(Player player)
    {
    }
}