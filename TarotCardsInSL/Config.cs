using UnityEngine;

namespace TarotCardsInSL;

public class Config
{
    // enables or disables the plugin.
    public bool IsEnabled { get; set; } = true;
    // activates debug mode for the plugin
    public bool Debug { get; set; } = false;
    public int FoolSpawnWeight { get; set; } = 80;
    public int PriestessSpawnWeight { get; set; } = 75;
    public int EmperorSpawnWeight { get; set; } = 50;
    public int EmpressSpawnWeight { get; set; } = 50;
    public int HierophantSpawnWeight { get; set; } = 75;
    public int LoversSpawnWeight { get; set; } = 75;
    public int ChariotSpawnWeight { get; set; } = 25;
    public int JusticeSpawnWeight { get; set; } = 35;
    public int HermitSpawnWeight { get; set; } = 60;
    public int WheelSpawnWeight { get; set; } = 45;
    public int StrengthSpawnWeight { get; set; } = 25;
    public int HangedManSpawnWeight { get; set; } = 50;
    public int DeathSpawnWeight { get; set; } = 75;
    public int WorldSpawnWeight  { get; set; } = 80;
    public int TemperanceSpawnWeight { get; set; } = 75;
    public int DevilSpawnWeight  { get; set; } = 25;
    public int TowerSpawnWeight { get; set; } = 65;
    public int StarsSpawnWeight { get; set; } = 25;
    public int MoonSpawnWeight { get; set; } = 50;
    public int SunSpawnWeight { get; set; } = 25;
    public int JudgementSpawnWeight { get; set; } = 15;
    // this disables card lights from being able to spawn in, disable this if you want people to guess if its a tarot card or not
    public bool EnableCardLights  { get; set; } = true;
    // Magician is disabled by default because when a player is using it, it looks like theyre rage hacking lol
    // enable natural spawns at your own risks, im not responsible for any players that maybe falsely banned with this card
    public bool EnableMagician { get; set; } = false;
    public int MagicianSpawnWeight { get; set; } = 0;
    // this is the spawn chance for a tarot master, what else can i say. set this to 0f to disable tarot master spawning
    public float TarotMasterSpawnChance { get; set; } = 0.05f;
    // this is the spawn cap for the amount of tarot masters that can be alive in a round, increasing this allows for more to spawn. setting this to 0 also disables master spawning.
    public int TarotMasterSpawnCap { get; set; } = 1;
    // this is the card crafting time for the tarot master, mininum is the lowest time it takes to create a card, maximum is the highest time to craft a card.
    public float MininumCardCraftingTime { get; set; } = 90f;
    public float MaximumCardCraftingTime { get; set; } = 150f;
    // enables or disables the light seen on the tarot master
    public bool EnableTarotMasterLight { get; set; } = true;
    // sets the color for tarot master
    public Color TarotMasterLight { get; set; } = Color.cyan;
    // this is used to track if a player has set their binds correctly so the hint on join doesnt appear (thank you nw).
    // set this to false if you dont want data storing, when set to false all future datastoring will be denied and players will have the hint telling them to set their binds pop up on join
    public bool EnableDataStoring { get; set; } = true;
    // this is used to tell a player to set their binds in the server specific settings because theyre not set by default (thank you nw).
    // setting this to false will not allow this to appear when a player joins.
    public bool EnableReminderHint { get; set; } = true;
    // when picking up a card, instead of giving a vague hint seen in the binding of isaac, you will see exactly what the card does.
    public bool EnableTechnicalHints { get; set; } = false;
    // if other plugins utilize the Tutorial class for gameplay/non admin reasons you can change this to true and card interactions will be able to work with Tutorials
    // this also allows tarot master to spawn for Tutorials
    public bool EnableTutorialInteractions  { get; set; } = false;
    // This allows SCP's to grab and use tarot cards, toggle this to false if you do not want SCP's to be able to grab any cards. (this disables the scp pick up hint as well)
    public bool EnableScpPickUp { get; set; } = true;
}