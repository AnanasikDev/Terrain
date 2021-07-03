using UnityEngine;
using System.Collections.Generic;
using static Utils;
public class TerrainSettings : MonoBehaviour
{
    public int density = 1; // if -1 then random
    public float brushSize = 25;

    public SpawnPlaceType placementType;

    [HideInInspector] public bool active = false;
    public Transform parent;

    [HideInInspector] public bool erase = false;
    [HideInInspector] public int eraseSmoothness = 0;

    [HideInInspector] public List<SpawnableObject> objs = new List<SpawnableObject>();
    [HideInInspector] public List<GameObject> spawnedObjects = new List<GameObject>();
    [HideInInspector] public List<string> layers = new List<string>() { "default" };

    [HideInInspector] public string layerSelected;

    public static TerrainSettings terrainSettings;

    [HideInInspector] public int exchangeSmoothness = 0;
    [HideInInspector] public bool exchangeRotation = true;
    [HideInInspector] public bool exchangePosition = false;
    [HideInInspector] public bool exchangeScale = true;
    [HideInInspector] public bool exchangeParent = true;
}
