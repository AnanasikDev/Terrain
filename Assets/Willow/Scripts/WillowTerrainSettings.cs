﻿using UnityEngine;
using System;
using System.Collections.Generic;
using static WillowUtils;
public class WillowTerrainSettings : MonoBehaviour
{
    static public bool validated = false;
    static public WillowTerrainSettings instance;

    static public int density = 4;
    static public float brushSize = 25;

    static public bool indexObjects = true;

    static public BrushShape brushShape = BrushShape.Circle;
    static public bool fillBrush = true;

    static public long spawnedIndecies = 0;
    static public string indexFormat = " ({0} clone)";

    static public bool debugMode = true;
    static public bool autoSave = true;

    static public bool ignoreInactiveLayers = false;

    static public SpawnPlaceType placementType;

    static public bool active = false;
    static public Transform parent;

    static public bool erase = false;
    static public int eraseSmoothness = 0;

    static public List<SpawnableObject> spawnableObjects = new List<SpawnableObject>();
    static public List<GameObject> spawnedObjects = new List<GameObject>();
    static public List<GameObject> destroyedObjects = new List<GameObject>();

    static public List<string> layersName = new List<string>() { "default" };
    static public List<bool> layersState = new List<bool>() { true };

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

    static public float RecalculatingLength = 50;
}