using UnityEditor;
[CustomEditor(typeof(TerrainSettings))]
public class TerrainEditorInit : Editor
{
    private void OnDisable()
    {
        TerrainSettings.validated = false;
    }
    private void OnEnable()
    {
        TerrainSettings.instance = (TerrainSettings)target;
        TerrainSettings.validated = true;
    }
}
