using UnityEditor;
using UnityEngine;
using System.Linq;
using static WillowGlobalConfig;
public class WillowClearCache : Editor
{
    [MenuItem(Path + "Clear Change Log")]
    public static void ClearChangeLog()
    {
        WillowTerrainSettings.changelog.Clear();

        foreach (GameObject obj in WillowTerrainSettings.destroyedObjects)
        {
            DestroyImmediate(obj);
        }
        if (WillowTerrainSettings.debugMode) Debug.Log(WillowUtils.FormatLog("Changelog cleared", "#00FF00FF"));
    }
    [MenuItem(Path + "Enable Destoryed Objects")]
    public static void EnableDestroyedObjects()
    {
        if (WillowTerrainSettings.destroyedObjects.Count == 0)
        {
            if (WillowTerrainSettings.debugMode) Debug.LogError(WillowUtils.FormatLog("There are no destroyed objects!"));
        }
        else foreach (GameObject obj in WillowTerrainSettings.destroyedObjects.Where(x => x != null))
        {
            obj.hideFlags = HideFlags.None;
            obj.SetActive(true);
        }
        SceneView.RepaintAll();
    }
    [MenuItem(Path + "Disable Destoryed Objects")]
    public static void DisableDestroyedObjects()
    {
        if (WillowTerrainSettings.destroyedObjects.Count == 0)
        {
            if (WillowTerrainSettings.debugMode) Debug.LogError(WillowUtils.FormatLog("There are no destroyed objects!"));
        }
        else foreach (GameObject obj in WillowTerrainSettings.destroyedObjects.Where(x => x != null))
        {
            obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
            obj.SetActive(false);
        }
        SceneView.RepaintAll();
    }
    [MenuItem(Path + "Reset indecies")]
    public static void ResetIndecies()
    {
        WillowTerrainSettings.spawnedIndecies = 0;
        if (WillowTerrainSettings.debugMode) Debug.Log(WillowUtils.FormatLog("Indecies reseted.", "#00FF00FF"));
    }
    [MenuItem(Path + "Save")]
    public static void Save()
    {
        WillowFileManager.Write();
    }
}
