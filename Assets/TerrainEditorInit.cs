using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditorInit : Editor
{
    public static TerrainSettings terrain;

    private void OnValidate()
    {
        terrain = (TerrainSettings)target;
    }
}
