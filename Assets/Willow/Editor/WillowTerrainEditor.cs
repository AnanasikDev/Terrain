using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static WillowUtils;
using static WillowObjectsController;
using static WillowGlobalConfig;


public sealed class WillowTerrainEditor : EditorWindow
{
    private bool sceneview = true;
    private string newLayerName = "defaultLayer";
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Tools/Willow/Prefab brush")]
    public static void ShowWindow()
    {
        GetWindow<WillowTerrainEditor>("Terrain++");
    }
    private void _DrawBrush(Vector3 pos, float size)
    {
        Color c = new Color(0.1f, 0.1f, 0.2f, 0.5f);
        Handles.color = c;
        if (WillowTerrainSettings.brushShape == BrushShape.Circle)
            Handles.DrawSolidDisc(pos, Vector3.up, size);
        else if (WillowTerrainSettings.brushShape == BrushShape.Square)
        {
            float normSize = size * 1.6f;
            Handles.DrawSolidRectangleWithOutline(new Vector3[4] {
                new Vector3(pos.x - normSize/2, pos.y, pos.z - normSize / 2),
                new Vector3(pos.x - normSize/2, pos.y, pos.z + normSize / 2),
                new Vector3(pos.x + normSize/2, pos.y, pos.z + normSize / 2),
                new Vector3(pos.x + normSize/2, pos.y, pos.z - normSize / 2)
            },  new Color(0.1f, 0.1f, 0.2f, 0.8f), c);
        }
    }
    public void BrushVis()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit screenHit;
        Physics.Raycast(ray, out screenHit);

        _DrawBrush(screenHit.point, WillowTerrainSettings.brushSize);

        SceneView.RepaintAll();
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

