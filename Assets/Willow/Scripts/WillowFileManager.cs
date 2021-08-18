using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using static WillowUtils;
public static class WillowFileManager
{
    public static string path = "./WillowSaveFile.txt";
    //static string prefabsFolder = "Assets/Willow/Example/Prefabs/";
    public static void Write()
    {
        StringBuilder output = new StringBuilder();

        output.AppendLine(WillowTerrainSettings.active.ToString());
        output.AppendLine(WillowTerrainSettings.density.ToString());
        output.AppendLine(WillowTerrainSettings.brushSize.ToString());

        output.AppendLine(WillowTerrainSettings.parent == null ? "null" : WillowTerrainSettings.parent.name);
        output.AppendLine(WillowTerrainSettings.placementType.ToString());

        output.AppendLine(WillowTerrainSettings.layersName.Count.ToString());
        for (int layer = 0; layer < WillowTerrainSettings.layersName.Count; layer++)
        {
            output.AppendLine(WillowTerrainSettings.layersName[layer].Replace("\r", "").Replace("\n", ""));
            output.AppendLine(WillowTerrainSettings.layersState[layer].ToString());
        }

        output.AppendLine(WillowTerrainSettings.spawnableObjects.Count.ToString());
        foreach (SpawnableObject obj in WillowTerrainSettings.spawnableObjects)
        {
            output.AppendLine(obj.Object == null ? "null" : obj.Object.name);
            output.AppendLine(obj.Spawn.ToString());
            output.AppendLine(obj.SpawnChance.ToString());
            output.AppendLine(obj.CustomParent.ToString());
            output.AppendLine(obj.Parent == null ? "null" : obj.Parent.name);
            output.AppendLine(obj.CenterObject.ToString());
            //6

            output.AppendLine(obj.RotationType.ToString());
            output.AppendLine(obj.RotationAxis.ToString());
            output.AppendLine(obj.CustomEulersRotation.ToString());
            output.AppendLine(obj.LerpValue.ToString());
            output.AppendLine(obj.MinLerpValue.ToString());
            output.AppendLine(obj.MaxLerpValue.ToString());

            output.AppendLine(obj.MultiRotationAxis.ToString());
            output.AppendLine(obj.RandomizeLerpValue.ToString());
            output.AppendLine(obj.RandomMinRotation.ToString());
            output.AppendLine(obj.RandomMaxRotation.ToString());
            //16

            output.AppendLine(obj.ModifyColor.ToString());
            output.AppendLine(obj.ColorModPercentage.ToString());

            output.AppendLine(obj.ModifyPosition.ToString());
            output.AppendLine(obj.PositionAddition.ToString());

            output.AppendLine(obj.RenameObject.ToString());
            output.AppendLine(obj.NewObjectName.Replace("\r", ""));
            //22

            output.AppendLine(obj.ScaleType.ToString());
            output.AppendLine(obj.ScaleAxis.ToString());
            output.AppendLine(obj.ModifyScale.ToString());
            output.AppendLine(obj.CustomScale.ToString());
            output.AppendLine(obj.ScaleMinSeparated.ToString());
            output.AppendLine(obj.ScaleMin.ToString());
            output.AppendLine(obj.ScaleMaxSeparated.ToString());
            output.AppendLine(obj.ScaleMax.ToString());
            output.AppendLine(obj.SeparateScaleAxis.ToString());
            //31
            output.AppendLine(obj.Layer.Replace("\r", ""));
            output.AppendLine(obj.LayerIndex.ToString());

            output.AppendLine(obj.RotationEulerAddition.ToString());
        }

        output.AppendLine(WillowTerrainSettings.spawnedObjects.Where(o => o != null && o.hideFlags == HideFlags.None).ToArray().Length .ToString());
        foreach (GameObject spawnedObj in WillowTerrainSettings.spawnedObjects.Where(o => o != null && o.hideFlags == HideFlags.None))
        {
            output.AppendLine(spawnedObj.name.Replace("\r", ""));
        }

        output.AppendLine(WillowTerrainSettings.indexObjects.ToString());
        output.AppendLine(WillowTerrainSettings.indexFormat.Replace("\r", ""));
        output.AppendLine(WillowTerrainSettings.spawnedIndecies.ToString());

        output.AppendLine(WillowTerrainSettings.eraseSmoothness.ToString());

        output.AppendLine(WillowTerrainSettings.exchangeColor.ToString());
        output.AppendLine(WillowTerrainSettings.exchangeParent.ToString());
        output.AppendLine(WillowTerrainSettings.exchangePosition.ToString());
        output.AppendLine(WillowTerrainSettings.exchangeRotation.ToString());
        output.AppendLine(WillowTerrainSettings.exchangeScale.ToString());
        output.AppendLine(WillowTerrainSettings.exchangeSmoothness.ToString());

        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.Write(output);
        }

