using Godot;

[Tool]
public partial class MapDisplay : Node
{

    [Export] private TextureRect _mapTextureRect;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        var width = noiseMap.GetLength(0);
        var height = noiseMap.GetLength(1);
        var image = new Image();
        image.SetData(width, height, false, Image.Format.Rgba8, new byte[width * height * 4]);
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                image.SetPixel(x, y, Colors.Black.Lerp(Colors.White, noiseMap[x, y]));
            }

        foreach (var item in GetChildren())
        {
            
        }
        _mapTextureRect.Texture = ImageTexture.CreateFromImage(image);
    }
}
