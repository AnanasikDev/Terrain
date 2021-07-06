using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static Utils;

//[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditor : EditorWindow
{
    public TerrainSettings terrain;
    public List<SpawnableObject> objs;
    string newLayerName = "def";
    Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/Terrain")]
    public static void ShowWindow()
    {
        GetWindow<TerrainEditor>("Terrain++");
    }
    private void Awake()
    {
        //terrain = (TerrainSettings)target;
        terrain = TerrainEditorInit.terrain;
        TerrainSettings.terrainSettings = terrain;
        EditorApplication.update += Update;
        objs = terrain.objs;
    }
    private void OnDestroy()
    {
        EditorApplication.update -= Update;
    }

    void Update()
    {
        if (terrain.active) 
            Selection.activeTransform = terrain.transform;
    }

    void EraseObjects()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(ray, out hit);

        List<GameObject> spawnedObjectsTemp = terrain.spawnedObjects;
        GameObject[] objsToDestroy = 
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= terrain.brushSize * terrain.brushSize))
            .ToArray();

        foreach (GameObject o in objsToDestroy)
        {
            if (Random.Range(0, terrain.eraseSmoothness + 1) == terrain.eraseSmoothness || terrain.eraseSmoothness == 0)
            {
                if (o != null)
                {
                    spawnedObjectsTemp.Remove(o);
                    DestroyImmediate(o);
                }
            }
        }
        spawnedObjectsTemp.RemoveAll(o => o == null);
        terrain.spawnedObjects = spawnedObjectsTemp;
    }
    void PlaceObjects()
    {
        RaycastHit screenHit;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        bool ableToSpawn = Physics.Raycast(ray, out screenHit);
        if (ableToSpawn)
        for (int i = 0; i < terrain.density; i++)
        {
            RaycastHit hit;

            if (Physics.Raycast(screenHit.point + Vector3.up*5, 
                new Vector3(UnityEngine.Random.Range(-0.085f * terrain.brushSize, 0.085f * terrain.brushSize), -1,
                            UnityEngine.Random.Range(-0.085f * terrain.brushSize, 0.085f * terrain.brushSize)), out hit) &&

                ((terrain.placementType == SpawnPlaceType.onTerrainOnly && hit.collider.gameObject.GetComponent<TerrainSettings>() != null) ||
                (terrain.placementType == SpawnPlaceType.onObjectsOnly && hit.collider.gameObject.GetComponent<TerrainSettings>() == null) ||
                (terrain.placementType == SpawnPlaceType.onTerrainAndObjects)) )
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

                if (spawnableObject.renameObject)
                    temp.name = spawnableObject.newObjectName;

                if (spawnableObject.centerObject)
                    temp.transform.localPosition += new Vector3(0, spawnableObject.spawnableObject.transform.localScale.y / 2, 0);
                terrain.spawnedObjects.Add(temp);
            }
        }
    }
    void ExchangeObjects()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        List<GameObject> spawnedObjectsTemp = terrain.spawnedObjects;
        GameObject[] objsToExchange =
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= terrain.brushSize * terrain.brushSize))
            .ToArray();

        foreach (GameObject o in objsToExchange)
        {
            if (Random.Range(0, terrain.exchangeSmoothness + 1) == terrain.exchangeSmoothness || terrain.exchangeSmoothness == 0)
            {
                if (o != null)
                {
                    SpawnableObject spawnableObject = GetObject();
                    Vector3 position = terrain.exchangePosition ? o.transform.position : o.transform.position - o.GetComponent<SpawnedObject>().positionAdd + GetObjectPositionAdd(spawnableObject);
                    RaycastHit normalHit;
                    bool normalCasted = Physics.Raycast(position, -o.transform.up, out normalHit, 3);
                    Vector3 normal = normalCasted ? normalHit.normal : Vector3.up;
                    Transform parent = terrain.exchangeParent ? o.transform.parent : spawnableObject.parent;
                    GameObject spawned = Instantiate(spawnableObject.spawnableObject, position, Quaternion.identity, parent);
                    if (terrain.exchangeRotation)
                        spawned.transform.localRotation = o.transform.localRotation;
                    else
                        SetObjectRotation(spawnableObject, spawned, normal, spawnableObject.customEulersRotation);
                    if (terrain.exchangeColor)
                        SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, spawned, o.GetComponent<SpawnedObject>().renderer.sharedMaterial.color);
                    else SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, spawned);
                    
                    spawned.GetComponent<SpawnedObject>().positionAdd =
                        spawnableObject.modifyPosition ? spawnableObject.positionAddition : Vector3.zero;

                    spawned.transform.localScale = terrain.exchangeScale ? o.transform.localScale : GetObjectScale(spawnableObject);

                    spawnedObjectsTemp.Remove(o);
                    spawnedObjectsTemp.Add(spawned);
                    DestroyImmediate(o);
                }
            }
        }
        spawnedObjectsTemp.RemoveAll(o => o == null);
        terrain.spawnedObjects = spawnedObjectsTemp;
    }

    SpawnableObject GetObject()
    {
        int[] chances = new int[objs.Count];
        bool ableToSpawn = false;
        for (int i = 0; i < chances.Length; i++)
        {
            chances[i] = objs[i].spawn && objs[i].spawnableObject != null ? objs[i].spawnChance : 0;
            if (chances[i] > 0) ableToSpawn = true;
        }
        if (!ableToSpawn) return null;
        return new SpawnableObject( objs[GetChance(chances)] );
    }
    void SetObjectRotation(SpawnableObject spawnableObject, GameObject spawnedObject, Vector3 normal, Vector3 custom)
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
            //spawnedObject.transform.eulerAngles = Vector3.Lerp(custom.Abs(), Quaternion.FromToRotation(Vector3.up, normal).eulerAngles.Abs(), spawnableObject.lerpValue);
            spawnedObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(custom), Quaternion.FromToRotation(Vector3.up, normal), spawnableObject.lerpValue);
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
    void SetObjectColor(bool modifyColor, float colorModificationPercentage, GameObject gameObject, Color? color = null)
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
    Transform GetObjectParent(SpawnableObject obj)
    {
        if (obj.customParent) return obj.parent;
        else return terrain.parent;
    }
    Vector3 GetObjectPositionAdd(SpawnableObject obj)
    {
        return obj.modifyPosition ? obj.positionAddition : Vector3.zero;
    }
    Vector3 GetObjectScale(SpawnableObject spawnableObject)
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
    void onSceneGUI()
    {
        if (!terrain.active) return;
        if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control) ||
            (Event.current.type == EventType.MouseDown && Event.current.button == 0 && terrain.brushTabSelectedId == 1)) // Destroying objects
        {
            EraseObjects();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && terrain.brushTabSelectedId == 0) // Placing objects
        {
            PlaceObjects();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && terrain.brushTabSelectedId == 2) // Exchanging objects
        {
            ExchangeObjects();
        }
    }
    void OnFocus()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView) => onSceneGUI();

    void DrawHeader()
    {
        Color oldBackgroundColor = GUI.backgroundColor;
        Color oldContentColor = GUI.contentColor;
        if (terrain.active)
        {
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f, 1);
            GUI.contentColor = new Color(0.9f, 0.6f, 0.6f, 1);
            if (GUILayout.Button("Disable"))
            {
                terrain.active = false;
            }
        }
        if (!terrain.active)
        {
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f, 1);
            GUI.contentColor = new Color(0.6f, 0.9f, 0.6f, 1);
            if (GUILayout.Button("Enable"))
            {
                terrain.active = true;
            }
        }

        GUI.contentColor = oldContentColor;
        GUI.backgroundColor = oldBackgroundColor;

        DrawBrushTabs();

        if (terrain.erase)
        {
            EditorGUILayout.BeginVertical("box");
            terrain.eraseSmoothness = EditorGUILayout.IntField("  Erase smoothness", terrain.eraseSmoothness);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(20);
    }
    void DrawBrushTabs()
    {
        terrain.brushTabSelectedId = GUILayout.Toolbar(terrain.brushTabSelectedId, terrain.brushTabs);
        if (terrain.brushTabSelectedId == 1) // Erasing
        {
            DrawErasingTab();
        }
        if (terrain.brushTabSelectedId == 2) // Exchanging
        {
            DrawExchangingTab();
        }
    }
    // BRUSHES TABS
    void DrawErasingTab()
    {
        EditorGUILayout.BeginVertical("box");

        terrain.eraseSmoothness = EditorGUILayout.IntField("Erase smoothness", terrain.eraseSmoothness);

        EditorGUILayout.EndVertical();
    }
    void DrawExchangingTab()
    {
        EditorGUILayout.BeginVertical("box");

        terrain.exchangeSmoothness = EditorGUILayout.IntField("Exchange smoothness", terrain.exchangeSmoothness);

        terrain.exchangePosition = EditorGUILayout.Toggle("Exchange position", terrain.exchangePosition);
        terrain.exchangeRotation = EditorGUILayout.Toggle("Exchange rotation", terrain.exchangeRotation);
        terrain.exchangeScale = EditorGUILayout.Toggle("Exchange scale", terrain.exchangeScale);
        terrain.exchangeParent = EditorGUILayout.Toggle("Exchange parent", terrain.exchangeParent);
        terrain.exchangeColor = EditorGUILayout.Toggle("Exchange color", terrain.exchangeColor);

        EditorGUILayout.EndVertical();
    }
    // OPTIONS TABS
    void DrawTabs()
    {
        terrain.optionsTabSelectedId = GUILayout.Toolbar(terrain.optionsTabSelectedId, terrain.optionsTabs);
    }
    void DrawSettingsTab()
    {
        //base.OnInspectorGUI();

        terrain.density = EditorGUILayout.IntField("Brush density", terrain.density);
        terrain.brushSize = EditorGUILayout.FloatField("Brush size", terrain.brushSize);

        terrain.parent = (Transform)EditorGUILayout.ObjectField("Parent", terrain.parent, typeof(Transform), true);
        terrain.placementType = (Utils.SpawnPlaceType)EditorGUILayout.EnumPopup("Placement type", terrain.placementType);


        // General Info

        EditorGUILayout.Space(25);

        Color color = GUI.contentColor;

        GUIStyle style = GUIStyle.none;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 16;

        EditorGUILayout.LabelField($"<b><color=#CCCCCCFF>General Info</color></b>", style);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Total spawned: " + terrain.spawnedObjects.Where(o => o != null).ToArray().Length.ToString());
        EditorGUILayout.LabelField("Layers amount: " + terrain.layers.Count.ToString());
        EditorGUILayout.LabelField("Total spawnable objects: " + objs.Count);
    }
    void DrawLayersTab()
    {
        EditorGUILayout.BeginHorizontal("box");
        bool add = false;
        if (GUILayout.Button("Add"))
            add = true;

        newLayerName = EditorGUILayout.TextField("New layer name: ", newLayerName);
        if (add && newLayerName != "")
        {
            if (!terrain.layers.Contains(newLayerName))
            terrain.layers.Add(newLayerName);
        }

        EditorGUILayout.EndHorizontal();

        DrawLayersArray();
    }
    void DrawLayersArray()
    {
        terrain.layers.RemoveAll(layerName => layerName == "");
        for (int layerId = 0; layerId < terrain.layers.Count; layerId++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("box");
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18)))
            {
                if (terrain.layers.Count > 1)
                    terrain.layers[layerId] = "";
            }
            GUI.backgroundColor = oldBgColor;
            terrain.layers[layerId] = EditorGUILayout.TextField("Name: ", terrain.layers[layerId]);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
    void DrawObjectsTab()
    {
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label(objs.Count.ToString());
        if (GUILayout.Button("Add"))
        {
            objs.Add(new SpawnableObject());
        }
        EditorGUILayout.EndHorizontal();
        

        for (int i = 0; i < objs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box");
            int removeBtnHeight = 40;
            if (i < objs.Count - 1 || i == 0) removeBtnHeight = 60;
            if (!objs[i].hidden)
            {
                if (GUILayout.Button("‹", GUILayout.Width(18), GUILayout.Height(18)))
                {

                    objs[i].hidden = true;
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
                    objs[i].hidden = false;
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label(objs[i].spawnableObject != null ? (objs[i].renameObject ? objs[i].newObjectName : objs[i].spawnableObject.name) : "null");
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

            }
            if (i > 0)
                if (GUILayout.Button("˄", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    var temp = objs[i];
                    objs[i] = objs[i - 1];
                    objs[i - 1] = temp;
                }
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(removeBtnHeight)))
            {
                objs.RemoveAt(i);
                continue;
            }
            if (i < objs.Count - 1)
                if (GUILayout.Button("˅", GUILayout.Width(18), GUILayout.Height(18)))
                {
                    var temp = objs[i];
                    objs[i] = objs[i + 1];
                    objs[i + 1] = temp;
                }
            if (GUILayout.Button("+", GUILayout.Width(18), GUILayout.Height(18)))
            {
                objs.Insert(i, new SpawnableObject(objs[i]));
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box");
            objs[i].spawn = EditorGUILayout.Toggle("Spawn", objs[i].spawn);
            if (!objs[i].spawn)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }
            objs[i].spawnableObject = (GameObject)EditorGUILayout.ObjectField("GameObject", objs[i].spawnableObject, typeof(GameObject), true);

            objs[i].renameObject = EditorGUILayout.Toggle("Rename object", objs[i].renameObject);
            if (objs[i].renameObject)
                objs[i].newObjectName = EditorGUILayout.TextField("  Name: ", objs[i].newObjectName);

            objs[i].layerIndex = EditorGUILayout.Popup("Layer: ", objs[i].layerIndex, terrain.layers.ToArray());
            if (objs[i].layerIndex >= terrain.layers.Count)
                objs[i].layer = terrain.layers[0];
            else
                objs[i].layer = terrain.layers[objs[i].layerIndex];

            objs[i].spawnChance = EditorGUILayout.IntField("Chance", objs[i].spawnChance); //objs[i].spawnChance
            if (objs[i].spawnChance < 0) objs[i].spawnChance = 0;

            objs[i].multiRotationAxis = EditorGUILayout.Toggle("Multi axis", objs[i].multiRotationAxis);
            
            objs[i].rotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", objs[i].rotationType);
            if (objs[i].rotationType == RotationType.Random ||
                objs[i].rotationType == RotationType.RandomAsNormal ||
                objs[i].rotationType == RotationType.LerpedRandomAsNormal)
            {
                if (objs[i].multiRotationAxis)
                {
                    objs[i].randomMinRotation = EditorGUILayout.Vector3Field("  Min rotation", objs[i].randomMinRotation);
                    objs[i].randomMaxRotation = EditorGUILayout.Vector3Field("  Max rotation", objs[i].randomMaxRotation);
                }
                else
                    objs[i].rotationAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", objs[i].rotationAxis);
            }

            if (objs[i].rotationType == RotationType.Static ||
                objs[i].rotationType == RotationType.StaticAsNormal ||
                objs[i].rotationType == RotationType.LerpedStaticAsNormal)
                objs[i].customEulersRotation = EditorGUILayout.Vector3Field("  Custom Euler Rotation", objs[i].customEulersRotation);

            if (objs[i].rotationType == RotationType.LerpedStaticAsNormal)
                objs[i].lerpValue = EditorGUILayout.FloatField("  Lerp value", objs[i].lerpValue);


            objs[i].modifyPosition = EditorGUILayout.Toggle("Modify position", objs[i].modifyPosition);
            if (objs[i].modifyPosition)
                objs[i].positionAddition = EditorGUILayout.Vector3Field("  Position addition", objs[i].positionAddition);


            objs[i].modScale = EditorGUILayout.Toggle("Modify scale", objs[i].modScale);
            if (objs[i].modScale)
            {
                objs[i].scaleType = (ScaleType)EditorGUILayout.EnumPopup(  "Scale", objs[i].scaleType);
                objs[i].separateScaleAxis = EditorGUILayout.Toggle("  Separate axis", objs[i].separateScaleAxis);
                if (objs[i].scaleType == ScaleType.Random)
                {
                    objs[i].scaleAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", objs[i].scaleAxis);
                    if (objs[i].separateScaleAxis)
                    {
                        objs[i].scaleMaxSeparated = EditorGUILayout.Vector3Field("  Max scale", objs[i].scaleMaxSeparated);
                        objs[i].scaleMinSeparated = EditorGUILayout.Vector3Field("  Min scale", objs[i].scaleMinSeparated);
                    }
                    else
                    {
                        objs[i].scaleMin = EditorGUILayout.FloatField("  Min scale", objs[i].scaleMin);
                        objs[i].scaleMax = EditorGUILayout.FloatField("  Max scale", objs[i].scaleMax);
                    }
                }
                if (objs[i].scaleType == ScaleType.Static)
                    objs[i].customScale = EditorGUILayout.Vector3Field("  Custom scale", objs[i].customScale);

            }


            objs[i].centerObject = EditorGUILayout.Toggle("Center Object", objs[i].centerObject);


            objs[i].modColor = EditorGUILayout.Toggle("Modify color", objs[i].modColor);
            
            if (objs[i].modColor)
            {
                objs[i].colorModPercentage = EditorGUILayout.FloatField("  Color modification %", objs[i].colorModPercentage);
                if (objs[i].colorModPercentage < 0) objs[i].colorModPercentage = 0;
            }


            objs[i].customParent = EditorGUILayout.Toggle("Custom parent", objs[i].customParent);
            if (objs[i].customParent)
                objs[i].parent = (Transform)EditorGUILayout.ObjectField("  Parent", objs[i].parent, typeof(Transform), true);


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
    
    }
    public void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        DrawHeader();

        DrawTabs();

        switch (terrain.optionsTabSelectedId)
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
}