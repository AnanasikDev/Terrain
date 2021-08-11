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
        if (ableToSpawn)
        {
            List<SpawnableObject> spawnableObjects = WillowTerrainSettings.spawnableObjects;
            if (WillowTerrainSettings.ignoreInactiveLayers)
                spawnableObjects = spawnableObjects
                    .Where(spawnableObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(spawnableObject.layer)])
                    .Where(spawnableObject => spawnableObject.spawn && spawnableObject.spawnableObject != null)
                    .ToList();


            bool anySpawnableObjects = spawnableObjects.Any();

            if (WillowTerrainSettings.spawnableObjects.Count > 0 && anySpawnableObjects)
            {
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

                        GameObject temp = Object.Instantiate(spawnableObject.spawnableObject, hit.point, Quaternion.identity);

                        SetObjectRotation(spawnableObject, temp, hit.normal, spawnableObject.customEulersRotation);
                        SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, temp);
                        temp.transform.localPosition += GetObjectPositionAdd(spawnableObject);
                        temp.transform.parent = GetObjectParent(spawnableObject);
                        temp.transform.localScale = GetObjectScale(spawnableObject);

                        temp.GetComponent<WillowSpawnedObject>().PositionAdd =
                            spawnableObject.modifyPosition ? spawnableObject.positionAddition : Vector3.zero;

                        temp.GetComponent<WillowSpawnedObject>().Layer = spawnableObject.layer;

                        temp.name = spawnableObject.spawnableObject.name;
                        if (spawnableObject.renameObject)
                            temp.name = spawnableObject.newObjectName;

                        if (WillowTerrainSettings.indexObjects)
                            temp.name += string.Format(WillowTerrainSettings.indexFormat, WillowTerrainSettings.spawnedIndecies);
                        WillowTerrainSettings.spawnedIndecies++;

                        if (spawnableObject.centerObject)
                            temp.transform.localPosition += new Vector3(0, spawnableObject.spawnableObject.transform.localScale.y / 2, 0);

                        WillowTerrainSettings.spawnedObjects.Add(temp);

                        spawnedObjs.Add(temp);
                    }
                }

                if (WillowTerrainSettings.autoSave) WillowFileManager.Write();
                WillowTerrainSettings.changelog.Push(new Change(WillowUtils.ChangeType.Placement, spawnedObjs, null));
                OnRepaint?.Invoke();
            }
            else
            {
                if (WillowTerrainSettings.debugMode) Debug.LogError(WillowUtils.FormatLog("There are no objects to spawn!"));
            }
        }
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
            spawnableObjects = spawnableObjects.Where(spawnableObject => WillowTerrainSettings.layersState[WillowTerrainSettings.layersName.IndexOf(spawnableObject.layer)]).ToList();
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
                    Transform parent = WillowTerrainSettings.exchangeParent ? o.transform.parent : spawnableObject.parent;
                    GameObject spawned = Object.Instantiate(spawnableObject.spawnableObject, position, Quaternion.identity, parent);
                    spawned.name = o.name;

                    if (WillowTerrainSettings.exchangeRotation)
                        spawned.transform.localRotation = o.transform.localRotation;
                    else
                        SetObjectRotation(spawnableObject, spawned, normal, spawnableObject.customEulersRotation);
                    if (WillowTerrainSettings.exchangeColor)
                        SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, spawned, o.GetComponent<WillowSpawnedObject>().Renderer.sharedMaterial.color);
                    else SetObjectColor(spawnableObject.modColor, spawnableObject.colorModPercentage, spawned);

                    spawned.GetComponent<WillowSpawnedObject>().PositionAdd =
                        spawnableObject.modifyPosition ? spawnableObject.positionAddition : Vector3.zero;

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
            chances[i] = spawnableObjects[i].spawn && spawnableObjects[i].spawnableObject != null ? spawnableObjects[i].spawnChance : 0;
            if (chances[i] > 0) ableToSpawn = true;
        }
        if (!ableToSpawn) return null;
        return new SpawnableObject(spawnableObjects[GetChance(chances)]);
    }
    public static void SetObjectRotation(SpawnableObject spawnableObject, GameObject spawnedObject, Vector3 normal, Vector3 custom)
    {
        if (spawnableObject.rotationType == RotationType.Static)
        {
            spawnedObject.transform.localEulerAngles = custom;
        }
        else if (spawnableObject.rotationType == RotationType.Random)
        {
            spawnedObject.transform.localEulerAngles = GetRandomRotation();
        }
        else if (spawnableObject.rotationType == RotationType.AsNormal)
        {
            spawnedObject.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
        }
        else if (spawnableObject.rotationType == RotationType.RandomAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            spawnedObject.transform.Rotate(GetRandomRotation(), Space.Self);
        }
        else if (spawnableObject.rotationType == RotationType.StaticAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            spawnedObject.transform.Rotate(custom, Space.Self);
        }
        else if (spawnableObject.rotationType == RotationType.LerpedStaticAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(custom), Quaternion.FromToRotation(Vector3.up, normal), spawnableObject.lerpValue);
        }
        else if (spawnableObject.rotationType == RotationType.LerpedRandomAsNormal)
        {
            float lerpV = spawnableObject.lerpValue;
            if (spawnableObject.randomizeLerpValue) lerpV = UnityEngine.Random.Range(spawnableObject.minLerpValue, spawnableObject.maxLerpValue);

            spawnedObject.transform.rotation = Quaternion.Lerp(Quaternion.Euler(custom), Quaternion.FromToRotation(Vector3.up, normal), lerpV);
            spawnedObject.transform.Rotate(GetRandomRotation(), Space.Self);
        }
        else if (spawnableObject.rotationType == RotationType.LerpedAsPrefabAsNormal)
        {
            spawnedObject.transform.rotation = Quaternion.Lerp(spawnableObject.spawnableObject.transform.rotation, Quaternion.FromToRotation(Vector3.up, normal), spawnableObject.lerpValue);
        }
        else // if AsPrefab
        {
            spawnedObject.transform.rotation = spawnableObject.spawnableObject.transform.rotation;
        }

        Vector3 GetRandomRotation()
        {
            float x = 0;
            float y = 0;
            float z = 0;

            if (spawnableObject.multiRotationAxis)
            {
                x = UnityEngine.Random.Range(spawnableObject.randomMinRotation.x, spawnableObject.randomMaxRotation.x);
                y = UnityEngine.Random.Range(spawnableObject.randomMinRotation.y, spawnableObject.randomMaxRotation.y);
                z = UnityEngine.Random.Range(spawnableObject.randomMinRotation.z, spawnableObject.randomMaxRotation.z);
            }
            else
            {
                string axis = spawnableObject.rotationAxis.ToString();

                if (axis.Contains("X")) x = UnityEngine.Random.Range(0f, 360f);
                if (axis.Contains("Y")) y = UnityEngine.Random.Range(0f, 360f);
                if (axis.Contains("Z")) z = UnityEngine.Random.Range(0f, 360f);
            }

            return new Vector3(x, y, z);
        }
    }
    public static void SetObjectColor(bool modifyColor, float colorModificationPercentage, GameObject gameObject, Color? color = null)
    {
        if (modifyColor)
        {
            Renderer renderer = gameObject.GetComponent<WillowSpawnedObject>().Renderer;
            var tempMaterial = new Material(renderer.sharedMaterial);
            if (color == null)
                tempMaterial.color *= UnityEngine.Random.Range(1 - (colorModificationPercentage / 100), 1 + (colorModificationPercentage / 100));
            else
                tempMaterial.color = (Color)color;
            renderer.sharedMaterial = tempMaterial;
        }
    }
    public static Transform GetObjectParent(SpawnableObject obj)
    {
        if (obj.customParent) return obj.parent;
        else return WillowTerrainSettings.parent;
    }
    public static Vector3 GetObjectPositionAdd(SpawnableObject obj)
    {
        return obj.modifyPosition ? obj.positionAddition : Vector3.zero;
    }
    public static Vector3 GetObjectScale(SpawnableObject spawnableObject)
    {
        Vector3 scale = Vector3.one;
        switch (spawnableObject.scaleType)
        {
            case ScaleType.AsPrefab:
                scale = spawnableObject.spawnableObject.transform.localScale;
                break;
            case ScaleType.Random:
                if (spawnableObject.modScale)
                {
                    float x = 1;
                    float y = 1;
                    float z = 1;

                    string axis = spawnableObject.scaleAxis.ToString();
                    if (spawnableObject.separateScaleAxis)
                    {
                        if (axis.Contains("X")) x = UnityEngine.Random.Range(spawnableObject.scaleMinSeparated.x, spawnableObject.scaleMaxSeparated.x);
                        if (axis.Contains("Y")) y = UnityEngine.Random.Range(spawnableObject.scaleMinSeparated.y, spawnableObject.scaleMaxSeparated.y);
                        if (axis.Contains("Z")) z = UnityEngine.Random.Range(spawnableObject.scaleMinSeparated.z, spawnableObject.scaleMaxSeparated.z);
                    }
                    else
                    {
                        float value = UnityEngine.Random.Range(spawnableObject.scaleMin, spawnableObject.scaleMax);
                        x = y = z = value;
                    }

                    scale = new Vector3(x, y, z);
                }

                break;
            case ScaleType.Static:
                scale = spawnableObject.customScale;
                break;
        }
        return scale;
    }
}
