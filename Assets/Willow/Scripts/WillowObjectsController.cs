using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using static WillowUtils;
using static WillowGlobalConfig;
using static WillowDebug;
public static class WillowObjectsController
{
    public static event System.Action OnRepaint;
    public static void PlaceObjects()
    {
        RaycastHit screenHit;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        bool ableToSpawn = Physics.Raycast(ray, out screenHit);
        List<GameObject> spawnedObjs = new List<GameObject>();

        if (!ableToSpawn) return;

        List<WillowSpawnableObject> spawnableObjects = WillowTerrainSettings.spawnableObjects;

        spawnableObjects = spawnableObjects
            .Where(spawnableObject => spawnableObject.Spawn && spawnableObject.Object != null)
            .ToList();

        if (WillowTerrainSettings.ignoreInactiveLayers)
        {
            spawnableObjects = 
                spawnableObjects.Where(spawnableObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(spawnableObject.Layer)]).ToList();
        }


        bool anySpawnableObjects = spawnableObjects.Any();

        if (WillowTerrainSettings.spawnableObjects.Count == 0 || !anySpawnableObjects)
        {
            Log("There are no objects to spawn! Make sure you have active spawnable objects", Yellow, Debug.LogError);
            return;
        }

        for (int i = 0; i < WillowTerrainSettings.density; i++)
        {
            WillowSpawnableObject spawnableObject = GetObject(spawnableObjects);
            
            if (spawnableObject == null) 
                continue;

            RaycastHit hit;

            Vector3 position = GetRandomPointOnBrush();

            if (Physics.Raycast(screenHit.point + Vector3.up * 5 + position, Vector3.down, out hit) && CheckSurface(hit.collider.gameObject))
            {
                GameObject spawned = SpawnObject(spawnableObject, hit);

                spawnedObjs.Add(spawned);

                EditorUtility.SetDirty(spawned);
            }
        }

