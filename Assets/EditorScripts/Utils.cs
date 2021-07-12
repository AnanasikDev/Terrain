using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public enum SpawnPlaceType
    {
        onTerrainOnly,
        onObjectsOnly,
        onTerrainAndObjects
    }
    public enum RotationType
    {
        Random,
        Static,
        AsPrefab,
        AsNormal,
        RandomAsNormal,
        StaticAsNormal,
        LerpedRandomAsNormal,
        LerpedStaticAsNormal,
        LerpedAsPrefabAsNormal
    }
    public enum ScaleType
    {
        Random,
        Static,
        AsPrefab
    }
    public enum Axis
    {
        X,
        Y,
        Z,
        XY,
        XZ,
        YZ,
        XYZ
    }
    public enum ChangeType
    {
        Placement,
        Erasure,
        Exchange
    }
    public static int GetChance(int[] chances)
    {
        int n = chances.Length;
        List<int> res = new List<int>(n);
        for (int i = 0; i < n; i++)
        {
            for (int _ = 0; _ < chances[i]; _++) res.Add(i);
        }
        return res[Random.Range(0, res.Count)];
    }

    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    public static string FormatLog(string message, string color = "#FFFF00FF")
    {
        return LogPrefix() + ": " + $"<b><color={color}>{message}</color></b>";
    }
    static string LogPrefix()
    {
        string c0 = "#00FF00FF";
        string c1 = "#00FE00FF";
        string c2 = "#00EE00FF";
        string c3 = "#00DE00FF";
        string c4 = "#00CE00FF";
        string c5 = "#00BE00FF";
        string c6 = "#00AE00FF";
        string c7 = "#00AA00FF";
        return $"<color={c0}>[</color><color={c1}>W</color><color={c2}>i</color><color={c3}>l</color><color={c4}>l</color><color={c5}>o</color><color={c6}>w</color><color={c7}>]</color>";
    }
}
[System.Serializable]
public class SpawnableObject
{
    public bool spawn = true;
    public GameObject spawnableObject;
    [SerializeField, Min(0)] public int spawnChance = 1;

    public Utils.RotationType rotationType = Utils.RotationType.AsPrefab;
    public Utils.Axis rotationAxis = Utils.Axis.Y;
    public Vector3 customEulersRotation = Vector3.zero;
    public float lerpValue = 0.15f;
    public float minLerpValue = 0.1f;
    public float maxLerpValue = 0.2f;
    public bool randomizeLerpValue = false;
    public Vector3 randomMinRotation = Vector3.zero;
    public Vector3 randomMaxRotation = Vector3.one * 360;
    public bool multiRotationAxis = false;

    public bool modColor = true;
    public float colorModPercentage = 35;

    public bool customParent = false;
    public Transform parent;

    public bool centerObject = false;
    public bool modifyPosition = false;
    public Vector3 positionAddition = Vector3.zero;

    public string layer;
    public int layerIndex = 0;

    public bool renameObject = false;
    public string newObjectName = "Object";

    public Utils.ScaleType scaleType = Utils.ScaleType.AsPrefab;
    public Utils.Axis scaleAxis = Utils.Axis.XYZ;
    public bool modScale;
    public Vector3 customScale = Vector3.one;
    public Vector3 scaleMinSeparated = new Vector3(0.9f, 0.9f, 0.9f);
    public float scaleMin = 1;
    public Vector3 scaleMaxSeparated = new Vector3(1.1f, 1.1f, 1.1f);
    public float scaleMax = 1;
    public bool separateScaleAxis = true;

    public bool hidden = false;

    public SpawnableObject()
    {

    }
    public SpawnableObject (SpawnableObject clone)
    {
        spawn = clone.spawn;
        centerObject = clone.centerObject;
        spawnableObject = clone.spawnableObject;
        spawnChance = clone.spawnChance;
        layer = clone.layer;
        layerIndex = clone.layerIndex;
        renameObject = clone.renameObject;
        newObjectName = clone.newObjectName;

        rotationType = clone.rotationType;
        rotationAxis = clone.rotationAxis;
        customEulersRotation = clone.customEulersRotation;
        lerpValue = clone.lerpValue;
        minLerpValue = clone.minLerpValue;
        maxLerpValue = clone.maxLerpValue;
        randomizeLerpValue = clone.randomizeLerpValue;
        randomMinRotation = clone.randomMinRotation;
        randomMaxRotation = clone.randomMaxRotation;
        multiRotationAxis = clone.multiRotationAxis;

        modColor = clone.modColor;
        colorModPercentage = clone.colorModPercentage;

        customParent = clone.customParent;
        parent = clone.parent;

        modifyPosition = clone.modifyPosition;
        positionAddition = clone.positionAddition;

        scaleType = clone.scaleType;
        modScale = clone.modScale;
        separateScaleAxis = clone.separateScaleAxis;
        scaleMin = clone.scaleMin;
        scaleMax = clone.scaleMax;
        scaleMinSeparated = clone.scaleMinSeparated;
        scaleMaxSeparated = clone.scaleMaxSeparated;
    }
}
[System.Serializable]
public struct Change
{
    public Utils.ChangeType type;
    public List<GameObject> spawnedObjects;
    public List<GameObject> destroyedObjects;
    public Change (Utils.ChangeType Type, List<GameObject> SpawnedObjects, List<GameObject> DestroyedObjects)
    {
        type = Type;
        spawnedObjects = SpawnedObjects;
        destroyedObjects = DestroyedObjects;
    }
}