using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using static Utils;
[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditor : Editor
{
    TerrainSettings terrain;
    public List<SpawnableObject> objs = new List<SpawnableObject>();
    //public readonly List<GameObject> objs;
    void Update()
    {
       if (terrain.active) Selection.activeTransform = terrain.transform;
    }
    private void Awake()
    {
        terrain = (TerrainSettings)target;
        EditorApplication.update += Update;
    }
    private void OnDestroy()
    {
        EditorApplication.update -= Update;
    }
    void DestroyObject()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer != terrain.terrainLayer)
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
            if (Physics.Raycast(ray, out hit) &&
                ((terrain.place == SpawnPlaceType.onTerrainOnly && hit.collider.gameObject.layer == terrain.terrainLayer) ||
                (terrain.place == SpawnPlaceType.onObjectsOnly && hit.collider.gameObject.layer != terrain.terrainLayer) ||
                (terrain.place == SpawnPlaceType.onTerrainAndObjects)))
            {
                SpawnableObject spawnableObject = GetObject();
                GameObject temp = Instantiate(spawnableObject.spawnableObject, hit.point, Quaternion.identity, terrain.parent);
                SetObjectRotation(temp, hit.normal);
                if (terrain.centerObject) temp.transform.localPosition += new Vector3(0, spawnableObject.spawnableObject.transform.localScale.y / 2, 0);
                //Debug.Log(temp);
                //objs.Add(temp);
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
        int[] chances = new int[terrain.amount];
        for (int i = 0; i < chances.Length; i++)
        {
            chances[i] = objs[i].spawnChance;
        }
        return objs[GetChance(chances)];
    }
    void SetObjectRotation(GameObject obj, Vector3 normal)
    {
        obj.transform.rotation = terrain.placeAsNormals ? Quaternion.FromToRotation(Vector3.up, normal) : Quaternion.identity;
    }
    //int a = 0;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        if (GUILayout.Button("Add"))
        {
            terrain.amount++;
            objs.Add(new SpawnableObject());
        }
        /*if (GUILayout.Button("Remove"))
        {
            terrain.amount--;
            objs = new SpawnableObject[terrain.amount];
        }*/
        for (int i = 0; i < terrain.amount; i++)
        {
            EditorGUILayout.BeginVertical("box");
            //int o = 5;
            objs[i].spawnableObject = (GameObject)EditorGUILayout.ObjectField("GameObject", objs[i].spawnableObject, typeof(GameObject), false);
            objs[i].spawnChance = EditorGUILayout.IntField("Chance", objs[i].spawnChance); //objs[i].spawnChance
            objs[i].rotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", objs[i].rotationType);
            if (objs[i].rotationType == RotationType.Random)
                objs[i].rotationAxis = (RotationAxis)EditorGUILayout.EnumPopup("Axis", objs[i].rotationAxis);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space();
    }
}
