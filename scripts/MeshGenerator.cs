using Godot;

public class MeshGenerator
{
    private static MeshGenerator _instance;
    public static MeshGenerator Instance => _instance ?? new MeshGenerator();

    public ArrayMesh GenerateMesh(float[,] heightMap)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);
        var topLeftX = -(width - 1) / 2;
        var topLeftZ = (height - 1) / 2;

        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        var vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                surfaceTool.SetUV(new(x / (float)width, y / (float)height));
                surfaceTool.AddVertex(new Vector3(x + topLeftX, heightMap[x, y], topLeftZ - y));

                // if (x < width - 1 && y < height - 1)
                // {
                //     surfaceTool.AddIndex(vertexIndex);
                //     surfaceTool.AddIndex(vertexIndex + width + 1);
                //     surfaceTool.AddIndex(vertexIndex + width);
                //     surfaceTool.AddIndex(vertexIndex + width + 1);
                //     surfaceTool.AddIndex(vertexIndex);
                //     surfaceTool.AddIndex(vertexIndex + 1);
                // }
            }
        }

        // surfaceTool.SetUV(new Vector2(0, 0));
        // surfaceTool.AddVertex(new Vector3(0, 0, 0));
        // surfaceTool.SetUV(new Vector2(0, 0.5f));
        // surfaceTool.AddVertex(new Vector3(0, 0, 1));
        // surfaceTool.SetUV(new Vector2(0, 1));
        // surfaceTool.AddVertex(new Vector3(0, 1, 0));

        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();

        return surfaceTool.Commit();
    }
}