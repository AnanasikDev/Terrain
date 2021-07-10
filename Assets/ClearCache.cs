using UnityEditor;
using UnityEngine;
public class ClearCache : Editor
{
    [MenuItem("Terrain/Clear Change Log")]
    public static void ClearChangeLog()
    {
        TerrainSettings.changelog.Clear();
        foreach (GameObject obj in TerrainSettings.destroyedObjects)
        {
             DestroyImmediate(obj);
        }
        Debug.Log(Utils.FormatLog("Changelog cleared", "#00FF00FF"));
    }
    [MenuItem("Terrain/Enable Destoryed Objects")]
    public static void EnableDestroyedObjects()
    {
        if (TerrainSettings.destroyedObjects.Count == 0)
        {
            Debug.LogError(Utils.FormatLog("There are no destroyed objects!"));
        }
        else foreach (GameObject obj in TerrainSettings.destroyedObjects)
        {
            obj.hideFlags = HideFlags.None;
            obj.SetActive(true);
        }
    }
    [MenuItem("Terrain/Disable Destoryed Objects")]
    public static void DisableDestroyedObjects()
    {
        if (TerrainSettings.destroyedObjects.Count == 0)
        {
            Debug.LogError(Utils.FormatLog("There are no destroyed objects!"));
        }
        else foreach (GameObject obj in TerrainSettings.destroyedObjects)
        {
            obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
            obj.SetActive(false);
        }
    }
}
