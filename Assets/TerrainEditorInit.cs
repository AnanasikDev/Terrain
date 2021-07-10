using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditorInit : Editor
{
    private void OnEnable()
    {
        TerrainSettings.instance = (TerrainSettings)target;
        TerrainSettings.validated = true;
    }
}
