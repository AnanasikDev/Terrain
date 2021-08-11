using UnityEngine;

[System.Serializable]
public sealed class SpawnableObject
{
    public bool spawn = true;
    public GameObject spawnableObject;
    [SerializeField, Min(0)] public int spawnChance = 1;

    public WillowUtils.RotationType rotationType = WillowUtils.RotationType.AsPrefab;
    public WillowUtils.Axis rotationAxis = WillowUtils.Axis.Y;
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

    public string layer = "default";
    public int layerIndex = 0;

    public bool renameObject = false;
    public string newObjectName = "Object";

    public WillowUtils.ScaleType scaleType = WillowUtils.ScaleType.AsPrefab;
    public WillowUtils.Axis scaleAxis = WillowUtils.Axis.XYZ;
    public bool modScale = false;
    public Vector3 customScale = Vector3.one;
    public Vector3 scaleMinSeparated = new Vector3(0.9f, 0.9f, 0.9f);
    public float scaleMin = 1;
    public Vector3 scaleMaxSeparated = new Vector3(1.1f, 1.1f, 1.1f);
    public float scaleMax = 1;
    public bool separateScaleAxis = true;

    public bool hidden = false;

    public bool RecalculatePosition = true;

    public SpawnableObject()
    {

    }
    public SpawnableObject(SpawnableObject clone)
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
        scaleAxis = clone.scaleAxis;
        modScale = clone.modScale;
        customScale = clone.customScale;
        separateScaleAxis = clone.separateScaleAxis;
        scaleMin = clone.scaleMin;
        scaleMax = clone.scaleMax;
        scaleMinSeparated = clone.scaleMinSeparated;
        scaleMaxSeparated = clone.scaleMaxSeparated;
    }
}