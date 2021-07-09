using UnityEditor;
using UnityEngine;
public class ClearCache : Editor
{
    [MenuItem("Terrain/ClearChangeLog")]
    public static void ClearChangeLog()
    {
        TerrainSettings.changelog.Clear();
        foreach (GameObject obj in TerrainSettings.spawnedObjects)
        {
            if (obj.hideFlags != HideFlags.None) // Inactive
            {
                DestroyImmediate(obj);
            }
        }
    }
}
