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
            if (!WillowTerrainSettings.layersName.Contains(newLayerName)) // If this name is free
            {
                WillowTerrainSettings.layersName.Add(newLayerName);
                WillowTerrainSettings.layersState.Add(true);
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
        WillowTerrainSettings.layersName.RemoveAll(layerName => layerName == "");
        for (int layerId = 0; layerId < WillowTerrainSettings.layersName.Count; layerId++)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginHorizontal("box");
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 1f);
            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18)))
            {
                if (WillowTerrainSettings.layersName.Count > 1)
                {
                    int dependedAmount = 0;
                    foreach (GameObject spawnedObj in WillowTerrainSettings.spawnedObjects)
                    {
                        if (spawnedObj.GetComponent<WillowSpawnedObject>().Layer == WillowTerrainSettings.layersName[layerId])
                        {
                            dependedAmount++;
                        }
                    }
                    if (dependedAmount > 0)
                    {
                        EditorUtility.DisplayDialog("Willow Error", $"Impossible to remove the layer: {dependedAmount} objects depend on it.", "Ok");
                    }
                    else
                        WillowTerrainSettings.layersName[layerId] = "";
                }
                else
                {
                    EditorUtility.DisplayDialog("Willow Error", "Impossible to remove the last layer.", "Ok");
                }
            }
            GUI.backgroundColor = oldBgColor;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginVertical("box");

            bool layerActiveBefore = WillowTerrainSettings.layersState[layerId];
            WillowTerrainSettings.layersState[layerId] = EditorGUILayout.Toggle("Active", WillowTerrainSettings.layersState[layerId]);

            if (layerActiveBefore != WillowTerrainSettings.layersState[layerId]) // Toggle value changed
            {
                List<GameObject> spawned = WillowTerrainSettings.spawnedObjects.Where(
                                    o => o != null &&
                                    o.hideFlags == active &&
                                    o.GetComponent<WillowSpawnedObject>().Layer == WillowTerrainSettings.layersName[layerId]).ToList();

                if (WillowTerrainSettings.layersState[layerId]) // If active
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(true);
                    }
                if (!WillowTerrainSettings.layersState[layerId]) // If inactive
                    foreach (GameObject spawnedObj in spawned)
                    {
                        spawnedObj.SetActive(false);
                    }
            }

            string lastLayerName = WillowTerrainSettings.layersName[layerId];
            WillowTerrainSettings.layersName[layerId] = EditorGUILayout.TextField("Name: ", WillowTerrainSettings.layersName[layerId]);

            if (lastLayerName != WillowTerrainSettings.layersName[layerId]) // Layer was renamed
            {
                if (WillowTerrainSettings.layersName[layerId] == "") WillowTerrainSettings.layersName[layerId] += "layer";
                if (WillowTerrainSettings.layersName.FindAll(x => x == WillowTerrainSettings.layersName[layerId]).Count > 1)
                {
                    WillowTerrainSettings.layersName[layerId] = lastLayerName;
                    Log("Impossible to hold several layers with the same name.", Yellow, Debug.LogError);
                }
                else foreach (GameObject spawnedObj in WillowTerrainSettings.spawnedObjects)
                    {
                        if (spawnedObj != null && spawnedObj.hideFlags == active)
                        {
                            WillowSpawnedObject spawnedObjectSc = spawnedObj.GetComponent<WillowSpawnedObject>();
                            if (spawnedObjectSc != null && spawnedObjectSc.Layer == lastLayerName)
                            {
                                spawnedObjectSc.Layer = WillowTerrainSettings.layersName[layerId];
                            }
                        }
                    }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
