using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
public class FileManager : Editor
{
    static string path = "./file.txt";
    public static void Write()
    {
        StringBuilder output = new StringBuilder();

        output.AppendLine(TerrainSettings.validated.ToString());
        output.AppendLine(TerrainSettings.density.ToString());
        output.AppendLine(TerrainSettings.brushSize.ToString());
        output.AppendLine(TerrainSettings.spawnableObjects.Count.ToString());

        foreach (SpawnableObject obj in TerrainSettings.spawnableObjects)
        {
            output.AppendLine(obj.spawnableObject == null ? "null" : obj.spawnableObject.name.ToString());
            output.AppendLine(obj.spawn.ToString());
        }
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.Write(output);
        }
    }
    public static void Read()
    {
        string[] lines;

        using (StreamReader reader = new StreamReader(path)) //
        {
            lines = reader.ReadToEnd().Split('\n');
            TerrainSettings.validated = System.Convert.ToBoolean(lines[0]);
            TerrainSettings.density = System.Convert.ToInt32(lines[1]);
            TerrainSettings.brushSize = System.Convert.ToInt32(lines[2]);
            TerrainSettings.spawnableObjects.Clear();//

            TerrainSettings.spawnableObjects = new System.Collections.Generic.List<SpawnableObject>(System.Convert.ToInt32(lines[3]));
            for (int line = 4; line < lines.Length - 2; line+=2)
            {
                SpawnableObject obj = new SpawnableObject();
                if (lines[line] == "null")
                    obj.spawnableObject = null;
                else
                    obj.spawnableObject = AssetDatabase.LoadAssetAtPath($"Assets/Prefabs/{lines[line].Replace("\r", "")}.prefab", typeof(GameObject)) as GameObject;
                obj.spawn = System.Convert.ToBoolean(lines[line + 1].Replace("\r", ""));
                TerrainSettings.spawnableObjects.Add(obj);
            }
        }
    }
}
