using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct Change
{
    public WillowUtils.ChangeType type;
    public List<GameObject> spawnedObjects;
    public List<GameObject> destroyedObjects;
    public Change(WillowUtils.ChangeType Type, List<GameObject> SpawnedObjects, List<GameObject> DestroyedObjects)
    {
        type = Type;
        spawnedObjects = SpawnedObjects;
        destroyedObjects = DestroyedObjects;
    }
}

