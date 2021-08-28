using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static WillowStyles;
using static WillowDebug;
using static WillowGlobalConfig;
public static class WillowLayers
{
    private static string newLayerName = "defaultLayer";
    public static void DrawLayersTab()
    {
        EditorGUILayout.BeginHorizontal("box");
        bool add = false;

        GUI.backgroundColor = GreenColor;

        if (GUILayout.Button("Create"))
            add = true;

        GUI.backgroundColor = DefaultBackGroundColor;

        newLayerName = EditorGUILayout.TextField("New layer name: ", newLayerName).RemoveSlashR().RemoveSlashN();
        if (add && newLayerName != "")
        {
            if (!WillowTerrainSettings.LayersName.Contains(newLayerName)) // If this name is free
            {
                WillowTerrainSettings.LayersName.Add(newLayerName);
                WillowTerrainSettings.LayersState.Add(true);
            }
            else
            {
                EditorUtility.DisplayDialog("Willow Error", $"You can not create layer with already taken name {newLayerName}.", "Ok");
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawLayersArray();
    }
    private static void DrawLayersArray()
    {
        WillowTerrainSettings.LayersName.RemoveAll(layerName => layerName == "");
        for (int layerId = 0; layerId < WillowTerrainSettings.LayersName.Count; layerId++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginHorizontal("box");
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18)))
            {
                if (WillowTerrainSettings.LayersName.Count > 1)
                {
                    int dependedAmount = 0;
                    foreach (GameObject spawnedObj in WillowTerrainSettings.SpawnedObjects)
                    {
                        if (spawnedObj.GetComponent<WillowSpawnedObject>().Layer == WillowTerrainSettings.LayersName[layerId])
                        {
                            dependedAmount++;
                        }
                    }
                    if (dependedAmount > 0)
                    {
                        EditorUtility.DisplayDialog("Willow Error", $"Impossible to remove the layer: {dependedAmount} objects depend on it.", "Ok");
                    }
                    else
                        WillowTerrainSettings.LayersName[layerId] = "";
                }
                else
                {
                    EditorUtility.DisplayDialog("Willow Error", "Impossible to remove the last layer.", "Ok");
                }
            }
            GUI.backgroundColor = oldBgColor;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginVertical("box");

            bool layerActiveBefore = WillowTerrainSettings.LayersState[layerId];
            WillowTerrainSettings.LayersState[layerId] = EditorGUILayout.Toggle("Active", WillowTerrainSettings.LayersState[layerId]);

            if (layerActiveBefore != WillowTerrainSettings.LayersState[layerId]) // Toggle value changed
            {
                List<GameObject> spawned = WillowTerrainSettings.SpawnedObjects.Where(
                                    o => o != null &&
                                    o.hideFlags == active &&
                                    o.GetComponent<WillowSpawnedObject>().Layer == WillowTerrainSettings.LayersName[layerId]).ToList();

                if (WillowTerrainSettings.LayersState[layerId]) // If active
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(true);
                    }
                if (!WillowTerrainSettings.LayersState[layerId]) // If inactive
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(false);
                    }
            }

            string lastLayerName = WillowTerrainSettings.LayersName[layerId];
            WillowTerrainSettings.LayersName[layerId] = EditorGUILayout.TextField("Name: ", WillowTerrainSettings.LayersName[layerId]);

            if (lastLayerName != WillowTerrainSettings.LayersName[layerId]) // Layer was renamed
            {
                if (WillowTerrainSettings.LayersName[layerId] == "") WillowTerrainSettings.LayersName[layerId] += "layer";
                if (WillowTerrainSettings.LayersName.FindAll(x => x == WillowTerrainSettings.LayersName[layerId]).Count > 1)
                {
                    WillowTerrainSettings.LayersName[layerId] = lastLayerName;
                    Log("Impossible to hold several layers with the same name.", Yellow, Debug.LogError);
                }
                else foreach (GameObject spawnedObj in WillowTerrainSettings.SpawnedObjects)
                    {
                        if (spawnedObj != null && spawnedObj.hideFlags == active)
                        {
                            WillowSpawnedObject spawnedObjectSc = spawnedObj.GetComponent<WillowSpawnedObject>();
                            if (spawnedObjectSc != null && spawnedObjectSc.Layer == lastLayerName)
                            {
                                spawnedObjectSc.Layer = WillowTerrainSettings.LayersName[layerId];
                            }
                        }
                    }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
