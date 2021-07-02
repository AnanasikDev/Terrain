/*using UnityEditor;
using UnityEngine;

public class SpawnableObjEditor : Editor
{
    private void Awake()a
    {
        EditorApplication.update += Update;
    }
    private void OnDestroy()
    {
        EditorApplication.update -= Update;
    }

    void Update()
    {
        ReselectChildren();
    }
    void ReselectChildren()
    {
        Selection.objects = new Object[0];
    }
}*/