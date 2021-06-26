using UnityEditor;
using UnityEngine;
//[ExecuteInEditMode]
[CustomEditor(typeof(TerrainScript))]
public class TerrainEditor : Editor
{
    public TerrainScript terrain;
    public GameObject tree;
    Camera cam;
    private void Awake()
    {
        terrain = (TerrainScript)target;
        tree = terrain.obj;
        Debug.Log(tree);
        //objects = new GameObject[1];
        //objects[0] = Resources.Load<GameObject>("Prefabs/Cube");
        cam = SceneView.lastActiveSceneView.camera;
    }
    private void OnSceneGUI()
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        //if (Input.GetMouseButton(0))
        {
            Debug.Log("Spawn");
            Vector3 hitPosition;
            RaycastHit hit;

            Physics.Raycast(cam.WorldToScreenPoint(Input.mousePosition), cam.transform.forward, out hit);
            hitPosition = hit.point;

            Instantiate(tree, hitPosition, Quaternion.Euler(hit.normal));
        }
    }
}
