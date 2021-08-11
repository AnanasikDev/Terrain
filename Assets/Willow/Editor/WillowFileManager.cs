using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
public class WillowFileManager : Editor
{
    static string path = "./file.txt";
    static string prefabsFolder = "Assets/Willow/Example/Prefabs/";
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
            output.AppendLine(obj.spawnableObject == null ? "null" : obj.spawnableObject.name);
            output.AppendLine(obj.spawn.ToString());
            output.AppendLine(obj.spawnChance.ToString());
            output.AppendLine(obj.customParent.ToString());
            output.AppendLine(obj.parent == null ? "null" : obj.parent.name);
            output.AppendLine(obj.centerObject.ToString());
            //6

            output.AppendLine(obj.rotationType.ToString());
            output.AppendLine(obj.rotationAxis.ToString());
            output.AppendLine(obj.customEulersRotation.ToString());
            output.AppendLine(obj.lerpValue.ToString());
            output.AppendLine(obj.minLerpValue.ToString());
            output.AppendLine(obj.maxLerpValue.ToString());

            output.AppendLine(obj.multiRotationAxis.ToString());
            output.AppendLine(obj.randomizeLerpValue.ToString());
            output.AppendLine(obj.randomMinRotation.ToString());
            output.AppendLine(obj.randomMaxRotation.ToString());
            //16

            output.AppendLine(obj.modColor.ToString());
            output.AppendLine(obj.colorModPercentage.ToString());

            output.AppendLine(obj.modifyPosition.ToString());
            output.AppendLine(obj.positionAddition.ToString());

            output.AppendLine(obj.renameObject.ToString());
            output.AppendLine(obj.newObjectName.Replace("\r", ""));
            //22

            output.AppendLine(obj.scaleType.ToString());
            output.AppendLine(obj.scaleAxis.ToString());
            output.AppendLine(obj.modScale.ToString());
            output.AppendLine(obj.customScale.ToString());
            output.AppendLine(obj.scaleMinSeparated.ToString());
            output.AppendLine(obj.scaleMin.ToString());
            output.AppendLine(obj.scaleMaxSeparated.ToString());
            output.AppendLine(obj.scaleMax.ToString());
            output.AppendLine(obj.separateScaleAxis.ToString());
            //31
            output.AppendLine(obj.layer.Replace("\r", ""));
            output.AppendLine(obj.layerIndex.ToString());
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
                    obj.spawnableObject = null;
                else
                    obj.spawnableObject = AssetDatabase.LoadAssetAtPath(prefabsFolder + $"{lines[line].Replace("\r", "")}.prefab", typeof(GameObject)) as GameObject;

                obj.spawn = Convert.ToBoolean(lines[line + 1].Replace("\r", "").Replace("\n", ""));
                obj.spawnChance = Convert.ToInt32(lines[line + 2]);
                obj.customParent = Convert.ToBoolean(lines[line + 3]);

                if (lines[line + 4].Replace("\r", "") == "null") obj.parent = null;
                else obj.parent = GameObject.Find(lines[line + 4].Replace("\r", "")).transform;

                obj.centerObject = Convert.ToBoolean(lines[line + 5]);

                Enum.TryParse(lines[line + 6], out obj.rotationType);
                Enum.TryParse(lines[line + 7], out obj.rotationAxis);

                var customEulerRotation = lines[line + 8].Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
                for (int i = 0; i < 3; i++) customEulerRotation[i] = customEulerRotation[i].Replace(".", ",");
                obj.customEulersRotation = new Vector3((float)Convert.ToDouble(customEulerRotation[0]), (float)Convert.ToDouble(customEulerRotation[1]), (float)Convert.ToDouble(customEulerRotation[2]));

                obj.lerpValue = (float)Convert.ToDouble(lines[line + 9]);
                obj.minLerpValue = (float)Convert.ToDouble(lines[line + 10]);
                obj.maxLerpValue = (float)Convert.ToDouble(lines[line + 11]);
                obj.multiRotationAxis = Convert.ToBoolean(lines[line + 12]);
                obj.randomizeLerpValue = Convert.ToBoolean(lines[line + 13]);

