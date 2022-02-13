#if UNITY_EDITOR
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
        PrepareForWrite();

        Push(WillowTerrainSettings.IsActive);
        Push(WillowTerrainSettings.BrushDensity);
        Push(WillowTerrainSettings.BrushMode);
        Push(WillowTerrainSettings.RandomizeBrushDensity);
        Push(WillowTerrainSettings.BrushDensityRandomizationModificator);
        Push(WillowTerrainSettings.BrushSize);

        Push(WillowTerrainSettings.DebugMode);
        Push(WillowTerrainSettings.AutoSave);
        Push(WillowTerrainSettings.SafeMode);

        Push(WillowTerrainSettings.BaseParent == null ? "null" : WillowTerrainSettings.BaseParent.name);
        Push(WillowTerrainSettings.PlacementType);

        Push(WillowTerrainSettings.LayersName.Count);
        for (int layer = 0; layer < WillowTerrainSettings.LayersName.Count; layer++)
        {
            Push(WillowTerrainSettings.LayersName[layer].RemoveSlashN().RemoveSlashR());
            Push(WillowTerrainSettings.LayersState[layer]);
        }

        Push(WillowTerrainSettings.SpawnableObjects.Count);
        foreach (WillowSpawnableObject obj in WillowTerrainSettings.SpawnableObjects)
        {
            Push(obj.Object == null ? "null" : AssetDatabase.GetAssetPath(obj.Object));
            Push(obj.Spawn);
            Push(obj.SpawnChance);
            Push(obj.CustomParent);
            Push(obj.Parent == null ? "null" : obj.Parent.name);
            Push(obj.CenterObject);

            Push(obj.RotationType);
            Push(obj.RotationAxis);
            Push(obj.CustomEulersRotation);
            Push(obj.LerpValue);
            Push(obj.MinLerpValue);
            Push(obj.MaxLerpValue);

            Push(obj.MultiRotationAxis);
            Push(obj.RandomizeLerpValue);
            Push(obj.RandomMinRotation);
            Push(obj.RandomMaxRotation);

            Push(obj.ModifyColor);
            Push(obj.ColorModPercentage);

            Push(obj.ModifyPosition);
            Push(obj.PositionAddition);

            Push(obj.RenameObject);
            Push(obj.NewObjectName.RemoveSlashR());

            Push(obj.ScaleType);
            Push(obj.ScaleAxis);
            Push(obj.ModifyScale);
            Push(obj.CustomScale);
            Push(obj.ScaleMinSeparated);
            Push(obj.ScaleMin);
            Push(obj.ScaleMaxSeparated);
            Push(obj.ScaleMax);
            Push(obj.SeparateScaleAxis);

            Push(obj.Layer.RemoveSlashR());
            Push(obj.LayerIndex);

            Push(obj.RotationEulerAddition);

            Push(obj.AvoidObstacles);
            Push(obj.ObstaclesTagType);
            Push(obj.ObstaclesAvoidanceAction);
            Push(obj.AvoidanceRadius);
            Push(obj.AvoidanceHeight);
            Push(obj.AvoidanceLocalShift);
            Push(obj.AvoidanceWorldShift);
        }

        /*Push(WillowTerrainSettings.SpawnedObjects.Where(o => o != null && o.hideFlags == HideFlags.None).ToArray().Length );
        foreach (GameObject spawnedObj in WillowTerrainSettings.SpawnedObjects.Where(o => o != null && o.hideFlags == HideFlags.None))
        {
            Push(spawnedObj.name.RemoveSlashR());
        }*/

        Push(WillowTerrainSettings.IndexObjects);
        Push(WillowTerrainSettings.IndexFormat.RemoveSlashR());
        Push(WillowTerrainSettings.SpawnedIndecies);

        Push(WillowTerrainSettings.EraseSmoothness);
        Push(WillowTerrainSettings.ExchangeColor);
        Push(WillowTerrainSettings.ExchangeParent);
        Push(WillowTerrainSettings.ExchangePosition);
        Push(WillowTerrainSettings.ExchangeRotation);
        Push(WillowTerrainSettings.ExchangeScale);
        Push(WillowTerrainSettings.ExchangeSmoothness);

        Push(WillowTerrainSettings.AvoidAutomatically);

        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.Write(WriterStringBuilder);
        }

        Log("Templates successfully saved!", Green);
    }
    public static void Read()
    {
        PrepareForRead();

        using (StreamReader reader = new StreamReader(path))
        {
            WillowTerrainSettings.IsActive = Pull().ToBool();
            WillowTerrainSettings.BrushDensity = Pull().ToInt();
            WillowTerrainSettings.BrushMode = ParseEnum<BrushMode>(Pull());
            WillowTerrainSettings.RandomizeBrushDensity = Pull().ToBool();
            WillowTerrainSettings.BrushDensityRandomizationModificator = Pull().ToFloat();
            WillowTerrainSettings.BrushSize = Convert.ToSingle(Pull());

            WillowTerrainSettings.DebugMode = Pull().ToBool();
            WillowTerrainSettings.AutoSave = Pull().ToBool();
            WillowTerrainSettings.SafeMode = Pull().ToBool();

            string parent = Pull().RemoveSlashR();
            if (parent == "null") WillowTerrainSettings.BaseParent = null;
            else WillowTerrainSettings.BaseParent = GameObject.Find(parent).transform;

            Enum.TryParse(Pull(), out WillowTerrainSettings.PlacementType);

            int layerAmount = Pull().ToInt();
            WillowTerrainSettings.LayersName.Clear();
            WillowTerrainSettings.LayersName = new List<string>(layerAmount);
            WillowTerrainSettings.LayersState.Clear();
            WillowTerrainSettings.LayersState = new List<bool>(layerAmount);

            for (int layer = 0; layer < layerAmount; layer++)
            {
                WillowTerrainSettings.LayersName.Add(Pull());
                WillowTerrainSettings.LayersState.Add(Pull().ToBool());
            }

            WillowTerrainSettings.SpawnableObjects.Clear();
            int spawnablesAmount = Pull().ToInt();
            WillowTerrainSettings.SpawnableObjects = new List<WillowSpawnableObject>(spawnablesAmount);

            for (int i = 0; i < WillowTerrainSettings.SpawnableObjects.Capacity; i++)
            {
                WillowSpawnableObject spawnable = new WillowSpawnableObject();

                string spawnObject = Pull();
                if (spawnObject == "null")
                    spawnable.Object = null;
                else
                    spawnable.Object = LoadPrefab(spawnObject);

                spawnable.Spawn = Pull().ToBool();
                spawnable.SpawnChance = Pull().ToInt();
                spawnable.CustomParent = Pull().ToBool();

                string customParent = Pull().RemoveSlashR();
                if (customParent == "null") spawnable.Parent = null;
                else spawnable.Parent = GameObject.Find(customParent).transform;

                spawnable.CenterObject = Pull().ToBool();

                spawnable.RotationType = ParseEnum<RotationType>(Pull());
                spawnable.RotationAxis = ParseEnum<Axis>(Pull());

                spawnable.CustomEulersRotation = ParseVector(Pull());
                spawnable.LerpValue = Pull().ToFloat();
                spawnable.MinLerpValue = Pull().ToFloat();
                spawnable.MaxLerpValue = Pull().ToFloat();
                spawnable.MultiRotationAxis = Pull().ToBool();
                spawnable.RandomizeLerpValue = Pull().ToBool();
                spawnable.RandomMinRotation = ParseVector(Pull());
                spawnable.RandomMaxRotation = ParseVector(Pull());

                spawnable.ModifyColor = Pull().ToBool();
                spawnable.ColorModPercentage = Pull().ToFloat();

                spawnable.ModifyPosition = Pull().ToBool();
                
                spawnable.PositionAddition = ParseVector(Pull());

                spawnable.RenameObject = Pull().ToBool();
                spawnable.NewObjectName = Pull();

                spawnable.ScaleType = ParseEnum<ScaleType>(Pull());
                spawnable.ScaleAxis = ParseEnum<Axis>(Pull());

                spawnable.ModifyScale = Pull().ToBool();
                
                spawnable.CustomScale = ParseVector(Pull());

                spawnable.ScaleMinSeparated = ParseVector(Pull());

                spawnable.ScaleMin = Pull().ToFloat();


                spawnable.ScaleMaxSeparated = ParseVector(Pull());

                spawnable.ScaleMax = Pull().ToFloat();

                spawnable.SeparateScaleAxis = Pull().ToBool();

                spawnable.Layer = Pull();
                spawnable.LayerIndex = Pull().ToInt();

                spawnable.RotationEulerAddition = ParseVector(Pull());

                spawnable.AvoidObstacles = Pull().ToBool();
                spawnable.ObstaclesTagType = ParseEnum<ObstaclesTagType>(Pull());
                spawnable.ObstaclesAvoidanceAction = ParseEnum<ObstaclesAvoidanceAction>(Pull());
                spawnable.AvoidanceRadius = Pull().ToFloat();
                spawnable.AvoidanceHeight = Pull().ToFloat();
                spawnable.AvoidanceLocalShift = ParseVector(Pull());
                spawnable.AvoidanceWorldShift = ParseVector(Pull());

                WillowTerrainSettings.SpawnableObjects.Add(spawnable);
            }

            WillowTerrainSettings.SpawnedObjects.Clear();
            WillowTerrainSettings.SpawnedObjects = GameObject.FindObjectsOfType<WillowSpawnedObject>(true).Select(x => x.gameObject).Where(x => x.gameObject).ToList(); ; // Resources.FindObjectsOfTypeAll<WillowSpawnedObject>().Select(x => x.gameObject).ToList(); // new List<GameObject>(Convert.ToInt32(Pull().RemoveSlashN()));

            /*foreach (GameObject g in WillowTerrainSettings.SpawnedObjects)
            {
                GameObject.DestroyImmediate(g);
            }*/

            //GameObject.FindObjectsOfType
            foreach (GameObject spawned in WillowTerrainSettings.SpawnedObjects)
            {
                //GameObject spawned = GameObject.Find(Pull());
                // GameObject spawned = 
                //var fooGroup = Resources.FindObjectsOfTypeAll<WillowSpawnedObject>();
                //Debug.Log(spawned);
                spawned.GetComponent<WillowSpawnedObject>().SpawnableObject =
                    spawned.GetComponent<WillowSpawnedObject>().SpawnableObject.GetOriginal();
            }

            WillowTerrainSettings.IndexObjects = Pull().ToBool();
            WillowTerrainSettings.IndexFormat = Pull();
            WillowTerrainSettings.SpawnedIndecies = Convert.ToInt64(Pull());

            WillowTerrainSettings.EraseSmoothness = Pull().ToInt();

            WillowTerrainSettings.ExchangeColor = Pull().ToBool();
            WillowTerrainSettings.ExchangeParent = Pull().ToBool();
            WillowTerrainSettings.ExchangePosition = Pull().ToBool();
            WillowTerrainSettings.ExchangeRotation = Pull().ToBool();
            WillowTerrainSettings.ExchangeScale = Pull().ToBool();
            WillowTerrainSettings.ExchangeSmoothness = Pull().ToInt();

            WillowTerrainSettings.AvoidAutomatically = Pull().ToBool();
        }

        Log("Templates read!", Green);
    }
    public static bool TryRead()
    {
        /*try
        {
            Read();
            return true;
        }
        catch (NullReferenceException)
        {
            Log("There is an error occured while reading. Impossible to read.", Yellow, Debug.LogWarning);
        }
        return false;*/

        Read();
        return true;
    }

    private static void Push(object obj)
    {
        WriterStringBuilder.AppendLine(obj.ToString());
    }
    private static void PrepareForWrite()
    {
        WriterStringBuilder = new StringBuilder();
    }
    private static void PrepareForRead()
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
#endif