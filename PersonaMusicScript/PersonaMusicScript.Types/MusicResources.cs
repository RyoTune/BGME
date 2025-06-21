using PersonaMusicScript.Types.Games;
using PersonaMusicScript.Types.Serializer;
using YamlDotNet.Serialization.NamingConventions;

namespace PersonaMusicScript.Types;

public class MusicResources
{
    public static readonly Dictionary<Game, GameProperties> Games = new()
    {
        [Game.P4G_PC] = new()
        {
            TotalEncounters = 944,
            TotalFloors = 300,
        },

        [Game.P3P_PC] = new()
        {
            TotalEncounters = 1024,
            TotalFloors = 264,
        },

        [Game.P5R_PC] = new()
        {
            TotalEncounters = 1000,
        },

        [Game.P3R_PC] = new()
        {
            TotalEncounters = 3000,
        },

        [Game.Metaphor] = new()
        {
            TotalEncounters = 2000,
        },

        [Game.SMT5V] = new()
        {
            TotalEncounters = 3001,
        },
    };

    private readonly Game game;
    private readonly GameMusic gameMusic;
    private readonly Dictionary<int, int> cueAwbSet = new();

    public MusicResources(Game game, string? resourcesDir = null)
    {
        this.game = game;
        if (string.IsNullOrEmpty(resourcesDir))
        {
            this.ResourcesDir = Directory.CreateDirectory(game.GameFolder(AppDomain.CurrentDomain.BaseDirectory)).FullName;
        }
        else
        {
            this.ResourcesDir = Directory.CreateDirectory(game.GameFolder(resourcesDir)).FullName;
        }

        this.Constants = Games.ContainsKey(game) ? Games[game] : new();
        this.Collections = this.GetCollections();

        this.gameMusic = this.GetGameMusic();

        if (game != Game.P3R_PC)
        {
            this.cueAwbSet = this.gameMusic.Tracks
                .Where(x => x.CueId != 0)
                .ToDictionary(x => x.CueId, x => int.Parse(Path.GetFileNameWithoutExtension(x.OutputPath)!));
        }

        var songCueIdData = this.gameMusic.Tracks.ToDictionary(x => x.Name, y => y.CueId);
        this.Songs = new(songCueIdData, StringComparer.OrdinalIgnoreCase);
    }

    public string ResourcesDir { get; }

    public Dictionary<string, int> Songs { get; }

    public Dictionary<string, int[]> Collections { get; }

    public GameProperties Constants { get; }

    public string? GetDefaultEncoder()
        => this.gameMusic.DefaultEncoder;

    public string GetReplacementPath(int bgmId)
    {
        var awbIndex = this.GetAwbIndex(bgmId);
        return this.game switch
        {
            Game.P5R_PC => (bgmId >= 10000)
            ? $"FEmulator/AWB/BGM_42.AWB/{awbIndex}.adx"
            : $"{this.gameMusic.DefaultOutputPath}/{awbIndex}.adx",

            Game.P4G_PC => $"{this.gameMusic.DefaultOutputPath}/{awbIndex}.hca",

            Game.P3P_PC => $"{this.gameMusic.DefaultOutputPath}/{awbIndex}.adx",
            _ => throw new Exception("Unknown game."),
        };
    }

    private GameMusic GetGameMusic()
    {
        var musicFile = Path.Join(this.ResourcesDir, "music.yaml");
        if (!File.Exists(musicFile))
        {
            return new();
        }

        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var gameMusic = deserializer.Deserialize<GameMusic>(File.ReadAllText(musicFile));
        return gameMusic;
    }

    private Dictionary<string, int[]> GetCollections()
    {
        var collectionsDir = Path.Join(this.ResourcesDir, "collections");
        var collections = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(collectionsDir))
        {
            foreach (var file in Directory.EnumerateFiles(collectionsDir, "*.enc", SearchOption.AllDirectories))
            {
                var collection = CollectionSerializer.DeserializeFile(file);
                collections.Add(Path.GetFileNameWithoutExtension(file), collection);
            }
        }

        // Add normal battles collection
        // by inversing the Special Battles collection.
        var normalBattles = new List<int>();
        if (collections.TryGetValue("Special Battles", out var specialBattles))
        {
            for (int i = 0; i < this.Constants.TotalEncounters; i++)
            {
                if (!specialBattles.Contains(i))
                {
                    normalBattles.Add(i);
                }
            }
        }

        collections.Add("Normal Battles", normalBattles.ToArray());
        return collections;
    }

    private int GetAwbIndex(int bgmId)
    {
        // BGM ID is a Cue ID, get Cue AWB index.
        if (this.cueAwbSet.TryGetValue(bgmId, out var cueAwbIndex))
        {
            return cueAwbIndex;
        }

        return this.game switch
        {
            Game.P5R_PC => (bgmId >= 10000) ? bgmId - 10000 : bgmId,
            _ => bgmId,
        };
    }
}

public class GameProperties
{
    public int TotalEncounters { get; init; }

    public int TotalFloors { get; init; }
}

internal record SongCueAwb(int CueId, int AwbIndex);