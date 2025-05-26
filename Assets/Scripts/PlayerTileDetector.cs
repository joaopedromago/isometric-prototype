using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerTileDetector : MonoBehaviour
{

    public Transform chunksRoot;
    public List<TileBase> stairUpTiles;

    [SerializeField] private Vector2 worldOrigin = new Vector2(-9, -4);
    [SerializeField] private int chunkSize = 32;

    private Vector3? previousPosition = null;

    private PlayerAttributes playerAttributes;

    void Awake()
    {
        playerAttributes = GetComponent<PlayerAttributes>();
    }
    void Update()
    {
        if (playerAttributes.IsMoving) return;
        if (previousPosition == transform.position) return;
        previousPosition = transform.position;

        ProcessTileTrigger();
    }

    public Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        Vector2 adjusted = new Vector2(worldPos.x - worldOrigin.x, worldPos.y - worldOrigin.y);
        int x = Mathf.FloorToInt(adjusted.x / chunkSize);
        int y = Mathf.FloorToInt(adjusted.y / chunkSize);
        return new Vector2Int(x, y);
    }


    public List<Tilemap> GetTilemapByPosition(int x, int y, int height)
    {
        string heightName = $"Chunks_{height}";
        string chunkName = $"Chunk_{y}_{x}";

        foreach (Transform chunkGroup in chunksRoot)
        {
            if (chunkGroup.name != heightName) continue;
            foreach (Transform tilemapTransform in chunkGroup)
            {
                if (tilemapTransform.name == chunkName)
                {
                    List<Tilemap> tilemaps = new List<Tilemap>();

                    foreach (Transform child in tilemapTransform)
                    {
                        Tilemap tilemap = child.GetComponent<Tilemap>();
                        if (tilemap != null)
                        {
                            tilemaps.Add(tilemap);
                        }
                    }

                    return tilemaps;
                }
            }
        }

        return null;
    }

    void ProcessTileTrigger()
    {
        Vector2Int playerChunkCoord = GetChunkCoord(transform.position);

        var tilemaps = GetTilemapByPosition(playerChunkCoord.x, playerChunkCoord.y, (int)transform.position.z);

        if (tilemaps == null || tilemaps.Count == 0) return;

        playerAttributes.CurrentTilemap = tilemaps;

        foreach (var tilemap in tilemaps)
        {
            if (!tilemap) continue;

            Vector3Int tilePos = tilemap.WorldToCell(transform.position);
            TileBase tile = tilemap.GetTile(tilePos);

            if (tile != null && tile is CustomTile customTile)
            {
                if (customTile.isStair)
                {
                    PerformEnterStair(customTile, tilemap);
                }
                if (customTile.isStairDown)
                {
                    PerformEnterStairDown(customTile, tilemap);
                }
            }
        }
    }

    public static Vector3 GetTileRotation(Tilemap tilemap, Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        if (tile == null)
        {
            return Vector3.zero;
        }

        Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(position);
        Quaternion rotation = transformMatrix.rotation;
        return rotation.eulerAngles;
    }

    public static Vector3 AdjustPositionByRotation(Vector3 position, Vector3 rotation)
    {
        if ((int)Math.Ceiling(rotation.z) == 0)
        {
            position.y += 1.0f;
        }
        else if ((int)Math.Ceiling(rotation.z) == 180)
        {
            position.y -= 1.0f;
        }
        else if ((int)Math.Ceiling(rotation.z) == 90)
        {
            position.x -= 1.0f;
        }
        else if ((int)Math.Ceiling(rotation.z) == 270)
        {
            position.x += 1.0f;
        }

        return position;
    }

    void PerformEnterStair(CustomTile tile, Tilemap tilemap)
    {
        Vector3Int tilePos = tilemap.WorldToCell(transform.position);
        Vector3 rotation = GetTileRotation(tilemap, tilePos);

        Vector3 pos = AdjustPositionByRotation(transform.position, rotation);
        pos.z += 1;
        GetComponent<SpriteRenderer>().sortingOrder += 10;

        transform.position = pos;
    }

    void PerformEnterStairDown(CustomTile tile, Tilemap tilemap)
    {
        Vector3Int tilePos = tilemap.WorldToCell(transform.position);
        Vector3 rotation = GetTileRotation(tilemap, tilePos);

        Vector3 pos = AdjustPositionByRotation(transform.position, rotation);
        pos.z -= 1;
        GetComponent<SpriteRenderer>().sortingOrder -= 10;

        transform.position = pos;
    }
}
