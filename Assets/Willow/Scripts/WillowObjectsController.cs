using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using static WillowUtils;
using static WillowGlobalConfig;
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

        List<SpawnableObject> spawnableObjects = WillowTerrainSettings.spawnableObjects;
        if (WillowTerrainSettings.ignoreInactiveLayers)
            spawnableObjects = spawnableObjects
                .Where(spawnableObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(spawnableObject.Layer)])
                .Where(spawnableObject => spawnableObject.Spawn && spawnableObject.Object != null)
                .ToList();


        bool anySpawnableObjects = spawnableObjects.Any();

        if (!(WillowTerrainSettings.spawnableObjects.Count > 0 && anySpawnableObjects))
        {
            if (WillowTerrainSettings.debugMode) Debug.LogError(WillowUtils.FormatLog("There are no objects to spawn!"));
            return;
        }

        for (int i = 0; i < WillowTerrainSettings.density; i++)
        {
            SpawnableObject spawnableObject = GetObject(spawnableObjects);
            if (spawnableObject == null) continue;

            RaycastHit hit;

            Vector3 position = Vector3.zero;
            switch (WillowTerrainSettings.brushShape)
            {
                case BrushShape.Circle:

                    float a = UnityEngine.Random.Range(0f, 360f);
                    float r = WillowTerrainSettings.brushSize;
                    if (WillowTerrainSettings.fillBrush) r = UnityEngine.Random.Range(0, WillowTerrainSettings.brushSize);
                    position = new Vector3(Mathf.Sin(a) * r, 0, Mathf.Cos(a) * r);

                    break;

                case BrushShape.Square:

                    float size = WillowTerrainSettings.brushSize * 0.75f;
                    float x = 0;
                    float z = 0;
                    if (WillowTerrainSettings.fillBrush)
                    {
                        x = UnityEngine.Random.Range(-size, size);
                        z = UnityEngine.Random.Range(-size, size);
                    }
                    else
                    {
                        var lims = new float[2] { -size, size };
                        bool g = UnityEngine.Random.value > 0.5f;
                        if (g)
                        {
                            x = UnityEngine.Random.Range(-size, size);
                            z = lims[UnityEngine.Random.Range(0, 2)];
                        }
                        else
                        {
                            z = UnityEngine.Random.Range(-size, size);
                            x = lims[UnityEngine.Random.Range(0, 2)];
                        }
                    }
                    position = new Vector3(x, 0, z);

                    break;
            }

            if (Physics.Raycast(screenHit.point + Vector3.up * 5 + position, Vector3.down, out hit) &&

                ((WillowTerrainSettings.placementType == SpawnPlaceType.onTerrainOnly && hit.collider.gameObject.GetComponent<WillowTerrainSettings>() != null) ||
                (WillowTerrainSettings.placementType == SpawnPlaceType.onObjectsOnly && hit.collider.gameObject.GetComponent<WillowTerrainSettings>() == null) ||
                (WillowTerrainSettings.placementType == SpawnPlaceType.onTerrainAndObjects)))
            {

                GameObject temp = Object.Instantiate(spawnableObject.Object, hit.point, Quaternion.identity);

                SetObjectRotation(spawnableObject, temp, hit.normal, spawnableObject.CustomEulersRotation);
                SetObjectColor(spawnableObject.ModifyColor, spawnableObject.ColorModPercentage, temp.GetComponent<WillowSpawnedObject>().Renderers);
                temp.transform.localPosition += GetObjectPositionAdd(spawnableObject);
                temp.transform.parent = GetObjectParent(spawnableObject);
                temp.transform.localScale = GetObjectScale(spawnableObject);

                temp.GetComponent<WillowSpawnedObject>().PositionAdd =
                    spawnableObject.ModifyPosition ? spawnableObject.PositionAddition : Vector3.zero;

                temp.GetComponent<WillowSpawnedObject>().Layer = spawnableObject.Layer;

                temp.GetComponent<WillowSpawnedObject>().SpawnableObject = spawnableObject; //Init(spawnableObject);

                temp.name = spawnableObject.Object.name;
                if (spawnableObject.RenameObject)
                    temp.name = spawnableObject.NewObjectName;

                if (WillowTerrainSettings.indexObjects)
                    temp.name += string.Format(WillowTerrainSettings.indexFormat, WillowTerrainSettings.spawnedIndecies);
                WillowTerrainSettings.spawnedIndecies++;

                if (spawnableObject.CenterObject)
                    temp.transform.localPosition += new Vector3(0, spawnableObject.Object.transform.localScale.y / 2, 0);

                WillowTerrainSettings.spawnedObjects.Add(temp);

                spawnedObjs.Add(temp);
            }
        }

        if (WillowTerrainSettings.autoSave) WillowFileManager.Write();
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
            if (UnityEngine.Random.Range(0, WillowTerrainSettings.eraseSmoothness + 1) == WillowTerrainSettings.eraseSmoothness || WillowTerrainSettings.eraseSmoothness == 0)
            {
                if (o != null)
                {
                    o.gameObject.hideFlags = hidden;
                    o.SetActive(false);
                    spawnedObjectsTemp.Remove(o);
                    WillowTerrainSettings.destroyedObjects.Add(o);

                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
        spawnedObjectsTemp.RemoveAll(o => o == null);
        WillowTerrainSettings.spawnedObjects = spawnedObjectsTemp;

        if (WillowTerrainSettings.autoSave) WillowFileManager.Write();
        WillowTerrainSettings.changelog.Push(new Change(WillowUtils.ChangeType.Erasure, null, objsToDestroy.ToList()));
        OnRepaint?.Invoke();
        EditorApplication.RepaintHierarchyWindow();
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

        List<SpawnableObject> spawnableObjects = WillowTerrainSettings.spawnableObjects;
        if (WillowTerrainSettings.ignoreInactiveLayers)
        {
            objsToExchange = objsToExchange.Where(gameObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(gameObject.GetComponent<WillowSpawnedObject>().Layer)]).ToArray();
            spawnableObjects = spawnableObjects.Where(spawnableObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(spawnableObject.Layer)]).ToList();
        }

        foreach (GameObject o in objsToExchange)
        {
            if (UnityEngine.Random.Range(0, WillowTerrainSettings.exchangeSmoothness + 1) == WillowTerrainSettings.exchangeSmoothness || WillowTerrainSettings.exchangeSmoothness == 0)
            {
                if (o != null)
                {
                    SpawnableObject spawnableObject = GetObject(spawnableObjects);
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
                }
            }
        }
        WillowTerrainSettings.changelog.Push(new Change(WillowUtils.ChangeType.Exchange, spawnedObjectsTemp.ToList(), objsToExchange.ToList()));
        spawnedObjectsTemp.RemoveAll(o => o == null);
        WillowTerrainSettings.spawnedObjects = spawnedObjectsTemp;
        if (WillowTerrainSettings.autoSave) WillowFileManager.Write();
        OnRepaint?.Invoke();
        EditorApplication.RepaintHierarchyWindow();
        EditorApplication.DirtyHierarchyWindowSorting();
    }

    public static SpawnableObject GetObject(List<SpawnableObject> spawnableObjects)
    {
        int[] chances = new int[spawnableObjects.Count];
        bool ableToSpawn = false;
        for (int i = 0; i < chances.Length; i++)
        {
            chances[i] = spawnableObjects[i].Spawn && spawnableObjects[i].Object != null ? spawnableObjects[i].SpawnChance : 0;
            if (chances[i] > 0) ableToSpawn = true;
        }
        if (!ableToSpawn) return null;
        return spawnableObjects[GetChance(chances)]; //new SpawnableObject(spawnableObjects[GetChance(chances)]);
    }
    public static void SetObjectRotation(SpawnableObject spawnableObject, GameObject spawnedObject, Vector3 normal, Vector3 custom)
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
    public static Transform GetObjectParent(SpawnableObject obj)
    {
        if (obj.CustomParent) return obj.Parent;
        else return WillowTerrainSettings.parent;
    }
    public static Vector3 GetObjectPositionAdd(SpawnableObject obj)
    {
        return obj.ModifyPosition ? obj.PositionAddition : Vector3.zero;
    }
    public static Vector3 GetObjectScale(SpawnableObject spawnableObject)
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
}
