using UnityEngine;
using System.Collections.Generic;
using static Utils;
public class TerrainSettings : MonoBehaviour
{
    static public bool validated = false;
    static public TerrainSettings instance;

    static public int density = 4;
    static public float brushSize = 25;
    static public bool indexObjects = true;

    static public long spawnedIndecies = 0;
    static public string indexFormat = " ({0} clone)";

    static public bool debugMode = true;

    static public SpawnPlaceType placementType;

    static public bool active = false;
    static public Transform parent;

    static public bool erase = false;
    static public int eraseSmoothness = 0;

    static public List<SpawnableObject> spawnableObjects = new List<SpawnableObject>();
    static public List<GameObject> spawnedObjects = new List<GameObject>();
    //static public List<Layer> layers = new List<Layer>() { new Layer("default", true) };
    static public List<string> layers = new List<string>() { "default" };
    static public List<bool> layerActive = new List<bool>() { true };
    static public List<GameObject> destroyedObjects = new List<GameObject>();

    static public string layerSelected;

    static public int exchangeSmoothness = 0;
    static public bool exchangeRotation = true;
    static public bool exchangePosition = false;
    static public bool exchangeScale = true;
    static public bool exchangeParent = true;
    static public bool exchangeColor = false;

    // Tabs names
    static public string[] optionsTabs = new string[3] { "settings", "layers", "objects" };
    static public int optionsTabSelectedId = 0;
    static public string[] brushTabs = new string[3] { "Place", "Erase", "Exchange" };
    static public int brushTabSelectedId = 0;

    // Changelog for undo implementation
    static public Stack<Change> changelog = new Stack<Change>();
}
