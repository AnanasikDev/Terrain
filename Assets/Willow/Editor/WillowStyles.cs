using UnityEditor;
using UnityEngine;

public static class WillowStyles
{
    public static GUIStyle labelStyle = GUIStyle.none;

    public static void InitializeStyles()
    {
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 12;
    }
}
