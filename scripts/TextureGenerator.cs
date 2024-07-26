using Godot;

public class TextureGenerator
{
    private static TextureGenerator _instance;

    public static TextureGenerator Instance { get => _instance ?? new TextureGenerator(); }

    private TextureGenerator() { }

    public ImageTexture TextureFromHeightMap(float[,] heightMap)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);
        var image = new Image();
        image.SetData(width, height, false, Image.Format.Rgba8, new byte[width * height * 4]);
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                image.SetPixel(x, y, Colors.Black.Lerp(Colors.White, heightMap[x, y]));
            }

        var texture = ImageTexture.CreateFromImage(image);
        return texture;
    }

    public ImageTexture TextureFromColorMap(Color[,] colorMap)
    {
        var width = colorMap.GetLength(0);
        var height = colorMap.GetLength(1);
        var image = new Image();
        image.SetData(width, height, false, Image.Format.Rgba8, new byte[width * height * 4]);
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                image.SetPixel(x, y, colorMap[x, y]);
            }

        var texture = ImageTexture.CreateFromImage(image);
        return texture;
    }
}