                var randomMinRotation = lines[line + 14].Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
                for (int i = 0; i < 3; i++) randomMinRotation[i] = randomMinRotation[i].Replace(".", ",");
                obj.randomMinRotation = new Vector3((float)Convert.ToDouble(randomMinRotation[0]), (float)Convert.ToDouble(randomMinRotation[1]), (float)Convert.ToDouble(randomMinRotation[2]));

                var randomMaxRotation = lines[line + 15].Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
                for (int i = 0; i < 3; i++) randomMaxRotation[i] = randomMaxRotation[i].Replace(".", ",");
                obj.randomMaxRotation = new Vector3((float)Convert.ToDouble(randomMaxRotation[0]), (float)Convert.ToDouble(randomMaxRotation[1]), (float)Convert.ToDouble(randomMaxRotation[2]));

                obj.modColor = Convert.ToBoolean(lines[line + 16]);
                obj.colorModPercentage = (float)Convert.ToDouble(lines[line + 17]);

                obj.modifyPosition = Convert.ToBoolean(lines[line + 18]);

                var positionAddition = lines[line + 19].Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
                for (int i = 0; i < 3; i++) positionAddition[i] = positionAddition[i].Replace(".", ",");
                obj.positionAddition = new Vector3((float)Convert.ToDouble(positionAddition[0]), (float)Convert.ToDouble(positionAddition[1]), (float)Convert.ToDouble(positionAddition[2]));

                obj.renameObject = Convert.ToBoolean(lines[line + 20]);
                obj.newObjectName = lines[line + 21];

                Enum.TryParse(lines[line + 22], out obj.scaleType);
                Enum.TryParse(lines[line + 23], out obj.scaleAxis);
                obj.modScale = Convert.ToBoolean(lines[line + 24]);

                var customScale = lines[line + 25].Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
                for (int i = 0; i < 3; i++) customScale[i] = customScale[i].Replace(".", ",");
                obj.customScale = new Vector3((float)Convert.ToDouble(customScale[0]), (float)Convert.ToDouble(customScale[1]), (float)Convert.ToDouble(customScale[2]));

                var scaleMinSeparated = lines[line + 26].Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
                for (int i = 0; i < 3; i++) scaleMinSeparated[i] = scaleMinSeparated[i].Replace(".", ",");
                obj.scaleMinSeparated = new Vector3((float)Convert.ToDouble(scaleMinSeparated[0]), (float)Convert.ToDouble(scaleMinSeparated[1]), (float)Convert.ToDouble(scaleMinSeparated[2]));

                obj.scaleMin = (float)Convert.ToDouble(lines[line + 27]);

                var scaleMaxSeparated = lines[line + 28].Replace(")", "").Replace("(", "").Replace(" ", "").Split(',').ToList();
                for (int i = 0; i < 3; i++) scaleMaxSeparated[i] = scaleMaxSeparated[i].Replace(".", ",");
                obj.scaleMaxSeparated = new Vector3((float)Convert.ToDouble(scaleMaxSeparated[0]), (float)Convert.ToDouble(scaleMaxSeparated[1]), (float)Convert.ToDouble(scaleMaxSeparated[2]));

                obj.scaleMax = (float)Convert.ToDouble(lines[line + 29]);

                obj.separateScaleAxis = Convert.ToBoolean(lines[line + 30]);

                obj.layer = lines[line + 31];
                obj.layerIndex = Convert.ToInt32(lines[line + 32]);

                WillowTerrainSettings.spawnableObjects.Add(obj);
            }

            WillowTerrainSettings.spawnedObjects.Clear();
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
            WillowTerrainSettings.exchangeSmoothness = Convert.ToInt32(lines[l + 9]); //
        }
    }
}
