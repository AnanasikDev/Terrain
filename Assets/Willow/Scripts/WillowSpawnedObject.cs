using UnityEngine;
using System.Linq;
public class WillowSpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public Renderer[] Renderers;
    [HideInInspector] public string Layer;
    [HideInInspector] public Vector3 PositionAdd;
    public SpawnableObject SpawnableObject;

    private RaycastHit[] hits = new RaycastHit[12];

    public void Init(SpawnableObject spawnableObject)
    {
        SpawnableObject = spawnableObject;
    }
    private bool GetHit()
    {
        /*bool result = Physics.Raycast(transform.position + Vector3.up * WillowTerrainSettings.RecalculatingLength, Vector3.down, out RaycastHit _hit, WillowTerrainSettings.RecalculatingLength * 2);
        hit = _hit;
        return result;*/

        Physics.RaycastNonAlloc(transform.position + Vector3.up * WillowTerrainSettings.RecalculatingLength, Vector3.down, hits);

        return hits.Where(x => x.collider != null).ToArray().Length != 0;

    }
    private bool GetNewPosition(out Vector3 position)
    {
        /*bool result = GetHit(out RaycastHit hit);

        position = hit.point;

        return result;*/

        bool res = GetHit();

        foreach (var hit in hits)
        {
            if (WillowObjectsController.CheckSurface(hit.collider.gameObject))
            {
                position = hit.point;
                return true;
            }
        }

        position = transform.position;

        return false;

        //return hit.collider.GetComponent<WillowTerrainSettings>() != null;
    }
    private bool GetNewRotation(out Vector3 normal)
    {
        /*bool result = GetHit(out RaycastHit hit);

        position = hit.point;

        return result;*/

        bool res = GetHit();

        foreach (var hit in hits)
        {
            if (WillowObjectsController.CheckSurface(hit.collider.gameObject))
            {
                normal = hit.normal;
                return true;
            }
        }

        normal = transform.up;

        return false;

        //return hit.collider.GetComponent<WillowTerrainSettings>() != null;
    }
    public void RecalculateObjectPosition()
    {
        Vector3 result = transform.position;
        if (GetNewPosition(out Vector3 newPosition))
        {
            Debug.Log("RAYCAST");
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
        GetNewPosition(out Vector3 normal);

        WillowObjectsController.SetObjectRotation(SpawnableObject, gameObject, normal, SpawnableObject.CustomEulersRotation);
    }
}
