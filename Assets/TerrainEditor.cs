using UnityEditor;
using UnityEngine;
using System.Collections;
//[ExecuteInEditMode]
[CustomEditor(typeof(TerrainScript))]
public class TerrainEditor : Editor
{
    public TerrainScript terrain;
    public GameObject tree;
    Camera cam;

    bool centerObject = true;
    void Update()
    {
        Selection.activeTransform = terrain.transform;
    }
    private void Awake()
    {
        terrain = (TerrainScript)target;
        tree = terrain.obj;
        Debug.Log(tree);
        //objects = new GameObject[1];
        //objects[0] = Resources.Load<GameObject>("Prefabs/Cube");
        cam = SceneView.lastActiveSceneView.camera;
        EditorApplication.update += Update;
    }
    private void OnDestroy()
    {
        EditorApplication.update -= Update;
    }
    private void OnSceneGUI()
    {
        //if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                GameObject temp = Instantiate(tree, hit.point, Quaternion.identity, terrain.transform);
                if (centerObject) temp.transform.localPosition += new Vector3(0, tree.transform.localScale.y / 2, 0);
            }
        }
    }
}
