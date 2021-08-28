using UnityEditor;
using UnityEngine;
using System;
using static WillowUtils;
using static WillowObjectsController;
using static WillowUndo;
public static class WillowInput
{
    // Events
    public static event Func<BrushMode> OnBrushModeChanged;
    public static event Func<float> OnBrushSizeChanged;
    public static event Action OnEnabled;
    public static event Action OnDisabled;
    public static event Action OnSaved;

    // Keys
    public static KeyCode PlaceModeKey = KeyCode.C;
    public static KeyCode EraseModeKey = KeyCode.V;
    public static KeyCode ExchangeModeKey = KeyCode.B;
    public static KeyCode BrushSizeModeKey = KeyCode.F;

    public static float MouseScrollSensitivity = 0.2f;

    private static bool SceneView = true;
    private static bool BrushSizeKeyHeld = false;

    private static BrushMode GetPlaceModeInput()
    {
        BrushMode mode = WillowTerrainSettings.BrushMode;
        
        if (Event.current.type != EventType.KeyDown) 
            return mode;

        if (Event.current.keyCode == PlaceModeKey)
        {
            mode = BrushMode.Place;
        }
        else if (Event.current.keyCode == EraseModeKey)
        {
            mode = BrushMode.Erase;
        }
        else if (Event.current.keyCode == ExchangeModeKey)
        {
            mode = BrushMode.Exchange;
        }

        OnBrushModeChanged?.Invoke();

        return mode;
    }

    private static float GetBrushSizeChange()
    {
        // s + mouse scroll
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == BrushSizeModeKey)
                BrushSizeKeyHeld = true;
        }
        else if (Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == BrushSizeModeKey)
                BrushSizeKeyHeld = false;
        }

        if (!BrushSizeKeyHeld) return 0;

        if (Event.current.type == EventType.ScrollWheel)
        {
            return -Event.current.delta.y * MouseScrollSensitivity;
        }

        return 0;
    }

    public static void SceneGUI()
    {

        if (!WillowTerrainSettings.IsActive) return;


        if (Event.current.type == EventType.MouseLeaveWindow)
            SceneView = false;
        else if (Event.current.type == EventType.MouseEnterWindow)
            SceneView = true;


        if (!SceneView)
        {
            EditorApplication.RepaintHierarchyWindow();
            return;
        }

        WillowTerrainSettings.BrushMode = GetPlaceModeInput();
        WillowTerrainSettings.BrushSize = (float)System.Math.Round(Mathf.Clamp(WillowTerrainSettings.BrushSize + GetBrushSizeChange(), 0.05f, 1024f), 2);

        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        // If Left Mouse Button is pressed
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            // Erasing objects
            if (WillowTerrainSettings.BrushMode == BrushMode.Erase || Event.current.control)
            {
                EraseObjects();

                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.DirtyHierarchyWindowSorting();

                GUIUtility.hotControl = controlId;
                Event.current.Use();
            }

            // Placing objects
            else if (WillowTerrainSettings.BrushMode == BrushMode.Place)
            {
                PlaceObjects();

                GUIUtility.hotControl = controlId;
                Event.current.Use();
            }

            // Exchanging objects
            else if (WillowTerrainSettings.BrushMode == BrushMode.Exchange)
            {
                ExchangeObjects();

                GUIUtility.hotControl = controlId;
                Event.current.Use();
            }
        }

        // Undo
        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Z)
        {
            if (Event.current.modifiers == EventModifiers.Control)
            {
                GUIUtility.hotControl = controlId;
                Event.current.Use();

                Undo();
            }
        }

        // Saving
        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S &&
            Event.current.modifiers == EventModifiers.Control)
        {
            WillowFileManager.Write();
        }

        // Brush visualization
        if (Event.current != null)
            WillowBrushVis.BrushVis();

        EditorApplication.RepaintHierarchyWindow();
    }
}
