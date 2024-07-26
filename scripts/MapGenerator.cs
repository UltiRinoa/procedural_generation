using System;
using Godot;
using static Godot.BaseButton;

namespace cmos.Test;

public enum DrawMode { NoiseMap, ColorMap }

[Tool]
public partial class MapGenerator : Node
{
    private Gradient _colorRamp = new();
    private int _mapWidth = 128;
    private int _mapHeight = 128;
    private float _noiseScale = 1;
    private int _octaves = 5;
    private int _seed;
    private float _persistance = 0.5f;
    private float _lacunarity = 1f;
    private Vector2 _offset;
    private DrawMode _drawMode;

    [Export(PropertyHint.Enum)]
    public DrawMode DrawMode
    {
        set
        {
            _drawMode = value;
            GenerateMap();
        }
        get => _drawMode;
    }
    [Export]
    public Gradient ColorRamp
    {
        set
        {
            _colorRamp = value;
            GenerateMap();
        }
        get => _colorRamp;
    }

    [ExportGroup("Noise")]
    [Export]
    public int MapWidth
    {
        set
        {
            _mapWidth = Math.Max(1, value);
            GenerateMap();
        }
        get => _mapWidth;
    }


    [Export]
    public int MapHeight
    {
        set
        {
            _mapHeight = Math.Max(1, value);
            GenerateMap();
        }
        get => _mapHeight;
    }


    [Export(PropertyHint.Range, "0.0001, 50, 0.0001, or_greater")]
    public float MapScale
    {
        set
        {
            _noiseScale = value;
            GenerateMap();
        }
        get => _noiseScale;
    }

    [Export(PropertyHint.Range, "1, 20, 1, or_greater")]
    public int Octaves
    {
        set
        {
            _octaves = Math.Max(0, value);
            GenerateMap();
        }
        get => _octaves;
    }

    [Export(PropertyHint.Range, "0, 1, 0.001")]
    public float Persistance
    {
        set
        {
            _persistance = value;
            GenerateMap();
        }
        get => _persistance;
    }

    [Export]
    public float Lacunarity
    {
        set
        {
            _lacunarity = Math.Max(1, value);
            GenerateMap();
        }
        get => _lacunarity;
    }

    [Export]
    public int Seed
    {
        set
        {
            _seed = value;
            GenerateMap();
        }
        get => _seed;
    }

    [Export]
    public Vector2 Offset
    {
        set
        {
            _offset = value;
            GenerateMap();
        }
        get => _offset;
    }

    public override void _ExitTree()
    {
        _colorRamp.Changed -= GenerateMap;
    }

    public override void _EnterTree()
    {
        _colorRamp.Changed += GenerateMap;
    }

    public void GenerateMap()
    {
        var noiseMap = NoiseGenerator.Instance.GenerateNoiseMap(MapWidth, MapHeight, Seed, MapScale, Octaves, Persistance, Lacunarity, Offset);

        var colorMap = new Color[MapWidth, MapHeight];

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                float currHeight = noiseMap[x, y];
                colorMap[x, y] = ColorRamp.Sample(currHeight);
            }
        }

        switch (DrawMode)
        {
            case DrawMode.NoiseMap:
                GetNode<MapDisplay>("%MapDisplay").DrawTexture(TextureGenerator.Instance.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColorMap:
                GetNode<MapDisplay>("%MapDisplay").DrawTexture(TextureGenerator.Instance.TextureFromColorMap(colorMap));
                break;
        }
    }

    [Export(PropertyHint.Range, "0, 1, 0.001")]
    public float Persistance
    {
        set
        {
            _persistance = value;
            GenerateMap();
        }
        get => _persistance;
    }

    [Export]
    public float Lacunarity
    {
        set
        {
            _lacunarity = Math.Max(1, value);
            GenerateMap();
        }
        get => _lacunarity;
    }

    [Export]
    public int Seed
    {
        set
        {
            _seed = value;
            GenerateMap();
        }
        get => _seed;
    }

    [Export]
    public Vector2 Offset
    {
        set
        {
            _offset = value;
            GenerateMap();
        }
        get => _offset;
    }

    public override void _ExitTree()
    {
        _colorRamp.Changed -= GenerateMap;
    }

    public override void _EnterTree()
    {
        _colorRamp.Changed += GenerateMap;
    }

    public void GenerateMap()
    {
        var noiseMap = NoiseGenerator.Instance.GenerateNoiseMap(MapWidth, MapHeight, Seed, MapScale, Octaves, Persistance, Lacunarity, Offset);

        var colorMap = new Color[MapWidth, MapHeight];

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                float currHeight = noiseMap[x, y];
                colorMap[x, y] = ColorRamp.Sample(currHeight);
            }
        }

        switch (DrawMode)
        {
            case DrawMode.NoiseMap:
                GetNode<MapDisplay>("%MapDisplay").DrawTexture(TextureGenerator.Instance.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColorMap:
                GetNode<MapDisplay>("%MapDisplay").DrawTexture(TextureGenerator.Instance.TextureFromColorMap(colorMap));
                break;
        }
    }
}
