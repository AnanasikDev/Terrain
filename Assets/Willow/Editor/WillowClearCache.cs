using UnityEditor;
using UnityEngine;
using System.Linq;
using static WillowGlobalConfig;
using static WillowDebug;
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
        Log("Changelog cleared", Green);
    }
    [MenuItem(Path + "Enable Destoryed Objects")]
    public static void EnableDestroyedObjects()
    {
        if (WillowTerrainSettings.destroyedObjects.Count == 0)
        {
            Log("There are no destroyed objects!", Yellow, Debug.LogError);
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
            Log("There are no destroyed objects!", Yellow, Debug.LogError);
        }
        else foreach (GameObject obj in WillowTerrainSettings.destroyedObjects.Where(x => x != null))
        {
            obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
            obj.SetActive(false);
        }
        SceneView.RepaintAll();
    }
    [MenuItem(Path + "Destroy Destoryed Objects")]
    public static void DestroyDestroyedObjects()
    {
        if (WillowTerrainSettings.destroyedObjects.Count == 0)
        {
            Log("There are no destroyed objects!", Yellow, Debug.LogError);
        }
        else for (int i = 0; i < WillowTerrainSettings.destroyedObjects.Count; i++)
        {
            if (WillowTerrainSettings.destroyedObjects[i] != null)
            {
                Object.DestroyImmediate(WillowTerrainSettings.destroyedObjects[i].gameObject);
            }
        }
        SceneView.RepaintAll();
    }
    [MenuItem(Path + "Reset indecies")]
    public static void ResetIndecies()
    {
        WillowTerrainSettings.spawnedIndecies = 0;//
        Log("Indecies reseted.", Green);
    }
    [MenuItem(Path + "Save")]
    public static void Save()
    {
        WillowFileManager.Write();
    }
}
