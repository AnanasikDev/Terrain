using UnityEditor;
[CustomEditor(typeof(WillowTerrainSettings))]
public class WillowTerrainEditorInit : Editor
{
    private void OnDisable()
    {
        WillowTerrainSettings.validated = false;
    }
    private void OnEnable()
    {
        WillowTerrainSettings.instance = (WillowTerrainSettings)target;
        WillowTerrainSettings.validated = true;
    }
}
