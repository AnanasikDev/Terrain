#if UNITY_EDITOR
using UnityEngine;
using System.Linq;

public class WillowSpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public Renderer[] Renderers;
    [HideInInspector] public string Layer;
    [HideInInspector] public Vector3 PositionAdd;
    [HideInInspector] public Vector3 SpawnedPosition;
    /*[HideInInspector]*/ public WillowSpawnableObject SpawnableObject;

    private const int CastCapacity = 15;

    private RaycastHit[] Hits = new RaycastHit[CastCapacity];
    private Collider[] Colliders = new Collider[CastCapacity];

    private bool GetHit()
    {
        Hits = new RaycastHit[CastCapacity];

        Vector3 direction = GetRaycastDirection();
        //bool useOffset = SpawnableObject.RecalculatingMode == WillowUtils.RecalculationMode.Static;
        Physics.RaycastNonAlloc
            (
                transform.position + -direction * WillowTerrainSettings.RecalculatingLength / 2f, 
                direction,
                Hits,
                WillowTerrainSettings.RecalculatingLength// * 5f
            );

        Hits = Hits.Where(hit => hit.collider && hit.collider.gameObject.activeInHierarchy && hit.collider.gameObject != gameObject).ToArray();

        /*foreach (var a in Hits) 
            Debug.Log(a.collider);*/

        return Hits.Reverse().Where(x => x.collider).ToArray().Length != 0;
    }
    private Vector3 GetRaycastDirection()
    {
        if (SpawnableObject.RecalculatingMode == WillowUtils.RecalculationMode.AsRotation)
        {
            return - transform.up;
        }
        else if (SpawnableObject.RecalculatingMode == WillowUtils.RecalculationMode.Static)
        {
            // Static mode

            return SpawnableObject.RecalculationStaticDirection;
        }
        else if (SpawnableObject.RecalculatingMode == WillowUtils.RecalculationMode.AsNearest)
        {
            // nearest mode

            Vector3 nearestPoint;
            Vector3 origin = transform.position;
            float radius = WillowTerrainSettings.RecalculatingLength;
            Physics.OverlapSphereNonAlloc(origin, radius, Colliders);
            
            //Debug.Log(string.Join<Collider>(", ", Colliders));

                Colliders = Colliders.Where(c => c != null && c.gameObject.activeInHierarchy).ToArray();
            Collider nearest = Colliders
                //.Where(c => c != null)
                .OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (nearest == null)
            {
                throw new UnityEngine.MissingReferenceException("No collider");
            }

            nearestPoint = nearest.ClosestPointOnBounds(transform.position);

            //Debug.Log($"{transform.position}, {nearestPoint}");

            //GameObject g = new GameObject();
            //g.transform.position = nearestPoint;

            Vector3 direction = (nearestPoint - transform.position).normalized;
            return direction;
        }
        return Vector3.zero;
    }
    private bool GetNewPosition(out Vector3 position)
    {
        GetHit();

        foreach (var hit in Hits.Reverse().Where(h => h.collider))
        {
            if (WillowObjectsController.CheckSurface(hit.collider.gameObject))
            {
                position = hit.point + (SpawnableObject.CenterObject ? transform.up * transform.localScale.y / 2 : Vector3.zero);
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

        /*if (SpawnableObject.CenterObject)
        {
            Vector3 dir = SpawnableObject.PositionAdditionSpace == Space.Self ? transform.up : Vector3.up;
            transform.position = SpawnedPosition;
            transform.Translate(dir * transform.localScale.y / 2 * (SpawnableObject.ModifyScale ? SpawnableObject.CustomScale.y : 1));
        }

        if (SpawnableObject.ModifyPosition)
        {
            transform.position = SpawnedPosition;
            transform.Translate(SpawnableObject.PositionAddition, SpawnableObject.PositionAdditionSpace);
        }*/
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
    public void AvoidObstacles()
    {
        if (SpawnableObject.AvoidObstacles)
        {
            Vector3 origin = transform.position - Vector3.up * (SpawnableObject.AvoidanceHeight / 2f);
            Vector3 target = transform.position + Vector3.up * (SpawnableObject.AvoidanceHeight / 2f);
            Collider[] colliders = Physics.OverlapCapsule(origin, target, SpawnableObject.AvoidanceRadius);
            foreach (Collider collider in colliders)
            {
                if (SpawnableObject.ObstaclesTagType == WillowUtils.ObstaclesTagType.WillowObstacle)
                {
                    if (collider.GetComponent<WillowObstacle>())
                    {
                        if (SpawnableObject.ObstaclesAvoidanceAction == WillowUtils.ObstaclesAvoidanceAction.Disable)
                        {
                            gameObject.SetActive(false);
                            return;
                        }
                    }
                }
            }
        }
        gameObject.SetActive(true);
    }
    private void OnDrawGizmos()
    {
        try
        {
            if (!SpawnableObject.AvoidObstacles) return;

            Vector3 origin = transform.position - Vector3.up * (SpawnableObject.AvoidanceHeight / 2f);
            Vector3 target = transform.position + Vector3.up * (SpawnableObject.AvoidanceHeight / 2f);

            Gizmos.DrawWireSphere(origin, SpawnableObject.AvoidanceRadius);
            Gizmos.DrawWireSphere(target, SpawnableObject.AvoidanceRadius);
        }
        catch { }
    }
}
#endif
