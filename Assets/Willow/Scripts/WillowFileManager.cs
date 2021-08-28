using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using static WillowUtils;
using static WillowDebug;
public static class WillowFileManager
{
    public static string path = "./WillowSaveFile.txt";
    public static void Write()
    {
        StringBuilder output = new StringBuilder();

        output.AppendLine(WillowTerrainSettings.PrefabsPath.RemoveSlashN().RemoveSlashR());

        output.AppendLine(WillowTerrainSettings.IsActive.ToString());
        output.AppendLine(WillowTerrainSettings.BrushDensity.ToString());
        output.AppendLine(WillowTerrainSettings.BrushSize.ToString());

        output.AppendLine(WillowTerrainSettings.BaseParent == null ? "null" : WillowTerrainSettings.BaseParent.name);
        output.AppendLine(WillowTerrainSettings.PlacementType.ToString());

        output.AppendLine(WillowTerrainSettings.LayersName.Count.ToString());
        for (int layer = 0; layer < WillowTerrainSettings.LayersName.Count; layer++)
        {
            output.AppendLine(WillowTerrainSettings.LayersName[layer].RemoveSlashN().RemoveSlashR());
            output.AppendLine(WillowTerrainSettings.LayersState[layer].ToString());
        }

        output.AppendLine(WillowTerrainSettings.SpawnableObjects.Count.ToString());
        foreach (WillowSpawnableObject obj in WillowTerrainSettings.SpawnableObjects)
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
            output.AppendLine(obj.NewObjectName.RemoveSlashR());
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
            output.AppendLine(obj.Layer.RemoveSlashR());
            output.AppendLine(obj.LayerIndex.ToString());

            output.AppendLine(obj.RotationEulerAddition.ToString());
        }

        output.AppendLine(WillowTerrainSettings.SpawnedObjects.Where(o => o != null && o.hideFlags == HideFlags.None).ToArray().Length .ToString());
        foreach (GameObject spawnedObj in WillowTerrainSettings.SpawnedObjects.Where(o => o != null && o.hideFlags == HideFlags.None))
        {
            output.AppendLine(spawnedObj.name.RemoveSlashR());
        }

        output.AppendLine(WillowTerrainSettings.IndexObjects.ToString());
        output.AppendLine(WillowTerrainSettings.IndexFormat.RemoveSlashR());
        output.AppendLine(WillowTerrainSettings.SpawnedIndecies.ToString());

        output.AppendLine(WillowTerrainSettings.EraseSmoothness.ToString());

