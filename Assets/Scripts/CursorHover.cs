using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetNormal();
    }
}