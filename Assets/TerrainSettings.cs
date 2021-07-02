using UnityEngine;
using System.Collections.Generic;
using static Utils;
public class TerrainSettings : MonoBehaviour
{
    public int objectsAmount = 1; // if -1 then random
    public float radius = 25;

    public SpawnPlaceType place;

    [HideInInspector] public bool active = false;
    public Transform parent;

    public bool selectOnlyChildren = true;

    [HideInInspector] public bool erase = false;
    [HideInInspector] public int eraseSmoothness = 0;

    [HideInInspector] public List<SpawnableObject> objs = new List<SpawnableObject>();
    public List<GameObject> spawnedObjects = new List<GameObject>();

    public static TerrainSettings terrainSettings;
}
