/*using UnityEngine;
using UnityEditor;
//[CustomEditor(typeof(SpawnableObj))]
public class SpawnableObjEditor : Editor
{
    Transform Transform;
    private void Awake()
    {
        Transform = (Transform)target;
        EditorApplication.update += Update;
    }
    private void OnDestroy()
    {
        EditorApplication.update -= Update;
    }
    void Update()
    {
        if (!TerrainSettings.terrainSettings.selectOnlyChildren) return;
        GameObject[] selectedObjects = Selection.gameObjects;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (Selection.gameObjects[i].transform.IsChildOf(Transform))
            {
                selectedObjects[i] = Transform.gameObject;
            }
        }
        Selection.objects = selectedObjects;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("BTN"))
        {
        }
    }
}
*/