using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using static Utils;
[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditor : Editor
{
    TerrainSettings terrain;
    public List<SpawnableObject> objs;
    //public readonly List<GameObject> objs;
    void Update()
    {
       if (terrain.active) Selection.activeTransform = terrain.transform;
    }
    private void Awake()
    {
        terrain = (TerrainSettings)target;
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
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.GetComponent<TerrainSettings>() == null)
        {
            DestroyImmediate(hit.collider.gameObject);
        }
    }
    void SpawnObject()
    {
        for (int i = 0; i < terrain.objectsAmount; i++)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition +
                (terrain.objectsAmount == 1 ? Vector2.zero :
                new Vector2(UnityEngine.Random.Range(-terrain.radius, terrain.radius),
                            UnityEngine.Random.Range(-terrain.radius, terrain.radius))));
            RaycastHit hit = new RaycastHit();
            /* &&
                ((terrain.place == SpawnPlaceType.onTerrainOnly && hit.collider.gameObject.GetComponent<TerrainSettings>() == null) ||
                (terrain.place == SpawnPlaceType.onObjectsOnly && hit.collider.gameObject.GetComponent<TerrainSettings>() != null) ||
                (terrain.place == SpawnPlaceType.onTerrainAndObjects))*/
            if (Physics.Raycast(ray, out hit))
            {
                SpawnableObject spawnableObject = GetObject();
                GameObject temp = Instantiate(spawnableObject.spawnableObject, hit.point, Quaternion.identity, terrain.parent);
                SetObjectRotation(spawnableObject, hit.normal, spawnableObject.customEulersRotation);
                SetObjectColor(spawnableObject);
                if (spawnableObject.centerObject) temp.transform.localPosition += new Vector3(0, spawnableObject.spawnableObject.transform.localScale.y / 2, 0);
            }
        }
    }
    private void OnSceneGUI()
    {
        if (!terrain.active) return;
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control)
        {
            DestroyObject();
        }
        else if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            SpawnObject();
        }
    }
    SpawnableObject GetObject()
    {
        int[] chances = new int[objs.Count];
        for (int i = 0; i < chances.Length; i++)
        {
            chances[i] = objs[i].spawnChance;
        }
        return objs[GetChance(chances)];
    }
    void SetObjectRotation(SpawnableObject obj, Vector3 normal, Vector3 custom)
    {
        if (obj.rotationType == RotationType.Static)
        {
            obj.spawnableObject.transform.eulerAngles = custom;
        }
        if (obj.rotationType == RotationType.Random)
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
            obj.spawnableObject.transform.eulerAngles = new Vector3(x, y, z);
        }
        if (obj.rotationType == RotationType.AsPrefab)
        {
            // default
        }
        if (obj.rotationType == RotationType.AsNormal)
        {
            obj.spawnableObject.transform.eulerAngles = normal;
        }
    }
    void SetObjectColor(SpawnableObject obj)
    {
        if (obj.modColor)
        {
            obj.renderableObject.sharedMaterial.color *= UnityEngine.Random.Range(1 - obj.colorModPercentage, 1 + obj.colorModPercentage);
        }
    }
    public override void OnInspectorGUI()
    {
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
                    //GUILayout.Space(20);
                    GUILayout.Label(objs[i].spawnableObject != null ? objs[i].spawnableObject.name : "null");
                    EditorGUILayout.EndHorizontal();
                    //EditorGUILayout.FlexibleSpace();
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
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box");
            objs[i].spawnableObject = (GameObject)EditorGUILayout.ObjectField("GameObject", objs[i].spawnableObject, typeof(GameObject), false);
            objs[i].spawnChance = EditorGUILayout.IntField("Chance", objs[i].spawnChance); //objs[i].spawnChance

            objs[i].rotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", objs[i].rotationType);
            if (objs[i].rotationType == RotationType.Random)
                objs[i].rotationAxis = (RotationAxis)EditorGUILayout.EnumPopup("Axis", objs[i].rotationAxis);

            if (objs[i].rotationType == RotationType.Static)
                objs[i].customEulersRotation = EditorGUILayout.Vector3Field("Custom Euler Rotation", objs[i].customEulersRotation);

            objs[i].centerObject = EditorGUILayout.Toggle("Center Object", objs[i].centerObject);

            objs[i].modColor = EditorGUILayout.Toggle("Modify color", objs[i].modColor);
            if (objs[i].modColor)
            {
                objs[i].colorModPercentage = EditorGUILayout.FloatField("Color modification %", objs[i].colorModPercentage);
                objs[i].renderableObject = (Renderer)EditorGUILayout.ObjectField("Renderer", objs[i].renderableObject, typeof(Renderer), false);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
    }
}
