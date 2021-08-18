using UnityEngine;

public class WillowSpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public Renderer[] Renderers;
    [HideInInspector] public string Layer;
    [HideInInspector] public Vector3 PositionAdd;
    public SpawnableObject SpawnableObject;

    public void Init(SpawnableObject spawnableObject)
    {
        SpawnableObject = spawnableObject;
    }
    private bool GetHit(out RaycastHit hit)
    {
        bool result = Physics.Raycast(transform.position + Vector3.up * WillowTerrainSettings.RecalculatingLength, Vector3.down, out RaycastHit _hit);
        hit = _hit;
        return result;
    }
    private bool GetNewPosition(out Vector3 position)
    {
        GetHit(out RaycastHit hit);

        position = hit.point;
        
        return hit.collider.GetComponent<WillowTerrainSettings>() != null;
    }
    public void RecalculateObjectPosition()
    {
        Vector3 result = transform.position;
        if (GetNewPosition(out Vector3 newPosition))
        {
            result = newPosition;
        }

        if (SpawnableObject.CenterObject)
        {
            result += Vector3.up * transform.localScale.y;
        }
        if (SpawnableObject.ModifyPosition)
        {
            result += SpawnableObject.PositionAddition;
        }

        transform.position = result;
    }
    public void RecalculateObjectRotation()
    {
        GetHit(out RaycastHit hit);

        WillowObjectsController.SetObjectRotation(SpawnableObject, gameObject, hit.normal, SpawnableObject.CustomEulersRotation);
    }
}
