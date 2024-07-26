using System;
using Godot;


namespace cmos.Test;

[Tool]
public partial class MapDisplay : Node
{

    [Export] private TextureRect _mapTextureRect;
    [Export] private MeshInstance3D _meshInstance3D;

    public void DrawTexture(ImageTexture texture)
    {
        _mapTextureRect.Texture = texture;
    }

    internal void DrawMesh(ArrayMesh arrayMesh, ImageTexture imageTexture)
    {
        _meshInstance3D.Mesh = arrayMesh;
        var material = _meshInstance3D.GetActiveMaterial(0) as BaseMaterial3D;
        material.AlbedoTexture = imageTexture;
    }
}