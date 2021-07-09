using UnityEditor;
[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditorInit : Editor
{
    private void OnValidate()
    {
        TerrainSettings.instance = (TerrainSettings)target;
        TerrainSettings.validated = true;
    }
}
