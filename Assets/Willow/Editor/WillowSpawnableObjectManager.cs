#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using static WillowUtils;
using static WillowStyles;
using static WillowObjectsRecalculation;
using static WillowDebug;
using System.Linq;
public static class WillowSpawnableObjectManager
{
    public static void DrawSpawnableObject(int index)
    {
        if (!DrawSpawnableUI(index)) return;
        DrawSpawnableSettings(index);
    }
    private static bool DrawSpawnableUI(int index)
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.BeginVertical("box");

        int removeBtnHeight = 40;
        if (index < WillowTerrainSettings.SpawnableObjects.Count - 1 || index == 0) removeBtnHeight = 60;
        if (!WillowTerrainSettings.SpawnableObjects[index].Hidden)
        {
            if (GUILayout.Button("‹", GUILayout.Width(18), GUILayout.Height(18)))
            {

                WillowTerrainSettings.SpawnableObjects[index].Hidden = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                return false;
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("›", GUILayout.Width(18), GUILayout.Height(18)))
            {
                WillowTerrainSettings.SpawnableObjects[index].Hidden = false;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label(WillowTerrainSettings.SpawnableObjects[index].Object != null ? (WillowTerrainSettings.SpawnableObjects[index].RenameObject ? WillowTerrainSettings.SpawnableObjects[index].NewObjectName : WillowTerrainSettings.SpawnableObjects[index].Object.name) : "null");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return false;
            }

        }
        if (index > 0)
            if (GUILayout.Button("˄", GUILayout.Width(18), GUILayout.Height(18)))
            {
                var temp = WillowTerrainSettings.SpawnableObjects[index];
                WillowTerrainSettings.SpawnableObjects[index] = WillowTerrainSettings.SpawnableObjects[index - 1];
                WillowTerrainSettings.SpawnableObjects[index - 1] = temp;
            }

