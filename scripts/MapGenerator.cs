using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Godot;

public enum DrawMode { NoiseMap, ColorMap, Mesh, falloff }

[Tool]
public partial class MapGenerator : Node
{
    public const int MapChunkSize = 239;

    [Export(PropertyHint.Enum)]
    public DrawMode DrawMode;

    [Export(PropertyHint.Enum)]
    public NoiseGenerator.NormalizeMode NormalizeMode;

    [Export] public float HeightScale;
    [Export] public bool useFalloff;
    [Export] public Gradient ColorRamp;
    [Export] public Curve HeightCurve;

    [Export(PropertyHint.Range, "0, 6, 1")] public int EditorLod;

    [ExportGroup("Noise")]
    [Export(PropertyHint.Range, "0.0001, 50, 0.0001, or_greater")]
    public float MapScale;

    [Export(PropertyHint.Range, "1, 20, 1, or_greater")]
    public int Octaves;
    [Export(PropertyHint.Range, "0, 1, 0.001")]
    public float Persistance;
    [Export] public float Lacunarity;
    [Export] public int Seed;
    [Export] public Vector2 Offset;

    private Queue<MapThreadInfo<MapData>> _mapDataThreadingQueue = new();
    private Queue<MapThreadInfo<ArrayMesh>> _arrayMeshThreadingQueue = new();

    private float[,] _falloffMapData;

    public override void _Ready()
    {
        _falloffMapData = FalloffGenerator.Instance.GenerateFalloffMap(MapChunkSize);
    }

    public override void _Process(double delta)
    {
        if (_mapDataThreadingQueue.Count > 0)
        {
            for (var i = 0; i < _mapDataThreadingQueue.Count; i++)
            {
                var threadInfo = _mapDataThreadingQueue.Dequeue();
                threadInfo.Callback(threadInfo.parameter);
            }
        }
        if (_arrayMeshThreadingQueue.Count > 0)
        {
            for (var i = 0; i < _arrayMeshThreadingQueue.Count; i++)
            {
                var threadInfo = _arrayMeshThreadingQueue.Dequeue();
                threadInfo.Callback(threadInfo.parameter);
            }
        }

        if (Input.IsKeyPressed(Key.Space))
        {
            DrawMap();
        }
    }

    public void DrawMap()
    {
        var mapData = GenerateMapData(Vector2.Zero);
        switch (DrawMode)
        {
            case DrawMode.NoiseMap:
                GetNode<MapDisplay>("%MapDisplay").DrawTexture(TextureGenerator.Instance.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                GetNode<MapDisplay>("%MapDisplay").DrawTexture(TextureGenerator.Instance.TextureFromColorMap(mapData.colorMap));
                break;
            case DrawMode.Mesh:
                GetNode<MapDisplay>("%MapDisplay").DrawMesh(MeshGenerator.Instance.GenerateMesh(mapData.heightMap, HeightScale, HeightCurve, EditorLod), TextureGenerator.Instance.TextureFromColorMap(mapData.colorMap));
                break;
            case DrawMode.falloff:
                GetNode<MapDisplay>("%MapDisplay").DrawTexture(TextureGenerator.Instance.TextureFromHeightMap(FalloffGenerator.Instance.GenerateFalloffMap(MapChunkSize)));
                break;
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        var mapData = GenerateMapData(center);
        lock (_mapDataThreadingQueue)
        {
            _mapDataThreadingQueue.Enqueue(new(callback, mapData));
        }
    }

    public void RequestArrayMesh(MapData mapData, int lod, Action<ArrayMesh> callback)
    {
        ThreadStart threadStart = delegate
        {
            ArrayMeshThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    private void ArrayMeshThread(MapData mapData, int lod, Action<ArrayMesh> callback)
    {
        var arrayMesh = MeshGenerator.Instance.GenerateMesh(mapData.heightMap, HeightScale, HeightCurve, lod);
        lock (_arrayMeshThreadingQueue)
        {
            _arrayMeshThreadingQueue.Enqueue(new(callback, arrayMesh));
        }
    }

    private MapData GenerateMapData(Vector2 center)
    {
        var noiseMap = NoiseGenerator.Instance.GenerateNoiseMap(MapChunkSize + 2, MapChunkSize + 2, Seed, MapScale, Octaves, Persistance, Lacunarity, center + Offset, NormalizeMode);
        var colorMap = new Color[MapChunkSize, MapChunkSize];

        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currHeight = noiseMap[x, y];
                if (useFalloff)
                {
                    currHeight = Mathf.Max(currHeight - _falloffMapData[x, y], 0);
                    noiseMap[x, y] = currHeight;
                }
                colorMap[x, y] = ColorRamp.Sample(currHeight);
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    private struct MapThreadInfo<T>
    {
        public readonly Action<T> Callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            Callback = callback;
            this.parameter = parameter;
        }
    }
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[,] colorMap;

    public MapData(float[,] heightMap, Color[,] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
