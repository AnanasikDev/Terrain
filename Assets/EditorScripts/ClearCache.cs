using UnityEditor;
using UnityEngine;
using System.Linq;
public class ClearCache : Editor
{
    [MenuItem("Willow/Clear Change Log")]
    public static void ClearChangeLog()
    {
        TerrainSettings.changelog.Clear();
        foreach (GameObject obj in TerrainSettings.destroyedObjects)
        {
             DestroyImmediate(obj);
        }
        if (TerrainSettings.debugMode) Debug.Log(Utils.FormatLog("Changelog cleared", "#00FF00FF"));
    }
    [MenuItem("Willow/Enable Destoryed Objects")]
    public static void EnableDestroyedObjects()
    {
        if (TerrainSettings.destroyedObjects.Count == 0)
        {
            if (TerrainSettings.debugMode) Debug.LogError(Utils.FormatLog("There are no destroyed objects!"));
        }
        else foreach (GameObject obj in TerrainSettings.destroyedObjects.Where(x => x != null))
        {
            obj.hideFlags = HideFlags.None;
            obj.SetActive(true);
        }
        SceneView.RepaintAll();
    }
    [MenuItem("Willow/Disable Destoryed Objects")]
    public static void DisableDestroyedObjects()
    {
        if (TerrainSettings.destroyedObjects.Count == 0)
        {
            if (TerrainSettings.debugMode) Debug.LogError(Utils.FormatLog("There are no destroyed objects!"));
        }
        else foreach (GameObject obj in TerrainSettings.destroyedObjects.Where(x => x != null))
        {
            obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
            obj.SetActive(false);
        }
        SceneView.RepaintAll();
    }
    [MenuItem("Willow/Reset indecies")]
    public static void ResetIndecies()
    {
        TerrainSettings.spawnedIndecies = 0;
        if (TerrainSettings.debugMode) Debug.Log(Utils.FormatLog("Indecies reseted.", "#00FF00FF"));
    }
    [MenuItem("Willow/Save changings")]
    public static void Save()
    {
        FileManager.Write();
    }
}
