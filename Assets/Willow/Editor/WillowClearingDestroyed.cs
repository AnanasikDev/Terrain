using UnityEditor;
using static WillowGlobalConfig;
public class WillowClearingDestroyed : Editor
{
    public static void ClearDestroyedObjects()
    {
        for (int i = 0; i < WillowTerrainSettings.DestroyedObjects.Count; i++)
        {
            //Object.DestroyImmediate(WillowTerrainSettings.destroyedObjects[i].gameObject);
            WillowTerrainSettings.DestroyedObjects[i].gameObject.hideFlags = hidden;
            EditorUtility.SetDirty(WillowTerrainSettings.DestroyedObjects[i].gameObject);
            //EditorUtility.
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
        EditorApplication.RepaintHierarchyWindow();
    }
}
