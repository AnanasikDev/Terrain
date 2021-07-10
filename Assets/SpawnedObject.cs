using UnityEngine;
public class SpawnedObject : MonoBehaviour // Do NOT remove this script from spawned object if you need to edit terrain
{
    public new Renderer renderer;
    [HideInInspector] public string layer;
    [HideInInspector] public Vector3 positionAdd;
}
