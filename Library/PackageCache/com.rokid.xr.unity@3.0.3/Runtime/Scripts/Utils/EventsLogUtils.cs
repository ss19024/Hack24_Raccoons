using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Utility
{
    public class EventsLogUtils : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IRayPointerEnter, IRayPointerExit, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private Text logText;
        [Tooltip("是否打印(UGUI)事件")]

        [SerializeField]
        private bool logPointerEvent = true;
        [Tooltip("是否打印自定义手势事件")]

        [SerializeField]
        private bool logCustomEvent = true;
        private int clickCount = 0;
        private int downCount = 0;
        private int upCount = 0;
        private int enterCount = 0;
        private int exitCount = 0;
        private int customClickCount = 0;

        private TextMesh text;

        private void Start()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text = $" OnPointerEnter, pointerEnter.Count: {++enterCount} {this.name}";
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text = $" OnPointerDown, pointerDown.Count: {++downCount} {this.name}";
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnPointerUp, pointerUp.Count: {++upCount} {this.name}";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnPointerClick, pointerClick.Count: {++clickCount} {this.name}";
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnPointerExit, pointerExit.Count: {++exitCount} {this.name}";
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnBeginDrag,  {this.name}";
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnEndDrag,  {this.name}";
            }
        }

        public void OnRayPointerEnter(PointerEventData eventData)
        {
            if (logCustomEvent)
            {
                if (logText != null)
                    logText.text = $" OnGesPointerClick, customPointerClick.Count: {++customClickCount} {this.name}";
            }
        }

        public void OnRayPointerExit(PointerEventData eventData)
        {
            if (logCustomEvent)
            {
                if (logText != null)
                    logText.text += $" OnGesPointerClick, customPointerClick.Count: {++customClickCount} {this.name}";
            }
        }
    }
}
