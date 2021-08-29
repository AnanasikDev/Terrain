using UnityEditor;
using UnityEngine;
using static WillowUtils;

public static class WillowBrushVis
{
    private static void _DrawBrush(Vector3 pos, Vector3 normal, float size)
    {
        Color c = new Color(0.1f, 0.1f, 0.2f, 0.5f);
        Handles.color = c;

        if (WillowTerrainSettings.BrushShape == BrushShape.Circle)
        {
            Handles.DrawSphere(0, pos + Vector3.down * size/6, Quaternion.identity, size*2);
            Color c1 = new Color(0.35f, 0.33f, 0.4f, 0.65f);
            Handles.color = c1;
            Handles.DrawSolidDisc(pos, normal, size);
        }

        /*if (WillowTerrainSettings.BrushShape == BrushShape.Circle)
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
        }*/
    }
    public static void BrushVis()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit screenHit))
            return; // Do not draw if no collision detected

        _DrawBrush(screenHit.point, screenHit.normal, WillowTerrainSettings.BrushSize);

        SceneView.RepaintAll();
    }
}
