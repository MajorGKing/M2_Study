using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

public class MapGenerator
{
    [MenuItem("Tools/GenerateMap %#m")]
    private static void GenerateMap()
    {
        string prefabPath = "Assets/@Resources/Prefabs/Map";
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                Debug.LogError("prefab is null");
                return;
            }

            Tilemap tilemapCollision = Utils.FindChild<Tilemap>(prefab, "Tilemap_Collision", true);

            if (tilemapCollision == null)
            {
                Debug.LogError("tilemap is null");
                return;
            }

            SaveTilemapCollisionData(tilemapCollision, prefab.name);
        }

        Debug.Log("Map Collision Generation Complete");
    }

    private static void SaveTilemapCollisionData(Tilemap tilemapCollision, string name)
    {
        string filePath = $"Assets/@Resources/Data/MapData/{name}Collision.txt";
        using (var writer = File.CreateText(filePath))
        {
            writer.WriteLine(tilemapCollision.cellBounds.xMin);
            writer.WriteLine(tilemapCollision.cellBounds.xMax);
            writer.WriteLine(tilemapCollision.cellBounds.yMin);
            writer.WriteLine(tilemapCollision.cellBounds.yMax);

            for (int y = tilemapCollision.cellBounds.yMax; y >= tilemapCollision.cellBounds.yMin; y--)
            {
                for (int x = tilemapCollision.cellBounds.xMin; x <= tilemapCollision.cellBounds.xMax; x++)
                {
                    TileBase tile = tilemapCollision.GetTile(new Vector3Int(x, y, 0));
                    if (tile != null)
                        writer.Write(Define.MAP_TOOL_NONE);
                    else
                        writer.Write(Define.MAP_TOOL_WALL);
                }
                writer.WriteLine();
            }
        }
    }

}
#endif
