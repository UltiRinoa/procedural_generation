using Godot;

namespace cmos.Test; 

[Tool]
public partial class MapGenerator : Node {
    private int _mapWidth = 512;
    private int _mapHeight = 512;
    private float _noiseScale = 1;
    
    [Export]
    public int MapWidth {
        set {
            _mapWidth = value;
            GenerateMap();
        }
        get => _mapWidth;
    }


    [Export]
    public int MapHeight {
        set {
            _mapHeight = value;
            GenerateMap();
        }
        get => _mapHeight;
    }


    [Export(PropertyHint.Range, "0.0001, 50, 0.0001, or_greater")]
    public float MapScale {
        set {
            _noiseScale = value;
            GenerateMap();
        }
        get => _noiseScale;
    }
    
    public void GenerateMap() {
        var noiseMap = NoiseGenerator.Instance.GenerateNoiseMap(MapWidth, MapHeight, MapScale);
        GetNode<MapDisplay>("%MapDisplay").DrawNoiseMap(noiseMap);
    }
}