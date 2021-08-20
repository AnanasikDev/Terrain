using System.Collections.Generic;
using UnityEngine;
public static class WillowUtils
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
    public enum BrushShape
    {
        Circle,
        Square
    }

    public enum ParameterActionType
    {
        MultiplyByVector3,
        MultiplyByNum,
        
        DivideByVector3,
        DivideByNum,

        AddVector3,

        SubtractByVector3,

        PowByNum,
        PowByVector3
    }
    public static int GetChance(int[] chances)
    {
        int n = chances.Length;
        List<int> res = new List<int>(n);
        for (int i = 0; i < n; i++)
        {
            for (int _ = 0; _ < chances[i]; _++) res.Add(i);
        }
        return res[UnityEngine.Random.Range(0, res.Count)];
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
    public static float ToFloat(this string input)
    {
        return (float)System.Convert.ToDouble(input);
    }
    public static string RemoveSlashN(this string input)
    {
        return input.Replace("\n", "");
    }
    public static string RemoveSlashR(this string input)
    {
        return input.Replace("\r", "");
    }
}