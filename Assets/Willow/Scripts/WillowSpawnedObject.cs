using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class WillowSpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public Renderer[] Renderers;
    [HideInInspector] public string Layer;
    [HideInInspector] public Vector3 PositionAdd;
    [HideInInspector] public WillowSpawnableObject SpawnableObject;

    private RaycastHit[] hits = new RaycastHit[12];

    private bool GetHit()
    {
        
        Physics.RaycastNonAlloc(transform.position + transform.up * WillowTerrainSettings.RecalculatingLength, -transform.up, hits, WillowTerrainSettings.RecalculatingLength * 2f);

        return hits.Where(x => x.collider != null).ToArray().Length != 0;
    }
    private bool GetNewPosition(out Vector3 position)
    {
        GetHit();

        foreach (var hit in hits.Reverse().Where(h => h.collider != null))
        {
            if (WillowObjectsController.CheckSurface(hit.collider.gameObject))
            {
                position = hit.point;
                return true;
            }
        }

        position = transform.position;

        return false;
    }
    private bool GetNewRotation(out Vector3 normal)
    {
        GetHit();

        foreach (var hit in hits.Reverse().Where(h => h.collider != null))
        {
            if (WillowObjectsController.CheckSurface(hit.collider.gameObject))
            {
                normal = hit.normal;
                return true;
            }
        }

        normal = transform.up;

        return false;
    }
    public void RecalculateObjectPosition()
    {
        Vector3 result = transform.position;
        if (GetNewPosition(out Vector3 newPosition))
        {
            result = newPosition;
        }

        transform.localPosition = result;

        if (SpawnableObject.CenterObject)
        {
            transform.Translate(Vector3.up * transform.localScale.y / 2);
        }

        if (SpawnableObject.ModifyPosition)
        {
            transform.Translate(SpawnableObject.PositionAddition);
        }
    }
    public void RecalculateObjectRotation()
    {
        GetNewRotation(out Vector3 normal);

        WillowObjectsController.SetObjectRotation(SpawnableObject, gameObject, normal, SpawnableObject.CustomEulersRotation);
    }
    public void RecalculateObjectScale()
    {
        transform.localScale = WillowObjectsController.GetObjectScale(SpawnableObject);
    }
}
