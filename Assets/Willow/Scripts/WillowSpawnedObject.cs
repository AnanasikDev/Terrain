using UnityEngine;

public class WillowSpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public Renderer Renderer;
    [HideInInspector] public string Layer;
    [HideInInspector] public Vector3 PositionAdd;
    [HideInInspector] public SpawnableObject SpawnableObject;

    public void Init(SpawnableObject spawnableObject)
    {
        SpawnableObject = spawnableObject;
    }
    private bool GetNewPosition(out Vector3 position)
    {
        Physics.Raycast(transform.position + Vector3.up * WillowTerrainSettings.PositionRayRecalculatingLength, Vector3.down, out RaycastHit hit);

        position = hit.point;
        
        return hit.collider.GetComponent<WillowTerrainSettings>() != null;
    }
    public void RecalculateObjectPosition()
    {
        Debug.Log("Recalc");
        if (GetNewPosition(out Vector3 newPosition))
        {
            transform.position = newPosition;
        }
        Vector3 result = transform.position;

        if (SpawnableObject.centerObject)
        {
            result += Vector3.up * transform.localScale.y;
        }
        if (SpawnableObject.modifyPosition)
        {
            result += SpawnableObject.positionAddition;
        }

        transform.position = result;
    }
}