        output.AppendLine(WillowTerrainSettings.ExchangeColor.ToString());
        output.AppendLine(WillowTerrainSettings.ExchangeParent.ToString());
        output.AppendLine(WillowTerrainSettings.ExchangePosition.ToString());
        output.AppendLine(WillowTerrainSettings.ExchangeRotation.ToString());
        output.AppendLine(WillowTerrainSettings.ExchangeScale.ToString());
        output.AppendLine(WillowTerrainSettings.ExchangeSmoothness.ToString());

        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.Write(output);
        }

        Log("Templates saved!", Green);
    }
    public static void Read()
    {
        string[] lines;

        using (StreamReader reader = new StreamReader(path))
        {
            lines = reader.ReadToEnd().Split('\n');

            WillowTerrainSettings.PrefabsPath = lines[0].RemoveSlashR().RemoveSlashN();

            WillowTerrainSettings.IsActive = Convert.ToBoolean(lines[1]);
            WillowTerrainSettings.BrushDensity = Convert.ToInt32(lines[2]);
            WillowTerrainSettings.BrushSize = Convert.ToSingle(lines[3]);

            if (lines[4].RemoveSlashR() == "null") WillowTerrainSettings.BaseParent = null;
            else WillowTerrainSettings.BaseParent = GameObject.Find(lines[4].RemoveSlashR()).transform;

            Enum.TryParse(lines[5], out WillowTerrainSettings.PlacementType);

            int layerAmount = Convert.ToInt32(lines[6]);
            WillowTerrainSettings.LayersName.Clear();
            WillowTerrainSettings.LayersName = new List<string>(layerAmount);
            WillowTerrainSettings.LayersState.Clear();
            WillowTerrainSettings.LayersState = new List<bool>(layerAmount);

            for (int layer = 0; layer < layerAmount * 2; layer += 2)
            {
                WillowTerrainSettings.LayersName.Add(lines[layer + 7]);
                WillowTerrainSettings.LayersState.Add(Convert.ToBoolean(lines[layer + 8]));
            }

            WillowTerrainSettings.SpawnableObjects.Clear();
            int spawnablesAmount = Convert.ToInt32(lines[6 + layerAmount * 2 + 1]);
            WillowTerrainSettings.SpawnableObjects = new List<WillowSpawnableObject>(spawnablesAmount);
            int line;
            for (line = 7 + layerAmount * 2 + 1; line < 7 + layerAmount * 2 + 1 + spawnablesAmount * 33; line += 33)
            {
                WillowSpawnableObject obj = new WillowSpawnableObject();
                if (lines[line] == "null")
                    obj.Object = null;
                else
                    obj.Object = LoadPrefab(lines[line]);

                obj.Spawn = Convert.ToBoolean(lines[line + 1].RemoveSlashR().RemoveSlashN());
                obj.SpawnChance = Convert.ToInt32(lines[line + 2]);
                obj.CustomParent = Convert.ToBoolean(lines[line + 3]);

                if (lines[line + 4].RemoveSlashR() == "null") obj.Parent = null;
                else obj.Parent = GameObject.Find(lines[line + 4].RemoveSlashR()).transform;

                obj.CenterObject = Convert.ToBoolean(lines[line + 5]);

                obj.RotationType = ParseEnum<RotationType>(lines[line + 6]);
                obj.RotationAxis = ParseEnum<Axis>(lines[line + 7]);

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

                obj.ScaleType = ParseEnum<ScaleType>(lines[line + 22]);
                obj.ScaleAxis = ParseEnum<Axis>(lines[line + 23]);

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

                WillowTerrainSettings.SpawnableObjects.Add(obj);
            }

            WillowTerrainSettings.SpawnedObjects.Clear();
            line++;
            WillowTerrainSettings.SpawnedObjects = new List<GameObject>(Convert.ToInt32(lines[line].RemoveSlashN()));
            int l;
            for (l = line + 1; l < WillowTerrainSettings.SpawnedObjects.Capacity + line + 1; l++)
            {
                var g = GameObject.Find(lines[l]);
                g.GetComponent<WillowSpawnedObject>().SpawnableObject = 
                    g.GetComponent<WillowSpawnedObject>().SpawnableObject.GetOriginal();
                WillowTerrainSettings.SpawnedObjects.Add(g);
            }

            WillowTerrainSettings.IndexObjects = Convert.ToBoolean(lines[l]);
            WillowTerrainSettings.IndexFormat = lines[l + 1];
            WillowTerrainSettings.SpawnedIndecies = Convert.ToInt64(lines[l + 2]);

            WillowTerrainSettings.EraseSmoothness = Convert.ToInt32(lines[l + 3]);

            WillowTerrainSettings.ExchangeColor = Convert.ToBoolean(lines[l + 4]);
            WillowTerrainSettings.ExchangeParent = Convert.ToBoolean(lines[l + 5]);
            WillowTerrainSettings.ExchangePosition = Convert.ToBoolean(lines[l + 6]);
            WillowTerrainSettings.ExchangeRotation = Convert.ToBoolean(lines[l + 7]);
            WillowTerrainSettings.ExchangeScale = Convert.ToBoolean(lines[l + 8]);
            WillowTerrainSettings.ExchangeSmoothness = Convert.ToInt32(lines[l + 9]);
        }
    }
    public static bool TryRead()
    {
        try
        {
            Read();
            return true;
        }
        catch (NullReferenceException)
        {
            Log("Impossible to read.", Yellow, Debug.LogWarning);
        }
        return false;
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

    private static GameObject LoadPrefab(string name)
    {
        return AssetDatabase.LoadAssetAtPath(WillowTerrainSettings.PrefabsPath + $"{name.RemoveSlashR()}.prefab", typeof(GameObject)) as GameObject;
    }
}
