using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rokid.UXR.Components
{
#if UNITY_2021_1_OR_NEWER

    [RequireComponent(typeof(Image))]
    public class SliderAdsorbArea : AdsorbArea, IDragHandler, IPointerMoveHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {

        [SerializeField]
        private bool freezeX = true;
        [SerializeField]
        private bool freezeY = true;
        [SerializeField]
        private Transform dragHandlerTsf;

        private IDragHandler dragHandler;
        private IBeginDragHandler beginDragHandler;
        private IEndDragHandler endDragHandler;
        private IPointerDownHandler pointerDownHandler;
        private IPointerUpHandler pointerUpHandler;
        private RectTransform rect;

        protected override void Start()
        {
            Assert.IsNotNull(dragHandlerTsf);
            dragHandler = dragHandlerTsf.GetComponent<IDragHandler>();
            beginDragHandler = dragHandlerTsf.GetComponent<IBeginDragHandler>();
            endDragHandler = dragHandlerTsf.GetComponent<IEndDragHandler>();
            pointerDownHandler = dragHandlerTsf.GetComponent<IPointerDownHandler>();
            pointerUpHandler = dragHandlerTsf.GetComponent<IPointerUpHandler>();
            rect = GetComponent<RectTransform>();
        }

        public override Vector3 GetBezierAdsorbPoint(int pointerId)
        {
            if (pointerDataHandles.TryGetValue(pointerId, out PointerDataHandle pointerData))
            {
                if (pointerData.active)
                {
                    return new Vector3(freezeX ? transform.position.x : pointerData.hoverPosition.x, freezeY ? transform.position.y : pointerData.hoverPosition.y, pointerData.hoverPosition.z);
                }
                else
                {
                    return pointerData.hoverPosition;
                }
            }
            return Vector3.zero;
        }

        public override Vector3 GetBezierAdsorbNormal(int pointerId)
        {
            return transform.forward;
        }


        public void OnPointerMove(PointerEventData eventData)
        {
            if (pointerDataHandles.TryGetValue(eventData.pointerId, out PointerDataHandle pointerData))
            {
                pointerData.hoverPosition = eventData.pointerCurrentRaycast.worldPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (pointerDataHandles.TryGetValue(eventData.pointerId, out PointerDataHandle pointerData))
            {
                if (pointerData.active)
                {
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint))
                    {
                        pointerData.hoverPosition = worldPoint;
                        dragHandler?.OnDrag(eventData);
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (pointerDataHandles.TryGetValue(eventData.pointerId, out PointerDataHandle pointerData))
            {
                if (pointerData.active)
                {
                    pointerData.dragging = true;
                    beginDragHandler?.OnBeginDrag(eventData);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (pointerDataHandles.TryGetValue(eventData.pointerId, out PointerDataHandle pointerData))
            {
                if (pointerData.active)
                {
                    endDragHandler?.OnEndDrag(eventData);
                    if (pointerData.hovering == false)
                    {
                        pointerDataHandles.Remove(pointerData.pointerId);
                        ChangeActivePointer();
                    }
                    else
                    {
                        pointerData.dragging = false;
                    }
                }
                else
                {
                    if (pointerData.hovering == false)
                    {
                        pointerDataHandles.Remove(pointerData.pointerId);
                    }
                    else
                    {
                        pointerData.dragging = false;
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerDataHandles.TryGetValue(eventData.pointerId, out PointerDataHandle pointerData))
            {
                if (pointerData.active)
                {
                    pointerDownHandler?.OnPointerDown(eventData);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerDataHandles.TryGetValue(eventData.pointerId, out PointerDataHandle pointerData))
            {
                if (pointerData.active)
                {
                    pointerUpHandler?.OnPointerUp(eventData);
                }
            }
        }
    }
#endif
}

