using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public sealed class WillowSpawnedWindow : EditorWindow
{
    public List<GameObject> spawned;

    [MenuItem(WillowGlobalConfig.Path + "Spawned manager", false, -1)]
    public static void ShowWindow()
    {
        GetWindow<WillowSpawnedWindow>("Willow Spawned Objects Manager");
    }

    private void OnGUI()
    {
        spawned = WillowTerrainSettings.SpawnedObjects;

        ScriptableObject scriptableObj = this;
        SerializedObject serialObj = new SerializedObject(scriptableObj);
        SerializedProperty serialProp = serialObj.FindProperty("spawned");

        EditorGUILayout.PropertyField(serialProp, true);
        serialObj.ApplyModifiedProperties();

        GUILayout.BeginHorizontal("box");

        if (GUILayout.Button("Delete all"))
        {
            foreach (GameObject gameObject in spawned)
            {
                GameObject.DestroyImmediate(gameObject);
            }
        }

        GUILayout.EndHorizontal();

        WillowTerrainSettings.SpawnedObjects = spawned;//
    }
}
