using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Utility
{
    public class OnlyDraggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private void OnDisable()
        {
            if (EventSystem.current != null)
                EventSystem.current.pixelDragThreshold = InputModuleManager.Instance.GetActiveEventInput().PixelDragThreshold;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current != null)
                EventSystem.current.pixelDragThreshold = 0;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current != null)
                EventSystem.current.pixelDragThreshold = InputModuleManager.Instance.GetActiveEventInput().PixelDragThreshold;
        }
    }
}
