#if UNITY_EDITOR
using UnityEngine;
using System.Linq;

public class WillowSpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public Renderer[] Renderers;
    [HideInInspector] public string Layer;
    [HideInInspector] public Vector3 PositionAdd;
    [HideInInspector] public WillowSpawnableObject SpawnableObject;

    private const int CastCapacity = 15;

    [SerializeField] private RaycastHit[] Hits = new RaycastHit[CastCapacity];
    [SerializeField] private Collider[] Colliders = new Collider[CastCapacity];

    private bool GetHit()
    {
        Vector3 direction = GetRaycastDirection();
        Physics.RaycastNonAlloc
            (
                transform.position + -direction * WillowTerrainSettings.RecalculatingLength, 
                direction, 
                Hits, 
                WillowTerrainSettings.RecalculatingLength * 2f
            );

        return Hits.Where(x => x.collider != null).ToArray().Length != 0;
    }
    private Vector3 GetRaycastDirection()
    {
        if (SpawnableObject.RecalculatingMode == WillowUtils.RecalculatingMode.AsRotation)
        {
            return - transform.up;
        }
        else if (SpawnableObject.RecalculatingMode == WillowUtils.RecalculatingMode.Static)
        {
            // Static mode

            return SpawnableObject.RecalculationStaticDirection;
        }
        else if (SpawnableObject.RecalculatingMode == WillowUtils.RecalculatingMode.AsNearest)
        {
            // nearest mode

            Vector3 nearestPoint;
            Vector3 origin = transform.position;
            float radius = WillowTerrainSettings.RecalculatingLength;
            //Physics.SphereCastNonAlloc(origin, radius, Vector3.forward, hits);
            Physics.OverlapSphereNonAlloc(origin, radius, Colliders);

            Debug.Log(string.Join<Collider>(", ", Colliders.Where(c => c != null).OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)));

            Collider nearest = Colliders
                .Where(c => c != null)
                //.Reverse()
                .OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)
                .FirstOrDefault();

            Debug.Log(nearest);

            if (nearest == null)
            {
                throw new UnityEngine.MissingReferenceException("No collider");
            }

            nearestPoint = nearest.ClosestPoint(transform.position);

            Vector3 direction = (transform.position - nearestPoint).normalized;
            Debug.Log(direction);
            //Physics.Raycast(/*new Ray(transform.position, nearestPoint)*/, out RaycastHit nearest);

            return direction;
        }
        return Vector3.zero;
    }
    private bool GetNewPosition(out Vector3 position)
    {
        GetHit();

        foreach (var hit in Hits.Reverse().Where(h => h.collider != null))
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

        foreach (var hit in Hits.Reverse().Where(h => h.collider != null))
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
        Debug.Log(normal);

        WillowObjectsController.SetObjectRotation(SpawnableObject, gameObject, normal, SpawnableObject.CustomEulersRotation);
    }
    public void RecalculateObjectScale()
    {
        transform.localScale = WillowObjectsController.GetObjectScale(SpawnableObject);
    }
}
#endif
