using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static Utils;

[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditor : Editor
{
    TerrainSettings terrain;
    public List<SpawnableObject> objs;
    string newLayerName = "def";
    string[] optionsTabs = new string[3] { "settings", "layers", "objects" };
    int optionsTabSelectedId = 0;
    string[] brushTabs = new string[4] { "Place", "Erase", "Exchange", "Move" };
    int brushTabSelectedId = 0;

    private void Awake()
    {
        terrain = (TerrainSettings)target;
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
        Physics.Raycast(ray, out screenHit);
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

                temp.transform.localRotation = GetObjectRotation(spawnableObject, hit.normal, spawnableObject.customEulersRotation);
                SetObjectColor(spawnableObject, temp);
                temp.transform.localPosition += GetObjectPositionAdd(spawnableObject);
                temp.transform.parent = GetObjectParent(spawnableObject);

                temp.GetComponent<SpawnedObject>().positionAdd = spawnableObject.modifyPosition ? spawnableObject.positionAddition : Vector3.zero;

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
        GameObject[] objsToClone =
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= terrain.brushSize * terrain.brushSize))
            .ToArray();

        foreach (GameObject o in objsToClone)
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
                    Quaternion rotation = terrain.exchangeRotation ? o.transform.rotation : GetObjectRotation(spawnableObject, normal, spawnableObject.customEulersRotation);
                    Transform parent = terrain.exchangeParent ? o.transform.parent : spawnableObject.parent;
                    Instantiate(spawnableObject.spawnableObject, position, rotation, parent);
                    
                    spawnedObjectsTemp.Remove(o);
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
    Quaternion GetObjectRotation(SpawnableObject obj, Vector3 normal, Vector3 custom)
    {
        if (obj.rotationType == RotationType.Static)
        {
            return Quaternion.Euler(custom);
        }
        else if (obj.rotationType == RotationType.Random)
        {
            float x = 0;
            float y = 0;
            float z = 0;
            switch (obj.rotationAxis)
            {
                case RotationAxis.X:
                    x = UnityEngine.Random.value;
                    break;
                case RotationAxis.Y:
                    y = UnityEngine.Random.value;
                    break;
                case RotationAxis.Z:
                    z = UnityEngine.Random.value;
                    break;
                case RotationAxis.XY:
                    x = UnityEngine.Random.value;
                    y = UnityEngine.Random.value;
                    break;
                case RotationAxis.XZ:
                    x = UnityEngine.Random.value;
                    z = UnityEngine.Random.value;
                    break;
                case RotationAxis.YZ:
                    y = UnityEngine.Random.value;
                    z = UnityEngine.Random.value;
                    break;
                case RotationAxis.XYZ:
                    x = UnityEngine.Random.value;
                    y = UnityEngine.Random.value;
                    z = UnityEngine.Random.value;
                    break;
            }
            return new Quaternion(x, y, z, obj.spawnableObject.transform.rotation.w);
        }
        else if (obj.rotationType == RotationType.AsNormal)
        {
            Debug.Log(normal);
            return Quaternion.FromToRotation(Vector3.up, normal);
        }
        else // if AsPrefab
        {
            return obj.spawnableObject.transform.rotation;
        }
    }
    void SetObjectColor(SpawnableObject obj, GameObject gameObject)
    {
        Renderer renderer = gameObject.GetComponent<SpawnedObject>().renderer;
        var tempMaterial = new Material(renderer.sharedMaterial);
        if (obj.modColor)
        {
            tempMaterial.color *= UnityEngine.Random.Range(1 - (obj.colorModPercentage / 100), 1 + (obj.colorModPercentage / 100));
        }
        renderer.sharedMaterial = tempMaterial;
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

    void OnSceneGUI()
    {
        if (!terrain.active) return;
        if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control) ||
            (Event.current.type == EventType.MouseDown && Event.current.button == 0 && brushTabSelectedId == 1)) // Destroying objects
        {
            EraseObjects();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && brushTabSelectedId == 0) // Placing objects
        {
            PlaceObjects();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && brushTabSelectedId == 2) // Exchanging objects
        {
            ExchangeObjects();
        }
    }

    new void DrawHeader()
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
        brushTabSelectedId = GUILayout.Toolbar(brushTabSelectedId, brushTabs);
        if (brushTabSelectedId == 1) // Erasing
        {
            DrawErasingTab();
        }
        if (brushTabSelectedId == 2) // Exchanging
        {
            DrawExchangingTab();
        }
    }
    // BRUSHES TABS
    void DrawErasingTab()
    {
        EditorGUILayout.BeginVertical("box");

        terrain.eraseSmoothness = EditorGUILayout.IntField("  Erase smoothness", terrain.eraseSmoothness);

        EditorGUILayout.EndVertical();
    }
    void DrawExchangingTab()
    {
        EditorGUILayout.BeginVertical("box");

        terrain.exchangeSmoothness = EditorGUILayout.IntField("  Exchange smoothness", terrain.exchangeSmoothness);

        terrain.exchangePosition = EditorGUILayout.Toggle("Exchange position", terrain.exchangePosition);
        terrain.exchangeRotation = EditorGUILayout.Toggle("Exchange rotation", terrain.exchangeRotation);
        terrain.exchangeScale = EditorGUILayout.Toggle("Exchange scale", terrain.exchangeScale);
        terrain.exchangeParent = EditorGUILayout.Toggle("Exchange parent", terrain.exchangeParent);

        EditorGUILayout.EndVertical();
    }
    // OPTIONS TABS
    void DrawTabs()
    {
        optionsTabSelectedId = GUILayout.Toolbar(optionsTabSelectedId, optionsTabs);
    }
    void DrawSettingsTab()
    {
        base.OnInspectorGUI();
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
                    GUILayout.Label(objs[i].spawnableObject != null ? objs[i].spawnableObject.name : "null");
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


            objs[i].rotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", objs[i].rotationType);
            if (objs[i].rotationType == RotationType.Random)
                objs[i].rotationAxis = (RotationAxis)EditorGUILayout.EnumPopup("  Axis", objs[i].rotationAxis);

            if (objs[i].rotationType == RotationType.Static)
                objs[i].customEulersRotation = EditorGUILayout.Vector3Field("  Custom Euler Rotation", objs[i].customEulersRotation);

            objs[i].centerObject = EditorGUILayout.Toggle("Center Object", objs[i].centerObject);

            objs[i].modColor = EditorGUILayout.Toggle("Modify color", objs[i].modColor);
            if (objs[i].modColor)
            {
                objs[i].colorModPercentage = EditorGUILayout.FloatField("  Color modification %", objs[i].colorModPercentage);
                //objs[i].renderableObject = (Renderer))EditorGUILayout.ObjectField("Renderer", objs[i].renderableObject, typeof(Renderer)), true))  ;
            }

            objs[i].modifyPosition = EditorGUILayout.Toggle("Modify position", objs[i].modifyPosition);
            if (objs[i].modifyPosition)
                objs[i].positionAddition = EditorGUILayout.Vector3Field("  Position addition", objs[i].positionAddition);

            objs[i].customParent = EditorGUILayout.Toggle("Custom parent", objs[i].customParent);
            if (objs[i].customParent)
                objs[i].parent = (Transform)EditorGUILayout.ObjectField("  Parent", objs[i].parent, typeof(Transform), true);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
    
    }
    public override void OnInspectorGUI()
    {
        DrawHeader();

        DrawTabs();

        switch (optionsTabSelectedId)
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
}