        if (WillowTerrainSettings.debugMode) Debug.Log(WillowUtils.FormatLog("Templates saved!", "#00FF00FF"));
    }
    public static void Read()
    {
        string[] lines;

        using (StreamReader reader = new StreamReader(path))
        {
            lines = reader.ReadToEnd().Split('\n');
            WillowTerrainSettings.active = Convert.ToBoolean(lines[0]);
            WillowTerrainSettings.density = Convert.ToInt32(lines[1]);
            WillowTerrainSettings.brushSize = Convert.ToInt32(lines[2]);

            if (lines[3].Replace("\r", "") == "null") WillowTerrainSettings.parent = null;
            else WillowTerrainSettings.parent = GameObject.Find(lines[3].Replace("\r", "")).transform;

            Enum.TryParse(lines[4], out WillowTerrainSettings.placementType);

            int layerAmount = Convert.ToInt32(lines[5]);
            WillowTerrainSettings.layersName.Clear();
            WillowTerrainSettings.layersName = new List<string>(layerAmount);
            WillowTerrainSettings.layersState.Clear();
            WillowTerrainSettings.layersState = new List<bool>(layerAmount);
            for (int layer = 0; layer < layerAmount * 2; layer += 2)
            {
                WillowTerrainSettings.layersName.Add(lines[layer + 6]);
                WillowTerrainSettings.layersState.Add(Convert.ToBoolean(lines[layer + 7]));
            }

            WillowTerrainSettings.spawnableObjects.Clear();
            int spawnablesAmount = Convert.ToInt32(lines[5 + layerAmount * 2 + 1]);
            WillowTerrainSettings.spawnableObjects = new List<SpawnableObject>(spawnablesAmount);
            int line;
            for (line = 6 + layerAmount * 2 + 1; line < 6 + layerAmount * 2 + 1 + spawnablesAmount * 33; line += 33)
            {
                SpawnableObject obj = new SpawnableObject();
                if (lines[line] == "null")
                    obj.Object = null;
                else
                    obj.Object = AssetDatabase.LoadAssetAtPath(WillowTerrainSettings.PrefabsPath + $"{lines[line].Replace("\r", "")}.prefab", typeof(GameObject)) as GameObject;

                obj.Spawn = Convert.ToBoolean(lines[line + 1].Replace("\r", "").Replace("\n", ""));
                obj.SpawnChance = Convert.ToInt32(lines[line + 2]);
                obj.CustomParent = Convert.ToBoolean(lines[line + 3]);

                if (lines[line + 4].Replace("\r", "") == "null") obj.Parent = null;
                else obj.Parent = GameObject.Find(lines[line + 4].Replace("\r", "")).transform;

                obj.CenterObject = Convert.ToBoolean(lines[line + 5]);

                obj.RotationType = ParseEnum<RotationType>(lines[line + 6]);
                obj.RotationAxis = ParseEnum<Axis>(lines[line + 7]);
                //Enum.TryParse(lines[line + 6], out obj.RotationType);
                //Enum.TryParse(lines[line + 7], out obj.RotationAxis);


                obj.CustomEulersRotation = ParseVector(lines[line + 8]);
                obj.LerpValue = lines[line + 9].ToFloat();
                obj.MinLerpValue = lines[line + 10].ToFloat();
                obj.MaxLerpValue = lines[line + 11].ToFloat();
                obj.MultiRotationAxis = Convert.ToBoolean(lines[line + 12]);
                obj.RandomizeLerpValue = Convert.ToBoolean(lines[line + 13]);
                obj.RandomMinRotation = ParseVector(lines[line + 14]);
                obj.RandomMaxRotation = ParseVector(lines[line + 15]);

                obj.ModifyColor = Convert.ToBoolean(lines[line + 16]);
                obj.ColorModPercentage = lines[line + 17].ToFloat();

                obj.ModifyPosition = Convert.ToBoolean(lines[line + 18]);

                
                obj.PositionAddition = ParseVector(lines[line + 19]);

                obj.RenameObject = Convert.ToBoolean(lines[line + 20]);
                obj.NewObjectName = lines[line + 21];

                Enum.TryParse(lines[line + 22], out obj.ScaleType);
                Enum.TryParse(lines[line + 23], out obj.ScaleAxis);
                obj.ModifyScale = Convert.ToBoolean(lines[line + 24]);

                
                obj.CustomScale = ParseVector(lines[line + 25]);

                obj.ScaleMinSeparated = ParseVector(lines[line + 26]);

                obj.ScaleMin = lines[line + 27].ToFloat();


                obj.ScaleMaxSeparated = ParseVector(lines[line + 28]);

                obj.ScaleMax = lines[line + 29].ToFloat();

                obj.SeparateScaleAxis = Convert.ToBoolean(lines[line + 30]);

                obj.Layer = lines[line + 31];
                obj.LayerIndex = Convert.ToInt32(lines[line + 32]);

                obj.RotationEulerAddition = ParseVector(lines[line + 33]);

                WillowTerrainSettings.spawnableObjects.Add(obj);
            }

            WillowTerrainSettings.spawnedObjects.Clear();
            line++;
            WillowTerrainSettings.spawnedObjects = new List<GameObject>(Convert.ToInt32(lines[line].Replace("\r", "").Replace("\n", "")));
            int l;
            for (l = line + 1; l < WillowTerrainSettings.spawnedObjects.Capacity + line + 1; l++)
            {
                var g = GameObject.Find(lines[l]);
                WillowTerrainSettings.spawnedObjects.Add(g);
            }

            WillowTerrainSettings.indexObjects = Convert.ToBoolean(lines[l]);
            WillowTerrainSettings.indexFormat = lines[l + 1];
            WillowTerrainSettings.spawnedIndecies = Convert.ToInt64(lines[l + 2]);

            WillowTerrainSettings.eraseSmoothness = Convert.ToInt32(lines[l + 3]);

            WillowTerrainSettings.exchangeColor = Convert.ToBoolean(lines[l + 4]);
            WillowTerrainSettings.exchangeParent = Convert.ToBoolean(lines[l + 5]);
            WillowTerrainSettings.exchangePosition = Convert.ToBoolean(lines[l + 6]);
            WillowTerrainSettings.exchangeRotation = Convert.ToBoolean(lines[l + 7]);
            WillowTerrainSettings.exchangeScale = Convert.ToBoolean(lines[l + 8]);
            WillowTerrainSettings.exchangeSmoothness = Convert.ToInt32(lines[l + 9]);
        }
    }
    private static Vector3 ParseVector(string input)
    {
        List<string> s = input.Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
        for (int i = 0; i < 3; i++) s[i] = s[i].Replace(".", ",");
        return new Vector3((float)Convert.ToDouble(s[0]), (float)Convert.ToDouble(s[1]), (float)Convert.ToDouble(s[2]));
    }
    private static T ParseEnum<T>(string input) where T : struct
    {
        Enum.TryParse(input, out T t);
        return t;
    }
}
