using UnityEditor;
using UnityEngine;
using static WillowUtils;

public static class WillowBrushVis
{
    private static void _DrawBrush(Vector3 pos, float size)
    {
        Color c = new Color(0.1f, 0.1f, 0.2f, 0.5f);
        Handles.color = c;
        if (WillowTerrainSettings.BrushShape == BrushShape.Circle)
            Handles.DrawSolidDisc(pos, Vector3.up, size);
        else if (WillowTerrainSettings.BrushShape == BrushShape.Square)
        {
            float normSize = size * 1.6f;
            Handles.DrawSolidRectangleWithOutline(new Vector3[4] {
                new Vector3(pos.x - normSize/2, pos.y, pos.z - normSize / 2),
                new Vector3(pos.x - normSize/2, pos.y, pos.z + normSize / 2),
                new Vector3(pos.x + normSize/2, pos.y, pos.z + normSize / 2),
                new Vector3(pos.x + normSize/2, pos.y, pos.z - normSize / 2)
            }, new Color(0.1f, 0.1f, 0.2f, 0.8f), c);
        }
    }
    public static void BrushVis()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit screenHit;
        if (!Physics.Raycast(ray, out screenHit))
            return; // Do not draw if no collision detected

        _DrawBrush(screenHit.point, WillowTerrainSettings.BrushSize);

        SceneView.RepaintAll();
    }
}
