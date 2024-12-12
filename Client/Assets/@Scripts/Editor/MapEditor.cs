using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
#endif

public class MapEditor : MonoBehaviour
{
#if UNITY_EDITOR

    [MenuItem("Tools/GenerateMap %#m")]
    private static void GenerateMap()
    {
        GameObject[] gameObjects = Selection.gameObjects;

        foreach (GameObject go in gameObjects)
        {
            Tilemap tm = Utils.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            using (var writer = File.CreateText($"Assets/@Resources/Data/MapData/{go.name}Collision.txt"))
            {
                writer.WriteLine(tm.cellBounds.xMin);
                writer.WriteLine(tm.cellBounds.xMax);
                writer.WriteLine(tm.cellBounds.yMin);
                writer.WriteLine(tm.cellBounds.yMax);

                for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
                {
                    for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                            writer.Write(Define.MAP_TOOL_NONE);
                        else
                            writer.Write(Define.MAP_TOOL_WALL);
                    }
                    writer.WriteLine();
                }
            }
        }

        Debug.Log("Map Collision Generation Complete");
    }
   

#endif

}
