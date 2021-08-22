using UnityEngine;
using System.Linq;
public class WillowSpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public Renderer[] Renderers;
    [HideInInspector] public string Layer;
    [HideInInspector] public Vector3 PositionAdd;
    public WillowSpawnableObject SpawnableObject;

    private RaycastHit[] hits = new RaycastHit[12];

    private bool GetHit()
    {
        Physics.RaycastNonAlloc(transform.position + Vector3.up * WillowTerrainSettings.RecalculatingLength, Vector3.down, hits);

        return hits.Where(x => x.collider != null).ToArray().Length != 0;

    }
    private bool GetNewPosition(out Vector3 position)
    {
       GetHit();

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
    }
    private bool GetNewRotation(out Vector3 normal)
    {
        GetHit();

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
        GetNewRotation(out Vector3 normal);

        WillowObjectsController.SetObjectRotation(SpawnableObject, gameObject, normal, SpawnableObject.CustomEulersRotation);
    }
    public void RecalculateObjectScale()
    {
        Debug.Log(SpawnableObject.CustomScale);
        transform.localScale = WillowObjectsController.GetObjectScale(SpawnableObject);//
    }
}
