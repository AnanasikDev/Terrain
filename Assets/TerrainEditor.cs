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
    
    void Update()
    {
       if (terrain.active) Selection.activeTransform = terrain.transform;
    }
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
    void DestroyObject()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(ray, out hit);

        List<GameObject> spawnedObjectsTemp = terrain.spawnedObjects;
        GameObject[] objsToDestroy = spawnedObjectsTemp.Where(gameObject => ((hit.point - gameObject.transform.position).sqrMagnitude <= terrain.radius * terrain.radius)).ToArray();

        foreach (GameObject o in objsToDestroy)
        {
            if (Random.Range(0, terrain.eraseSmoothness+1) == terrain.eraseSmoothness || terrain.eraseSmoothness == 0)
            {
                spawnedObjectsTemp.Remove(o);
                DestroyImmediate(o);
            }
        }
        terrain.spawnedObjects = spawnedObjectsTemp;
    }
    void SpawnObject()
    {
        RaycastHit screenHit = new RaycastHit();
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Physics.Raycast(ray, out screenHit);
        for (int i = 0; i < terrain.objectsAmount; i++)
        {
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(screenHit.point + Vector3.up*5, 
                new Vector3(UnityEngine.Random.Range(-0.085f * terrain.radius, 0.085f * terrain.radius), -1,
                            UnityEngine.Random.Range(-0.085f * terrain.radius, 0.085f * terrain.radius)), out hit) &&

                ((terrain.place == SpawnPlaceType.onTerrainOnly && hit.collider.gameObject.GetComponent<TerrainSettings>()) != null ||
                (terrain.place == SpawnPlaceType.onObjectsOnly && hit.collider.gameObject.GetComponent<TerrainSettings>()) == null ||
                (terrain.place == SpawnPlaceType.onTerrainAndObjects)) )
            {
                SpawnableObject spawnableObject = GetObject();
                if (spawnableObject == null) continue;
                GameObject temp = Instantiate(spawnableObject.spawnableObject, hit.point, Quaternion.identity);
                temp.transform.rotation = GetObjectRotation(spawnableObject, hit.normal, spawnableObject.customEulersRotation);
                SetObjectColor(spawnableObject, temp);
                temp.transform.parent = GetObjectParent(spawnableObject);
                if (spawnableObject.centerObject)
                    temp.transform.localPosition += new Vector3(0, spawnableObject.spawnableObject.transform.localScale.y / 2, 0);
                terrain.spawnedObjects.Add(temp);
            }
        }
    }
    void OnSceneGUI()
    {
        if (!terrain.active) return;
        if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control) ||
            (Event.current.type == EventType.MouseDown && Event.current.button == 0 && terrain.erase))
        {
            DestroyObject();
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 0) && !terrain.erase)
        {
            SpawnObject();
        }
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
            return Quaternion.Euler(normal);
        }
        else // if AsPrefab
        {
            return obj.spawnableObject.transform.rotation;
        }
    }
    void SetObjectColor(SpawnableObject obj, GameObject gameObject)
    {
        Renderer renderer = gameObject.GetComponent<SpawnableObj>().renderer;
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
    public override void OnInspectorGUI()
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

        EditorGUILayout.BeginHorizontal("box");
        if (terrain.erase)
        {
            if (GUILayout.Button("Spawn"))
            {
                terrain.erase = false;
            }
            GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f, 1);
            GUI.enabled = false;
            GUILayout.Button("Erase");
            GUI.enabled = true;
            GUI.backgroundColor = oldBackgroundColor;
        }
        else
        {
            GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f, 1);
            GUI.enabled = false;
            GUILayout.Button("Spawn");
            GUI.enabled = true;
            GUI.backgroundColor = oldBackgroundColor;
            if (GUILayout.Button("Erase"))
            {
                terrain.erase = true;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (terrain.erase)
        {
            EditorGUILayout.BeginVertical("box");
            terrain.eraseSmoothness = EditorGUILayout.IntField("  Erase smoothness", terrain.eraseSmoothness);
            EditorGUILayout.EndVertical();
        }


        EditorGUILayout.Space(20);

        base.OnInspectorGUI();

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
            if (i < objs.Count-1)
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
            objs[i].customParent = EditorGUILayout.Toggle("Custom parent", objs[i].customParent);
            if (objs[i].customParent)
                objs[i].parent = (Transform)EditorGUILayout.ObjectField("  Parent", objs[i].parent, typeof(Transform), true);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
    }
}
