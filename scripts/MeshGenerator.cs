using System.Collections.Generic;
using Godot;
using Godot.Collections;

public class MeshGenerator
{
    private static MeshGenerator _instance;
    public static MeshGenerator Instance => _instance ?? new MeshGenerator();

    public ArrayMesh GenerateMesh()
    {
        var mesh = new ArrayMesh();


        var surfaceArray = new Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        return mesh;
    }
}