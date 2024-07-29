using System;
using Godot;

public enum DrawMode { NoiseMap, ColorMap, Mesh }

[Tool]
public partial class MapGenerator : Node
{
    private Gradient _colorRamp;
    private float _noiseScale = 1;
    private int _octaves = 5;
    private int _seed;
    private float _persistance = 0.5f;
    private float _lacunarity = 1f;
    private Vector2 _offset;
    private DrawMode _drawMode;
    private float _heightScale;
    private Curve _heightCurve;

    public const int MapChunkSize = 241;

    [Export(PropertyHint.Enum)]
    public DrawMode DrawMode
    {
        set
        {
            _drawMode = value;
        }
        get => _drawMode;
    }

    [Export]
    public Gradient ColorRamp
    {
        set
        {
            _colorRamp = value;
        }
        get => _colorRamp;
    }

    [Export]
    public float HeightScale
    {
        set
        {
            _heightScale = value;
        }
        get => _heightScale;
    }

    [Export]
    public Curve HeightCurve
    {
        set
        {
            _heightCurve = value;
        }
        get => _heightCurve;
    }

    [Export(PropertyHint.Range, "0, 6, 1")] public int Lod;

    [ExportGroup("Noise")]

    [Export(PropertyHint.Range, "0.0001, 50, 0.0001, or_greater")]
    public float MapScale
    {
        set
        {
            _noiseScale = value;
        }
        get => _noiseScale;
    }

    [Export(PropertyHint.Range, "1, 20, 1, or_greater")]
    public int Octaves
    {
        set
        {
            _octaves = Math.Max(0, value);
        }
        get => _octaves;
    }

    [Export(PropertyHint.Range, "0, 1, 0.001")]
    public float Persistance
    {
        set
        {
            _persistance = value;
        }
        get => _persistance;
    }

    [Export]
    public float Lacunarity
    {
        set
        {
            _lacunarity = Math.Max(1, value);
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
        }
        get => _offset;
    }

    public void GenerateMap()
    {
        if (IsInsideTree() == false) return;
        if (ColorRamp == null || HeightCurve == null) return;
        if (GetNode<MapDisplay>("%MapDisplay") == null) return;
        var noiseMap = NoiseGenerator.Instance.GenerateNoiseMap(MapChunkSize, MapChunkSize, Seed, MapScale, Octaves, Persistance, Lacunarity, Offset);
        var colorMap = new Color[MapChunkSize, MapChunkSize];

        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
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
            case DrawMode.Mesh:
                GetNode<MapDisplay>("%MapDisplay").DrawMesh(MeshGenerator.Instance.GenerateMesh(noiseMap, HeightScale, HeightCurve, Lod), TextureGenerator.Instance.TextureFromColorMap(colorMap));
                break;
        }
    }
}
