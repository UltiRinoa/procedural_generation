using Godot;


namespace cmos.Test;

[Tool]
public partial class MapDisplay : Node
{

    [Export] private TextureRect _mapTextureRect;

    public void DrawTexture(ImageTexture texture)
    {
        _mapTextureRect.Texture = texture;
    }
}