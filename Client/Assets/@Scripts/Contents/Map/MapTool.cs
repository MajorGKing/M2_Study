using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MapTool : MonoBehaviour
{
    public void DrawCollision()
    {
#if UNITY_EDITOR
        Tilemap tilemap_collision = GetComponent<Tilemap>();
        TileBase terrainO = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/@Resources/TileMaps/01_asset/dev/Collider/terrain_O.asset");
        GameObject parent = tilemap_collision.transform.parent.gameObject;

        Tilemap pivot = Utils.FindChild<Tilemap>(parent, "Terrain_01");
        pivot.CompressBounds();

        tilemap_collision.origin = pivot.origin * 2;
        tilemap_collision.size = pivot.size * 2;

        for (int y = tilemap_collision.cellBounds.yMax; y >= tilemap_collision.cellBounds.yMin; y--)
        {
            for (int x = tilemap_collision.cellBounds.xMin; x <= tilemap_collision.cellBounds.xMax; x++)
            {
                tilemap_collision.SetTile(new Vector3Int(x, y), null);
            }
        }

        for (int y = tilemap_collision.cellBounds.yMax; y >= tilemap_collision.cellBounds.yMin; y--)
        {
            for (int x = tilemap_collision.cellBounds.xMin; x <= tilemap_collision.cellBounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 worldPos = tilemap_collision.CellToWorld(cellPos);

                {
                    Tilemap terrain = Utils.FindChild<Tilemap>(parent, "Terrain_01", true);
                    Vector3Int pos = terrain.WorldToCell(worldPos);
                    TileBase tile = terrain.GetTile(pos);
                    if (tile != null)
                    {
                        tilemap_collision.SetTile(cellPos, terrainO);
                    }
                }

                //1. Wall 확인
                {
                    Tilemap wall = Utils.FindChild<Tilemap>(parent, "Wall_01", true);
                    Vector3Int pos = wall.WorldToCell(worldPos);
                    TileBase tile = wall.GetTile(pos);
                    if (tile != null)
                    {
                        //장애물이 있는 지역
                        tilemap_collision.SetTile(cellPos, null);
                        continue;
                    }

                    Tilemap wall_02 = Utils.FindChild<Tilemap>(parent, "Wall_02", true);
                    if (wall_02 != null)
                    {
                        Vector3Int pos2 = wall_02.WorldToCell(worldPos);
                        TileBase wall2 = wall_02.GetTile(pos);
                        if (wall2 != null)
                        {
                            //장애물이 있는 지역
                            tilemap_collision.SetTile(cellPos, null);
                        }
                    }
                }
            }
        }
#endif

    }
}
