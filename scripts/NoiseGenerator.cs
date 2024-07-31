using System;
using System.ComponentModel.DataAnnotations;
using Godot;

public class NoiseGenerator
{
    public enum NormalizeMode { Local, Global };

    private static NoiseGenerator _instance;
    public static NoiseGenerator Instance => _instance ?? new NoiseGenerator();

    private readonly FastNoiseLite _fastNoiseLite = new();

    private NoiseGenerator()
    {
        _fastNoiseLite.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _fastNoiseLite.FractalType = FastNoiseLite.FractalTypeEnum.None;
        _fastNoiseLite.Frequency = 0.1f;
    }


    public float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        var noiseMap = new float[mapWidth, mapHeight];
        var prng = new System.Random(seed);
        var octaveOffsets = new Vector2[octaves];

        var amplitude = 1f;
        var Frequency = 1f;

        var maxPossibleHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            var offsetX = prng.Next(-10000, 10000) + offset.X;
            var offsetY = prng.Next(-10000, 10000) + offset.Y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;
        var halfWidth = mapWidth * .5f;
        var halfHeight = mapHeight * .5f;

        for (var y = 0; y < mapHeight; y++)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                amplitude = 1f;
                Frequency = 1f;
                var noiseHeight = 0f;

                for (var i = 0; i < octaves; i++)
                {

                    var sampleX = (x - halfWidth + octaveOffsets[i].X) / scale * Frequency;
                    var sampleY = (y - halfHeight + octaveOffsets[i].Y) / scale * Frequency;
                    var perlinValue = _fastNoiseLite.GetNoise2D(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    Frequency *= lacunarity;
                }



                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                switch (normalizeMode)
                {
                    case NormalizeMode.Local:
                        noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                        break;
                    case NormalizeMode.Global:
                        // noiseMap[x, y] = (noiseMap[x, y] / maxPossibleHeight + 1) / 2;
                        noiseMap[x, y] = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                        break;
                }
            }
        }

        return noiseMap;
    }
}
