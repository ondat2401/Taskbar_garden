using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleGarden
{
    public sealed class GardenFloatingWindow : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public RectTransform panel;
        public RectTransform canvasRect;

        public bool IsOpen => panel && panel.gameObject.activeSelf;

        public void Open()
        {
            if (panel) panel.gameObject.SetActive(true);
        }

        public void Close()
        {
            if (panel) panel.gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!panel || !canvasRect || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, eventData.position, eventData.pressEventCamera, out var pointer)) return;
            dragOffset = panel.anchoredPosition - pointer;
        }

        private Vector2 dragOffset;

        public void OnDrag(PointerEventData eventData)
        {
            if (!panel || !canvasRect || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, eventData.position, eventData.pressEventCamera, out var pointer)) return;
            Vector2 half = panel.rect.size * .5f;
            Rect bounds = canvasRect.rect;
            panel.anchoredPosition = new Vector2(
                bounds.width < panel.rect.width ? 0 : Mathf.Clamp(pointer.x + dragOffset.x, bounds.xMin + half.x, bounds.xMax - half.x),
                bounds.height < panel.rect.height ? 0 : Mathf.Clamp(pointer.y + dragOffset.y, bounds.yMin + half.y, bounds.yMax - half.y));
        }
    }
}
