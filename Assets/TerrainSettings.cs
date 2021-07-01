using UnityEngine;
using System.Collections.Generic;
using static Utils;
public class TerrainSettings : MonoBehaviour
{
    public int objectsAmount = 1; // if -1 then random
    public float radius = 25;

    public SpawnPlaceType place;

    public bool active = false;
    public Transform parent;

    [HideInInspector] public List<SpawnableObject> objs = new List<SpawnableObject>();
}
