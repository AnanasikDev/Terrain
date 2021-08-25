using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static WillowUtils;
using static WillowObjectsController;
using static WillowGlobalConfig;
using static WillowObjectsRecalculation;
using static WillowStyles;
using static WillowUndo;
using static WillowDebug;
using static WillowSpawnableObjectManager;
using System.IO;

public sealed class WillowTerrainEditor : EditorWindow
{
    private bool sceneview = true;
    private Vector2 scrollPos = Vector2.zero;
    private bool Quited = false;

    [MenuItem(WillowGlobalConfig.Path + "Prefab brush")]
    public static void ShowWindow()
    {
        GetWindow<WillowTerrainEditor>("Terrain++");
    }

    private void EnableWillow()
    {
        WillowTerrainSettings.active = true;

        Selection.activeObject = null;
    }
    private void DisableWillow()
    {
        WillowTerrainSettings.active = false;
    }

    private void SceneGUI()
    {
        if (!WillowTerrainSettings.active) return;

        if (Event.current.type == EventType.MouseLeaveWindow)
            sceneview = false;
        else if (Event.current.type == EventType.MouseEnterWindow)
            sceneview = true;
        if (!sceneview)
        {
            EditorApplication.RepaintHierarchyWindow();
            return;
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control) ||
            (Event.current.type == EventType.MouseDown && Event.current.button == 0 && WillowTerrainSettings.brushTabSelectedId == 1)) // Destroying objects
        {
            EraseObjects();

            Repaint();
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.DirtyHierarchyWindowSorting();

            GUIUtility.hotControl = controlId;
            Event.current.Use();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && WillowTerrainSettings.brushTabSelectedId == 0) // Placing objects
        {
            PlaceObjects();

            GUIUtility.hotControl = controlId;
            Event.current.Use();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && WillowTerrainSettings.brushTabSelectedId == 2) // Exchanging objects
        {
            ExchangeObjects();

            GUIUtility.hotControl = controlId;
            Event.current.Use();
        }

        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Z)
        {
            if (Event.current.modifiers == EventModifiers.Control)
            {
                GUIUtility.hotControl = controlId;
                Event.current.Use();

                Undo();
                Repaint();
            }
        }

        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S && 
            Event.current.modifiers == EventModifiers.Control)
        {
            // Save
            WillowFileManager.Write();
        }
        
        if (Event.current != null) 
            WillowBrushVis.BrushVis();

        EditorApplication.RepaintHierarchyWindow();
    }

    private void DrawHeader()
    {
        GUILayout.BeginHorizontal("box");

        GUI.backgroundColor = GreenColor;
        GUI.contentColor = TextColor;
        if (GUILayout.Button("Save"))
        {
            WillowFileManager.Write();
        }

        GUI.backgroundColor = RedColor;
        GUI.contentColor = TextColor;
        
        if (GUILayout.Button("Revert", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Willow Revert", "Are you sure to REVERT all changings of the last session? You can not undo this action.", "Revert", "Cancel"))
            {
                WillowFileManager.Read();
            }
        }

        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);


        if (WillowTerrainSettings.active)
        {
            GUI.backgroundColor = LiteGreenColor;
            GUI.contentColor = RedTextColor;
            if (GUILayout.Button("Disable")) DisableWillow();
        }
        if (!WillowTerrainSettings.active)
        {
            GUI.backgroundColor = LiteRedColor;
            GUI.contentColor = GreenTextColor;
            if (GUILayout.Button("Enable")) EnableWillow();
        }


        GUI.contentColor = DefaultContentColor;
        GUI.backgroundColor = DefaultBackGroundColor;


        DrawBrushTabs();

        if (WillowTerrainSettings.erase)
        {
            EditorGUILayout.BeginVertical("box");
            WillowTerrainSettings.eraseSmoothness = EditorGUILayout.IntField("  Erase smoothness", WillowTerrainSettings.eraseSmoothness);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(20);
    }
    private void DrawBrushTabs()
    {
        WillowTerrainSettings.brushTabSelectedId = GUILayout.Toolbar(WillowTerrainSettings.brushTabSelectedId, WillowTerrainSettings.brushTabs);
        if (WillowTerrainSettings.brushTabSelectedId == 0) // Placing
        {
            
        }
        if (WillowTerrainSettings.brushTabSelectedId == 1) // Erasing
        {
            DrawErasingTab();
        }
        if (WillowTerrainSettings.brushTabSelectedId == 2) // Exchanging
        {
            DrawExchangingTab();
        }
    }

    // BRUSHES TABS
    private void DrawErasingTab()
    {
        EditorGUILayout.BeginVertical("box");

        WillowTerrainSettings.eraseSmoothness = EditorGUILayout.IntField("Erase smoothness", WillowTerrainSettings.eraseSmoothness);

        EditorGUILayout.EndVertical();
    }
    private void DrawExchangingTab()
    {
        EditorGUILayout.BeginVertical("box");

        WillowTerrainSettings.exchangeSmoothness = EditorGUILayout.IntField("Exchange smoothness", WillowTerrainSettings.exchangeSmoothness);

        WillowTerrainSettings.exchangePosition = EditorGUILayout.Toggle("Exchange position", WillowTerrainSettings.exchangePosition);
        WillowTerrainSettings.exchangeRotation = EditorGUILayout.Toggle("Exchange rotation", WillowTerrainSettings.exchangeRotation);
        WillowTerrainSettings.exchangeScale = EditorGUILayout.Toggle("Exchange scale", WillowTerrainSettings.exchangeScale);
        WillowTerrainSettings.exchangeParent = EditorGUILayout.Toggle("Exchange parent", WillowTerrainSettings.exchangeParent);
        WillowTerrainSettings.exchangeColor = EditorGUILayout.Toggle("Exchange color", WillowTerrainSettings.exchangeColor);

        EditorGUILayout.EndVertical();
    }

    // OPTIONS TABS
    private void DrawTabs()
    {
        WillowTerrainSettings.optionsTabSelectedId = GUILayout.Toolbar(WillowTerrainSettings.optionsTabSelectedId, WillowTerrainSettings.optionsTabs);
        
        switch (WillowTerrainSettings.optionsTabSelectedId)
        {
            case 0:
                DrawSettingsTab();
                break;
            case 1:
                WillowLayers.DrawLayersTab();
                break;
            case 2:
                DrawObjectsTab();
                break;
        }
    }
    private void DrawSettingsTab()
    {
        WillowTerrainSettings.density = EditorGUILayout.IntField("Brush density", WillowTerrainSettings.density);
        
        if (WillowTerrainSettings.density < 0)
            WillowTerrainSettings.density = 0;
        WillowTerrainSettings.brushSize = EditorGUILayout.FloatField("Brush size", WillowTerrainSettings.brushSize);
        if (WillowTerrainSettings.brushSize < 0) 
            WillowTerrainSettings.brushSize = 0;

        WillowTerrainSettings.brushShape = (WillowUtils.BrushShape)EditorGUILayout.EnumPopup("Brush shape", WillowTerrainSettings.brushShape);
        WillowTerrainSettings.fillBrush = EditorGUILayout.Toggle("Fill brush", WillowTerrainSettings.fillBrush);

        WillowTerrainSettings.indexObjects = EditorGUILayout.Toggle("Index objects", WillowTerrainSettings.indexObjects);
        if (WillowTerrainSettings.indexObjects)
            WillowTerrainSettings.indexFormat = EditorGUILayout.TextField("Index format", WillowTerrainSettings.indexFormat);

        WillowTerrainSettings.parent = (Transform)EditorGUILayout.ObjectField("Parent", WillowTerrainSettings.parent, typeof(Transform), true);
        WillowTerrainSettings.placementType = (WillowUtils.SpawnPlaceType)EditorGUILayout.EnumPopup("Placement type", WillowTerrainSettings.placementType);

        WillowTerrainSettings.ignoreInactiveLayers = EditorGUILayout.Toggle("Ignore inactive layers", WillowTerrainSettings.ignoreInactiveLayers);

        WillowTerrainSettings.debugMode = EditorGUILayout.Toggle("Debug mode", WillowTerrainSettings.debugMode);

        WillowTerrainSettings.autoSave = EditorGUILayout.Toggle("Auto save", WillowTerrainSettings.autoSave);
        WillowTerrainSettings.PrefabsPath = EditorGUILayout.TextField("Prefabs path", WillowTerrainSettings.PrefabsPath);

        WillowTerrainSettings.RecalculatingLength = EditorGUILayout.FloatField("Recalculation check length", WillowTerrainSettings.RecalculatingLength);
        
        if (GUILayout.Button("Recalculate all"))
        {
            RecalculatePositionsSelected(WillowTerrainSettings.spawnedObjects.ToArray());
            RecalculateRotationsSelected(WillowTerrainSettings.spawnedObjects.ToArray());
            RecalculateScalesSelected(WillowTerrainSettings.spawnedObjects.ToArray());
        }

        // General Info

        EditorGUILayout.Space(25);

        Color color = GUI.contentColor;

        GUIStyle style = GUIStyle.none;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 16;

        EditorGUILayout.LabelField($"<b><color=#CCCCCCFF>General Info</color></b>", style);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Total spawned: " + WillowTerrainSettings.spawnedObjects.Where(o => o != null).ToArray().Length.ToString());
        EditorGUILayout.LabelField("Layers amount: " + WillowTerrainSettings.layersName.Count.ToString());
        EditorGUILayout.LabelField("Total spawnable objects: " + WillowTerrainSettings.spawnableObjects.Count);

    }
    private void DrawObjectsTab()
    {
        DrawSpawnablesAddButton();

        for (int i = 0; i < WillowTerrainSettings.spawnableObjects.Count; i++)
        {
            WillowSpawnableObjectManager.DrawSpawnableObject(i);
        }
        EditorGUILayout.Space();
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        
        DrawHeader();

        DrawTabs();
        
        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S && Event.current.modifiers == EventModifiers.Control)
        {
            // Save
            WillowFileManager.Write();
        }
        GUILayout.EndScrollView();
    }

    private void OnValidate()
    {
        InitializeStyles();
        OnEnable();
    }
    private void OnEnable()
    {
        WillowFileManager.Read();

        Log("Willow started..", Green);

        SceneView.duringSceneGui += OnSceneGUI;
        WillowObjectsController.OnRepaint += Repaint;
        //EditorApplication.quitting += WillowFileManager.Write;
        //UnityEditor.
        EditorApplication.quitting += Quit;
        //EditorApplication.quitting += WillowClearingDestroyed.ClearDestroyedObjects;
        //EditorApplication.update += SceneAutoSave;
        //UnityEditor.EventSystems.EventSystemEditor.
    }
    private void OnDisable()
    {
        if (Quited)
        {
            WillowClearingDestroyed.ClearDestroyedObjects();
            return;
        }

        //WillowFileanager.Write();
        
        Log("Willow ended..", Green);

        SceneView.duringSceneGui -= OnSceneGUI;
        WillowObjectsController.OnRepaint -= Repaint;
        //EditorApplication.quitting -= WillowFileManager.Write;
        EditorApplication.quitting -= Quit;
        //EditorApplication.quitting -= WillowClearingDestroyed.ClearDestroyedObjects;
        //EditorApplication.update -= SceneAutoSave;

    }
    private void Quit() => Quited = true;
    private void OnSceneGUI(SceneView sceneView) => SceneGUI();
}
