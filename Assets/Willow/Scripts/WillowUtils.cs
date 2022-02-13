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
    public enum BrushMode
    {
        Place,
        Erase,
        Exchange
    }
    public enum BrushShape
    {
        Circle,
        Square
    }
    public enum BrushSurface
    {
        Static,
        AsNormal
    }
    public enum RecalculationMode
    {
        Static,
        AsNearest,
        AsRotation
    }
    public enum ObstaclesAvoidanceAction
    {
        Disable,
        Delete
    }
    public enum ObstaclesTagType
    {
        WillowObstacle
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

    public static float ToFloat(this string input)
    {
        return (float)System.Convert.ToDouble(input);
    }
    public static bool ToBool(this string input)
    {
        return System.Convert.ToBoolean(input);
    }
    public static int ToInt(this string input)
    {
        return System.Convert.ToInt32(input);
    }
    public static string RemoveSlashN(this string input)
    {
        return input.Replace("\n", "");
    }
    public static string RemoveSlashR(this string input)
    {
        return input.Replace("\r", "");
    }

    public static int RandomSign()
    {
        return RandomBool() ? 1 : -1;
    }
    public static bool RandomBool()
    {
        return UnityEngine.Random.value > 0.5f;
    }
}