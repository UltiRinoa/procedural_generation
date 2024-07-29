using System.Collections.Generic;
using Godot;

[Tool]
public partial class EndlessTerrain : Node3D
{
    public const float MaxViewDist = 400;
    public static Vector2 ViewerPosition;
    [Export] public Node3D Viewer;
    public static MapGenerator MapGenerator;

    private int _chunkSize;
    private int _chunksVisibleInViewDist;
    private Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new();
    private List<TerrainChunk> _terrainChunksVisibleLastUpdateList = new();
    private bool _isEnabled;

    public override void _Ready()
    {
        _chunkSize = MapGenerator.MapChunkSize - 1;
        _chunksVisibleInViewDist = Mathf.RoundToInt(MaxViewDist / _chunkSize);
        MapGenerator = GetNode<MapGenerator>("%MapGenerator");

        if (!Engine.IsEditorHint())
        {
            _isEnabled = true;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.Space))
        {
            _isEnabled = !_isEnabled;
        }

        if (!_isEnabled) return;
        ViewerPosition = new Vector2(Viewer.GlobalPosition.X, Viewer.GlobalPosition.Z);
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        if (Viewer == null) return;
        if (MapGenerator == null) return;

        foreach (var chunk in _terrainChunksVisibleLastUpdateList)
        {
            chunk.SetVisible(false);
        }
        _terrainChunksVisibleLastUpdateList.Clear();

        var currentChunkCoordX = Mathf.RoundToInt(ViewerPosition.X / _chunkSize);
        var currentChunkCoordY = Mathf.RoundToInt(ViewerPosition.Y / _chunkSize);

        for (var yOffset = -_chunksVisibleInViewDist; yOffset <= _chunksVisibleInViewDist; yOffset++)
        {
            for (var xOffset = -_chunksVisibleInViewDist; xOffset <= _chunksVisibleInViewDist; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (_terrainChunkDictionary.TryGetValue(viewedChunkCoord, out var terrainChunk))
                {
                    terrainChunk.UpdateTerrainChunk();
                    if (terrainChunk.IsVisible())
                    {
                        _terrainChunksVisibleLastUpdateList.Add(terrainChunk);
                    }
                }
                else
                {
                    _terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, _chunkSize, this));
                }
            }
        }
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationEditorPreSave:
                foreach (var child in GetChildren())
                {
                    child.Owner = null;
                    child.QueueFree();
                }

                break;
        }
    }

    public class TerrainChunk
    {
        private MeshInstance3D _meshInstance;
        private Vector2 _globalPosition;
        private int _size;

        public TerrainChunk(Vector2 coord, int size, Node3D parent)
        {
            _globalPosition = coord * size;
            _size = size;
            var positionV3 = new Vector3(_globalPosition.X, 0, _globalPosition.Y);

            _meshInstance = new MeshInstance3D();
            _meshInstance.Position = positionV3;
            parent.AddChild(_meshInstance);
            _meshInstance.Owner = parent.Owner;
            _meshInstance.Visible = false;
            MapGenerator.RequestMapData(OnMapDataGenerated);
        }

        private void OnMapDataGenerated(MapData data)
        {
            MapGenerator.RequestArrayMesh(data, OnArrayMeshGenerated);
        }

        private void OnArrayMeshGenerated(ArrayMesh mesh)
        {
            _meshInstance.Mesh = mesh;
        }

        public void UpdateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Utils.BoxSdf(ViewerPosition - _globalPosition, new Vector2(_size, _size));
            SetVisible(viewerDstFromNearestEdge <= MaxViewDist);
        }

        public void SetVisible(bool visible)
        {
            _meshInstance.Visible = visible;
        }

        public bool IsVisible()
        {
            return _meshInstance.Visible;
        }
    }
}