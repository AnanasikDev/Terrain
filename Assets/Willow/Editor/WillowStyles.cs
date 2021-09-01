#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class WillowStyles
{
    public static GUIStyle labelStyle = GUIStyle.none;

    public static Color GreenColor = new Color(0.3f, 1f, 0.3f, 1);
    public static Color LiteGreenColor = new Color(0.3f, 1f, 0.3f, 1);
    public static Color RedColor = new Color(1f, 0.3f, 0.3f, 1);
    public static Color LiteRedColor = new Color(1f, 0.3f, 0.3f, 1);
    public static Color DefaultBackGroundColor;
    public static Color DefaultContentColor;
    public static Color TextColor = new Color(0.9f, 0.9f, 0.95f, 1);

    public static Color GreenTextColor = new Color(0.4f, 0.9f, 0.4f, 1);
    public static Color RedTextColor = new Color(0.9f, 0.4f, 0.4f, 1);

    public static void InitializeStyles()
    {
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 12;

        DefaultBackGroundColor = GUI.backgroundColor;
        DefaultContentColor = GUI.contentColor;
    }
}
#endif