    public void SceneGUI()
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
            }
        }

        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S && 
            Event.current.modifiers == EventModifiers.Control)
        {
            // Save
            WillowFileManager.Write();
        }
        
        if (Event.current != null) BrushVis();

        EditorApplication.RepaintHierarchyWindow();
    }
    
    public void Undo()
    {
        if (WillowTerrainSettings.changelog.Count == 0)
        {
            if (WillowTerrainSettings.debugMode) Debug.LogError(WillowUtils.FormatLog("Undo stack is empty!"));
            return;
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        Change lastChange = WillowTerrainSettings.changelog.Pop();
        if (lastChange.type == ChangeType.Placement)
        {
            GameObject[] changedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.gameObject.hideFlags = hidden;
                obj.SetActive(false);
                WillowTerrainSettings.spawnedObjects.Remove(obj);
                WillowTerrainSettings.destroyedObjects.Add(obj);
            }
        }
        else if (lastChange.type == ChangeType.Erasure)
        {
            GameObject[] changedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.hideFlags = active;
                obj.SetActive(true);
                WillowTerrainSettings.destroyedObjects.Remove(obj);
                WillowTerrainSettings.spawnedObjects.Add(obj);
            }
            GUIUtility.hotControl = controlId;
            Event.current.Use();
        }
        else if (lastChange.type == ChangeType.Exchange)
        {
            GameObject[] destroyedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in destroyedObjsTemp)
            {
                obj.hideFlags = active;
                obj.SetActive(true);
                WillowTerrainSettings.destroyedObjects.Remove(obj);
                WillowTerrainSettings.spawnedObjects.Add(obj);
            }

            GameObject[] spawnedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in spawnedObjsTemp)
            {
                obj.hideFlags = hidden;
                obj.SetActive(false);
                WillowTerrainSettings.spawnedObjects.Remove(obj);
                WillowTerrainSettings.destroyedObjects.Add(obj);
            }
        }
        Repaint();
        EditorApplication.RepaintHierarchyWindow();
    }

    public void DrawHeader()
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
   
    public void DrawBrushTabs()
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
    public void DrawErasingTab()
    {
        EditorGUILayout.BeginVertical("box");

        WillowTerrainSettings.eraseSmoothness = EditorGUILayout.IntField("Erase smoothness", WillowTerrainSettings.eraseSmoothness);

        EditorGUILayout.EndVertical();
    }
    public void DrawExchangingTab()
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
    public void DrawTabs()
    {
        WillowTerrainSettings.optionsTabSelectedId = GUILayout.Toolbar(WillowTerrainSettings.optionsTabSelectedId, WillowTerrainSettings.optionsTabs);
    }
    public void DrawSettingsTab()
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
    public void DrawLayersTab()
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
    public void DrawLayersArray()
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
                        if (WillowTerrainSettings.debugMode) Debug.LogError(WillowUtils.FormatLog($"Impossible to remove the layer: {dependedAmount} objects depend on it."));
                    }
                    else
                        WillowTerrainSettings.layersName[layerId] = "";
                }
                else
                {
                    if (WillowTerrainSettings.debugMode) Debug.LogError(WillowUtils.FormatLog("Impossible to remove the last layer."));
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
                    if (WillowTerrainSettings.debugMode) Debug.LogWarning(WillowUtils.FormatLog("Impossible to hold several layers with the same name."));
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
    public void DrawObjectsTab()
    {
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label(WillowTerrainSettings.spawnableObjects.Count.ToString());
        if (GUILayout.Button("Add"))
        {
            WillowTerrainSettings.spawnableObjects.Add(new SpawnableObject());
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < WillowTerrainSettings.spawnableObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box");
            
            int removeBtnHeight = 40;
            if (i < WillowTerrainSettings.spawnableObjects.Count - 1 || i == 0) removeBtnHeight = 60;
            if (!WillowTerrainSettings.spawnableObjects[i].hidden)
            {
                if (GUILayout.Button("‹", GUILayout.Width(18), GUILayout.Height(18)))
                {

                    WillowTerrainSettings.spawnableObjects[i].hidden = true;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    continue;
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("›", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    WillowTerrainSettings.spawnableObjects[i].hidden = false;
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label(WillowTerrainSettings.spawnableObjects[i].spawnableObject != null ? (WillowTerrainSettings.spawnableObjects[i].renameObject ? WillowTerrainSettings.spawnableObjects[i].newObjectName : WillowTerrainSettings.spawnableObjects[i].spawnableObject.name) : "null");
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

            }
            if (i > 0)
                if (GUILayout.Button("˄", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    var temp = WillowTerrainSettings.spawnableObjects[i];
                    WillowTerrainSettings.spawnableObjects[i] = WillowTerrainSettings.spawnableObjects[i - 1];
                    WillowTerrainSettings.spawnableObjects[i - 1] = temp;
                }
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(removeBtnHeight)))
            {
                WillowTerrainSettings.spawnableObjects.RemoveAt(i);
                continue;
            }
            if (i < WillowTerrainSettings.spawnableObjects.Count - 1)
                if (GUILayout.Button("˅", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    var temp = WillowTerrainSettings.spawnableObjects[i];
                    WillowTerrainSettings.spawnableObjects[i] = WillowTerrainSettings.spawnableObjects[i + 1];
                    WillowTerrainSettings.spawnableObjects[i + 1] = temp;
                }
            if (GUILayout.Button("+", GUILayout.Width(18), GUILayout.Height(18)))
            {
                WillowTerrainSettings.spawnableObjects.Insert(i, new SpawnableObject(WillowTerrainSettings.spawnableObjects[i]));
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box");

            WillowTerrainSettings.spawnableObjects[i].spawn = EditorGUILayout.Toggle("Spawn", WillowTerrainSettings.spawnableObjects[i].spawn);
            if (!WillowTerrainSettings.spawnableObjects[i].spawn)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }
            EditorGUILayout.BeginVertical("box");

            Label("GameObject");

            WillowTerrainSettings.spawnableObjects[i].spawnableObject = (GameObject)EditorGUILayout.ObjectField("GameObject", WillowTerrainSettings.spawnableObjects[i].spawnableObject, typeof(GameObject), true);

            WillowTerrainSettings.spawnableObjects[i].renameObject = EditorGUILayout.Toggle("Rename object", WillowTerrainSettings.spawnableObjects[i].renameObject);
            if (WillowTerrainSettings.spawnableObjects[i].renameObject)
                WillowTerrainSettings.spawnableObjects[i].newObjectName = EditorGUILayout.TextField("  Name ", WillowTerrainSettings.spawnableObjects[i].newObjectName);

            WillowTerrainSettings.spawnableObjects[i].centerObject = EditorGUILayout.Toggle("Center Object", WillowTerrainSettings.spawnableObjects[i].centerObject);

            WillowTerrainSettings.spawnableObjects[i].customParent = EditorGUILayout.Toggle("Custom parent", WillowTerrainSettings.spawnableObjects[i].customParent);
            if (WillowTerrainSettings.spawnableObjects[i].customParent)
                WillowTerrainSettings.spawnableObjects[i].parent = (Transform)EditorGUILayout.ObjectField("  Parent", WillowTerrainSettings.spawnableObjects[i].parent, typeof(Transform), true);

            WillowTerrainSettings.spawnableObjects[i].layerIndex = EditorGUILayout.Popup("Layer ", WillowTerrainSettings.spawnableObjects[i].layerIndex, WillowTerrainSettings.layersName.ToArray());
            if (WillowTerrainSettings.spawnableObjects[i].layerIndex >= WillowTerrainSettings.layersName.Count)
                WillowTerrainSettings.spawnableObjects[i].layer = WillowTerrainSettings.layersName[0];
            else
                WillowTerrainSettings.spawnableObjects[i].layer = WillowTerrainSettings.layersName[WillowTerrainSettings.spawnableObjects[i].layerIndex];

            WillowTerrainSettings.spawnableObjects[i].spawnChance = EditorGUILayout.IntField("Chance", WillowTerrainSettings.spawnableObjects[i].spawnChance); //objs[i].spawnChance
            if (WillowTerrainSettings.spawnableObjects[i].spawnChance < 0) WillowTerrainSettings.spawnableObjects[i].spawnChance = 0;

            WillowTerrainSettings.spawnableObjects[i].RecalculatePosition = GUILayout.Button("Recalculate position");
            if (WillowTerrainSettings.spawnableObjects[i].RecalculatePosition)
            {
                foreach (GameObject obj in WillowTerrainSettings.spawnedObjects)
                {
                    obj.GetComponent<WillowSpawnedObject>().RecalculateObjectPosition();
                }
            }

            Label("Rotation");
            
            WillowTerrainSettings.spawnableObjects[i].rotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", WillowTerrainSettings.spawnableObjects[i].rotationType);
            if (WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.Random ||
                WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.RandomAsNormal ||
                WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedRandomAsNormal)
            {
                WillowTerrainSettings.spawnableObjects[i].multiRotationAxis = EditorGUILayout.Toggle("Multi axis", WillowTerrainSettings.spawnableObjects[i].multiRotationAxis);
                if (WillowTerrainSettings.spawnableObjects[i].multiRotationAxis)
                {
                    WillowTerrainSettings.spawnableObjects[i].randomMinRotation = EditorGUILayout.Vector3Field("  Min rotation", WillowTerrainSettings.spawnableObjects[i].randomMinRotation);
                    WillowTerrainSettings.spawnableObjects[i].randomMaxRotation = EditorGUILayout.Vector3Field("  Max rotation", WillowTerrainSettings.spawnableObjects[i].randomMaxRotation);
                }
                else
                    WillowTerrainSettings.spawnableObjects[i].rotationAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", WillowTerrainSettings.spawnableObjects[i].rotationAxis);
            }

            if (WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.Static ||
                WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.StaticAsNormal ||
                WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedStaticAsNormal)
                WillowTerrainSettings.spawnableObjects[i].customEulersRotation = EditorGUILayout.Vector3Field("  Custom Euler Rotation", WillowTerrainSettings.spawnableObjects[i].customEulersRotation);

            if (WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedRandomAsNormal)
            {
                WillowTerrainSettings.spawnableObjects[i].randomizeLerpValue = EditorGUILayout.Toggle("  Randomize lerp value", WillowTerrainSettings.spawnableObjects[i].randomizeLerpValue);
                if (WillowTerrainSettings.spawnableObjects[i].randomizeLerpValue)
                {
                    WillowTerrainSettings.spawnableObjects[i].minLerpValue = EditorGUILayout.FloatField("  Min lerp value", WillowTerrainSettings.spawnableObjects[i].minLerpValue);
                    WillowTerrainSettings.spawnableObjects[i].maxLerpValue = EditorGUILayout.FloatField("  Max lerp value", WillowTerrainSettings.spawnableObjects[i].maxLerpValue);
                }
                else
                {
                    WillowTerrainSettings.spawnableObjects[i].lerpValue = EditorGUILayout.FloatField("  Lerp value", WillowTerrainSettings.spawnableObjects[i].lerpValue);
                }
            }
            if (WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedStaticAsNormal ||
                WillowTerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedAsPrefabAsNormal)
                WillowTerrainSettings.spawnableObjects[i].lerpValue = EditorGUILayout.FloatField("  Lerp value", WillowTerrainSettings.spawnableObjects[i].lerpValue);


            Label("Position");

            WillowTerrainSettings.spawnableObjects[i].modifyPosition = EditorGUILayout.Toggle("Modify position", WillowTerrainSettings.spawnableObjects[i].modifyPosition);
            if (WillowTerrainSettings.spawnableObjects[i].modifyPosition)
                WillowTerrainSettings.spawnableObjects[i].positionAddition = EditorGUILayout.Vector3Field("  Position addition", WillowTerrainSettings.spawnableObjects[i].positionAddition);

            Label("Scale");

            WillowTerrainSettings.spawnableObjects[i].modScale = EditorGUILayout.Toggle("Modify scale", WillowTerrainSettings.spawnableObjects[i].modScale);
            if (WillowTerrainSettings.spawnableObjects[i].modScale)
            {
                WillowTerrainSettings.spawnableObjects[i].scaleType = (ScaleType)EditorGUILayout.EnumPopup(  "Scale", WillowTerrainSettings.spawnableObjects[i].scaleType);
                
                if (WillowTerrainSettings.spawnableObjects[i].scaleType == ScaleType.Random)
                {
                    WillowTerrainSettings.spawnableObjects[i].separateScaleAxis = EditorGUILayout.Toggle("  Separate axis", WillowTerrainSettings.spawnableObjects[i].separateScaleAxis);
                    WillowTerrainSettings.spawnableObjects[i].scaleAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", WillowTerrainSettings.spawnableObjects[i].scaleAxis);
                    if (WillowTerrainSettings.spawnableObjects[i].separateScaleAxis)
                    {
                        WillowTerrainSettings.spawnableObjects[i].scaleMinSeparated = EditorGUILayout.Vector3Field("  Min scale", WillowTerrainSettings.spawnableObjects[i].scaleMinSeparated);
                        WillowTerrainSettings.spawnableObjects[i].scaleMaxSeparated = EditorGUILayout.Vector3Field("  Max scale", WillowTerrainSettings.spawnableObjects[i].scaleMaxSeparated);
                    }
                    else
                    {
                        WillowTerrainSettings.spawnableObjects[i].scaleMin = EditorGUILayout.FloatField("  Min scale", WillowTerrainSettings.spawnableObjects[i].scaleMin);
                        WillowTerrainSettings.spawnableObjects[i].scaleMax = EditorGUILayout.FloatField("  Max scale", WillowTerrainSettings.spawnableObjects[i].scaleMax);
                    }
                }
                if (WillowTerrainSettings.spawnableObjects[i].scaleType == ScaleType.Static)
                {
                    WillowTerrainSettings.spawnableObjects[i].separateScaleAxis = EditorGUILayout.Toggle("  Separate axis", WillowTerrainSettings.spawnableObjects[i].separateScaleAxis);
                    if (WillowTerrainSettings.spawnableObjects[i].separateScaleAxis)
                        WillowTerrainSettings.spawnableObjects[i].customScale = EditorGUILayout.Vector3Field("  Custom scale", WillowTerrainSettings.spawnableObjects[i].customScale);
                    else
                    {
                        WillowTerrainSettings.spawnableObjects[i].customScale = new Vector3(1, 1, 1);
                        float scale = EditorGUILayout.FloatField("  Scale", WillowTerrainSettings.spawnableObjects[i].customScale.x);
                        WillowTerrainSettings.spawnableObjects[i].customScale = new Vector3(scale, scale, scale);
                    }
                }

            }

            Label("Color");

            WillowTerrainSettings.spawnableObjects[i].modColor = EditorGUILayout.Toggle("Modify color", WillowTerrainSettings.spawnableObjects[i].modColor);
            
            if (WillowTerrainSettings.spawnableObjects[i].modColor)
            {
                WillowTerrainSettings.spawnableObjects[i].colorModPercentage = EditorGUILayout.FloatField("  Color modification %", WillowTerrainSettings.spawnableObjects[i].colorModPercentage);
                if (WillowTerrainSettings.spawnableObjects[i].colorModPercentage < 0) WillowTerrainSettings.spawnableObjects[i].colorModPercentage = 0;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        void Label(string text)
        {
            GUIStyle labelStyle = GUIStyle.none;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 12;

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField($"<b><color=#CCCCCCFF>{text}</color></b>", labelStyle);
            EditorGUILayout.Space(12);
            EditorGUILayout.BeginVertical("box");
        }
    }

    public void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        DrawHeader();

        DrawTabs();

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
        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S && Event.current.modifiers == EventModifiers.Control)
        {
            // Save
            WillowFileManager.Write();
        }
        GUILayout.EndScrollView();
    }

    private void OnEnable()
    {
        WillowFileManager.Read();
        
        if (WillowTerrainSettings.debugMode) Debug.Log(WillowUtils.FormatLog("Willow started!", "#00FF00FF"));

        SceneView.duringSceneGui += OnSceneGUI;
        WillowObjectsController.OnRepaint += Repaint;
        EditorApplication.quitting += WillowFileManager.Write;
    }
    private void OnDisable()
    {
        WillowFileManager.Write();
        
        if (WillowTerrainSettings.debugMode) Debug.Log(WillowUtils.FormatLog("Willow ended..", "#00FF00FF"));

        SceneView.duringSceneGui -= OnSceneGUI;
        WillowObjectsController.OnRepaint -= Repaint;
        EditorApplication.quitting -= WillowFileManager.Write;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        SceneGUI();
    }
}