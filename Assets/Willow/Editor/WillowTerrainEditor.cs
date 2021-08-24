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
public sealed class WillowTerrainEditor : EditorWindow
{
    private bool sceneview = true;
    private string newLayerName = "defaultLayer";
    private Vector2 scrollPos = Vector2.zero;
    private bool Quited = false;

    [MenuItem(Path + "Prefab brush")]
    public static void ShowWindow()
    {
        GetWindow<WillowTerrainEditor>("Terrain++");
    }

    private void Enable()
    {
        WillowTerrainSettings.active = true;

        Selection.activeObject = null;
    }
    private void Disable()
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
        Color oldBackgroundColor = GUI.backgroundColor;
        Color oldContentColor = GUI.contentColor;
        GUI.backgroundColor = new Color(0.3f, 1f, 0.3f, 1);
        GUI.contentColor = new Color(0.9f, 0.9f, 0.95f, 1);
        if (GUILayout.Button("Save"))
        {
            WillowFileManager.Write();
        }

        GUILayout.Space(20);

        if (WillowTerrainSettings.active)
        {
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f, 1);
            GUI.contentColor = new Color(0.9f, 0.6f, 0.6f, 1);
            if (GUILayout.Button("Disable")) Disable();
        }
        if (!WillowTerrainSettings.active)
        {
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f, 1);
            GUI.contentColor = new Color(0.6f, 0.9f, 0.6f, 1);
            if (GUILayout.Button("Enable")) Enable();
        }

        GUI.contentColor = oldContentColor;
        GUI.backgroundColor = oldBackgroundColor;

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
                DrawLayersTab();
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
    private void DrawLayersTab()
    {
        EditorGUILayout.BeginHorizontal("box");
        bool add = false;
        if (GUILayout.Button("Add"))
            add = true;

        newLayerName = EditorGUILayout.TextField("New layer name: ", newLayerName);
        if (add && newLayerName != "")
        {
            if (!WillowTerrainSettings.layersName.Contains(newLayerName))
            {
                WillowTerrainSettings.layersName.Add(newLayerName);
                WillowTerrainSettings.layersState.Add(true);
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawLayersArray();
    }
    private void DrawLayersArray()
    {
        WillowTerrainSettings.layersName.RemoveAll(layerName => layerName == "");
        for (int layerId = 0; layerId < WillowTerrainSettings.layersName.Count; layerId++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginHorizontal("box");
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18)))
            {
                if (WillowTerrainSettings.layersName.Count > 1)
                {
                    int dependedAmount = 0;
                    foreach (GameObject spawnedObj in WillowTerrainSettings.spawnedObjects)
                    {
                        if (spawnedObj.GetComponent<WillowSpawnedObject>().Layer == WillowTerrainSettings.layersName[layerId])
                        {
                            dependedAmount++;
                        }
                    }
                    if (dependedAmount > 0)
                    {
                        Log($"Impossible to remove the layer: {dependedAmount} objects depend on it.", Yellow);
                    }
                    else
                        WillowTerrainSettings.layersName[layerId] = "";
                }
                else
                {
                    Log("Impossible to remove the last layer.", Yellow, Debug.LogError);
                }
            }
            GUI.backgroundColor = oldBgColor;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginVertical("box");

            bool layerActiveBefore = WillowTerrainSettings.layersState[layerId];
            WillowTerrainSettings.layersState[layerId] = EditorGUILayout.Toggle("Active", WillowTerrainSettings.layersState[layerId]);

            if (layerActiveBefore != WillowTerrainSettings.layersState[layerId]) // Toggle value changed
            {
                List<GameObject> spawned = WillowTerrainSettings.spawnedObjects.Where(
                                    o => o != null && 
                                    o.hideFlags == active && 
                                    o.GetComponent<WillowSpawnedObject>().Layer == WillowTerrainSettings.layersName[layerId]).ToList();

                if (WillowTerrainSettings.layersState[layerId]) // If active
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(true);
                    }
                if (!WillowTerrainSettings.layersState[layerId]) // If inactive
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(false);
                    }
            }

            string lastLayerName = WillowTerrainSettings.layersName[layerId];
            WillowTerrainSettings.layersName[layerId] = EditorGUILayout.TextField("Name: ", WillowTerrainSettings.layersName[layerId]);

            if (lastLayerName != WillowTerrainSettings.layersName[layerId]) // Layer was renamed
            {
                if (WillowTerrainSettings.layersName[layerId] == "") WillowTerrainSettings.layersName[layerId] += "layer";
                if (WillowTerrainSettings.layersName.FindAll(x => x == WillowTerrainSettings.layersName[layerId]).Count > 1)
                {
                    WillowTerrainSettings.layersName[layerId] = lastLayerName;
                    Log("Impossible to hold several layers with the same name.", Yellow, Debug.LogWarning);
                }
                else foreach (GameObject spawnedObj in WillowTerrainSettings.spawnedObjects)
                {
                    if (spawnedObj != null && spawnedObj.hideFlags == active)
                    {
                        WillowSpawnedObject spawnedObjectSc = spawnedObj.GetComponent<WillowSpawnedObject>();
                        if (spawnedObjectSc != null && spawnedObjectSc.Layer == lastLayerName)
                        {
                            spawnedObjectSc.Layer = WillowTerrainSettings.layersName[layerId];
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
    private void DrawObjectsTab()
    {
        DrawSpawnablesAddButton();

        for (int i = 0; i < WillowTerrainSettings.spawnableObjects.Count; i++)
        {
            DrawSpawnableObject(i);
        }
        EditorGUILayout.Space();
    }

    private void DrawSpawnablesAddButton()
    {
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label(WillowTerrainSettings.spawnableObjects.Count.ToString());
        if (GUILayout.Button("Add"))
        {
            WillowTerrainSettings.spawnableObjects.Add(new WillowSpawnableObject());
        }
        EditorGUILayout.EndHorizontal();
    }
    private void DrawLabel(string text, int offstep = 12)
    {
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(offstep);
        EditorGUILayout.LabelField($"<b><color=#CCCCCCFF>{text}</color></b>", labelStyle);
        EditorGUILayout.Space(offstep);
        EditorGUILayout.BeginVertical("box");
    }
    private void DrawSpawnableObject(int index)
    {
        if (!DrawSpawnableUI(index)) return;
        DrawSpawnableSettings(index);
    }
    private bool DrawSpawnableUI(int index)
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.BeginVertical("box");

        int removeBtnHeight = 40;
        if (index < WillowTerrainSettings.spawnableObjects.Count - 1 || index == 0) removeBtnHeight = 60;
        if (!WillowTerrainSettings.spawnableObjects[index].Hidden)
        {
            if (GUILayout.Button("‹", GUILayout.Width(18), GUILayout.Height(18)))
            {

                WillowTerrainSettings.spawnableObjects[index].Hidden = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                return false;
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("›", GUILayout.Width(18), GUILayout.Height(18)))
            {
                WillowTerrainSettings.spawnableObjects[index].Hidden = false;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label(WillowTerrainSettings.spawnableObjects[index].Object != null ? (WillowTerrainSettings.spawnableObjects[index].RenameObject ? WillowTerrainSettings.spawnableObjects[index].NewObjectName : WillowTerrainSettings.spawnableObjects[index].Object.name) : "null");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return false;
            }

        }
        if (index > 0)
            if (GUILayout.Button("˄", GUILayout.Width(18), GUILayout.Height(18)))
            {
                var temp = WillowTerrainSettings.spawnableObjects[index];
                WillowTerrainSettings.spawnableObjects[index] = WillowTerrainSettings.spawnableObjects[index - 1];
                WillowTerrainSettings.spawnableObjects[index - 1] = temp;
            }

        Color bgc = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.9f, 0.45f, 0.44f);

        if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(removeBtnHeight)))
        {
            WillowTerrainSettings.spawnableObjects.RemoveAt(index);
            return false;
        }

        GUI.backgroundColor = bgc;

        if (index < WillowTerrainSettings.spawnableObjects.Count - 1)
            if (GUILayout.Button("˅", GUILayout.Width(18), GUILayout.Height(18)))
            {
                var temp = WillowTerrainSettings.spawnableObjects[index];
                WillowTerrainSettings.spawnableObjects[index] = WillowTerrainSettings.spawnableObjects[index + 1];
                WillowTerrainSettings.spawnableObjects[index + 1] = temp;
            }

        GUI.backgroundColor = new Color(0.35f, 0.85f, 0.32f);

        if (GUILayout.Button("+", GUILayout.Width(18), GUILayout.Height(18)))
        {
            WillowTerrainSettings.spawnableObjects.Insert(index, new WillowSpawnableObject(WillowTerrainSettings.spawnableObjects[index]));
        }

        GUI.backgroundColor = bgc;
        
        EditorGUILayout.EndVertical();

        return true;
    }
    private void DrawSpawnableSettings(int index)
    {
        WillowSpawnableObject spawnableObject = WillowTerrainSettings.spawnableObjects[index];

        EditorGUILayout.BeginVertical("box");

        spawnableObject.Spawn = EditorGUILayout.Toggle("Spawn", spawnableObject.Spawn);
        if (!spawnableObject.Spawn)
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
            //continue;
        }
        EditorGUILayout.BeginVertical("box");

        DrawLabel("GameObject");

        spawnableObject.Object = (GameObject)EditorGUILayout.ObjectField("GameObject", spawnableObject.Object, typeof(GameObject), true);

        spawnableObject.RenameObject = EditorGUILayout.Toggle("Rename object", spawnableObject.RenameObject);
        if (spawnableObject.RenameObject)
            spawnableObject.NewObjectName = EditorGUILayout.TextField("  Name ", spawnableObject.NewObjectName);

        spawnableObject.CenterObject = EditorGUILayout.Toggle("Center Object", spawnableObject.CenterObject);

        spawnableObject.CustomParent = EditorGUILayout.Toggle("Custom parent", spawnableObject.CustomParent);
        if (spawnableObject.CustomParent)
            spawnableObject.Parent = (Transform)EditorGUILayout.ObjectField("  Parent", spawnableObject.Parent, typeof(Transform), true);

        spawnableObject.LayerIndex = EditorGUILayout.Popup("Layer ", spawnableObject.LayerIndex, WillowTerrainSettings.layersName.ToArray());
        if (spawnableObject.LayerIndex >= WillowTerrainSettings.layersName.Count)
            spawnableObject.Layer = WillowTerrainSettings.layersName[0];
        else
            spawnableObject.Layer = WillowTerrainSettings.layersName[spawnableObject.LayerIndex];

        spawnableObject.SpawnChance = EditorGUILayout.IntField("Chance", spawnableObject.SpawnChance); //objs[i].spawnChance
        if (spawnableObject.SpawnChance < 0) spawnableObject.SpawnChance = 0;


        EditorGUILayout.BeginVertical("box");

        DrawLabel("Recalculate", 2);

        EditorGUILayout.BeginHorizontal("box");

        if (GUILayout.Button("Position"))
        {
            RecalculatePositionsSelected(WillowTerrainSettings.spawnedObjects
                .Where(o => o.GetComponent<WillowSpawnedObject>().Layer == spawnableObject.Layer).ToArray());

        }
        if (GUILayout.Button("Rotation"))
        {
            RecalculateRotationsSelected(WillowTerrainSettings.spawnedObjects
                .Where(o => o.GetComponent<WillowSpawnedObject>().Layer == spawnableObject.Layer).ToArray());
        }
        if (GUILayout.Button("Scale"))
        {
            RecalculateScalesSelected(WillowTerrainSettings.spawnedObjects
                .Where(o => o.GetComponent<WillowSpawnedObject>().Layer == spawnableObject.Layer).ToArray());
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        DrawLabel("Rotation");

        spawnableObject.RotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", spawnableObject.RotationType);
        if (spawnableObject.RotationType == RotationType.Random ||
            spawnableObject.RotationType == RotationType.RandomAsNormal ||
            spawnableObject.RotationType == RotationType.LerpedRandomAsNormal)
        {
            spawnableObject.MultiRotationAxis = EditorGUILayout.Toggle("Multi axis", spawnableObject.MultiRotationAxis);
            if (spawnableObject.MultiRotationAxis)
            {
                spawnableObject.RandomMinRotation = EditorGUILayout.Vector3Field("  Min rotation", spawnableObject.RandomMinRotation);
                spawnableObject.RandomMaxRotation = EditorGUILayout.Vector3Field("  Max rotation", spawnableObject.RandomMaxRotation);
            }
            else
                spawnableObject.RotationAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", spawnableObject.RotationAxis);
        }

        if (spawnableObject.RotationType == RotationType.Static ||
            spawnableObject.RotationType == RotationType.StaticAsNormal ||
            spawnableObject.RotationType == RotationType.LerpedStaticAsNormal)
            spawnableObject.CustomEulersRotation = EditorGUILayout.Vector3Field("  Custom Euler Rotation", spawnableObject.CustomEulersRotation);

        if (spawnableObject.RotationType == RotationType.LerpedRandomAsNormal)
        {
            spawnableObject.RandomizeLerpValue = EditorGUILayout.Toggle("  Randomize lerp value", spawnableObject.RandomizeLerpValue);
            if (spawnableObject.RandomizeLerpValue)
            {
                spawnableObject.MinLerpValue = EditorGUILayout.FloatField("  Min lerp value", spawnableObject.MinLerpValue);
                spawnableObject.MaxLerpValue = EditorGUILayout.FloatField("  Max lerp value", spawnableObject.MaxLerpValue);
            }
            else
            {
                spawnableObject.LerpValue = EditorGUILayout.FloatField("  Lerp value", spawnableObject.LerpValue);
            }
        }
        if (spawnableObject.RotationType == RotationType.LerpedStaticAsNormal ||
            spawnableObject.RotationType == RotationType.LerpedAsPrefabAsNormal)
            spawnableObject.LerpValue = EditorGUILayout.FloatField("  Lerp value", spawnableObject.LerpValue);

        spawnableObject.RotationEulerAddition = EditorGUILayout.Vector3Field("Add eulers", spawnableObject.RotationEulerAddition);

        DrawLabel("Position");

        spawnableObject.ModifyPosition = EditorGUILayout.Toggle("Modify position", spawnableObject.ModifyPosition);
        if (spawnableObject.ModifyPosition)
            spawnableObject.PositionAddition = EditorGUILayout.Vector3Field("  Position addition", spawnableObject.PositionAddition);

        DrawLabel("Scale");

        spawnableObject.ModifyScale = EditorGUILayout.Toggle("Modify scale", spawnableObject.ModifyScale);
        if (spawnableObject.ModifyScale)
        {
            spawnableObject.ScaleType = (ScaleType)EditorGUILayout.EnumPopup("Scale", spawnableObject.ScaleType);

            if (spawnableObject.ScaleType == ScaleType.Random)
            {
                spawnableObject.SeparateScaleAxis = EditorGUILayout.Toggle("  Separate axis", spawnableObject.SeparateScaleAxis);
                spawnableObject.ScaleAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", spawnableObject.ScaleAxis);
                if (spawnableObject.SeparateScaleAxis)
                {
                    spawnableObject.ScaleMinSeparated = EditorGUILayout.Vector3Field("  Min scale", spawnableObject.ScaleMinSeparated);
                    spawnableObject.ScaleMaxSeparated = EditorGUILayout.Vector3Field("  Max scale", spawnableObject.ScaleMaxSeparated);
                }
                else
                {
                    spawnableObject.ScaleMin = EditorGUILayout.FloatField("  Min scale", spawnableObject.ScaleMin);
                    spawnableObject.ScaleMax = EditorGUILayout.FloatField("  Max scale", spawnableObject.ScaleMax);
                }
            }
            if (spawnableObject.ScaleType == ScaleType.Static)
            {
                spawnableObject.SeparateScaleAxis = EditorGUILayout.Toggle("  Separate axis", spawnableObject.SeparateScaleAxis);
                if (spawnableObject.SeparateScaleAxis)
                    spawnableObject.CustomScale = EditorGUILayout.Vector3Field("  Custom scale", spawnableObject.CustomScale);
                else
                {
                    spawnableObject.CustomScale = new Vector3(1, 1, 1);
                    float scale = EditorGUILayout.FloatField("  Scale", spawnableObject.CustomScale.x);
                    spawnableObject.CustomScale = new Vector3(scale, scale, scale);
                }
            }

        }

        DrawLabel("Color");

        spawnableObject.ModifyColor = EditorGUILayout.Toggle("Modify color", spawnableObject.ModifyColor);

        if (spawnableObject.ModifyColor)
        {
            spawnableObject.ColorModPercentage = EditorGUILayout.FloatField("  Color modification %", spawnableObject.ColorModPercentage);
            if (spawnableObject.ColorModPercentage < 0) spawnableObject.ColorModPercentage = 0;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
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
        Application.quitting += Quit;
        EditorApplication.update += SceneAutoSave;
        //UnityEditor.EventSystems.EventSystemEditor.
    }
    private void SceneAutoSave()
    {
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
    private void OnDisable()
    {
        if (Quited) return;

        //WillowFileManager.Write();
        
        Log("Willow ended..", Green);

        SceneView.duringSceneGui -= OnSceneGUI;
        WillowObjectsController.OnRepaint -= Repaint;
        //EditorApplication.quitting -= WillowFileManager.Write;
        Application.quitting -= Quit;
        EditorApplication.update -= SceneAutoSave;
    }
    private void Quit()
    {
        Quited = true;
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        SceneGUI();
    }   
}
