﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using static WillowUtils;
using static WillowObjectsController;
using static WillowUndo;
public static class WillowInput
{
    // Keys
    public static bool UseHotkeys = true;

    public static KeyCode PlaceModeKey      = KeyCode.C;
    public static KeyCode EraseModeKey      = KeyCode.V;
    public static KeyCode ExchangeModeKey   = KeyCode.B;
    public static KeyCode BrushSizeModeKey  = KeyCode.F;
    public static KeyCode EnableKey         = KeyCode.E;

    public static float MouseScrollSensitivity = 0.2f;

    private static bool SceneView = true;
    private static bool BrushSizeKeyHeld = false;

    public static void GetInput()
    {
        if (Event.current == null || !UseHotkeys) 
            return;

        bool active = GetEnableChange();
        if (active)
            WillowTerrainEditor.EnableWillow();
        else
            WillowTerrainEditor.DisableWillow();

        if (!WillowTerrainSettings.IsActive) 
            return;

        WillowTerrainSettings.BrushMode = GetPlaceModeInput();
        WillowTerrainSettings.BrushSize = GetBrushSizeChange();
    }
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

        if (!BrushSizeKeyHeld) 
            return WillowTerrainSettings.BrushSize;

        if (Event.current.type == EventType.ScrollWheel)
        {
            return (float)System.Math.Round
                (
                Mathf.Clamp
                (
                    WillowTerrainSettings.BrushSize + (-Event.current.delta.y * MouseScrollSensitivity),
                    0.05f, 1024f
                    ), 2
                );
        }

        return WillowTerrainSettings.BrushSize;
    }
    private static bool GetEnableChange()
    {
        // ctrl + e
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == EnableKey)
        {
            if (Event.current.modifiers == EventModifiers.Control)
            {
                return !WillowTerrainSettings.IsActive;
            }
        }
        return WillowTerrainSettings.IsActive;
    }

    public static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GetInput();
    }
    public static void SceneGUI()
    {
        GetInput();

        if (WillowTerrainSettings.AvoidAutomatically) AvoidObstacles();

        if (!WillowTerrainSettings.IsActive) return; //

        if (Event.current.type == EventType.MouseLeaveWindow)
            SceneView = false;
        else if (Event.current.type == EventType.MouseEnterWindow)
            SceneView = true;


        if (!SceneView)
        {
            EditorApplication.RepaintHierarchyWindow();
            return;
        }


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

                if (WillowTerrainSettings.DeselectAutomatically)
                {
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                }
            }

            // Placing objects
            else if (WillowTerrainSettings.BrushMode == BrushMode.Place)
            {
                PlaceObjects();

                if (WillowTerrainSettings.DeselectAutomatically)
                {
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                }
            }

            // Exchanging objects
            else if (WillowTerrainSettings.BrushMode == BrushMode.Exchange)
            {
                ExchangeObjects();

                if (WillowTerrainSettings.DeselectAutomatically)
                {
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                }
            }
        }

        // Undo
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Z)
        {
            if (Event.current.modifiers == EventModifiers.Control)
            {
                Undo();

                if (WillowTerrainSettings.DeselectAutomatically)
                {
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                }
            }
        }

        // Saving

        GetSaving();

        // Brush visualization
        WillowBrushVis.BrushVis();

        EditorApplication.RepaintHierarchyWindow();
    }
    private static void GetSaving()
    {
        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S &&
            Event.current.modifiers == EventModifiers.Control)
        {
            WillowFileManager.Write();
        }
    }

    public static void AvoidObstacles()
    {
        foreach (GameObject spawnedObject in WillowTerrainSettings.SpawnedObjects.Where(x => x))
        {
            spawnedObject.GetComponent<WillowSpawnedObject>().AvoidObstacles();
        }
    }
}
#endif
