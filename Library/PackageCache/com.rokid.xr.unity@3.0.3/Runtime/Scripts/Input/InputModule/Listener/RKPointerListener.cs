using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Global Event Listener
    /// </summary>
    public static class RKPointerListener
    {
        public static Action<PointerEventData> OnPointerEnter;
        public static Action<PointerEventData, GameObject> OnPointerEnterWithObj;
        public static Action<PointerEventData, GameObject> OnPointerExit;
        public static Action<PointerEventData> OnPointerHover;
        public static Action<PointerEventData> OnPointerDown;
        public static Action<PointerEventData> OnPointerUp;
        public static Action<PointerEventData> OnPointerClick;
        public static Action<PointerEventData> OnPointerDrag;
        public static Action<PointerEventData> OnPointerDragBegin;
        public static Action<PointerEventData> OnPointerDragEnd;
        public static Action<PointerEventData> OnPointerNothingUp;
        public static Action<PointerEventData> OnPointerNothingDown;

        // Add For UI Graphic Raycast
        public static Action<PointerEventData> OnGraphicPointerEnter;
        public static Action<PointerEventData> OnGraphicPointerHover;
        public static Action<PointerEventData> OnGraphicPointerExit;
        public static Action<PointerEventData> OnGraphicPointerDown;
        public static Action<PointerEventData> OnGraphicPointerUp;
        public static Action<PointerEventData> OnGraphicDragBegin;
        public static Action<PointerEventData> OnGraphicDragEnd;

        // Add For Physical Raycast
        public static Action<PointerEventData> OnPhysicalPointerEnter;
        public static Action<PointerEventData> OnPhysicalPointerHover;
        public static Action<PointerEventData, GameObject> OnPhysicalPointerExit;
        public static Action<PointerEventData> OnPhysicalPointerDown;
        public static Action<PointerEventData> OnPhysicalPointerUp;
        public static Action<PointerEventData> OnPhysicalDragBegin;
        public static Action<PointerEventData> OnPhysicalDragEnd;
    }
}
