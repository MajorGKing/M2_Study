using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(MapTool))]
public class MapToolEditor : Editor
{
    private Tilemap _tilemap;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapTool tool = (MapTool)target;

        EditorGUILayout.Space(20);
        DrawUILine(thickness: 3);
        GUILayout.Label("Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Draw Collision"))
        {
            tool.DrawCollision();
        }
        DrawUILine(thickness: 3);

    }

    #region Helper
    public static void DrawUILine(Color color = default, int thickness = 1, int padding = 10, int margin = 0)
    {
        color = color != default ? color : Color.grey;
        Rect r = EditorGUILayout.GetControlRect(false, GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding * 0.5f;

        switch (margin)
        {
            case < 0:
                r.x = 0;
                r.width = EditorGUIUtility.currentViewWidth;

                break;
            case > 0:
                r.x += margin;
                r.width -= margin * 2;
                break;
        }

        EditorGUI.DrawRect(r, color);
    }

    #endregion
}
