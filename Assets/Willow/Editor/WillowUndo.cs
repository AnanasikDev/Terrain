#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using static WillowUtils;
using static WillowGlobalConfig;
using static WillowDebug;
public static class WillowUndo
{
    public static void Undo()
    {
        if (WillowTerrainSettings.ChangeLog.Count == 0)
        {
            Log("Undo stack is empty!", Yellow, Debug.LogError);
            return;
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        Change lastChange = WillowTerrainSettings.ChangeLog.Pop();
        if (lastChange.type == BrushMode.Place)
        {
            GameObject[] changedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.gameObject.hideFlags = hidden;
                obj.SetActive(false);
                WillowTerrainSettings.SpawnedObjects.Remove(obj);
                WillowTerrainSettings.DestroyedObjects.Add(obj);
            }
        }
        else if (lastChange.type == BrushMode.Erase)
        {
            GameObject[] changedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.hideFlags = active;
                obj.SetActive(true);
                WillowTerrainSettings.DestroyedObjects.Remove(obj);
                WillowTerrainSettings.SpawnedObjects.Add(obj);
            }
            GUIUtility.hotControl = controlId;
            Event.current.Use();
        }
        else if (lastChange.type == BrushMode.Exchange)
        {
            GameObject[] destroyedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in destroyedObjsTemp)
            {
                obj.hideFlags = active;
                obj.SetActive(true);
                WillowTerrainSettings.DestroyedObjects.Remove(obj);
                WillowTerrainSettings.SpawnedObjects.Add(obj);
            }

            GameObject[] spawnedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in spawnedObjsTemp)
            {
                obj.hideFlags = hidden;
                obj.SetActive(false);
                WillowTerrainSettings.SpawnedObjects.Remove(obj);
                WillowTerrainSettings.DestroyedObjects.Add(obj);
            }
        }
        EditorApplication.RepaintHierarchyWindow();
    }
}
#endif
