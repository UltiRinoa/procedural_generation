using Godot;

[Tool]
public partial class MapDisplay : Node
{

    [Export] private TextureRect _mapTextureRect;
    [Export] private MeshInstance3D _meshInstance;
    [Export] private ShaderMaterial _terrainMaterial;


    public void DrawTexture(ImageTexture texture)
    {
        _mapTextureRect.Texture = texture;
    }

    public void DrawMesh(ArrayMesh mesh, ImageTexture texture)
    {
        _meshInstance.Mesh = mesh;
        mesh.SurfaceSetMaterial(0, _terrainMaterial);
        (mesh.SurfaceGetMaterial(0) as ShaderMaterial).SetShaderParameter("main_texture", texture);
    }
}
