using LabApi.Features.Wrappers;
using UnityEngine;

namespace TarotCardsInSL.Cards;

public class Groundcard(CustomCard card, LightSourceToy light, InteractableToy interactable) : MonoBehaviour
{
    public CustomCard Card { get; set; } = card;
    
    public LightSourceToy Light { get; set; } = light;

    public InteractableToy  Interactable { get; set; } = interactable;
}