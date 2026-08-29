using System.Text.Json;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace TarotCardsInSL;

public class TarotDatastoring
{
    private readonly string _filePath;
    private Dictionary<string, DisableThatFuckingHintThatIHate> _storage = new();
    
    private readonly Config _config;

    public class DisableThatFuckingHintThatIHate
    {
        public bool Disabled { get; set; }
    }
    
    public TarotDatastoring(string configDirectory, Config config)
    {
        _config = config;
        
        Directory.CreateDirectory(configDirectory);
        _filePath = Path.Combine(configDirectory, "NotificationDisabling.json");
        Load();
    }

    public DisableThatFuckingHintThatIHate GetDnt(Player? player)
    {
        if (player == null || player.DoNotTrack || !_config.EnableDataStoring) return new DisableThatFuckingHintThatIHate();

        return _storage.TryGetValue(player.UserId, out var prefs) ? prefs : new DisableThatFuckingHintThatIHate();
    }

    public void Save(Player player, DisableThatFuckingHintThatIHate newsettings)
    {
        if (player.DoNotTrack)
        {
            if (_storage.Remove(player.UserId))
            {
                WriteToFile();
            }
            return;
        }
        _storage[player.UserId] = newsettings;
        WriteToFile();
    }

    private void WriteToFile()
    {
        try
        {
            var json = JsonSerializer.Serialize(_storage, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Logger.Error($"[TarotDatastoring] Error writing to {_filePath}: {ex.Message}]");
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return;
            var json = File.ReadAllText(_filePath);
            _storage = JsonSerializer.Deserialize<Dictionary<string, DisableThatFuckingHintThatIHate>>(json) ??
                       new Dictionary<string, DisableThatFuckingHintThatIHate>();
        }
        catch (Exception ex)
        {
            Logger.Error($"[TarotDatastoring] Error loading {_filePath}: {ex.Message}");
        }
    }
}
