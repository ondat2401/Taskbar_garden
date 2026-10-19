using UnityEngine;
using UnityEngine.EventSystems;

namespace IdleGarden
{
    public sealed class WindowHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public DesktopWindow window;
        public bool resize;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (resize) window.BeginResize(); else window.BeginDrag();
        }
        public void OnPointerUp(PointerEventData eventData) { window.EndInteraction(); }
    }
}
