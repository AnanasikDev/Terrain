using UnityEngine;
using UnityEngine.EventSystems;
public class TerrainScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject obj;

    public bool placeAsNormals = false;
    public bool centerObject = true;

    public int objectsAmount = 1; // if -1 then random
    public float radius = 25;

    public placeType place;

    public bool mouseOn { get; private set; }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        mouseOn = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        mouseOn = false;
    }

    public enum placeType
    {
        onTerrainOnly,
        onObjectsOnly,
        onTerrainAndObjects
    }
    
}
