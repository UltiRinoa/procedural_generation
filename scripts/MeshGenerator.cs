using Godot;

public class MeshGenerator
{
    private static MeshGenerator _instance;
    public static MeshGenerator Instance => _instance ?? new MeshGenerator();

    public ArrayMesh GenerateMesh(float[,] heightMap, float scale, Curve heightCurve, int lod)
    {
        var vertexStepIncrement = lod == 0 ? 1 : lod * 2;
        var borderedSize = heightMap.GetLength(0);
        var meshSize = borderedSize - vertexStepIncrement * 2;
        var meshSizeUnsimplified = borderedSize - 2;

        var topLeftX = -(meshSizeUnsimplified - 1) * .5f;
        var topLeftY = -(meshSizeUnsimplified - 1) * .5f;

        var verticesPerLine = (meshSize - 1) / vertexStepIncrement + 1;

        var meshVertexIndex = 0;
        var borderVertexIndex = -1;
        var vertexIndicesMap = new int[borderedSize, borderedSize];

        var meshData = new MeshData(verticesPerLine);

        for (int y = 0; y < borderedSize; y += vertexStepIncrement)
        {
            for (int x = 0; x < borderedSize; x += vertexStepIncrement)
            {
                var isBorderVertex = x == 0 || y == 0 || x == borderedSize - 1 || y == borderedSize - 1;

                if (isBorderVertex)
                    vertexIndicesMap[x, y] = borderVertexIndex--;
                else
                    vertexIndicesMap[x, y] = meshVertexIndex++;
            }
        }

        for (int y = 0; y < borderedSize; y += vertexStepIncrement)
        {
            for (int x = 0; x < borderedSize; x += vertexStepIncrement)
            {
                var vertexIndex = vertexIndicesMap[x, y];
                var percent = new Vector2((x - vertexStepIncrement) / (float)meshSize, (y - vertexStepIncrement) / (float)meshSize);

                var vertexPosition = new Vector3(percent.X * meshSizeUnsimplified + topLeftX, heightCurve.Sample(heightMap[x, y]) * scale, percent.Y * meshSizeUnsimplified + topLeftY);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + vertexStepIncrement, y];
                    int c = vertexIndicesMap[x, y + vertexStepIncrement];
                    int d = vertexIndicesMap[x + vertexStepIncrement, y + vertexStepIncrement];

                    meshData.AddIndices(a, d, c);
                    meshData.AddIndices(d, a, b);
                }
            }
        }
        return meshData.CreateMesh();
    }

    public class MeshData
    {
        private Vector3[] _vertices;
        private int[] _indices;
        private Vector2[] _uvs;

        private Vector3[] _borderVertices;
        private int[] _borderIndices;

        private int _indicesIndex;
        private int _borderTriangleIndex;
        private Vector3[] _bakedNormals;

        public MeshData(int verticesPerLine)
        {
            _vertices = new Vector3[verticesPerLine * verticesPerLine];
            _indices = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];
            _uvs = new Vector2[verticesPerLine * verticesPerLine];

            _borderVertices = new Vector3[verticesPerLine * 4 + 4];
            _borderIndices = new int[verticesPerLine * 24];
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0)
                _borderVertices[-vertexIndex - 1] = vertexPosition;
            else
            {
                _vertices[vertexIndex] = vertexPosition;
                _uvs[vertexIndex] = uv;
            }
        }

        public void AddIndices(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
            {
                _borderIndices[_borderTriangleIndex++] = a;
                _borderIndices[_borderTriangleIndex++] = b;
                _borderIndices[_borderTriangleIndex++] = c;
            }
            else
            {
                _indices[_indicesIndex++] = a;
                _indices[_indicesIndex++] = b;
                _indices[_indicesIndex++] = c;
            }
        }

        public ArrayMesh CreateMesh()
        {
            BakeNormals();

            var surfaceArray = new Godot.Collections.Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            surfaceArray[(int)Mesh.ArrayType.Vertex] = _vertices;
            surfaceArray[(int)Mesh.ArrayType.TexUV] = _uvs;
            surfaceArray[(int)Mesh.ArrayType.Normal] = _bakedNormals;
            surfaceArray[(int)Mesh.ArrayType.Index] = _indices;

            var arrayMesh = new ArrayMesh();
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
            return arrayMesh;
        }

        private Vector3[] CalculateNormals()
        {
            Vector3[] normals = new Vector3[_vertices.Length];

            var triangleCount = _indices.Length / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                var normalTriangleIndex = i * 3;
                var vertexIndexA = _indices[normalTriangleIndex];
                var vertexIndexB = _indices[normalTriangleIndex + 1];
                var vertexIndexC = _indices[normalTriangleIndex + 2];

                var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                normals[vertexIndexA] += triangleNormal;
                normals[vertexIndexB] += triangleNormal;
                normals[vertexIndexC] += triangleNormal;
            }

            var borderTriangleCount = _borderIndices.Length / 3;
            for (int i = 0; i < borderTriangleCount; i++)
            {
                var normalTriangleIndex = i * 3;
                var vertexIndexA = _borderIndices[normalTriangleIndex];
                var vertexIndexB = _borderIndices[normalTriangleIndex + 1];
                var vertexIndexC = _borderIndices[normalTriangleIndex + 2];

                var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                if (vertexIndexA >= 0)
                    normals[vertexIndexA] += triangleNormal;
                if (vertexIndexB >= 0)
                    normals[vertexIndexB] += triangleNormal;
                if (vertexIndexC >= 0)
                    normals[vertexIndexC] += triangleNormal;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].Normalized();
            }

            return normals;
        }

        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            var a = indexA < 0 ? _borderVertices[-indexA - 1] : _vertices[indexA];
            var b = indexB < 0 ? _borderVertices[-indexB - 1] : _vertices[indexB];
            var c = indexC < 0 ? _borderVertices[-indexC - 1] : _vertices[indexC];

            var ab = b - a;
            var ac = c - a;
            return ac.Cross(ab).Normalized();
        }

        private void BakeNormals()
        {
            _bakedNormals = CalculateNormals();
        }
    }
}