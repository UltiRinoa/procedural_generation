using System;
using System.Collections.Generic;
using Godot;

[Tool]
public partial class EndlessTerrain : Node3D
{
    [Export] public Node3D Viewer;
    [Export] public ShaderMaterial TerrainMaterial;
    public static MapGenerator MapGenerator;
    public static float MaxViewDist;
    public LODInfo[] _detailLevels = new[] {
        new LODInfo(0, 200),
        new LODInfo(2, 400),
        new LODInfo(4,600),
    };
    public static Vector2 ViewerPosition;

    private Vector2 _viewerPositionOld;
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    private int _chunkSize;
    private int _chunksVisibleInViewDist;
    private Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new();
    private List<TerrainChunk> _terrainChunksVisibleLastUpdateList = new();

    public override void _Ready()
    {
        _chunkSize = MapGenerator.MapChunkSize - 1;
        MaxViewDist = _detailLevels[^1].visibleDstThreshold;
        _chunksVisibleInViewDist = Mathf.RoundToInt(MaxViewDist / _chunkSize);
        MapGenerator = GetNode<MapGenerator>("%MapGenerator");

        UpdateVisibleChunks();
    }

    public override void _Process(double delta)
    {
        ViewerPosition = new Vector2(Viewer.GlobalPosition.X, Viewer.GlobalPosition.Z);
        if (ViewerPosition.DistanceSquaredTo(_viewerPositionOld) > sqrViewerMoveThresholdForChunkUpdate)
        {
            _viewerPositionOld = ViewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks()
    {
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
                    _terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, _chunkSize, _detailLevels, this, TerrainMaterial));
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
        private MapData _mapData;
        private bool _mapDataReceived;
        private LODInfo[] _detailLevels;
        private LODMesh[] _lodMeshes;
        private int _previousLodIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Node3D parent, ShaderMaterial material)
        {
            _detailLevels = detailLevels;
            _globalPosition = coord * size;
            _size = size;

            _meshInstance.MaterialOverride = material.Duplicate();
            var positionV3 = new Vector3(_globalPosition.X, 0, _globalPosition.Y);
            _meshInstance = new MeshInstance3D();
            _meshInstance.Position = positionV3;
            parent.AddChild(_meshInstance);
            _meshInstance.Owner = parent.Owner;
            _meshInstance.Visible = false;

            _lodMeshes = new LODMesh[_detailLevels.Length];
            for (var i = 0; i < _lodMeshes.Length; i++)
            {
                _lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            MapGenerator.RequestMapData(OnMapDataGenerated);
        }

        private void OnMapDataGenerated(MapData data)
        {
            _mapData = data;
            _mapDataReceived = true;

            var texture = TextureGenerator.Instance.TextureFromColorMap(data.colorMap);
            (_meshInstance.GetSurfaceOverrideMaterial(0) as ShaderMaterial).SetShaderParameter("main_texture", texture);
            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!_mapDataReceived) return;

            float viewerDstFromNearestEdge = Utils.BoxSdf(ViewerPosition - _globalPosition, new Vector2(_size, _size));
            var visible = viewerDstFromNearestEdge <= MaxViewDist;
            if (visible)
            {
                var lodIndex = 0;
                for (int i = 0; i < _detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > _detailLevels[i].visibleDstThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else break;
                }
                if (lodIndex != _previousLodIndex)
                {
                    var lodMesh = _lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        _previousLodIndex = lodIndex;
                        _meshInstance.Mesh = lodMesh.ArrayMesh;
                    }
                    else if (!lodMesh.hasRequestMesh)
                    {
                        lodMesh.RequestMesh(_mapData);
                    }
                }

            }
            SetVisible(visible);
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

    private class LODMesh
    {
        public ArrayMesh ArrayMesh;
        public bool hasRequestMesh;
        public bool hasMesh;
        private int _lod;
        private Action _updateCallback;

        public LODMesh(int lod, Action callback)
        {
            _lod = lod;
            _updateCallback = callback;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestMesh = true;
            MapGenerator.RequestArrayMesh(mapData, _lod, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(ArrayMesh mesh)
        {
            ArrayMesh = mesh;
            hasMesh = true;
            _updateCallback();
        }
    }

    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;

        public LODInfo(int lod, float visibleDstThreshold)
        {
            this.lod = lod;
            this.visibleDstThreshold = visibleDstThreshold;
        }
    }
}