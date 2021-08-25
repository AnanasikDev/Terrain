using UnityEditor;
using static WillowGlobalConfig;
public class WillowClearingDestroyed : Editor
{
    public static void ClearDestroyedObjects()
    {
        for (int i = 0; i < WillowTerrainSettings.destroyedObjects.Count; i++)
        {
            //Object.DestroyImmediate(WillowTerrainSettings.destroyedObjects[i].gameObject);
            WillowTerrainSettings.destroyedObjects[i].gameObject.hideFlags = hidden;
            EditorUtility.SetDirty(WillowTerrainSettings.destroyedObjects[i].gameObject);
            //EditorUtility.
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
        EditorApplication.RepaintHierarchyWindow();
    }
}