        Color bgc = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.9f, 0.45f, 0.44f);

        if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(removeBtnHeight)))
        {
            if (SpawnedDependsOnSpawnable(WillowTerrainSettings.SpawnableObjects[index], out int amount))
            {
                if (!WillowTerrainSettings.SafeMode) 
                    EditorUtility.DisplayDialog("Willow Error", $"Unable to remove spawnable object. {amount} spawned objects depend on it.", "Ok");
                return false;
            }
            WillowTerrainSettings.SpawnableObjects.RemoveAt(index);
            return false;
        }

        GUI.backgroundColor = bgc;

        if (index < WillowTerrainSettings.SpawnableObjects.Count - 1)
            if (GUILayout.Button("˅", GUILayout.Width(18), GUILayout.Height(18)))
            {
                var temp = WillowTerrainSettings.SpawnableObjects[index];
                WillowTerrainSettings.SpawnableObjects[index] = WillowTerrainSettings.SpawnableObjects[index + 1];
                WillowTerrainSettings.SpawnableObjects[index + 1] = temp;
            }

        GUI.backgroundColor = new Color(0.35f, 0.85f, 0.32f);

        if (GUILayout.Button("+", GUILayout.Width(18), GUILayout.Height(18)))
        {
            //new WillowSpawnableObject(WillowTerrainSettings.SpawnableObjects[index])
            WillowTerrainSettings.SpawnableObjects.Insert(index + 1, WillowTerrainSettings.SpawnableObjects[index].Clone());
        }

        GUI.backgroundColor = bgc;

        EditorGUILayout.EndVertical();

        return true;
    }
    private static void DrawSpawnableSettings(int index)
    {
        WillowSpawnableObject spawnableObject = WillowTerrainSettings.SpawnableObjects[index];

        EditorGUILayout.BeginVertical("box");

        spawnableObject.Spawn = EditorGUILayout.Toggle("Active", spawnableObject.Spawn);
        if (!spawnableObject.Spawn)
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
            //continue;
        }
        EditorGUILayout.BeginVertical("box");

        DrawLabel("GameObject");

        spawnableObject.Object = (GameObject)EditorGUILayout.ObjectField("GameObject", spawnableObject.Object, typeof(GameObject), true);

        spawnableObject.RenameObject = EditorGUILayout.Toggle("Rename object", spawnableObject.RenameObject);
        if (spawnableObject.RenameObject)
            spawnableObject.NewObjectName = EditorGUILayout.TextField("  Name ", spawnableObject.NewObjectName);

        spawnableObject.CenterObject = EditorGUILayout.Toggle("Center Object", spawnableObject.CenterObject);

        spawnableObject.CustomParent = EditorGUILayout.Toggle("Custom parent", spawnableObject.CustomParent);
        if (spawnableObject.CustomParent)
            spawnableObject.Parent = (Transform)EditorGUILayout.ObjectField("  Parent", spawnableObject.Parent, typeof(Transform), true);

        spawnableObject.LayerIndex = EditorGUILayout.Popup("Layer ", spawnableObject.LayerIndex, WillowTerrainSettings.LayersName.ToArray());
        if (spawnableObject.LayerIndex >= WillowTerrainSettings.LayersName.Count)
            spawnableObject.Layer = WillowTerrainSettings.LayersName[0];
        else
            spawnableObject.Layer = WillowTerrainSettings.LayersName[spawnableObject.LayerIndex];

        spawnableObject.SpawnChance = EditorGUILayout.IntField("Chance", spawnableObject.SpawnChance); //objs[i].spawnChance
        
        if (spawnableObject.SpawnChance < 0) 
            spawnableObject.SpawnChance = 0;


        EditorGUILayout.BeginVertical("box");

        DrawLabel("Recalculate", 2);

        EditorGUILayout.BeginHorizontal("box");

        if (GUILayout.Button("Position"))
        {
            RecalculatePositionsSelected(WillowTerrainSettings.SpawnedObjects
                .Where(o => o != null && o.GetComponent<WillowSpawnedObject>().Layer == spawnableObject.Layer).ToArray());

        }
        if (GUILayout.Button("Rotation"))
        {
            RecalculateRotationsSelected(WillowTerrainSettings.SpawnedObjects
                .Where(o => o != null && o.GetComponent<WillowSpawnedObject>().Layer == spawnableObject.Layer).ToArray());
        }
        if (GUILayout.Button("Scale"))
        {
            RecalculateScalesSelected(WillowTerrainSettings.SpawnedObjects
                .Where(o => o != null && o.GetComponent<WillowSpawnedObject>().Layer == spawnableObject.Layer).ToArray());
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        DrawLabel("Rotation");

        spawnableObject.RotationType = (RotationType)EditorGUILayout.EnumPopup("Rotation", spawnableObject.RotationType);
        if (spawnableObject.RotationType == RotationType.Random ||
            spawnableObject.RotationType == RotationType.RandomAsNormal ||
            spawnableObject.RotationType == RotationType.LerpedRandomAsNormal)
        {
            spawnableObject.MultiRotationAxis = EditorGUILayout.Toggle("Multi axis", spawnableObject.MultiRotationAxis);
            if (spawnableObject.MultiRotationAxis)
            {
                spawnableObject.RandomMinRotation = EditorGUILayout.Vector3Field("  Min rotation", spawnableObject.RandomMinRotation);
                spawnableObject.RandomMaxRotation = EditorGUILayout.Vector3Field("  Max rotation", spawnableObject.RandomMaxRotation);
            }
            else
                spawnableObject.RotationAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", spawnableObject.RotationAxis);
        }

        if (spawnableObject.RotationType == RotationType.Static ||
            spawnableObject.RotationType == RotationType.StaticAsNormal ||
            spawnableObject.RotationType == RotationType.LerpedStaticAsNormal)
            spawnableObject.CustomEulersRotation = EditorGUILayout.Vector3Field("  Custom Euler Rotation", spawnableObject.CustomEulersRotation);

        if (spawnableObject.RotationType == RotationType.LerpedRandomAsNormal)
        {
            spawnableObject.RandomizeLerpValue = EditorGUILayout.Toggle("  Randomize lerp value", spawnableObject.RandomizeLerpValue);
            if (spawnableObject.RandomizeLerpValue)
            {
                spawnableObject.MinLerpValue = EditorGUILayout.FloatField("  Min lerp value", spawnableObject.MinLerpValue);
                spawnableObject.MaxLerpValue = EditorGUILayout.FloatField("  Max lerp value", spawnableObject.MaxLerpValue);
            }
            else
            {
                spawnableObject.LerpValue = EditorGUILayout.FloatField("  Lerp value", spawnableObject.LerpValue);
            }
        }
        if (spawnableObject.RotationType == RotationType.LerpedStaticAsNormal ||
            spawnableObject.RotationType == RotationType.LerpedAsPrefabAsNormal)
            spawnableObject.LerpValue = EditorGUILayout.FloatField("  Lerp value", spawnableObject.LerpValue);

        spawnableObject.RotationEulerAddition = EditorGUILayout.Vector3Field("Add eulers", spawnableObject.RotationEulerAddition);

        DrawLabel("Position");

        spawnableObject.ModifyPosition = EditorGUILayout.Toggle("Modify position", spawnableObject.ModifyPosition);
        if (spawnableObject.ModifyPosition)
        {
            spawnableObject.PositionAddition = EditorGUILayout.Vector3Field("  Position addition", spawnableObject.PositionAddition);
            spawnableObject.PositionAdditionSpace = (Space)EditorGUILayout.EnumPopup("  Space", spawnableObject.PositionAdditionSpace);
        }

        DrawLabel("Scale");

        spawnableObject.ModifyScale = EditorGUILayout.Toggle("Modify scale", spawnableObject.ModifyScale);
        if (spawnableObject.ModifyScale)
        {
            spawnableObject.ScaleType = (ScaleType)EditorGUILayout.EnumPopup("Scale", spawnableObject.ScaleType);

            if (spawnableObject.ScaleType == ScaleType.Random)
            {
                spawnableObject.SeparateScaleAxis = EditorGUILayout.Toggle("  Separate axis", spawnableObject.SeparateScaleAxis);
                spawnableObject.ScaleAxis = (Axis)EditorGUILayout.EnumPopup("  Axis", spawnableObject.ScaleAxis);
                if (spawnableObject.SeparateScaleAxis)
                {
                    spawnableObject.ScaleMinSeparated = EditorGUILayout.Vector3Field("  Min scale", spawnableObject.ScaleMinSeparated);
                    spawnableObject.ScaleMaxSeparated = EditorGUILayout.Vector3Field("  Max scale", spawnableObject.ScaleMaxSeparated);
                }
                else
                {
                    spawnableObject.ScaleMin = EditorGUILayout.FloatField("  Min scale", spawnableObject.ScaleMin);
                    spawnableObject.ScaleMax = EditorGUILayout.FloatField("  Max scale", spawnableObject.ScaleMax);
                }
            }
            if (spawnableObject.ScaleType == ScaleType.Static)
            {
                spawnableObject.SeparateScaleAxis = EditorGUILayout.Toggle("  Separate axis", spawnableObject.SeparateScaleAxis);
                if (spawnableObject.SeparateScaleAxis)
                    spawnableObject.CustomScale = EditorGUILayout.Vector3Field("  Custom scale", spawnableObject.CustomScale);
                else
                {
                    spawnableObject.CustomScale.x = EditorGUILayout.FloatField("  Scale", spawnableObject.CustomScale.x);
                    float scale = spawnableObject.CustomScale.x;
                    spawnableObject.CustomScale = Vector3.one * scale;
                }
            }

        }

        DrawLabel("Color");

        spawnableObject.ModifyColor = EditorGUILayout.Toggle("Modify color", spawnableObject.ModifyColor);

        if (spawnableObject.ModifyColor)
        {
            spawnableObject.ColorModPercentage = EditorGUILayout.FloatField("  Color modification %", spawnableObject.ColorModPercentage);
            if (spawnableObject.ColorModPercentage < 0) spawnableObject.ColorModPercentage = 0;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    private static void DrawLabel(string text, int offstep = 12)
    {
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(offstep);
        EditorGUILayout.LabelField($"<b><color=#CCCCCCFF>{text}</color></b>", labelStyle);
        EditorGUILayout.Space(offstep);
        EditorGUILayout.BeginVertical("box");
    }
    public static void ResetAllSpawnableObjects()
    {
        WillowTerrainSettings.SpawnableObjects.Clear();
    }
    public static void DrawSpawnablesAddButton()
    {
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label(WillowTerrainSettings.SpawnableObjects.Count.ToString());

        GUI.backgroundColor = GreenColor;

        if (GUILayout.Button("Create new", GUILayout.Width(120)))
        {
            WillowTerrainSettings.SpawnableObjects.Add(new WillowSpawnableObject());
        }

        GUI.backgroundColor = RedColor;

        if (GUILayout.Button("Reset", GUILayout.Width(60)))
        {
            if (!WillowTerrainSettings.SafeMode || EditorUtility.DisplayDialog("Reset all spawnable objects", "Are you sure to RESET all spawnable objects info? You can not undo this action.", "Reset all", "Cancel"))
            {
                WillowSpawnableObjectManager.ResetAllSpawnableObjects();
            }
        }

        GUI.backgroundColor = DefaultBackGroundColor;

        EditorGUILayout.EndHorizontal();
    }

    private static bool SpawnedDependsOnSpawnable(WillowSpawnableObject spawnableObject, out int amount)
    {
        amount = 0;
        foreach (GameObject spawned in WillowTerrainSettings.SpawnedObjects)
        {
            if (spawned.GetComponent<WillowSpawnedObject>().SpawnableObject == spawnableObject)
            {
                amount++;
            }
        }
        if (amount > 0)
        {
            return true; // DEPENDS, UNABLE TO REMOVE
        }
        return false;
    }
}
#endif
