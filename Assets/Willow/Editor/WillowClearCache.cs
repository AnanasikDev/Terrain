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
        WillowTerrainSettings.ChangeLog.Clear();

        foreach (GameObject obj in WillowTerrainSettings.DestroyedObjects)
        {
            DestroyImmediate(obj);
        }
        Log("Changelog cleared", Green);
    }
    [MenuItem(Path + "Enable Destoryed Objects")]
    public static void EnableDestroyedObjects()
    {
        if (WillowTerrainSettings.DestroyedObjects.Count == 0)
        {
            Log("There are no destroyed objects!", Yellow, Debug.LogError);
        }
        else foreach (GameObject obj in WillowTerrainSettings.DestroyedObjects.Where(x => x != null))
        {
            obj.hideFlags = HideFlags.None;
            obj.SetActive(true);
        }
        SceneView.RepaintAll();
    }
    [MenuItem(Path + "Disable Destoryed Objects")]
    public static void DisableDestroyedObjects()
    {
        if (WillowTerrainSettings.DestroyedObjects.Count == 0)
        {
            Log("There are no destroyed objects!", Yellow, Debug.LogError);
        }
        else foreach (GameObject obj in WillowTerrainSettings.DestroyedObjects.Where(x => x != null))
        {
            obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
            obj.SetActive(false);
        }
        SceneView.RepaintAll();
    }
    [MenuItem(Path + "Destroy Destoryed Objects")]
    public static void DestroyDestroyedObjects()
    {
        if (WillowTerrainSettings.DestroyedObjects.Count == 0)
        {
            Log("There are no destroyed objects!", Yellow, Debug.LogError);
        }
        else for (int i = 0; i < WillowTerrainSettings.DestroyedObjects.Count; i++)
        {
            if (WillowTerrainSettings.DestroyedObjects[i] != null)
            {
                Object.DestroyImmediate(WillowTerrainSettings.DestroyedObjects[i].gameObject);
            }
        }
        SceneView.RepaintAll();
    }
    [MenuItem(Path + "Reset indecies")]
    public static void ResetIndecies()
    {
        WillowTerrainSettings.SpawnedIndecies = 0;//
        Log("Indecies reseted.", Green);
    }
    [MenuItem(Path + "Save")]
    public static void Save()
    {
        WillowFileManager.Write();
    }
}
