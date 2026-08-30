using LabApi.Loader.Features.Plugins;
using TarotCardsInSL.Cards;
using TarotCardsInSL.Subclasses;

namespace TarotCardsInSL;

public class TarotPlugin : Plugin<Config>
{
    public override string Name => "TarotCardsInSL";
    public override string Description => "Adds Tarot Cards from 'The Binding of Isaac' into SL";
    public override string Author => "acecrum";
    public override Version Version => new(1, 0, 0);
    public override Version RequiredApiVersion => new Version(1, 1, 7);

    public static CardManager CardManager { get; private set; } = null!;
    public static SubclassManager? SubclassManager { get; private set; }
    private EventHandlers? _eventHandlers;
    private TarotDatastoring? _dataStoring;

    private void RegisterCards()
    {
        CardManager.RegisterCard(new Fool(Config));
        CardManager.RegisterCard(new DebugCard());
        if (Config.EnableMagician)
        {
            CardManager.RegisterCard(new Magician(Config));
        }
        CardManager.RegisterCard(new HighPriestess(Config));
        CardManager.RegisterCard(new Emperor(Config));
        CardManager.RegisterCard(new Empress(Config));
        CardManager.RegisterCard(new Hierophant(Config));
        CardManager.RegisterCard(new Lovers(Config));
        CardManager.RegisterCard(new Chariot(Config));
        CardManager.RegisterCard(new Justice(Config));
        CardManager.RegisterCard(new Hermit(Config));
        CardManager.RegisterCard(new WheelOfFortune(Config));
        CardManager.RegisterCard(new Strength(Config));
        CardManager.RegisterCard(new HangedMan(Config));
        CardManager.RegisterCard(new Death(Config));
        CardManager.RegisterCard(new World(Config));
        CardManager.RegisterCard(new Temperance(Config));
        CardManager.RegisterCard(new Devil(Config));
    }
    
    public override void Enable()
    {
        CardManager = new CardManager(Config);
        RegisterCards();
        SubclassManager = new SubclassManager(CardManager, Config);
        _dataStoring = new TarotDatastoring("tarotdatastoring", Config);
        _eventHandlers = new EventHandlers(CardManager, _dataStoring, SubclassManager, Config);
        _eventHandlers.Register();
    }

    public override void Disable()
    {
        _eventHandlers?.Unregister();
        _eventHandlers = null;
        CardManager.Clear();
    }
}