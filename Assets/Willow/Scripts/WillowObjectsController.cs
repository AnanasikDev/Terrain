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
        bool ableToSpawn = RaycastFromCursor(out RaycastHit screenHit);

        List<GameObject> spawnedObjs = new List<GameObject>();

        if (!ableToSpawn) return;

        List<WillowSpawnableObject> spawnableObjects = WillowTerrainSettings.SpawnableObjects;

        spawnableObjects = spawnableObjects
            .Where(spawnableObject => spawnableObject.Spawn && spawnableObject.Object != null)
            .ToList();

        if (WillowTerrainSettings.IgnoreInactiveLayers)
        {
            spawnableObjects = 
                spawnableObjects.Where(spawnableObject => WillowTerrainSettings.LayersState[WillowTerrainSettings.LayersName.IndexOf(spawnableObject.Layer)]).ToList();
        }


        bool anySpawnableObjects = spawnableObjects.Any();

        if (WillowTerrainSettings.SpawnableObjects.Count == 0 || !anySpawnableObjects)
        {
            Log("There are no objects to spawn! Make sure you have active spawnable objects", Yellow, Debug.LogError);
            return;
        }

        float density = WillowTerrainSettings.BrushDensity;
        if (WillowTerrainSettings.RandomizeBrushDensity)
            density = RandomizeBrushDensity();
        for (int i = 0; i < density; i++)
        {
            WillowSpawnableObject spawnableObject = GetObject(spawnableObjects);
            
            if (spawnableObject == null) 
                continue;

            if (RaycastBrush(out RaycastHit hit, screenHit))
            {
                if (spawnableObject.Object.GetComponent<WillowSpawnedObject>() == null)
                {
                    Log("It seems that object you are trying to spawn does not contain WillowSpawnedObject component. Please, add it.", Yellow, Debug.LogError);
                    return;
                }

                GameObject spawned = SpawnObject(spawnableObject, hit);

                spawnedObjs.Add(spawned);

                EditorUtility.SetDirty(spawned);
            }
        }

        AutoSave();
        WillowTerrainSettings.ChangeLog.Push(new Change(WillowUtils.BrushMode.Place, spawnedObjs, null));
        OnRepaint?.Invoke();
    }
    public static void EraseObjects()
    {
        RaycastFromCursor(out RaycastHit hit);

        List<GameObject> spawnedObjectsTemp = WillowTerrainSettings.SpawnedObjects;
        GameObject[] objsToDestroy =
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= WillowTerrainSettings.BrushSize * WillowTerrainSettings.BrushSize))
            .ToArray();

        if (WillowTerrainSettings.IgnoreInactiveLayers)
            objsToDestroy = objsToDestroy.Where(gameObject => WillowTerrainSettings.LayersState[WillowTerrainSettings.LayersName.IndexOf(gameObject.GetComponent<WillowSpawnedObject>().Layer)]).ToArray();

        foreach (GameObject o in objsToDestroy)
        {
            if (DecideIfSmooth(WillowTerrainSettings.EraseSmoothness))
            {
                if (o != null)
                {
                    o.gameObject.hideFlags = hidden;
                    o.SetActive(false);
                    
                    EditorUtility.SetDirty(o.gameObject);

                    spawnedObjectsTemp.Remove(o);
                    WillowTerrainSettings.DestroyedObjects.Add(o);
                    
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
        spawnedObjectsTemp.RemoveAll(o => o == null);
        WillowTerrainSettings.SpawnedObjects = spawnedObjectsTemp;

        AutoSave();
        WillowTerrainSettings.ChangeLog.Push(new Change(WillowUtils.BrushMode.Erase, null, objsToDestroy.ToList()));
        OnRepaint?.Invoke();
        EditorApplication.RepaintHierarchyWindow();
    }
    public static void ExchangeObjects()
    {
        RaycastFromCursor(out RaycastHit hit);

        List<GameObject> spawnedObjectsTemp = WillowTerrainSettings.SpawnedObjects;
        GameObject[] objsToExchange =
            spawnedObjectsTemp
            .Where(gameObject => gameObject != null && ((hit.point - gameObject.transform.position).sqrMagnitude <= WillowTerrainSettings.BrushSize * WillowTerrainSettings.BrushSize))
            .ToArray();

        List<WillowSpawnableObject> spawnableObjects = WillowTerrainSettings.SpawnableObjects;
        if (WillowTerrainSettings.IgnoreInactiveLayers)
        {
            objsToExchange = objsToExchange.Where(gameObject => WillowTerrainSettings.LayersState[WillowTerrainSettings.LayersName.IndexOf(gameObject.GetComponent<WillowSpawnedObject>().Layer)]).ToArray();
            spawnableObjects = spawnableObjects.Where(spawnableObject => WillowTerrainSettings.LayersState[WillowTerrainSettings.LayersName.IndexOf(spawnableObject.Layer)]).ToList();
        }

        foreach (GameObject o in objsToExchange)
        {
            if (DecideIfSmooth(WillowTerrainSettings.ExchangeSmoothness))
            {
                if (o != null)
                {
                    WillowSpawnableObject spawnableObject = GetObject(spawnableObjects);
                    Vector3 position = WillowTerrainSettings.ExchangePosition ? o.transform.position : o.transform.position - o.GetComponent<WillowSpawnedObject>().PositionAdd + GetObjectPositionAdd(spawnableObject);
                    RaycastHit normalHit;
                    bool normalCasted = Physics.Raycast(position, -o.transform.up, out normalHit, 3);
                    Vector3 normal = normalCasted ? normalHit.normal : Vector3.up;
                    Transform parent = WillowTerrainSettings.ExchangeParent ? o.transform.parent : spawnableObject.Parent;
                    GameObject spawned = Object.Instantiate(spawnableObject.Object, position, Quaternion.identity, parent);
                    spawned.name = o.name;

                    if (WillowTerrainSettings.ExchangeRotation)
                        spawned.transform.localRotation = o.transform.localRotation;
                    else
                        SetObjectRotation(spawnableObject, spawned, normal, spawnableObject.CustomEulersRotation);
                    if (WillowTerrainSettings.ExchangeColor)    
                        SetObjectColor(spawnableObject.ModifyColor, spawnableObject.ColorModPercentage, spawned.GetComponent<WillowSpawnedObject>().Renderers, o.GetComponent<WillowSpawnedObject>().Renderers[0].sharedMaterial.color);
                    else 
                        SetObjectColor(spawnableObject.ModifyColor, spawnableObject.ColorModPercentage, spawned.GetComponent<WillowSpawnedObject>().Renderers);

                    o.GetComponent<WillowSpawnedObject>().SpawnableObject = spawnableObject;

                    spawned.GetComponent<WillowSpawnedObject>().PositionAdd =
                        spawnableObject.ModifyPosition ? spawnableObject.PositionAddition : Vector3.zero;

                    spawned.transform.localScale = WillowTerrainSettings.ExchangeScale ? o.transform.localScale : GetObjectScale(spawnableObject);

                    spawnedObjectsTemp.Remove(o);
                    spawnedObjectsTemp.Add(spawned);

                    o.gameObject.hideFlags = hidden;
                    o.SetActive(false);
                    WillowTerrainSettings.DestroyedObjects.Add(o);

                    EditorUtility.SetDirty(o);
                    EditorUtility.SetDirty(spawned);
                }
            }
        }
        WillowTerrainSettings.ChangeLog.Push(new Change(WillowUtils.BrushMode.Exchange, spawnedObjectsTemp.ToList(), objsToExchange.ToList()));
        spawnedObjectsTemp.RemoveAll(o => o == null);
        WillowTerrainSettings.SpawnedObjects = spawnedObjectsTemp;
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

        spawnedObject.transform.Rotate(spawnableObject.RotationEulerAddition);

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
        else return WillowTerrainSettings.BaseParent;
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
        return ((WillowTerrainSettings.PlacementType == SpawnPlaceType.onTerrainOnly && gameObject.GetComponent<WillowTerrainSettings>() != null) ||
                (WillowTerrainSettings.PlacementType == SpawnPlaceType.onObjectsOnly && gameObject.GetComponent<WillowTerrainSettings>() == null) ||
                (WillowTerrainSettings.PlacementType == SpawnPlaceType.onTerrainAndObjects));
    }
    private static bool DecideIfSmooth(int smoothness)
    {
        return UnityEngine.Random.Range(0, smoothness + 1) == smoothness || smoothness == 0;
    }
    private static void AutoSave()
    {
        if (WillowTerrainSettings.AutoSave) WillowFileManager.Write();
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

        if (WillowTerrainSettings.IndexObjects)
            spawned.name += IndexName();

        if (spawnableObject.CenterObject)
            spawned.transform.localPosition += new Vector3(0, spawnableObject.Object.transform.localScale.y / 2, 0);

        WillowTerrainSettings.SpawnedObjects.Add(spawned);

        return spawned;
    }
    private static string IndexName()
    {
        return string.Format(WillowTerrainSettings.IndexFormat, WillowTerrainSettings.SpawnedIndecies++);
    }
    private static Vector3 GetRandomPointOnBrush()
    {
        Vector3 position = Vector3.zero;
        switch (WillowTerrainSettings.BrushShape)
        {
            case BrushShape.Circle:

                float angle = UnityEngine.Random.Range(0f, 360f);
                float radius = WillowTerrainSettings.BrushSize;
                if (WillowTerrainSettings.FillBrush) 
                    radius = UnityEngine.Random.Range(0f, WillowTerrainSettings.BrushSize);
                position = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                break;

            case BrushShape.Square:

                float size = WillowTerrainSettings.BrushSize * 0.75f;
                float x, z;
                if (WillowTerrainSettings.FillBrush)
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

    private static bool RaycastFromCursor(out RaycastHit hit)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        return Physics.Raycast(ray, out hit);
    }

    private static bool RaycastBrush(out RaycastHit hit, RaycastHit screenHit)
    {
        Vector3 position = GetRandomPointOnBrush();
        Vector3 normal = GetBrushNormal(screenHit.normal);
        return Physics.Raycast(screenHit.point + position + normal * 10, -normal, out hit) && CheckSurface(hit.collider.gameObject);
    }
    private static float RandomizeBrushDensity()
    {
        return Random.Range(WillowTerrainSettings.BrushDensity * (1f / WillowTerrainSettings.BrushDensityRandomizationModificator),
                            WillowTerrainSettings.BrushDensity * (1f + WillowTerrainSettings.BrushDensityRandomizationModificator));
    }
    private static Vector3 GetBrushNormal(Vector3 surfaceNormal)
    {
        Vector3 normal = Vector3.zero;

        if (WillowTerrainSettings.BrushSurface == BrushSurface.AsNormal)
        {
            normal = surfaceNormal;
        }
        if (WillowTerrainSettings.BrushSurface == BrushSurface.Static)
        {
            normal = WillowTerrainSettings.BrushSurfaceStaticNormal;
        }

        return normal;
    }
}
