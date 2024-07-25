using Godot;


namespace cmos.Test; 

public class NoiseGenerator {
    private static NoiseGenerator _instance;
    public static NoiseGenerator Instance => _instance ?? new NoiseGenerator();

    private readonly FastNoiseLite _fastNoiseLite = new ();

    private NoiseGenerator() {
        _fastNoiseLite.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _fastNoiseLite.FractalType = FastNoiseLite.FractalTypeEnum.None;
        _fastNoiseLite.Frequency = 0.1f;
    }
    
    
    public float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale) {
        var noiseMap = new float[mapWidth, mapHeight];
        if (scale <= 0) {
            scale = 0.0001f;
        }

        var max = -999f;
        var min = 999f;
        
        for (var y = 0; y < mapHeight; y++) {
            for (var x = 0; x < mapWidth; x++) {
                var sampleX = x / scale;
                var sampleY = y / scale;

                var perlinValue = _fastNoiseLite.GetNoise2D(sampleX, sampleY);
                perlinValue = (perlinValue + 1) * 0.5f;
                noiseMap[x, y] = perlinValue;

                if ( perlinValue < min) {
                    min = perlinValue;
                }

                if (perlinValue > max) {
                    max = perlinValue;
                }
                
                
            }
        }
        
        GD.Print($"max: {max}, min: {min}");
            
        return noiseMap;
    }
}