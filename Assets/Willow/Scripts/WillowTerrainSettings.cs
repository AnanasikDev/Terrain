using UnityEngine;
using System;
using System.Collections.Generic;
using static WillowUtils;
public class WillowTerrainSettings : MonoBehaviour
{
    static public bool Validated = false;
    //static public WillowTerrainSettings TerrainInstance;

    static public int BrushDensity = 4;
    static public float BrushSize = 25;

    static public bool IndexObjects = true;

    static public BrushShape BrushShape = BrushShape.Circle;
    static public bool FillBrush = true;

    static public long SpawnedIndecies = 0;
    static public string IndexFormat = " ({0} clone)";

    static public bool DebugMode = true;
    static public bool AutoSave = true;

    static public bool IgnoreInactiveLayers = false;

    static public SpawnPlaceType PlacementType;

    static public bool IsActive = false;
    static public Transform BaseParent;

    static public bool Erase = false;
    static public int EraseSmoothness = 0;

    static public List<WillowSpawnableObject> SpawnableObjects = new List<WillowSpawnableObject>();
    static public List<GameObject> SpawnedObjects = new List<GameObject>();
    static public List<GameObject> DestroyedObjects = new List<GameObject>();

    static public List<string> LayersName = new List<string>() { "default" };
    static public List<bool> LayersState = new List<bool>() { true };

    static public int ExchangeSmoothness = 0;
    static public bool ExchangeRotation = true;
    static public bool ExchangePosition = false;
    static public bool ExchangeScale = true;
    static public bool ExchangeParent = true;
    static public bool ExchangeColor = false;

    // Tabs names
    static public string[] OptionsTabs = new string[3] { "Settings", "Layers", "Objects" };
    static public int OptionsTabSelectedId = 0;
    static public string[] BrushTabs = new string[3] { "Place", "Erase", "Exchange" };
    static public BrushMode BrushMode = BrushMode.Place;

    // Changelog for undo implementation
    static public Stack<Change> ChangeLog = new Stack<Change>();

    static public float RecalculatingLength = 50;

    static public string PrefabsPath = "Willow/Examples/Prefabs/";
}
