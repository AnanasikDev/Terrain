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
using static WillowInput;
using System.IO;
using UnityEditor.SceneManagement;

public sealed class WillowTerrainEditor : EditorWindow
{
    private Vector2 scrollPos = Vector2.zero;
    private bool Quited = false;

    [MenuItem(WillowGlobalConfig.Path + "Prefab brush")]
    public static void ShowWindow()
    {
        GetWindow<WillowTerrainEditor>("Terrain++");
    }

    private void EnableWillow()
    {
        WillowTerrainSettings.IsActive = true;

        Selection.activeObject = null;
    }
    private void DisableWillow()
    {
        WillowTerrainSettings.IsActive = false;
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
                WillowFileManager.TryRead();
            }
        }

        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);


        if (WillowTerrainSettings.IsActive)
        {
            GUI.backgroundColor = LiteGreenColor;
            GUI.contentColor = RedTextColor;
            if (GUILayout.Button("Disable")) DisableWillow();
        }
        if (!WillowTerrainSettings.IsActive)
        {
            GUI.backgroundColor = LiteRedColor;
            GUI.contentColor = GreenTextColor;
            if (GUILayout.Button("Enable")) EnableWillow();
        }


        GUI.contentColor = DefaultContentColor;
        GUI.backgroundColor = DefaultBackGroundColor;


        DrawBrushTabs();

        if (WillowTerrainSettings.Erase)
        {
            EditorGUILayout.BeginVertical("box");
            WillowTerrainSettings.EraseSmoothness = EditorGUILayout.IntField("  Erase smoothness", WillowTerrainSettings.EraseSmoothness);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(20);
    }
    private void DrawBrushTabs()
    {
        int id = GUILayout.Toolbar((int)WillowTerrainSettings.BrushMode, WillowTerrainSettings.BrushTabs);

        WillowTerrainSettings.BrushMode = (BrushMode)id;

        if (WillowTerrainSettings.BrushMode == BrushMode.Place)
        {
            
        }
        if (WillowTerrainSettings.BrushMode == BrushMode.Erase)
        {
            DrawErasingTab();
        }
        if (WillowTerrainSettings.BrushMode == BrushMode.Exchange)
        {
            DrawExchangingTab();
        }
    }

    // BRUSHES TABS
    private void DrawErasingTab()
    {
        EditorGUILayout.BeginVertical("box");

        WillowTerrainSettings.EraseSmoothness = EditorGUILayout.IntField("Erase smoothness", WillowTerrainSettings.EraseSmoothness);

        EditorGUILayout.EndVertical();
    }
    private void DrawExchangingTab()
    {
        EditorGUILayout.BeginVertical("box");

        WillowTerrainSettings.ExchangeSmoothness = EditorGUILayout.IntField("Exchange smoothness", WillowTerrainSettings.ExchangeSmoothness);

        WillowTerrainSettings.ExchangePosition = EditorGUILayout.Toggle("Exchange position", WillowTerrainSettings.ExchangePosition);
        WillowTerrainSettings.ExchangeRotation = EditorGUILayout.Toggle("Exchange rotation", WillowTerrainSettings.ExchangeRotation);
        WillowTerrainSettings.ExchangeScale = EditorGUILayout.Toggle("Exchange scale", WillowTerrainSettings.ExchangeScale);
        WillowTerrainSettings.ExchangeParent = EditorGUILayout.Toggle("Exchange parent", WillowTerrainSettings.ExchangeParent);
        WillowTerrainSettings.ExchangeColor = EditorGUILayout.Toggle("Exchange color", WillowTerrainSettings.ExchangeColor);

        EditorGUILayout.EndVertical();
    }

    // OPTIONS TABS
    private void DrawTabs()
    {
        WillowTerrainSettings.OptionsTabSelectedId = GUILayout.Toolbar(WillowTerrainSettings.OptionsTabSelectedId, WillowTerrainSettings.OptionsTabs);
        
        switch (WillowTerrainSettings.OptionsTabSelectedId)
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
        WillowTerrainSettings.BrushDensity = EditorGUILayout.IntField("Brush density", WillowTerrainSettings.BrushDensity);
        
        if (WillowTerrainSettings.BrushDensity < 0)
            WillowTerrainSettings.BrushDensity = 0;
        WillowTerrainSettings.BrushSize = EditorGUILayout.FloatField("Brush size", WillowTerrainSettings.BrushSize);
        if (WillowTerrainSettings.BrushSize < 0) 
            WillowTerrainSettings.BrushSize = 0;

        WillowTerrainSettings.BrushShape = (WillowUtils.BrushShape)EditorGUILayout.EnumPopup("Brush shape", WillowTerrainSettings.BrushShape);
        WillowTerrainSettings.FillBrush = EditorGUILayout.Toggle("Fill brush", WillowTerrainSettings.FillBrush);

        WillowTerrainSettings.IndexObjects = EditorGUILayout.Toggle("Index objects", WillowTerrainSettings.IndexObjects);
        if (WillowTerrainSettings.IndexObjects)
            WillowTerrainSettings.IndexFormat = EditorGUILayout.TextField("Index format", WillowTerrainSettings.IndexFormat);

        WillowTerrainSettings.BaseParent = (Transform)EditorGUILayout.ObjectField("Parent", WillowTerrainSettings.BaseParent, typeof(Transform), true);
        WillowTerrainSettings.PlacementType = (WillowUtils.SpawnPlaceType)EditorGUILayout.EnumPopup("Placement type", WillowTerrainSettings.PlacementType);

        WillowTerrainSettings.IgnoreInactiveLayers = EditorGUILayout.Toggle("Ignore inactive layers", WillowTerrainSettings.IgnoreInactiveLayers);

        WillowTerrainSettings.DebugMode = EditorGUILayout.Toggle("Debug mode", WillowTerrainSettings.DebugMode);

        WillowTerrainSettings.AutoSave = EditorGUILayout.Toggle("Auto save", WillowTerrainSettings.AutoSave);
        WillowTerrainSettings.PrefabsPath = EditorGUILayout.TextField("Prefabs path", WillowTerrainSettings.PrefabsPath);

        WillowTerrainSettings.RecalculatingLength = EditorGUILayout.FloatField("Recalculation check length", WillowTerrainSettings.RecalculatingLength);
        
        if (GUILayout.Button("Recalculate all"))
        {
            RecalculatePositionsSelected(WillowTerrainSettings.SpawnedObjects.ToArray());
            RecalculateRotationsSelected(WillowTerrainSettings.SpawnedObjects.ToArray());
            RecalculateScalesSelected(WillowTerrainSettings.SpawnedObjects.ToArray());
        }

        // General Info

        EditorGUILayout.Space(25);

        Color color = GUI.contentColor;

        GUIStyle style = GUIStyle.none;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 16;

        EditorGUILayout.LabelField($"<b><color=#CCCCCCFF>General Info</color></b>", style);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Total spawned: " + WillowTerrainSettings.SpawnedObjects.Where(o => o != null).ToArray().Length.ToString());
        EditorGUILayout.LabelField("Layers amount: " + WillowTerrainSettings.LayersName.Count.ToString());
        EditorGUILayout.LabelField("Total spawnable objects: " + WillowTerrainSettings.SpawnableObjects.Count);

    }
    private void DrawObjectsTab()
    {
        DrawSpawnablesAddButton();

        for (int i = 0; i < WillowTerrainSettings.SpawnableObjects.Count; i++)
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
        OnEnable();
    }
    private void OnEnable()
    {
        InitializeStyles();
        WillowFileManager.Read();

        Log("Willow started..", Green);

        EditorSceneManager.sceneOpened += Read;

        SceneView.duringSceneGui += OnSceneGUI;
        WillowObjectsController.OnRepaint += Repaint;
        EditorApplication.quitting += Quit;
        EditorApplication.quitting += WillowClearingDestroyed.ClearDestroyedObjects;
    }
    private void OnDisable()
    {
        if (Quited) return;

        EditorSceneManager.sceneOpened -= Read;
        
        Log("Willow ended..", Green);

        SceneView.duringSceneGui -= OnSceneGUI;
        WillowObjectsController.OnRepaint -= Repaint;
        EditorApplication.quitting -= Quit;
        EditorApplication.quitting -= WillowClearingDestroyed.ClearDestroyedObjects;

    }
    private void Read(UnityEngine.SceneManagement.Scene newScene, OpenSceneMode mode)
    {
        WillowFileManager.TryRead();
    }
    private void Quit() => Quited = true;
    private void OnSceneGUI(SceneView sceneView)
    {
        SceneGUI();

        Repaint();
    }
}
