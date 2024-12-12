using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor
{
    private Tilemap _tilemap;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _tilemap = (Tilemap)target;

        EditorGUILayout.Space(20);
        GUILayout.Label("Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("선택한 타일맵 초기화(삭제)"))
        {
            _tilemap.ClearAllTiles();
        }
    }
}