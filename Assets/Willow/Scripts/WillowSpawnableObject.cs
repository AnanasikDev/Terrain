using UnityEngine;

[System.Serializable]
public sealed class SpawnableObject
{
    public bool Spawn = true;
    public GameObject Object;
    [SerializeField, Min(0)] public int SpawnChance = 1;

    public WillowUtils.RotationType RotationType = WillowUtils.RotationType.AsPrefab;
    public WillowUtils.Axis RotationAxis = WillowUtils.Axis.Y;
    public Vector3 CustomEulersRotation = Vector3.zero;
    public float LerpValue = 0.15f;
    public float MinLerpValue = 0.1f;
    public float MaxLerpValue = 0.2f;
    public bool RandomizeLerpValue = false;
    public Vector3 RandomMinRotation = Vector3.zero;
    public Vector3 RandomMaxRotation = Vector3.one * 360;
    public bool MultiRotationAxis = false;
    public Vector3 RotationEulerAddition = Vector3.zero;

    public bool ModifyColor = true;
    public float ColorModPercentage = 35;

    public bool CustomParent = false;
    public Transform Parent;

    public bool CenterObject = false;
    public bool ModifyPosition = false;
    public Vector3 PositionAddition = Vector3.zero;

    public string Layer = "default";
    public int LayerIndex = 0;

    public bool RenameObject = false;
    public string NewObjectName = "Object";

    public WillowUtils.ScaleType ScaleType = WillowUtils.ScaleType.AsPrefab;
    public WillowUtils.Axis ScaleAxis = WillowUtils.Axis.XYZ;
    public bool ModifyScale = false;
    public Vector3 CustomScale = Vector3.one;
    public Vector3 ScaleMinSeparated = new Vector3(0.9f, 0.9f, 0.9f);
    public float ScaleMin = 0.9f;
    public Vector3 ScaleMaxSeparated = new Vector3(1.1f, 1.1f, 1.1f);
    public float ScaleMax = 1.15f;
    public bool SeparateScaleAxis = true;

    public bool Hidden = false;

    public SpawnableObject()
    {

    }
    public SpawnableObject(SpawnableObject clone)
    {
        Spawn = clone.Spawn;
        CenterObject = clone.CenterObject;
        Object = clone.Object;
        SpawnChance = clone.SpawnChance;
        Layer = clone.Layer;
        LayerIndex = clone.LayerIndex;
        RenameObject = clone.RenameObject;
        NewObjectName = clone.NewObjectName;

        RotationType = clone.RotationType;
        RotationAxis = clone.RotationAxis;
        CustomEulersRotation = clone.CustomEulersRotation;
        LerpValue = clone.LerpValue;
        MinLerpValue = clone.MinLerpValue;
        MaxLerpValue = clone.MaxLerpValue;
        RandomizeLerpValue = clone.RandomizeLerpValue;
        RandomMinRotation = clone.RandomMinRotation;
        RandomMaxRotation = clone.RandomMaxRotation;
        MultiRotationAxis = clone.MultiRotationAxis;

        ModifyColor = clone.ModifyColor;
        ColorModPercentage = clone.ColorModPercentage;

        CustomParent = clone.CustomParent;
        Parent = clone.Parent;

        ModifyPosition = clone.ModifyPosition;
        PositionAddition = clone.PositionAddition;

        ScaleType = clone.ScaleType;
        ScaleAxis = clone.ScaleAxis;
        ModifyScale = clone.ModifyScale;
        CustomScale = clone.CustomScale;
        SeparateScaleAxis = clone.SeparateScaleAxis;
        ScaleMin = clone.ScaleMin;
        ScaleMax = clone.ScaleMax;
        ScaleMinSeparated = clone.ScaleMinSeparated;
        ScaleMaxSeparated = clone.ScaleMaxSeparated;
    }
}