using UnityEngine;
using UnityEditor;
using static WillowUtils;
using static WillowGlobalConfig;
using static WillowDebug;
public static class WillowUndo
{
    public static void Undo()
    {
        if (WillowTerrainSettings.changelog.Count == 0)
        {
            Log("Undo stack is empty!", Yellow, Debug.LogError);
            return;
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        Change lastChange = WillowTerrainSettings.changelog.Pop();
        if (lastChange.type == ChangeType.Placement)
        {
            GameObject[] changedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.gameObject.hideFlags = hidden;
                obj.SetActive(false);
                WillowTerrainSettings.spawnedObjects.Remove(obj);
                WillowTerrainSettings.destroyedObjects.Add(obj);
            }
        }
        else if (lastChange.type == ChangeType.Erasure)
        {
            GameObject[] changedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in changedObjsTemp)
            {
                obj.hideFlags = active;
                obj.SetActive(true);
                WillowTerrainSettings.destroyedObjects.Remove(obj);
                WillowTerrainSettings.spawnedObjects.Add(obj);
            }
            GUIUtility.hotControl = controlId;
            Event.current.Use();
        }
        else if (lastChange.type == ChangeType.Exchange)
        {
            GameObject[] destroyedObjsTemp = lastChange.destroyedObjects.ToArray();
            foreach (GameObject obj in destroyedObjsTemp)
            {
                obj.hideFlags = active;
                obj.SetActive(true);
                WillowTerrainSettings.destroyedObjects.Remove(obj);
                WillowTerrainSettings.spawnedObjects.Add(obj);
            }

            GameObject[] spawnedObjsTemp = lastChange.spawnedObjects.ToArray();
            foreach (GameObject obj in spawnedObjsTemp)
            {
                obj.hideFlags = hidden;
                obj.SetActive(false);
                WillowTerrainSettings.spawnedObjects.Remove(obj);
                WillowTerrainSettings.destroyedObjects.Add(obj);
            }
        }
        EditorApplication.RepaintHierarchyWindow();
    }
}
