using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainScript))]
public class TerrainEditor : Editor
{
    TerrainScript terrain;
    GameObject obj;

    void Update()
    {
       if (terrain.mouseOn) Selection.activeTransform = terrain.transform;
    }
    private void Awake()
    {
        terrain = (TerrainScript)target;
        obj = terrain.obj;
        EditorApplication.update += Update;
    }
    private void OnDestroy()
    {
        EditorApplication.update -= Update;
    }
    private void OnSceneGUI()
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.layer != 30)
            {
                DestroyImmediate(hit.collider.gameObject);
            }
        }
        else if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            for (int i = 0; i < terrain.objectsAmount; i++)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition + 
                    new Vector2(Random.Range(-terrain.radius, terrain.radius),
                                Random.Range(-terrain.radius, terrain.radius)));
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit) &&
                    ((terrain.place == TerrainScript.placeType.onTerrainOnly && hit.collider.gameObject.layer == 30) ||
                    (terrain.place == TerrainScript.placeType.onObjectsOnly && hit.collider.gameObject.layer != 30) ||
                    (terrain.place == TerrainScript.placeType.onTerrainAndObjects)))
                {
                    Quaternion rotation = terrain.placeAsNormals ? Quaternion.Euler(hit.normal) : Quaternion.identity;
                    Debug.Log(rotation);
                    GameObject temp = Instantiate(obj, hit.point, rotation, terrain.transform);
                    if (terrain.centerObject) temp.transform.localPosition += new Vector3(0, obj.transform.localScale.y / 2, 0);
                }
            }
        }
    }
}
