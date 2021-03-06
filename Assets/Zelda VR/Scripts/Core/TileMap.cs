﻿using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public const float TILE_EXTENT = 0.5f;      // (half of tile size)
    static public Vector3 TileExtents { get { return TILE_EXTENT * Vector3.one; } }

    readonly static Color SPECIAL_BLOCK_HIGHLIGHT_COLOR = new Color(1, 0.5f, 0.5f);


    public Vector3 WorldOffset { get { return WorldInfo.Instance.WorldOffset; } }


    [SerializeField]
    TileMapData _tileMapData;
    public TileMapData TileMapData { get { return _tileMapData; } }

    [SerializeField]
    TileMapTexture _tileMapTexture;
    public TileMapTexture TileMapTexture { get { return _tileMapTexture; } }


    /*float[,] _tileHeights;
    public float TryGetTileHeight(Index2 n)
    {
        return TryGetTileHeight(n.x, n.y);
    }
    public float TryGetTileHeight(int x, int y)
    {
        if (!IsTileIndexValid(x, y)) { return -1f; }

        return _tileHeights[y, x];
    }*/


    Transform _specialBlocksContainer;
    bool[,] _specialBlockPopulationFlags;
    public bool IsTileSpecial(Index2 n)
    {
        return IsTileSpecial(n.x, n.y);
    }
    public bool IsTileSpecial(int x, int y)
    {
        if (!IsTileIndexValid(x, y)) { return false; }

        return _specialBlockPopulationFlags[y, x];
    }


    public int TilesWide { get { return _tileMapData.TilesWide; } }
    public int TilesHigh { get { return _tileMapData.TilesHigh; } }

    public int TryGetTile(Index2 n)
    {
        return _tileMapData.TryGetTile(n);
    }
    public int TryGetTile(int x, int y)
    {
        return _tileMapData.TryGetTile(x, y);
    }

    public List<Index2> GetTilesInArea(Rect area, List<int> requisiteTileTypes = null)
    {
        return GetTilesInArea((int)area.xMin, (int)area.yMin, (int)area.width, (int)area.height, requisiteTileTypes);
    }
    public List<Index2> GetTilesInArea(int xMin, int yMin, int width, int height, List<int> requisiteTileTypes = null)
    {
        List<Index2> tileIndices = new List<Index2>();

        int right = xMin + width;
        int top = yMin + height;

        xMin = Mathf.Clamp(xMin, 0, TilesWide);
        right = Mathf.Clamp(right, 0, TilesWide);
        yMin = Mathf.Clamp(yMin, 0, TilesHigh);
        top = Mathf.Clamp(top, 0, TilesHigh);

        int[,] tiles = _tileMapData._tiles;

        for (int z = yMin; z < top; z++)
        {
            for (int x = xMin; x < right; x++)
            {
                int tileCode = tiles[z, x];
                if (requisiteTileTypes.Contains(tileCode))
                {
                    tileIndices.Add(new Index2(x, z));
                }
            }
        }

        return tileIndices;
    }

    public bool IsTileIndexValid(Index2 index)
    {
        return _tileMapData.IsTileIndexValid(index);
    }
    public bool IsTileIndexValid(int x, int y)
    {
        return _tileMapData.IsTileIndexValid(x, y);
    }


    #region Sector

    int _sectorHeightInTiles, _sectorWidthInTiles;
    int _sectorsWide, _sectorsHigh;

    public int SectorHeightInTiles { get { return _sectorHeightInTiles; } }
    public int SectorWidthInTiles { get { return _sectorWidthInTiles; } }
    public int SectorsWide { get { return _sectorsWide; } }
    public int SectorsHigh { get { return _sectorsHigh; } }

    public Index2 GetSectorContainingPosition(Vector3 p)
    {
        return GetSectorContainingPosition(p.x, p.z);
    }
    public Index2 GetSectorContainingPosition(float x, float z)
    {
        Vector3 p = new Vector3(x, 0, z) - WorldOffset;
        int sectorX = Mathf.FloorToInt(p.x / SectorWidthInTiles);
        int sectorY = Mathf.FloorToInt(p.z / SectorHeightInTiles);
        return new Index2(sectorX, sectorY);
    }
    public Vector3 GetCenterPositionOfSector(Index2 s)
    {
        Vector2 c = GetBoundsForSector(s).center;
        return new Vector3(c.x, WorldOffset.y, c.y);
    }
    public Rect GetBoundsForSector(Index2 sIdx)
    {
        float w = SectorWidthInTiles, h = SectorHeightInTiles;
        Vector3 p = sIdx.ToVector3();
        p.x *= w;
        p.z *= h;
        p += WorldOffset;
        return new Rect(p.x, p.z, w, h);
    }
    public bool SectorContainsPosition(Index2 sector, Vector3 pos)
    {
        Vector2 p = new Vector2(pos.x, pos.z);
        return GetBoundsForSector(sector).Contains(p);
    }


    public int GetTileInSector(Index2 sector, Index2 tileIdx_S)
    {
        Index2 t = TileIndex_SectorToWorld(tileIdx_S, sector);
        return TryGetTile(t);
    }

    public Index2 TileIndex_WorldToSector(Index2 tile, out Index2 sector)
    {
        return TileIndex_WorldToSector(tile.x, tile.y, out sector);
    }
    public Index2 TileIndex_WorldToSector(int x, int y, out Index2 sector)
    {
        sector = GetSectorContainingPosition(x, y);

        Index2 sIdx = new Index2();
        sIdx.x = x % _sectorWidthInTiles;
        sIdx.y = y % _sectorHeightInTiles;

        if (sIdx.x < 0) { sIdx.x += _sectorWidthInTiles; }
        if (sIdx.y < 0) { sIdx.y += _sectorHeightInTiles; }

        return sIdx;
    }
    public Index2 TileIndex_SectorToWorld(Index2 tile_S, Index2 sector)
    {
        return TileIndex_SectorToWorld(tile_S.x, tile_S.y, sector);
    }
    public Index2 TileIndex_SectorToWorld(int sX, int sY, Index2 sector)
    {
        Index2 idx = new Index2();
        idx.x = sector.x * _sectorWidthInTiles + sX;
        idx.y = sector.y * _sectorHeightInTiles + sY;

        return idx;
    }

    #endregion Sector


    void Awake()
    {
        InitFromSettings(ZeldaVRSettings.Instance);
        LoadMap();

        InitTileHeights();
        InitSpecialBlocks();

        if (Cheats.Instance.SecretDetectionModeIsEnabled)
        {
            HighlightAllSpecialBlocks();
        }
    }

    public void InitFromSettings(ZeldaVRSettings s)
    {
        _sectorWidthInTiles = s.overworldSectorWidthInTiles;
        _sectorHeightInTiles = s.overworldSectorHeightInTiles;
        _sectorsWide = s.overworldWidthInSectors;
        _sectorsHigh = s.overworldHeightInSectors;

        _tileMapData.InitFromSettings(s);
        _tileMapTexture.InitFromSettings(s);
    }

    void InitTileHeights()
    {

    }

    void InitSpecialBlocks()
    {
        float shortBlockHeight = ZeldaVRSettings.Instance.shortBlockHeight;

        _specialBlocksContainer = GameObject.Find("Special Blocks").transform;
        _specialBlockPopulationFlags = new bool[TilesHigh, TilesWide];

        foreach (Transform sb in _specialBlocksContainer)
        {
            if (!sb.gameObject.activeSelf) { continue; }

            float blockHeight = 0;
            Block b = sb.GetComponent<Block>();
            if (b != null)
            {
                blockHeight = b.isShortBlock ? shortBlockHeight : 1;
                SetBlockHeight(b.gameObject, blockHeight);
                SetBlockTexture(b.gameObject, b.tileCode);
            }

            // Set Population Flags
            Vector3 p = sb.position;
            Vector3 s = sb.lossyScale;
            int xLen = (int)s.x;
            int zLen = (int)s.z;
            int startX = (int)p.x;
            int startZ = (int)p.z;

            for (int z = startZ; z < startZ + zLen; z++)
            {
                for (int x = startX; x < startX + xLen; x++)
                {
                    if (!IsTileIndexValid(x, z)) { continue; }

                    _specialBlockPopulationFlags[z, x] = true;
                }
            }
        }
    }

    public void LoadMap()
    {
        if (!_tileMapData.HasLoaded)
        {
            _tileMapData.LoadMap();
        }
    }


    void SetBlockTexture(GameObject block, int tileCode, Material sourceMaterial = null, float actualBlockHeight = 1.0f)
    {
        Renderer r = block.GetComponent<Renderer>();

        Texture2D tex = _tileMapTexture.GetTexture(tileCode);
        if (sourceMaterial == null)
        {
            sourceMaterial = r.sharedMaterial;
        }
        Material mat = new Material(sourceMaterial);
        tex.filterMode = FilterMode.Point;
        mat.SetTexture("_MainTex", tex);

        Destroy(r.material);
        r.material = mat;
        r.material.mainTextureScale = new Vector2(1, actualBlockHeight);
    }

    void SetBlockHeight(GameObject block, float height, float yOffset = 0.0f)
    {
        Vector3 pos = block.transform.position;
        pos.y = WorldOffset.y + (height * 0.5f) + yOffset;
        block.transform.position = pos;

        Vector3 scale = block.transform.localScale;
        scale.y = height;
        block.transform.localScale = scale;
    }

    public void HighlightAllSpecialBlocks(bool doHighlight = true)
    {
        foreach (Transform child in _specialBlocksContainer)
        {
            Block b = child.GetComponent<Block>();
            if (b != null)
            {
                Color c = doHighlight ? SPECIAL_BLOCK_HIGHLIGHT_COLOR : Color.white;
                b.Colorize(c);
            }
        }
    }
}