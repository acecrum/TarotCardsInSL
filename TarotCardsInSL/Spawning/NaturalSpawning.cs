using LabApi.Features.Wrappers;
using MapGeneration;
using TarotCardsInSL.Cards;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace TarotCardsInSL.Spawning;

public class NaturalSpawning(CardManager cardManager)
{
    private class SpawnPoints(RoomName room, Vector3 localPosition, float spawnChance)
    {
        public RoomName Room { get; } = room;
        public Vector3 LocalPosition { get; } = localPosition;
        public float SpawnChance { get; } = spawnChance;
    }
    
    private readonly Dictionary<LockerChamber, CustomCard> _lockerCards = new();
    private readonly List<SpawnPoints> _spawnPoints =
    [
        new(RoomName.Lcz173, new Vector3(-2.50f, 12.58f, -3.61f), 1f),
        new(RoomName.Lcz914, new Vector3(3.68f, 1.81f, 1.12f), 1f),
        new(RoomName.Lcz330, new Vector3(0.71f, 1.01f, -2.43f), 1f),
        new(RoomName.LczArmory, new Vector3(0.20f, 0.62f, -3.68f), 1f),
        new(RoomName.LczGlassroom, new Vector3(8.21f, 1.64f, -5.89f), 1f),
        new(RoomName.LczGreenhouse, new Vector3(-6.21f, 0.11f, 6.66f), .25f),
        new(RoomName.LczToilets, new Vector3(0.82f, 0.57f, -4.68f), .15f),
        new(RoomName.LczToilets, new Vector3(-0.87f, 0.57f, -4.70f), .15f),
        new(RoomName.LczToilets, new Vector3(-0.88f, 0.57f, -6.18f), .15f),
        new(RoomName.LczComputerRoom, new Vector3(-2.57f, 0.96f, 4.35f), 0.05f),
        new(RoomName.LczComputerRoom, new Vector3(-2.36f, 0.96f, 1.17f), 0.05f),
        new(RoomName.LczComputerRoom, new Vector3(-2.58f, 0.96f, 1.09f), 0.05f),
        new(RoomName.LczComputerRoom, new Vector3(-2.32f, 0.96f, -4.25f), 0.05f),
        new(RoomName.LczComputerRoom, new Vector3(7.95f, 0.15f, -4.05f), 0.05f),
        new(RoomName.LczComputerRoom, new Vector3(7.85f, 0.96f, 4.19f), 0.05f),
        new(RoomName.Hcz127, new Vector3(0.90f, 0.95f, -0.72f), 1f),
        new(RoomName.Hcz127, new Vector3(2.76f, 0.98f, 3.88f), 0.25f),
        new(RoomName.Hcz049, new Vector3(3.46f, 0.40f, 3.45f), 0.50f),
        new(RoomName.Hcz096, new Vector3(-2.93f, 0.15f, 0.87f), 1f),
        new(RoomName.HczAcroamaticAbatement, new Vector3(-3.26f, 0.42f, 5.02f), 0.25f),
        new(RoomName.Hcz939, new Vector3(-3.94f, 0.98f, 6.03f), 0.5f),
        new(RoomName.Hcz106, new Vector3(11.27f, 1.03f, -20.98f), 1f),
        new(RoomName.HczMicroHID, new Vector3(1.95f, 4.65f, 1.81f), 1f),
        new(RoomName.HczServers, new Vector3(-1.85f, 2.59f, 0.63f), 0.25f),
        new(RoomName.HczServers, new Vector3(1.46f, 2.67f, 2.17f), 0.25f),
        new(RoomName.HczServers, new Vector3(2.63f, -4.35f, -6.58f), 0.25f),
        new(RoomName.HczRampTunnel, new Vector3(-3.47f, 1.17f, 1.45f), 0.35f),
        new(RoomName.Hcz079, new Vector3(6.04f, -2.27f, -5.08f), 1f),
        new(RoomName.Hcz079, new Vector3(2.41f, -2.49f, -8.20f), 1f),
        new(RoomName.Hcz079, new Vector3(3.23f, -1.41f, -4.26f), 1f),
        new(RoomName.HczWarhead, new Vector3(33.38f, -70.25f, -9.30f), 1f),
        new(RoomName.HczWarhead, new Vector3(29.41f, -75.37f, 9.70f), 1f),
        new(RoomName.HczWarhead, new Vector3(0.34f, -71.01f, 0.53f), 0.65f),
        new(RoomName.HczWarhead, new Vector3(13.21f, -73.39f, 2.77f), 0.25f),
        new(RoomName.Hcz049, new Vector3(-3.45f, 90.00f, 4.45f), 0.75f),
        new(RoomName.Hcz049, new Vector3(-5.47f, 90.93f, -3.06f), 1f),
        new(RoomName.HczArmory, new Vector3(0.39f, 1.04f, -1.79f), 1f),
        new(RoomName.HczCheckpointToEntranceZone, new Vector3(-6.91f, 0.90f, -5.59f), 0.5f),
        new(RoomName.EzIntercom, new Vector3(-6.84f, -4.28f, -1.85f), 0.35f),
        new(RoomName.EzIntercom, new Vector3(-5.51f, -4.86f, 2.78f), 0.25f),
        new(RoomName.EzOfficeStoried, new Vector3(-3.94f, 1.85f, -3.43f), 0.25f),
        new(RoomName.EzOfficeStoried, new Vector3(-0.99f, 1.85f, 4.59f), 0.25f),
        new(RoomName.EzOfficeLarge, new Vector3(-2.30f, 1.64f, -6.96f), 0.05f),
        new(RoomName.EzOfficeLarge, new Vector3(-2.84f, 1.64f, -6.94f), 0.05f),
        new(RoomName.EzOfficeLarge, new Vector3(-3.68f, 1.64f, -6.95f), 0.05f),
        new(RoomName.EzOfficeLarge, new Vector3(-4.32f, 1.64f, -6.93f), 0.05f),
        new(RoomName.EzGateA, new Vector3(1.00f, 4.18f, 0.19f), 0.5f),
        new(RoomName.EzGateA, new Vector3(4.39f, 0.15f, 6.24f), 0.75f),
        new(RoomName.EzGateB, new Vector3(2.20f, 0.16f, -9.23f), 0.5f),
        new(RoomName.EzGateB, new Vector3(0.40f, 3.50f, 0.65f), 0.75f),
        new(RoomName.EzOfficeSmall, new Vector3(1.60f, -1.28f, -4.66f), 0.35f),
        new(RoomName.EzOfficeSmall, new Vector3(7.16f, -0.39f, 3.28f), 0.25f),
    ];
    
