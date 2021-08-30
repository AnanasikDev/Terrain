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
    
    private static StringBuilder WriterStringBuilder = new StringBuilder();
    private static int ReadLineIndex = 0;
    private static string[] Lines;

    public static void Write()
    {
        StringBuilder output = new StringBuilder();

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
            //output.AppendLine(obj.Object == null ? "null" : obj.Object.name);
            output.AppendLine(obj.Object == null ? "null" : AssetDatabase.GetAssetPath(obj.Object));
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
        ReadFile();
        using (StreamReader reader = new StreamReader(path))
        {
            WillowTerrainSettings.IsActive = Convert.ToBoolean(Pull());
            WillowTerrainSettings.BrushDensity = Convert.ToInt32(Pull());
            WillowTerrainSettings.BrushSize = Convert.ToSingle(Pull());

            string parent = Pull().RemoveSlashR();
            if (parent == "null") WillowTerrainSettings.BaseParent = null;
            else WillowTerrainSettings.BaseParent = GameObject.Find(parent).transform;

            Enum.TryParse(Pull(), out WillowTerrainSettings.PlacementType);

            int layerAmount = Convert.ToInt32(Pull());
            WillowTerrainSettings.LayersName.Clear();
            WillowTerrainSettings.LayersName = new List<string>(layerAmount);
            WillowTerrainSettings.LayersState.Clear();
            WillowTerrainSettings.LayersState = new List<bool>(layerAmount);

            for (int layer = 0; layer < layerAmount; layer++)
            {
                WillowTerrainSettings.LayersName.Add(Pull());
                WillowTerrainSettings.LayersState.Add(Convert.ToBoolean(Pull()));
            }

            WillowTerrainSettings.SpawnableObjects.Clear();
            int spawnablesAmount = Convert.ToInt32(Pull());
            WillowTerrainSettings.SpawnableObjects = new List<WillowSpawnableObject>(spawnablesAmount);

            for (int i = 0; i < WillowTerrainSettings.SpawnableObjects.Capacity; i++)
            {
                WillowSpawnableObject spawnable = new WillowSpawnableObject();

                string spawnObject = Pull();
                if (spawnObject == "null")
                    spawnable.Object = null;
                else
                    spawnable.Object = LoadPrefab(spawnObject);

                spawnable.Spawn = Convert.ToBoolean(Pull());
                spawnable.SpawnChance = Convert.ToInt32(Pull());
                spawnable.CustomParent = Convert.ToBoolean(Pull());

                string customParent = Pull().RemoveSlashR();
                if (customParent == "null") spawnable.Parent = null;
                else spawnable.Parent = GameObject.Find(customParent).transform;

                spawnable.CenterObject = Convert.ToBoolean(Pull());

                spawnable.RotationType = ParseEnum<RotationType>(Pull());
                spawnable.RotationAxis = ParseEnum<Axis>(Pull());

                spawnable.CustomEulersRotation = ParseVector(Pull());
                spawnable.LerpValue = Pull().ToFloat();
                spawnable.MinLerpValue = Pull().ToFloat();
                spawnable.MaxLerpValue = Pull().ToFloat();
                spawnable.MultiRotationAxis = Convert.ToBoolean(Pull());
                spawnable.RandomizeLerpValue = Convert.ToBoolean(Pull());
                spawnable.RandomMinRotation = ParseVector(Pull());
                spawnable.RandomMaxRotation = ParseVector(Pull());

                spawnable.ModifyColor = Convert.ToBoolean(Pull());
                spawnable.ColorModPercentage = Pull().ToFloat();

                spawnable.ModifyPosition = Convert.ToBoolean(Pull());
                
                spawnable.PositionAddition = ParseVector(Pull());

                spawnable.RenameObject = Convert.ToBoolean(Pull());
                spawnable.NewObjectName = Pull();

                spawnable.ScaleType = ParseEnum<ScaleType>(Pull());
                spawnable.ScaleAxis = ParseEnum<Axis>(Pull());

                spawnable.ModifyScale = Convert.ToBoolean(Pull());
                
                spawnable.CustomScale = ParseVector(Pull());

                spawnable.ScaleMinSeparated = ParseVector(Pull());

                spawnable.ScaleMin = Pull().ToFloat();


                spawnable.ScaleMaxSeparated = ParseVector(Pull());

                spawnable.ScaleMax = Pull().ToFloat();

                spawnable.SeparateScaleAxis = Convert.ToBoolean(Pull());

                spawnable.Layer = Pull();
                spawnable.LayerIndex = Convert.ToInt32(Pull());

                spawnable.RotationEulerAddition = ParseVector(Pull());

                WillowTerrainSettings.SpawnableObjects.Add(spawnable);
            }

            WillowTerrainSettings.SpawnedObjects.Clear();
            WillowTerrainSettings.SpawnedObjects = new List<GameObject>(Convert.ToInt32(Pull().RemoveSlashN()));
            for (int l = 0; l < WillowTerrainSettings.SpawnedObjects.Capacity; l++)
            {
                var g = GameObject.Find(Pull());
                g.GetComponent<WillowSpawnedObject>().SpawnableObject = 
                    g.GetComponent<WillowSpawnedObject>().SpawnableObject.GetOriginal();
                WillowTerrainSettings.SpawnedObjects.Add(g);
            }

            WillowTerrainSettings.IndexObjects = Convert.ToBoolean(Pull());
            WillowTerrainSettings.IndexFormat = Pull();
            WillowTerrainSettings.SpawnedIndecies = Convert.ToInt64(Pull());

            WillowTerrainSettings.EraseSmoothness = Convert.ToInt32(Pull());

            WillowTerrainSettings.ExchangeColor = Convert.ToBoolean(Pull());
            WillowTerrainSettings.ExchangeParent = Convert.ToBoolean(Pull());
            WillowTerrainSettings.ExchangePosition = Convert.ToBoolean(Pull());
            WillowTerrainSettings.ExchangeRotation = Convert.ToBoolean(Pull());
            WillowTerrainSettings.ExchangeScale = Convert.ToBoolean(Pull());
            WillowTerrainSettings.ExchangeSmoothness = Convert.ToInt32(Pull());
        }

        Log("Templates read!", Green);
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
            Log("There is an error occured while reading. Impossible to read.", Yellow, Debug.LogWarning);
        }
        return false;
    }

    private static void Push(object obj)
    {
        WriterStringBuilder.AppendLine(obj.ToString());
    }
    private static void ReadFile()
    {
        ReadLineIndex = 0;
        using (StreamReader reader = new StreamReader(path))
        {
            Lines = reader.ReadToEnd().Split('\n');
        }
    }
    private static string Pull()
    {
        //Debug.Log(ReadLineIndex);
        ReadLineIndex += 1;
        return Lines[ReadLineIndex - 1];
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
        return AssetDatabase.LoadAssetAtPath($"{name.RemoveSlashR()}", typeof(GameObject)) as GameObject;
    }
}
