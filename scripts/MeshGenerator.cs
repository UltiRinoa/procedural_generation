using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Godot;

public class MeshGenerator
{
    private static MeshGenerator _instance;
    public static MeshGenerator Instance => _instance ?? new MeshGenerator();

    public ArrayMesh GenerateMesh(float[,] heightMap, float scale)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);

        var topLeftX = -(width - 1) * .5f;
        var topLeftY = -(height - 1) * .5f;

        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        var vertexIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                surfaceTool.SetUV(new(x / (float)width, y / (float)height));
                surfaceTool.AddVertex(new(topLeftX + x, heightMap[x, y] * scale, topLeftY + y));

                if (x < width - 1 && y < height - 1)
                {
                    surfaceTool.AddIndex(vertexIndex);
                    surfaceTool.AddIndex(vertexIndex + 1);
                    surfaceTool.AddIndex(vertexIndex + width + 1);
                    surfaceTool.AddIndex(vertexIndex + width + 1);
                    surfaceTool.AddIndex(vertexIndex + width);
                    surfaceTool.AddIndex(vertexIndex);
                }
                vertexIndex++;
            }
        }

        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();

        return surfaceTool.Commit();
    }
}