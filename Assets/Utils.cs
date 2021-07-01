using System.Collections;
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
    public enum ColorModType
    {
        Random,
        Static,
        None
    }
    public enum RotationType
    {
        Random,
        Static,
        AsPrefab,
    }
    public enum RotationAxis
    {
        X,
        Y,
        Z,
        XY,
        XZ,
        YZ,
        XYZ
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
}
[System.Serializable]
public class SpawnableObject
{
    public GameObject spawnableObject;
    [SerializeField, Min(0)] public int spawnChance = 1;
    public Utils.RotationType rotationType = Utils.RotationType.AsPrefab;
    public Utils.RotationAxis rotationAxis = Utils.RotationAxis.Y;
}