    public void SpawnCards()
    {
        foreach (var spawnPoint in _spawnPoints)
        {
            if (UnityEngine.Random.value > spawnPoint.SpawnChance) continue;
            
            var card = cardManager.GetRandomCard();
            if (card == null) continue;

            var rooms = Room.Get(spawnPoint.Room).ToList();
            if (rooms.Count == 0) continue;

            var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
            var worldPosition = room.Transform.TransformPoint(spawnPoint.LocalPosition);
            cardManager.SpawnCard(card, worldPosition);
        }
    }
    public void PrepareLockers()
    {
        _lockerCards.Clear();
        
        foreach (var locker in StandardLocker.List)
        {
            if(!locker.GameObject.name.Contains("MiscLocker")) continue;
            
            if (UnityEngine.Random.value > 0.25f) continue;
            
            var card = cardManager.GetRandomCard();
            if (card == null) continue;
            
            var chambers = locker.Chambers.Where(chamber => chamber.CanInteract).ToList();
            if (chambers.Count == 0 ) continue;
            
            var chamber = chambers[UnityEngine.Random.Range(0, chambers.Count)];
            _lockerCards[chamber] = card;
        }
    }

    public bool SpawnLockerCard(LockerChamber chamber)
    {
        Logger.Info($"Chamber {chamber.Id} was opened, Pending card: {_lockerCards.ContainsKey(chamber)}");
        if (!_lockerCards.TryGetValue(chamber, out var card)) return false;
        
        _lockerCards.Remove(chamber);
        cardManager.SpawnCardInLocker(card, chamber);
        Logger.Info($"spawned {card.Name} from locker");
        return true;
    }
}