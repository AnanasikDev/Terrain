#if UNITY_EDITOR
using System.Linq;
using UnityEngine;

public class WillowObjectsRecalculation
{
    public static void RecalculatePositionsSelected(GameObject[] spawnedObjects)
    {
        foreach (GameObject obj in spawnedObjects.Where(obj => obj != null))
        {
            obj.GetComponent<WillowSpawnedObject>().RecalculateObjectPosition();
        }
    }
    public static void RecalculateRotationsSelected(GameObject[] spawnedObjects)
    {
        foreach (GameObject obj in spawnedObjects.Where(obj => obj != null))
        {
            obj.GetComponent<WillowSpawnedObject>().RecalculateObjectRotation();
        }
    }
    public static void RecalculateScalesSelected(GameObject[] spawnedObjects)
    {
        foreach (GameObject obj in spawnedObjects.Where(obj => obj != null))
        {
            obj.GetComponent<WillowSpawnedObject>().RecalculateObjectScale();
        }
    }
}
#endif
