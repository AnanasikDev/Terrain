using UnityEngine;
using static Utils;
public class TerrainSettings : MonoBehaviour
{

    public int terrainLayer = 30;

    public bool placeAsNormals = false;
    public bool centerObject = true;

    public int objectsAmount = 1; // if -1 then random
    public float radius = 25;

    public SpawnPlaceType place;

    public bool active = false;
    public Transform parent;

    public int amount = 2;

}
