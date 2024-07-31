using Godot;

public class FalloffGenerator
{
    private static FalloffGenerator _instance;
    public static FalloffGenerator Instance => _instance ?? new FalloffGenerator();

    private FalloffGenerator() { }

    public float[,] GenerateFalloffMap(int size)
    {
        var mapData = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var x = i / (float)size;
                var y = j / (float)size;

                x = x * 2 - 1;
                y = y * 2 - 1;

                mapData[i, j] = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                mapData[i, j] = Evaluate(mapData[i, j]);
            }
        }

        return mapData;
    }

    public float Evaluate(float x)
    {
        var a = 3f;
        var b = 2.2f;

        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow((1 - x), a) * b);
    }
}