        AutoSave();
        WillowTerrainSettings.changelog.Push(new Change(WillowUtils.ChangeType.Placement, spawnedObjs, null));
        OnRepaint?.Invoke();
    }
    public static void EraseObjects()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit = new RaycastHit();
        Physics.Raycast(ray, out hit);

        List<GameObject> spawnedObjectsTemp = WillowTerrainSettings.spawnedObjects;
        GameObject[] objsToDestroy =
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= WillowTerrainSettings.brushSize * WillowTerrainSettings.brushSize))
            .ToArray();

        if (WillowTerrainSettings.ignoreInactiveLayers)
            objsToDestroy = objsToDestroy.Where(gameObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(gameObject.GetComponent<WillowSpawnedObject>().Layer)]).ToArray();

        foreach (GameObject o in objsToDestroy)
        {
            if (DecideIfSmooth(WillowTerrainSettings.eraseSmoothness))
            {
                if (o != null)
                {
                    o.gameObject.hideFlags = hidden;
                    o.SetActive(false);
                    spawnedObjectsTemp.Remove(o);
                    WillowTerrainSettings.destroyedObjects.Add(o);

                    EditorUtility.SetDirty(o);
                    
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
        spawnedObjectsTemp.RemoveAll(o => o == null);
        WillowTerrainSettings.spawnedObjects = spawnedObjectsTemp;

        AutoSave();
        WillowTerrainSettings.changelog.Push(new Change(WillowUtils.ChangeType.Erasure, null, objsToDestroy.ToList()));
        OnRepaint?.Invoke();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        //EditorApplication.RepaintHierarchyWindow();
    }
    public static void ExchangeObjects()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        List<GameObject> spawnedObjectsTemp = WillowTerrainSettings.spawnedObjects;
        GameObject[] objsToExchange =
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= WillowTerrainSettings.brushSize * WillowTerrainSettings.brushSize))
            .ToArray();

        List<WillowSpawnableObject> spawnableObjects = WillowTerrainSettings.spawnableObjects;
        if (WillowTerrainSettings.ignoreInactiveLayers)
        {
            objsToExchange = objsToExchange.Where(gameObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(gameObject.GetComponent<WillowSpawnedObject>().Layer)]).ToArray();
            spawnableObjects = spawnableObjects.Where(spawnableObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(spawnableObject.Layer)]).ToList();
        }

        foreach (GameObject o in objsToExchange)
        {
            if (DecideIfSmooth(WillowTerrainSettings.exchangeSmoothness))
            {
                if (o != null)
                {
                    WillowSpawnableObject spawnableObject = GetObject(spawnableObjects);
                    Vector3 position = WillowTerrainSettings.exchangePosition ? o.transform.position : o.transform.position - o.GetComponent<WillowSpawnedObject>().PositionAdd + GetObjectPositionAdd(spawnableObject);
                    RaycastHit normalHit;
                    bool normalCasted = Physics.Raycast(position, -o.transform.up, out normalHit, 3);
                    Vector3 normal = normalCasted ? normalHit.normal : Vector3.up;
                    Transform parent = WillowTerrainSettings.exchangeParent ? o.transform.parent : spawnableObject.Parent;
                    GameObject spawned = Object.Instantiate(spawnableObject.Object, position, Quaternion.identity, parent);
                    spawned.name = o.name;

                    if (WillowTerrainSettings.exchangeRotation)
                        spawned.transform.localRotation = o.transform.localRotation;
                    else
                        SetObjectRotation(spawnableObject, spawned, normal, spawnableObject.CustomEulersRotation);
                    if (WillowTerrainSettings.exchangeColor)    
                        SetObjectColor(spawnableObject.ModifyColor, spawnableObject.ColorModPercentage, spawned.GetComponent<WillowSpawnedObject>().Renderers, o.GetComponent<WillowSpawnedObject>().Renderers[0].sharedMaterial.color);
                    else 
                        SetObjectColor(spawnableObject.ModifyColor, spawnableObject.ColorModPercentage, spawned.GetComponent<WillowSpawnedObject>().Renderers);

                    o.GetComponent<WillowSpawnedObject>().SpawnableObject = spawnableObject;

                    spawned.GetComponent<WillowSpawnedObject>().PositionAdd =
                        spawnableObject.ModifyPosition ? spawnableObject.PositionAddition : Vector3.zero;

                    spawned.transform.localScale = WillowTerrainSettings.exchangeScale ? o.transform.localScale : GetObjectScale(spawnableObject);

                    spawnedObjectsTemp.Remove(o);
                    spawnedObjectsTemp.Add(spawned);

                    o.gameObject.hideFlags = hidden;
                    o.SetActive(false);
                    WillowTerrainSettings.destroyedObjects.Add(o);

                    EditorUtility.SetDirty(o);
                    EditorUtility.SetDirty(spawned);
                }
            }
        }
        WillowTerrainSettings.changelog.Push(new Change(WillowUtils.ChangeType.Exchange, spawnedObjectsTemp.ToList(), objsToExchange.ToList()));
        spawnedObjectsTemp.RemoveAll(o => o == null);
        WillowTerrainSettings.spawnedObjects = spawnedObjectsTemp;
        AutoSave();
        OnRepaint?.Invoke();
        EditorApplication.RepaintHierarchyWindow();
        EditorApplication.DirtyHierarchyWindowSorting();
    }

    public static WillowSpawnableObject GetObject(List<WillowSpawnableObject> spawnableObjects)
    {
        int[] chances = new int[spawnableObjects.Count];
        bool ableToSpawn = false;
        for (int i = 0; i < chances.Length; i++)
        {
            chances[i] = spawnableObjects[i].Spawn && spawnableObjects[i].Object != null ? spawnableObjects[i].SpawnChance : 0;
            if (chances[i] > 0) ableToSpawn = true;
        }
        if (!ableToSpawn) return null;
        return spawnableObjects[GetChance(chances)];
    }
    public static void SetObjectRotation(WillowSpawnableObject spawnableObject, GameObject spawnedObject, Vector3 normal, Vector3 custom)
    {
        if (spawnableObject.RotationType == RotationType.Static)
        {
            spawnedObject.transform.localEulerAngles = custom;
        }
        else if (spawnableObject.RotationType == RotationType.Random)
        {
            spawnedObject.transform.localEulerAngles = GetRandomRotation();
        }
        else if (spawnableObject.RotationType == RotationType.AsNormal)
        {
            spawnedObject.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
        }
        else if (spawnableObject.RotationType == RotationType.RandomAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            spawnedObject.transform.Rotate(GetRandomRotation(), Space.Self);
        }
        else if (spawnableObject.RotationType == RotationType.StaticAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            spawnedObject.transform.Rotate(custom, Space.Self);
        }
        else if (spawnableObject.RotationType == RotationType.LerpedStaticAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(custom), Quaternion.FromToRotation(Vector3.up, normal), spawnableObject.LerpValue);
        }
        else if (spawnableObject.RotationType == RotationType.LerpedRandomAsNormal)
        {
            float lerpV = spawnableObject.LerpValue;
            if (spawnableObject.RandomizeLerpValue) lerpV = UnityEngine.Random.Range(spawnableObject.MinLerpValue, spawnableObject.MaxLerpValue);

            spawnedObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(custom), Quaternion.FromToRotation(Vector3.up, normal), lerpV);
            spawnedObject.transform.Rotate(GetRandomRotation(), Space.Self);
        }
        else if (spawnableObject.RotationType == RotationType.LerpedAsPrefabAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.Lerp(spawnableObject.Object.transform.rotation, Quaternion.FromToRotation(Vector3.up, normal), spawnableObject.LerpValue);
        }
        else if (spawnableObject.RotationType == RotationType.AsPrefab)
        {
            spawnedObject.transform.rotation = spawnableObject.Object.transform.rotation;
        }

        spawnedObject.transform.eulerAngles += spawnableObject.RotationEulerAddition;

        Vector3 GetRandomRotation()
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (spawnableObject.MultiRotationAxis)
            {
                x = UnityEngine.Random.Range(spawnableObject.RandomMinRotation.x, spawnableObject.RandomMaxRotation.x);
                y = UnityEngine.Random.Range(spawnableObject.RandomMinRotation.y, spawnableObject.RandomMaxRotation.y);
                z = UnityEngine.Random.Range(spawnableObject.RandomMinRotation.z, spawnableObject.RandomMaxRotation.z);
            }
            else
            {
                string axis = spawnableObject.RotationAxis.ToString();

                if (axis.Contains("X")) x = UnityEngine.Random.Range(0f, 360f);
                if (axis.Contains("Y")) y = UnityEngine.Random.Range(0f, 360f);
                if (axis.Contains("Z")) z = UnityEngine.Random.Range(0f, 360f);
            }

            return new Vector3(x, y, z);
        }
    }
    public static void SetObjectColor(bool modifyColor, float colorModificationPercentage, Renderer[] renderers, Color? color = null)
    {
        if (modifyColor)
        {
            foreach (Renderer renderer in renderers)
            {
                var tempMaterial = new Material(renderer.sharedMaterial);
                if (color == null)
                    tempMaterial.color *= UnityEngine.Random.Range(1 - (colorModificationPercentage / 100), 1 + (colorModificationPercentage / 100));
                else
                    tempMaterial.color = (Color)color;
                renderer.sharedMaterial = tempMaterial;
            }
        }
    }
    public static Transform GetObjectParent(WillowSpawnableObject obj)
    {
        if (obj.CustomParent) return obj.Parent;
        else return WillowTerrainSettings.parent;
    }
    public static Vector3 GetObjectPositionAdd(WillowSpawnableObject obj)
    {
        return obj.ModifyPosition ? obj.PositionAddition : Vector3.zero;
    }
    public static Vector3 GetObjectScale(WillowSpawnableObject spawnableObject)
    {
        Vector3 scale = Vector3.one;
        switch (spawnableObject.ScaleType)
        {
            case ScaleType.AsPrefab:
                scale = spawnableObject.Object.transform.localScale;
                break;
            case ScaleType.Random:
                if (spawnableObject.ModifyScale)
                {
                    float x = 1;
                    float y = 1;
                    float z = 1;

                    string axis = spawnableObject.ScaleAxis.ToString();
                    if (spawnableObject.SeparateScaleAxis)
                    {
                        if (axis.Contains("X")) x = UnityEngine.Random.Range(spawnableObject.ScaleMinSeparated.x, spawnableObject.ScaleMaxSeparated.x);
                        if (axis.Contains("Y")) y = UnityEngine.Random.Range(spawnableObject.ScaleMinSeparated.y, spawnableObject.ScaleMaxSeparated.y);
                        if (axis.Contains("Z")) z = UnityEngine.Random.Range(spawnableObject.ScaleMinSeparated.z, spawnableObject.ScaleMaxSeparated.z);
                    }
                    else
                    {
                        float value = UnityEngine.Random.Range(spawnableObject.ScaleMin, spawnableObject.ScaleMax);
                        x = y = z = value;
                    }

                    scale = new Vector3(x, y, z);
                }

                break;
            case ScaleType.Static:
                scale = spawnableObject.CustomScale;
                break;
        }
        return scale;
    }

    public static bool CheckSurface(GameObject gameObject)
    {
        return ((WillowTerrainSettings.placementType == SpawnPlaceType.onTerrainOnly && gameObject.GetComponent<WillowTerrainSettings>() != null) ||
                (WillowTerrainSettings.placementType == SpawnPlaceType.onObjectsOnly && gameObject.GetComponent<WillowTerrainSettings>() == null) ||
                (WillowTerrainSettings.placementType == SpawnPlaceType.onTerrainAndObjects));
    }
    private static bool DecideIfSmooth(float smoothness)
    {
        return UnityEngine.Random.Range(0, smoothness + 1) == smoothness || smoothness == 0;
    }
    private static void AutoSave()
    {
        if (WillowTerrainSettings.autoSave) WillowFileManager.Write();
    }
    private static GameObject SpawnObject(WillowSpawnableObject spawnableObject, RaycastHit hit)
    {
        GameObject spawned = Object.Instantiate(spawnableObject.Object, hit.point, Quaternion.identity);

        SetObjectRotation(spawnableObject, spawned, hit.normal, spawnableObject.CustomEulersRotation);
        SetObjectColor(spawnableObject.ModifyColor, spawnableObject.ColorModPercentage, spawned.GetComponent<WillowSpawnedObject>().Renderers);
        spawned.transform.localPosition += GetObjectPositionAdd(spawnableObject);
        spawned.transform.parent = GetObjectParent(spawnableObject);
        spawned.transform.localScale = GetObjectScale(spawnableObject);

        spawned.GetComponent<WillowSpawnedObject>().PositionAdd =
            spawnableObject.ModifyPosition ? spawnableObject.PositionAddition : Vector3.zero;

        spawned.GetComponent<WillowSpawnedObject>().Layer = spawnableObject.Layer;

        spawned.GetComponent<WillowSpawnedObject>().SpawnableObject = spawnableObject;

        spawned.name = spawnableObject.Object.name;
        if (spawnableObject.RenameObject)
            spawned.name = spawnableObject.NewObjectName;

        if (WillowTerrainSettings.indexObjects)
            spawned.name += IndexName();

        if (spawnableObject.CenterObject)
            spawned.transform.localPosition += new Vector3(0, spawnableObject.Object.transform.localScale.y / 2, 0);

        WillowTerrainSettings.spawnedObjects.Add(spawned);

        return spawned;
    }
    private static string IndexName()
    {
        return string.Format(WillowTerrainSettings.indexFormat, WillowTerrainSettings.spawnedIndecies++);
    }
    private static Vector3 GetRandomPointOnBrush()
    {
        Vector3 position = Vector3.zero;
        switch (WillowTerrainSettings.brushShape)
        {
            case BrushShape.Circle:

                float angle = UnityEngine.Random.Range(0f, 360f);
                float radius = WillowTerrainSettings.brushSize;
                if (WillowTerrainSettings.fillBrush) 
                    radius = UnityEngine.Random.Range(0f, WillowTerrainSettings.brushSize);
                position = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                break;

            case BrushShape.Square:

                float size = WillowTerrainSettings.brushSize * 0.75f;
                float x, z;
                if (WillowTerrainSettings.fillBrush)
                {
                    x = UnityEngine.Random.Range(-size, size);
                    z = UnityEngine.Random.Range(-size, size);
                }
                else
                {
                    if (WillowUtils.RandomBool())
                    {
                        x = UnityEngine.Random.Range(-size, size);
                        z = WillowUtils.RandomSign() * size;
                    }
                    else
                    {
                        x = WillowUtils.RandomSign() * size;
                        z = UnityEngine.Random.Range(-size, size);
                    }
                }

                position = new Vector3(x, 0f, z);
                break;
        }
        
        return position;
    }
}
