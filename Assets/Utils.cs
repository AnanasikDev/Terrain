using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public enum spawnPlaceType
    {
        onTerrainOnly,
        onObjectsOnly,
        onTerrainAndObjects
    }
    public enum colorModType
    {
        Random,
        Static,
        None
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

}