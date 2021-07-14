using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static Utils;

public class TerrainEditor : EditorWindow
{
    string newLayerName = "defaultLayer";
    Vector2 scrollPos = Vector2.zero;
    Projector brushProjector;

    [MenuItem("Terrain/Prefab brush")]
    public static void ShowWindow()
    {
        GetWindow<TerrainEditor>("Terrain++");
    }
    private void Update()
    {
        if (TerrainSettings.active && TerrainSettings.validated)
        {
            Selection.objects = new Object[0];
            Selection.activeTransform = TerrainSettings.instance.transform;
        }
    }
    public void drawBrush(Vector3 Pos, Vector3 norm, float radius)
    {
        //Debug.Log("drawwww");
        Handles.color = new Color(0.1f, 0.1f, 0.2f, 0.5f);
        Handles.DrawSolidDisc(Pos, norm, radius);
        
    }
    public virtual void EraseObjects()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(ray, out hit);

        List<GameObject> spawnedObjectsTemp = TerrainSettings.spawnedObjects;
        GameObject[] objsToDestroy = 
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= TerrainSettings.brushSize * TerrainSettings.brushSize))
            .ToArray();

        foreach (GameObject o in objsToDestroy)
        {
            if (Random.Range(0, TerrainSettings.eraseSmoothness + 1) == TerrainSettings.eraseSmoothness || TerrainSettings.eraseSmoothness == 0)
            {
                if (o != null)
                {
                    o.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
                    o.SetActive(false);
                    spawnedObjectsTemp.Remove(o);
                    TerrainSettings.destroyedObjects.Add(o);
                }
            }
        }
        spawnedObjectsTemp.RemoveAll(o => o == null);
        TerrainSettings.spawnedObjects = spawnedObjectsTemp;

        TerrainSettings.changelog.Push(new Change(Utils.ChangeType.Erasure, null, objsToDestroy.ToList()));
        Repaint();
    }
    public virtual void PlaceObjects()
    {
        RaycastHit screenHit;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        bool ableToSpawn = Physics.Raycast(ray, out screenHit);
        List<GameObject> spawnedObjs = new List<GameObject>();
        if (ableToSpawn)
        {
            bool anySpawnableObjects = false;
            foreach (SpawnableObject spObj in TerrainSettings.spawnableObjects)
            {
                if (spObj.spawn)
                {
                    anySpawnableObjects = true;
                    break;
                }
            }
            if (TerrainSettings.spawnableObjects.Count > 0 && anySpawnableObjects)
            {
                for (int i = 0; i < TerrainSettings.density; i++)
                {
                    RaycastHit hit;

                    if (Physics.Raycast(screenHit.point + Vector3.up*5, 
                        new Vector3(UnityEngine.Random.Range(-0.085f * TerrainSettings.brushSize, 0.085f * TerrainSettings.brushSize), -1,
                                    UnityEngine.Random.Range(-0.085f * TerrainSettings.brushSize, 0.085f * TerrainSettings.brushSize)), out hit) &&

                        ((TerrainSettings.placementType == SpawnPlaceType.onTerrainOnly && hit.collider.gameObject.GetComponent<TerrainSettings>() != null) ||
                        (TerrainSettings.placementType == SpawnPlaceType.onObjectsOnly && hit.collider.gameObject.GetComponent<TerrainSettings>() == null) ||
                        (TerrainSettings.placementType == SpawnPlaceType.onTerrainAndObjects)) )
                    {
                        SpawnableObject spawnableObject = GetObject();
                        if (spawnableObject == null) continue;

                        GameObject temp = Instantiate(spawnableObject.spawnableObject, hit.point, Quaternion.identity);

                        SetObjectRotation(spawnableObject, temp, hit.normal, spawnableObject.customEulersRotation);
                        SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, temp);
                        temp.transform.localPosition += GetObjectPositionAdd(spawnableObject);
                        temp.transform.parent = GetObjectParent(spawnableObject);
                        temp.transform.localScale = GetObjectScale(spawnableObject);

                        temp.GetComponent<SpawnedObject>().positionAdd =
                            spawnableObject.modifyPosition ? spawnableObject.positionAddition : Vector3.zero;

                        temp.GetComponent<SpawnedObject>().layer = spawnableObject.layer;

                        if (spawnableObject.renameObject)
                            temp.name = spawnableObject.newObjectName;

                        if (spawnableObject.centerObject)
                            temp.transform.localPosition += new Vector3(0, spawnableObject.spawnableObject.transform.localScale.y / 2, 0);

                        TerrainSettings.spawnedObjects.Add(temp);

                        spawnedObjs.Add(temp);
                    }
                }

                TerrainSettings.changelog.Push(new Change(Utils.ChangeType.Placement, spawnedObjs, null));
                Repaint();
            }
            else
            {
                Debug.LogError(Utils.FormatLog("There are no objects to spawn!"));
            }
        }
        
    }
    public virtual void ExchangeObjects()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        List<GameObject> spawnedObjectsTemp = TerrainSettings.spawnedObjects;
        GameObject[] objsToExchange =
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= TerrainSettings.brushSize * TerrainSettings.brushSize))
            .ToArray();

        foreach (GameObject o in objsToExchange)
        {
            if (Random.Range(0, TerrainSettings.exchangeSmoothness + 1) == TerrainSettings.exchangeSmoothness || TerrainSettings.exchangeSmoothness == 0)
            {
                if (o != null)
                {
                    SpawnableObject spawnableObject = GetObject();
                    Vector3 position = TerrainSettings.exchangePosition ? o.transform.position : o.transform.position - o.GetComponent<SpawnedObject>().positionAdd + GetObjectPositionAdd(spawnableObject);
                    RaycastHit normalHit;
                    bool normalCasted = Physics.Raycast(position, -o.transform.up, out normalHit, 3);
                    Vector3 normal = normalCasted ? normalHit.normal : Vector3.up;
                    Transform parent = TerrainSettings.exchangeParent ? o.transform.parent : spawnableObject.parent;
                    GameObject spawned = Instantiate(spawnableObject.spawnableObject, position, Quaternion.identity, parent);
                    if (TerrainSettings.exchangeRotation)
                        spawned.transform.localRotation = o.transform.localRotation;
                    else
                        SetObjectRotation(spawnableObject, spawned, normal, spawnableObject.customEulersRotation);
                    if (TerrainSettings.exchangeColor)
                        SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, spawned, o.GetComponent<SpawnedObject>().renderer.sharedMaterial.color);
                    else SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, spawned);
                    
                    spawned.GetComponent<SpawnedObject>().positionAdd =
                        spawnableObject.modifyPosition ? spawnableObject.positionAddition : Vector3.zero;

                    spawned.transform.localScale = TerrainSettings.exchangeScale ? o.transform.localScale : GetObjectScale(spawnableObject);

                    spawnedObjectsTemp.Remove(o);
                    spawnedObjectsTemp.Add(spawned);

                    o.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
                    o.SetActive(false);
                    TerrainSettings.destroyedObjects.Add(o);
                }
            }
        }
        TerrainSettings.changelog.Push(new Change(Utils.ChangeType.Exchange, spawnedObjectsTemp.ToList(), objsToExchange.ToList()));
        Repaint();
        spawnedObjectsTemp.RemoveAll(o => o == null);
        TerrainSettings.spawnedObjects = spawnedObjectsTemp;
    }

    public virtual SpawnableObject GetObject()
    {
        int[] chances = new int[TerrainSettings.spawnableObjects.Count];
        bool ableToSpawn = false;
        for (int i = 0; i < chances.Length; i++)
        {
            chances[i] = TerrainSettings.spawnableObjects[i].spawn && TerrainSettings.spawnableObjects[i].spawnableObject != null ? TerrainSettings.spawnableObjects[i].spawnChance : 0;
            if (chances[i] > 0) ableToSpawn = true;
        }
        if (!ableToSpawn) return null;
        return new SpawnableObject( TerrainSettings.spawnableObjects[GetChance(chances)] );
    }
    public virtual void SetObjectRotation(SpawnableObject spawnableObject, GameObject spawnedObject, Vector3 normal, Vector3 custom)
    {
        if (spawnableObject.rotationType == RotationType.Static)
        {
            spawnedObject.transform.localEulerAngles = custom;
        }
        else if (spawnableObject.rotationType == RotationType.Random)
        {
            spawnedObject.transform.localEulerAngles = GetRandomRotation();
        }
        else if (spawnableObject.rotationType == RotationType.AsNormal)
        {
            spawnedObject.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
        }
        else if (spawnableObject.rotationType == RotationType.RandomAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            spawnedObject.transform.Rotate(GetRandomRotation(), Space.Self);
        }
        else if (spawnableObject.rotationType == RotationType.StaticAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            spawnedObject.transform.Rotate(custom, Space.Self);
        }
        else if (spawnableObject.rotationType == RotationType.LerpedStaticAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(custom), Quaternion.FromToRotation(Vector3.up, normal), spawnableObject.lerpValue);
        }
        else if (spawnableObject.rotationType == RotationType.LerpedRandomAsNormal)
        {
            float lerpV = spawnableObject.lerpValue;
            if (spawnableObject.randomizeLerpValue) lerpV = Random.Range(spawnableObject.minLerpValue, spawnableObject.maxLerpValue);

            spawnedObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(custom), Quaternion.FromToRotation(Vector3.up, normal), lerpV);
            spawnedObject.transform.Rotate(GetRandomRotation(), Space.Self);
        }
        else if (spawnableObject.rotationType == RotationType.LerpedAsPrefabAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.Lerp(spawnableObject.spawnableObject.transform.rotation, Quaternion.FromToRotation(Vector3.up, normal), spawnableObject.lerpValue);
        }
        else // if AsPrefab
        {
            spawnedObject.transform.rotation = spawnableObject.spawnableObject.transform.rotation;
        }

        Vector3 GetRandomRotation()
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (spawnableObject.multiRotationAxis)
            {
                x = Random.Range(spawnableObject.randomMinRotation.x, spawnableObject.randomMaxRotation.x);
                y = Random.Range(spawnableObject.randomMinRotation.y, spawnableObject.randomMaxRotation.y);
                z = Random.Range(spawnableObject.randomMinRotation.z, spawnableObject.randomMaxRotation.z);
            }
            else
            {
                string axis = spawnableObject.rotationAxis.ToString();

                if (axis.Contains("X")) x = Random.Range(0f, 360f);
                if (axis.Contains("Y")) y = Random.Range(0f, 360f);
                if (axis.Contains("Z")) z = Random.Range(0f, 360f);
            }

            return new Vector3(x, y, z);
        }
    }
    public virtual void SetObjectColor(bool modifyColor, float colorModificationPercentage, GameObject gameObject, Color? color = null)
    {
        if (modifyColor)
        {
            Renderer renderer = gameObject.GetComponent<SpawnedObject>().renderer;
            var tempMaterial = new Material(renderer.sharedMaterial);
            if (color == null)
                tempMaterial.color *= UnityEngine.Random.Range(1 - (colorModificationPercentage / 100), 1 + (colorModificationPercentage / 100));
            else
                tempMaterial.color = (Color)color;
            renderer.sharedMaterial = tempMaterial;
        }
    }
    public virtual Transform GetObjectParent(SpawnableObject obj)
    {
        if (obj.customParent) return obj.parent;
        else return TerrainSettings.parent;
    }
    public virtual Vector3 GetObjectPositionAdd(SpawnableObject obj)
    {
        return obj.modifyPosition ? obj.positionAddition : Vector3.zero;
    }
    public virtual Vector3 GetObjectScale(SpawnableObject spawnableObject)
    {
        Vector3 scale = Vector3.one;
        switch (spawnableObject.scaleType)
        {
            case ScaleType.AsPrefab:
                scale = spawnableObject.spawnableObject.transform.localScale;
                break;
            case ScaleType.Random:
                if (spawnableObject.modScale)
                {
                    float x = 1;
                    float y = 1;
                    float z = 1;

                    string axis = spawnableObject.scaleAxis.ToString();
                    if (spawnableObject.separateScaleAxis)
                    {
                        if (axis.Contains("X")) x = Random.Range(spawnableObject.scaleMinSeparated.x, spawnableObject.scaleMaxSeparated.x);
                        if (axis.Contains("Y")) y = Random.Range(spawnableObject.scaleMinSeparated.y, spawnableObject.scaleMaxSeparated.y);
                        if (axis.Contains("Z")) z = Random.Range(spawnableObject.scaleMinSeparated.z, spawnableObject.scaleMaxSeparated.z);
                    }
                    else
                    {
                        float value = Random.Range(spawnableObject.scaleMin, spawnableObject.scaleMax);
                        x = y = z = value;
                    }

                    scale = new Vector3(x, y, z);
                }
                    
                break;
            case ScaleType.Static:
                scale = spawnableObject.customScale;
                break;
        }
        return scale;
    }

    public virtual void SceneGUI()
    {
        if (!TerrainSettings.active) return;
        if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control) ||
            (Event.current.type == EventType.MouseDown && Event.current.button == 0 && TerrainSettings.brushTabSelectedId == 1)) // Destroying objects
        {
            EraseObjects();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && TerrainSettings.brushTabSelectedId == 0) // Placing objects
        {
            PlaceObjects();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && TerrainSettings.brushTabSelectedId == 2) // Exchanging objects
        {
            ExchangeObjects();
        }

        if (Event.current != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Z)
        {
            if (Event.current.modifiers == EventModifiers.Control)
            {
                Undo();
            }
        }

        //Debug.Log("scnee gui");

        BrushVis();
    }
    public virtual void BrushVis()
    {
        if (Event.current != null) //Event.current != null
        {
            Debug.Log("drawing");
            //Debug.Log(Event.current.mousePosition);

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit screenHit;
            Physics.Raycast(ray, out screenHit);

            drawBrush(screenHit.point, screenHit.normal, 10);
        }
    }
    public virtual void Undo()
    {
        if (TerrainSettings.changelog.Count == 0)
        {
            Debug.LogError(Utils.FormatLog("Undo stack is empty!"));
            return;
        }

        Change lastChange = TerrainSettings.changelog.Pop();
        if (lastChange.type == ChangeType.Placement)
        {
            GameObject[] changedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
                obj.SetActive(false);
            }
        }
        else if (lastChange.type == ChangeType.Erasure)
        {
            GameObject[] changedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.hideFlags = HideFlags.None;
                obj.SetActive(true);
                TerrainSettings.destroyedObjects.Remove(obj);
                TerrainSettings.spawnedObjects.Add(obj);
            }
        }
        else if (lastChange.type == ChangeType.Exchange)
        {
            GameObject[] destroyedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in destroyedObjsTemp)
            {
                obj.hideFlags = HideFlags.None;
                obj.SetActive(true);
                TerrainSettings.destroyedObjects.Remove(obj);
                TerrainSettings.spawnedObjects.Add(obj);
            }

            GameObject[] spawnedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in spawnedObjsTemp)
            {
                obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
                obj.SetActive(false);
                TerrainSettings.spawnedObjects.Remove(obj);
                TerrainSettings.destroyedObjects.Add(obj);
            }
        }
        Repaint();
    }
    public virtual void DrawHeader()
    {
        Color oldBackgroundColor = GUI.backgroundColor;
        Color oldContentColor = GUI.contentColor;
        if (TerrainSettings.active)
        {
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f, 1);
            GUI.contentColor = new Color(0.9f, 0.6f, 0.6f, 1);
            if (GUILayout.Button("Disable"))
            {
                TerrainSettings.active = false;
            }
        }
        if (!TerrainSettings.active)
        {
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f, 1);
            GUI.contentColor = new Color(0.6f, 0.9f, 0.6f, 1);
            if (GUILayout.Button("Enable"))
            {
                TerrainSettings.active = true;
            }
        }

        GUI.contentColor = oldContentColor;
        GUI.backgroundColor = oldBackgroundColor;

        DrawBrushTabs();

        if (TerrainSettings.erase)
        {
            EditorGUILayout.BeginVertical("box");
            TerrainSettings.eraseSmoothness = EditorGUILayout.IntField("  Erase smoothness", TerrainSettings.eraseSmoothness);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(20);
    }
    public virtual void DrawBrushTabs()
    {
        TerrainSettings.brushTabSelectedId = GUILayout.Toolbar(TerrainSettings.brushTabSelectedId, TerrainSettings.brushTabs);
        if (TerrainSettings.brushTabSelectedId == 1) // Erasing
        {
            DrawErasingTab();
        }
        if (TerrainSettings.brushTabSelectedId == 2) // Exchanging
        {
            DrawExchangingTab();
        }
    }
    // BRUSHES TABS
    public virtual void DrawErasingTab()
    {
        EditorGUILayout.BeginVertical("box");

        TerrainSettings.eraseSmoothness = EditorGUILayout.IntField("Erase smoothness", TerrainSettings.eraseSmoothness);

        EditorGUILayout.EndVertical();
    }
    public virtual void DrawExchangingTab()
    {
        EditorGUILayout.BeginVertical("box");

        TerrainSettings.exchangeSmoothness = EditorGUILayout.IntField("Exchange smoothness", TerrainSettings.exchangeSmoothness);

        TerrainSettings.exchangePosition = EditorGUILayout.Toggle("Exchange position", TerrainSettings.exchangePosition);
        TerrainSettings.exchangeRotation = EditorGUILayout.Toggle("Exchange rotation", TerrainSettings.exchangeRotation);
        TerrainSettings.exchangeScale = EditorGUILayout.Toggle("Exchange scale", TerrainSettings.exchangeScale);
        TerrainSettings.exchangeParent = EditorGUILayout.Toggle("Exchange parent", TerrainSettings.exchangeParent);
        TerrainSettings.exchangeColor = EditorGUILayout.Toggle("Exchange color", TerrainSettings.exchangeColor);

        EditorGUILayout.EndVertical();
    }
    // OPTIONS TABS
    public virtual void DrawTabs()
    {
        TerrainSettings.optionsTabSelectedId = GUILayout.Toolbar(TerrainSettings.optionsTabSelectedId, TerrainSettings.optionsTabs);
    }
    public virtual void DrawSettingsTab()
    {
        TerrainSettings.density = EditorGUILayout.IntField("Brush density", TerrainSettings.density);
        if (TerrainSettings.density < 0) TerrainSettings.density = 0;
        TerrainSettings.brushSize = EditorGUILayout.FloatField("Brush size", TerrainSettings.brushSize);
        if (TerrainSettings.brushSize < 0) TerrainSettings.brushSize = 0;

        TerrainSettings.parent = (Transform)EditorGUILayout.ObjectField("Parent", TerrainSettings.parent, typeof(Transform), true);
        TerrainSettings.placementType = (Utils.SpawnPlaceType)EditorGUILayout.EnumPopup("Placement type", TerrainSettings.placementType);


        // General Info

        EditorGUILayout.Space(25);

        Color color = GUI.contentColor;

        GUIStyle style = GUIStyle.none;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 16;

        EditorGUILayout.LabelField($"<b><color=#CCCCCCFF>General Info</color></b>", style);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Total spawned: " + TerrainSettings.spawnedObjects.Where(o => o != null).ToArray().Length.ToString());
        EditorGUILayout.LabelField("Layers amount: " + TerrainSettings.layers.Count.ToString());
        EditorGUILayout.LabelField("Total spawnable objects: " + TerrainSettings.spawnableObjects.Count);

        //TerrainSettings.BrushProjector = (Projector)EditorGUILayout.ObjectField("Projector", TerrainSettings.BrushProjector, typeof(Projector), false);

        foreach (SpawnableObject obj in TerrainSettings.spawnableObjects)
        {
            Debug.Log("layer = " + obj.layer);
        }
    }
    public virtual void DrawLayersTab()
    {
        EditorGUILayout.BeginHorizontal("box");
        bool add = false;
        if (GUILayout.Button("Add"))
            add = true;

        newLayerName = EditorGUILayout.TextField("New layer name: ", newLayerName);
        if (add && newLayerName != "")
        {
            if (!TerrainSettings.layers.Contains(newLayerName))
            {
                TerrainSettings.layers.Add(newLayerName);
                TerrainSettings.layerActive.Add(true);
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawLayersArray();
    }
    public virtual void DrawLayersArray()
    {
        TerrainSettings.layers.RemoveAll(layerName => layerName == "");
        for (int layerId = 0; layerId < TerrainSettings.layers.Count; layerId++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginHorizontal("box");
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18)))
            {
                if (TerrainSettings.layers.Count > 1)
                {
                    int dependedAmount = 0;
                    foreach (GameObject spawnedObj in TerrainSettings.spawnedObjects)
                    {
                        if (spawnedObj.GetComponent<SpawnedObject>().layer == TerrainSettings.layers[layerId])
                        {
                            dependedAmount++;
                        }
                    }
                    if (dependedAmount > 0)
                    {
                        Debug.LogError(Utils.FormatLog($"Impossible to remove the layer: {dependedAmount} objects depend on it."));
                    }
                    else
                        TerrainSettings.layers[layerId] = "";
                }
                else
                {
                    Debug.LogError(Utils.FormatLog("Impossible to remove the last layer."));
                }
            }
            GUI.backgroundColor = oldBgColor;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginVertical("box");

            bool layerActiveBefore = TerrainSettings.layerActive[layerId];
            TerrainSettings.layerActive[layerId] = EditorGUILayout.Toggle("Active", TerrainSettings.layerActive[layerId]);

            if (layerActiveBefore != TerrainSettings.layerActive[layerId]) // Toggle value changed
            {
                List<GameObject> spawned = TerrainSettings.spawnedObjects.Where(
                                    o => o != null && 
                                    o.hideFlags == HideFlags.None && 
                                    o.GetComponent<SpawnedObject>().layer == TerrainSettings.layers[layerId]).ToList();

                if (TerrainSettings.layerActive[layerId]) // If active
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(true);
                    }
                if (!TerrainSettings.layerActive[layerId]) // If inactive
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(false);
                    }
            }

            string lastLayerName = TerrainSettings.layers[layerId];
            TerrainSettings.layers[layerId] = EditorGUILayout.TextField("Name: ", TerrainSettings.layers[layerId]);

            if (lastLayerName != TerrainSettings.layers[layerId]) // Layer was renamed
            {
                if (TerrainSettings.layers[layerId] == "") TerrainSettings.layers[layerId] += "layer";
                if (TerrainSettings.layers.FindAll(x => x == TerrainSettings.layers[layerId]).Count > 1)
                {
                    TerrainSettings.layers[layerId] = lastLayerName;
                    Debug.LogWarning(Utils.FormatLog("Impossible to hold several layers with the same name."));
                }
                else foreach (GameObject spawnedObj in TerrainSettings.spawnedObjects)
                {
                    if (spawnedObj != null && spawnedObj.hideFlags == HideFlags.None)
                    {
                        SpawnedObject spawnedObjectSc = spawnedObj.GetComponent<SpawnedObject>();
                        if (spawnedObjectSc != null && spawnedObjectSc.layer == lastLayerName)
                        {
                            spawnedObjectSc.layer = TerrainSettings.layers[layerId];
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
    public virtual void DrawObjectsTab()
    {
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label(TerrainSettings.spawnableObjects.Count.ToString());
        if (GUILayout.Button("Add"))
        {
            TerrainSettings.spawnableObjects.Add(new SpawnableObject());
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < TerrainSettings.spawnableObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box");
            
            int removeBtnHeight = 40;
            if (i < TerrainSettings.spawnableObjects.Count - 1 || i == 0) removeBtnHeight = 60;
            if (!TerrainSettings.spawnableObjects[i].hidden)
            {
                if (GUILayout.Button("‹", GUILayout.Width(18), GUILayout.Height(18)))
                {

                    TerrainSettings.spawnableObjects[i].hidden = true;
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
                    TerrainSettings.spawnableObjects[i].hidden = false;
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label(TerrainSettings.spawnableObjects[i].spawnableObject != null ? (TerrainSettings.spawnableObjects[i].renameObject ? TerrainSettings.spawnableObjects[i].newObjectName : TerrainSettings.spawnableObjects[i].spawnableObject.name) : "null");
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

            }
            if (i > 0)
                if (GUILayout.Button("˄", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    var temp = TerrainSettings.spawnableObjects[i];
                    TerrainSettings.spawnableObjects[i] = TerrainSettings.spawnableObjects[i - 1];
                    TerrainSettings.spawnableObjects[i - 1] = temp;
                }
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(removeBtnHeight)))
            {
                TerrainSettings.spawnableObjects.RemoveAt(i);
                continue;
            }
            if (i < TerrainSettings.spawnableObjects.Count - 1)
                if (GUILayout.Button("˅", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    var temp = TerrainSettings.spawnableObjects[i];
                    TerrainSettings.spawnableObjects[i] = TerrainSettings.spawnableObjects[i + 1];
                    TerrainSettings.spawnableObjects[i + 1] = temp;
                }
            if (GUILayout.Button("+", GUILayout.Width(18), GUILayout.Height(18)))
            {
                TerrainSettings.spawnableObjects.Insert(i, new SpawnableObject(TerrainSettings.spawnableObjects[i]));
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box");

            TerrainSettings.spawnableObjects[i].spawn = EditorGUILayout.Toggle("Spawn", TerrainSettings.spawnableObjects[i].spawn);
            if (!TerrainSettings.spawnableObjects[i].spawn)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }
            EditorGUILayout.BeginVertical("box");

            Label("GameObject");

            TerrainSettings.spawnableObjects[i].spawnableObject = (GameObject)EditorGUILayout.ObjectField("GameObject", TerrainSettings.spawnableObjects[i].spawnableObject, typeof(GameObject), true);

            TerrainSettings.spawnableObjects[i].renameObject = EditorGUILayout.Toggle("Rename object", TerrainSettings.spawnableObjects[i].renameObject);
            if (TerrainSettings.spawnableObjects[i].renameObject)
                TerrainSettings.spawnableObjects[i].newObjectName = EditorGUILayout.TextField("  Name: ", TerrainSettings.spawnableObjects[i].newObjectName);

            TerrainSettings.spawnableObjects[i].centerObject = EditorGUILayout.Toggle("Center Object", TerrainSettings.spawnableObjects[i].centerObject);

            TerrainSettings.spawnableObjects[i].customParent = EditorGUILayout.Toggle("Custom parent", TerrainSettings.spawnableObjects[i].customParent);
            if (TerrainSettings.spawnableObjects[i].customParent)
                TerrainSettings.spawnableObjects[i].parent = (Transform)EditorGUILayout.ObjectField("  Parent", TerrainSettings.spawnableObjects[i].parent, typeof(Transform), true);

            TerrainSettings.spawnableObjects[i].layerIndex = EditorGUILayout.Popup("Layer: ", TerrainSettings.spawnableObjects[i].layerIndex, TerrainSettings.layers.ToArray());
            if (TerrainSettings.spawnableObjects[i].layerIndex >= TerrainSettings.layers.Count)
                TerrainSettings.spawnableObjects[i].layer = TerrainSettings.layers[0];
            else
                TerrainSettings.spawnableObjects[i].layer = TerrainSettings.layers[TerrainSettings.spawnableObjects[i].layerIndex];

            TerrainSettings.spawnableObjects[i].spawnChance = EditorGUILayout.IntField("Chance", TerrainSettings.spawnableObjects[i].spawnChance); //objs[i].spawnChance
            if (TerrainSettings.spawnableObjects[i].spawnChance < 0) TerrainSettings.spawnableObjects[i].spawnChance = 0;

            Label("Rotation");
            
            TerrainSettings.spawnableObjects[i].rotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", TerrainSettings.spawnableObjects[i].rotationType);
            if (TerrainSettings.spawnableObjects[i].rotationType == RotationType.Random ||
                TerrainSettings.spawnableObjects[i].rotationType == RotationType.RandomAsNormal ||
                TerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedRandomAsNormal)
            {
                TerrainSettings.spawnableObjects[i].multiRotationAxis = EditorGUILayout.Toggle("Multi axis", TerrainSettings.spawnableObjects[i].multiRotationAxis);
                if (TerrainSettings.spawnableObjects[i].multiRotationAxis)
                {
                    TerrainSettings.spawnableObjects[i].randomMinRotation = EditorGUILayout.Vector3Field("  Min rotation", TerrainSettings.spawnableObjects[i].randomMinRotation);
                    TerrainSettings.spawnableObjects[i].randomMaxRotation = EditorGUILayout.Vector3Field("  Max rotation", TerrainSettings.spawnableObjects[i].randomMaxRotation);
                }
                else
                    TerrainSettings.spawnableObjects[i].rotationAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", TerrainSettings.spawnableObjects[i].rotationAxis);
            }

            if (TerrainSettings.spawnableObjects[i].rotationType == RotationType.Static ||
                TerrainSettings.spawnableObjects[i].rotationType == RotationType.StaticAsNormal ||
                TerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedStaticAsNormal)
                TerrainSettings.spawnableObjects[i].customEulersRotation = EditorGUILayout.Vector3Field("  Custom Euler Rotation", TerrainSettings.spawnableObjects[i].customEulersRotation);

            if (TerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedRandomAsNormal)
            {
                TerrainSettings.spawnableObjects[i].randomizeLerpValue = EditorGUILayout.Toggle("  Randomize lerp value", TerrainSettings.spawnableObjects[i].randomizeLerpValue);
                if (TerrainSettings.spawnableObjects[i].randomizeLerpValue)
                {
                    TerrainSettings.spawnableObjects[i].minLerpValue = EditorGUILayout.FloatField("  Min lerp value", TerrainSettings.spawnableObjects[i].minLerpValue);
                    TerrainSettings.spawnableObjects[i].maxLerpValue = EditorGUILayout.FloatField("  Max lerp value", TerrainSettings.spawnableObjects[i].maxLerpValue);
                }
                else
                {
                    TerrainSettings.spawnableObjects[i].lerpValue = EditorGUILayout.FloatField("  Lerp value", TerrainSettings.spawnableObjects[i].lerpValue);
                }
            }
            if (TerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedStaticAsNormal ||
                TerrainSettings.spawnableObjects[i].rotationType == RotationType.LerpedAsPrefabAsNormal)
                TerrainSettings.spawnableObjects[i].lerpValue = EditorGUILayout.FloatField("  Lerp value", TerrainSettings.spawnableObjects[i].lerpValue);


            Label("Position");

            TerrainSettings.spawnableObjects[i].modifyPosition = EditorGUILayout.Toggle("Modify position", TerrainSettings.spawnableObjects[i].modifyPosition);
            if (TerrainSettings.spawnableObjects[i].modifyPosition)
                TerrainSettings.spawnableObjects[i].positionAddition = EditorGUILayout.Vector3Field("  Position addition", TerrainSettings.spawnableObjects[i].positionAddition);

            Label("Scale");

            TerrainSettings.spawnableObjects[i].modScale = EditorGUILayout.Toggle("Modify scale", TerrainSettings.spawnableObjects[i].modScale);
            if (TerrainSettings.spawnableObjects[i].modScale)
            {
                TerrainSettings.spawnableObjects[i].scaleType = (ScaleType)EditorGUILayout.EnumPopup(  "Scale", TerrainSettings.spawnableObjects[i].scaleType);
                
                if (TerrainSettings.spawnableObjects[i].scaleType == ScaleType.Random)
                {
                    TerrainSettings.spawnableObjects[i].separateScaleAxis = EditorGUILayout.Toggle("  Separate axis", TerrainSettings.spawnableObjects[i].separateScaleAxis);
                    TerrainSettings.spawnableObjects[i].scaleAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", TerrainSettings.spawnableObjects[i].scaleAxis);
                    if (TerrainSettings.spawnableObjects[i].separateScaleAxis)
                    {
                        TerrainSettings.spawnableObjects[i].scaleMinSeparated = EditorGUILayout.Vector3Field("  Min scale", TerrainSettings.spawnableObjects[i].scaleMinSeparated);
                        TerrainSettings.spawnableObjects[i].scaleMaxSeparated = EditorGUILayout.Vector3Field("  Max scale", TerrainSettings.spawnableObjects[i].scaleMaxSeparated);
                    }
                    else
                    {
                        TerrainSettings.spawnableObjects[i].scaleMin = EditorGUILayout.FloatField("  Min scale", TerrainSettings.spawnableObjects[i].scaleMin);
                        TerrainSettings.spawnableObjects[i].scaleMax = EditorGUILayout.FloatField("  Max scale", TerrainSettings.spawnableObjects[i].scaleMax);
                    }
                }
                if (TerrainSettings.spawnableObjects[i].scaleType == ScaleType.Static)
                {
                    TerrainSettings.spawnableObjects[i].separateScaleAxis = EditorGUILayout.Toggle("  Separate axis", TerrainSettings.spawnableObjects[i].separateScaleAxis);
                    if (TerrainSettings.spawnableObjects[i].separateScaleAxis)
                        TerrainSettings.spawnableObjects[i].customScale = EditorGUILayout.Vector3Field("  Custom scale", TerrainSettings.spawnableObjects[i].customScale);
                    else
                    {
                        TerrainSettings.spawnableObjects[i].customScale = new Vector3(1, 1, 1);
                        float scale = EditorGUILayout.FloatField("  Scale", TerrainSettings.spawnableObjects[i].customScale.x);
                        TerrainSettings.spawnableObjects[i].customScale = new Vector3(scale, scale, scale);
                    }
                }

            }

            Label("Color");

            TerrainSettings.spawnableObjects[i].modColor = EditorGUILayout.Toggle("Modify color", TerrainSettings.spawnableObjects[i].modColor);
            
            if (TerrainSettings.spawnableObjects[i].modColor)
            {
                TerrainSettings.spawnableObjects[i].colorModPercentage = EditorGUILayout.FloatField("  Color modification %", TerrainSettings.spawnableObjects[i].colorModPercentage);
                if (TerrainSettings.spawnableObjects[i].colorModPercentage < 0) TerrainSettings.spawnableObjects[i].colorModPercentage = 0;
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

    public virtual void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        DrawHeader();

        DrawTabs();

        switch (TerrainSettings.optionsTabSelectedId)
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
        GUILayout.EndScrollView();
    }
    private void OnEnable()
    {
        Debug.Log(Utils.FormatLog("Willow started!", "#00FF00FF"));
        SceneView.duringSceneGui += OnSceneGUI;
        /*brushProjector = Instantiate(TerrainSettings.BrushProjector);
        brushProjector.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
        brushProjector.orthographic = true;
        brushProjector.orthographicSize = TerrainSettings.brushSize;*/
        FileManager.Read(); //
    }
    private void OnDisable()
    {
        Debug.Log(Utils.FormatLog("Willow ended..", "#00FF00FF"));
        SceneView.duringSceneGui -= OnSceneGUI;
        //DestroyImmediate(brushProjector);
        FileManager.Write();
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        //BrushVis();
        SceneGUI();
    }

}