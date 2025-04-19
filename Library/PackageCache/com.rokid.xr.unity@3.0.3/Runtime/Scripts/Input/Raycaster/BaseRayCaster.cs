using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using static UnityEngine.EventSystems.PointerEventData;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public abstract class BaseRayCaster : MonoBehaviour
    {
        [SerializeField, Tooltip("Threshold for determining whether to start dragging")]
        public float dragThreshold = 0.01f;
        [SerializeField, Tooltip("Cache the current hovering object")]
        protected GameObject hoveringObj;
        [SerializeField, Tooltip("Cache selected object")]
        protected GameObject selectedObj;
        [SerializeField, Tooltip("This attribute only affects UI graphics, detecting the front and back sides of the UI")]
        private bool ignoreReversedGraphics = true;
        [SerializeField, Tooltip("Is detect UI graphics")]
        private bool raycastGraphic = true;

        [SerializeField, Tooltip("Is detect physical collisions")]
        private bool raycastPhysical = true;
        [SerializeField, Tooltip("The ray origin position")]
        protected Transform rayOrigin;

        [SerializeField, Tooltip("The raycast level")]
        internal LayerMask raycastMask = (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6 | 1 << 7);
        [SerializeField, Tooltip("is dragging")]
        protected bool dragging = false;
        [SerializeField]
        protected RaycastResult result;
        [SerializeField]
        protected RayInteractor rayInteractor;
        [SerializeField]
        protected Camera eventCamera;
        [SerializeField]
        protected bool sendGlobalEvent = true;
        [SerializeField]
        protected BaseInput inputOverride;
        private BaseInput defaultInput;
        /// <summary>
        /// the time to judge click
        /// </summary>
        protected float clickTime = 0.5f;
        /// <summary>
        /// the has press time
        /// </summary>
        protected float pressTime = 0;

        /// <summary>
        /// The distance from the ray origin to the contact when the cache starts dragging
        /// </summary>
        protected float oriHitPointDis = 0;
        protected Ray ray;
        protected Vector3 oriHitPoint;
        protected PointerEventData pointerEventData;
        protected bool hasDown;
        protected Vector2 screenCenterPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        private static readonly Comparison<RaycastResult> raycastComparer = RaycastComparer;
        protected readonly List<RaycastResult> sortedRaycastResults = new List<RaycastResult>();
        private static readonly RaycastHit[] hits = new RaycastHit[64];
        public BaseInput input
        {
            get
            {
                if (inputOverride != null)
                {
                    return inputOverride;
                }
                if (defaultInput == null)
                {
                    BaseInput[] components = GetComponents<BaseInput>();
                    foreach (BaseInput baseInput in components)
                    {
                        if (baseInput != null && baseInput.GetType() == typeof(BaseInput))
                        {
                            defaultInput = baseInput;
                            break;
                        }
                    }
                    if (defaultInput == null)
                    {
                        defaultInput = base.gameObject.AddComponent<BaseInput>();
                    }
                }
                return defaultInput;
            }
        }

        public BaseInput InputOverride
        {
            get
            {
                return inputOverride;
            }
            set
            {
                inputOverride = value;
            }
        }

        protected virtual void Init()
        {
            pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.button = InputButton.Left;
            if (rayInteractor == null)
            {
                rayInteractor = GetComponent<RayInteractor>();
            }
        }

        private int GetRayIdentifier()
        {
            if (rayInteractor == null)
            {
                return 0;
            }
            else
            {
                return rayInteractor.realId;
            }
        }

        protected virtual void Start()
        {
            Init();
            if (eventCamera == null)
            {
                eventCamera = GetEventCamera();
            }
        }

        protected virtual Camera GetEventCamera()
        {
            return MainCameraCache.mainCamera;
        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void OnDisable()
        {
            if (dragging)
            {
                GameObject pointerDragEndHandler = ExecuteEvents.GetEventHandler<IRayEndDrag>(selectedObj);
                if (pointerDragEndHandler)
                {
                    RKExecuteEvents.Execute(pointerDragEndHandler, pointerEventData,
                                 RKExecuteEvents.rayEndDragHandler);
                    if (sendGlobalEvent)
                    {
                        RKPointerListener.OnPointerDragEnd?.Invoke(pointerEventData);
                        RKPointerListener.OnPhysicalDragEnd?.Invoke(pointerEventData);
                    }
                }
                dragging = false;
            }
            else
            {
                if (pressTime < clickTime && selectedObj == result.gameObject && result.gameObject != null)
                {
                    GameObject pointerClickHandler = ExecuteEvents.GetEventHandler<IRayPointerClick>(selectedObj);
                    if (pointerClickHandler)
                    {
                        RKExecuteEvents.Execute(pointerClickHandler, pointerEventData,
                                     RKExecuteEvents.rayPointerClickHandler);
                    }
                }
            }
            if (hasDown)
            {
                if (selectedObj != null)
                {
                    GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IRayPointerUp>(selectedObj);
                    if (pointerUpHandler)
                    {
                        RKExecuteEvents.Execute(pointerUpHandler, pointerEventData,
                                     RKExecuteEvents.rayPointerUpHandler);
                        if (sendGlobalEvent)
                        {
                            RKPointerListener.OnPointerUp?.Invoke(pointerEventData);
                            RKPointerListener.OnPhysicalPointerUp?.Invoke(pointerEventData);
                        }
                    }
                }
                hasDown = false;
            }
            selectedObj = null;
            pointerEventData.pointerDrag = null;
        }

        protected void UpdatePointerEventData()
        {
            pointerEventData.pointerCurrentRaycast = result;
            pointerEventData.pointerEnter = result.gameObject;
            pointerEventData.pointerId = GetRayIdentifier();
        }

        private void Update()
        {
            if (rayOrigin != null)
            {
                ray.origin = rayOrigin.position;
                ray.direction = rayOrigin.forward;
            }
            else
            {
                ray.origin = transform.position;
                ray.direction = transform.forward;
            }
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
#endif
            if (input.GetMouseButtonDown(0))
            {
                if (!TriggerPointerDown())
                {
                    pressTime = 0;
                    Raycast(ray, Mathf.Infinity, sortedRaycastResults);
                    result = FirstRaycastResult();
                    UpdatePointerEventData();
                    if (result.gameObject != null)
                    {
                        selectedObj = result.gameObject;
                        OnFirstSelect();
                        hasDown = true;
                        GameObject pointerDownHandler = ExecuteEvents.GetEventHandler<IRayPointerDown>(selectedObj);
                        if (pointerDownHandler)
                        {
                            RKExecuteEvents.Execute(pointerDownHandler, pointerEventData,
                                         RKExecuteEvents.rayPointerDownHandler);
                            if (sendGlobalEvent)
                            {
                                RKPointerListener.OnPointerDown?.Invoke(pointerEventData);
                                RKPointerListener.OnPhysicalPointerDown?.Invoke(pointerEventData);
                            }
                        }
                        pointerEventData.pointerDrag = ExecuteEvents.GetEventHandler<IRayBeginDrag>(selectedObj);
                    }
                    else
                    {
                        ProcessNothingDownEvent(pointerEventData);
                    }
                }
            }
            else if (!dragging)
            {
                Raycast(ray, Mathf.Infinity, sortedRaycastResults);
                result = FirstRaycastResult();
                UpdatePointerEventData();
                if (input.GetMouseButtonUp(0) && result.gameObject == null)
                {
                    ProcessNothingUpEvent(pointerEventData);
                }
                if (result.gameObject != null)
                {
                    if (hoveringObj == null)
                    {
                        hoveringObj = result.gameObject;
                        GameObject enterHandler = ExecuteEvents.GetEventHandler<IRayPointerEnter>(hoveringObj);
                        if (enterHandler)
                        {
                            RKExecuteEvents.Execute(enterHandler, pointerEventData,
                                         RKExecuteEvents.rayPointerEnterHandler);
                            if (sendGlobalEvent)
                            {
                                RKPointerListener.OnPointerEnter?.Invoke(pointerEventData);
                                RKPointerListener.OnPointerEnterWithObj?.Invoke(pointerEventData, pointerEventData.pointerEnter);
                                RKPointerListener.OnPhysicalPointerEnter?.Invoke(pointerEventData);
                            }
                        }
                    }
                    else
                    {
                        GameObject pointerHoverHandler = null;
                        if (hoveringObj != result.gameObject)
                        {
                            GameObject pointerExitHandler = ExecuteEvents.GetEventHandler<IRayPointerExit>(hoveringObj);
                            if (pointerExitHandler)
                            {
                                RKExecuteEvents.Execute(pointerExitHandler, pointerEventData,
                                             RKExecuteEvents.rayPointerExitHandler);
                            }
                            if (sendGlobalEvent)
                            {
                                RKPointerListener.OnPointerExit?.Invoke(pointerEventData, hoveringObj);
                                RKPointerListener.OnPhysicalPointerExit?.Invoke(pointerEventData, hoveringObj);
                            }
                            hoveringObj = result.gameObject;
                            GameObject pointerEnterHandler = ExecuteEvents.GetEventHandler<IRayPointerEnter>(hoveringObj);
                            pointerHoverHandler = ExecuteEvents.GetEventHandler<IRayPointerHover>(hoveringObj);
                            if (pointerEnterHandler)
                            {
                                RKExecuteEvents.Execute(pointerEnterHandler, pointerEventData,
                                             RKExecuteEvents.rayPointerEnterHandler);
                            }
                            if (sendGlobalEvent)
                            {
                                RKPointerListener.OnPointerEnter?.Invoke(pointerEventData);
                                RKPointerListener.OnPointerEnterWithObj?.Invoke(pointerEventData, pointerEventData.pointerEnter);
                                RKPointerListener.OnPhysicalPointerEnter?.Invoke(pointerEventData);
                            }
                        }
                        if (pointerHoverHandler != null)
                        {
                            RKExecuteEvents.Execute(pointerHoverHandler, pointerEventData,
                                                                        RKExecuteEvents.rayPointerHoverHandler);
                        }
                        if (sendGlobalEvent)
                        {
                            RKPointerListener.OnPointerHover?.Invoke(pointerEventData);
                            RKPointerListener.OnPhysicalPointerHover?.Invoke(pointerEventData);
                        }
                        if (input.GetMouseButton(0) && !dragging)
                        {
                            pressTime += Time.deltaTime;
                            if (selectedObj == hoveringObj)
                            {
                                Vector3 delta = CalDragDelta();
                                if (CanDrag(delta) && pointerEventData.pointerDrag)
                                {
                                    dragging = true;
                                    RKExecuteEvents.Execute(pointerEventData.pointerDrag, pointerEventData,
                                                 RKExecuteEvents.rayBeginDragHandler);
                                    if (sendGlobalEvent)
                                    {
                                        RKPointerListener.OnPointerDragBegin?.Invoke(pointerEventData);
                                        RKPointerListener.OnPhysicalDragBegin?.Invoke(pointerEventData);
                                    }
                                    OnBeginDrag();
                                    if (hasDown)
                                    {
                                        hasDown = false;
                                        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IRayPointerUp>(selectedObj);
                                        if (pointerUpHandler)
                                        {
                                            RKExecuteEvents.Execute(pointerUpHandler, pointerEventData,
                                                         RKExecuteEvents.rayPointerUpHandler);
                                            if (sendGlobalEvent)
                                            {
                                                RKPointerListener.OnPointerUp?.Invoke(pointerEventData);
                                                RKPointerListener.OnPhysicalPointerUp?.Invoke(pointerEventData);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (hoveringObj != null)
                {
                    GameObject pointerExitHandler = ExecuteEvents.GetEventHandler<IRayPointerExit>(hoveringObj);
                    if (pointerExitHandler)
                    {
                        RKExecuteEvents.Execute(pointerExitHandler, pointerEventData,
                                     RKExecuteEvents.rayPointerExitHandler);
                        if (sendGlobalEvent)
                        {
                            RKPointerListener.OnPointerExit?.Invoke(pointerEventData, hoveringObj);
                            RKPointerListener.OnPhysicalPointerExit?.Invoke(pointerEventData, hoveringObj);
                        }
                    }
                    result.gameObject = null;
                    hoveringObj = null;
                    pointerEventData.pointerDrag = null;
                }
            }

            if (dragging)
            {
                ProcessDrag(ray);
            }

            //处理点击释放逻辑
            if (input.GetMouseButtonUp(0))
            {
                if (dragging == false)
                {
                    if (hasDown)
                    {
                        hasDown = false;
                        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IRayPointerUp>(selectedObj);
                        if (pointerUpHandler)
                        {
                            RKExecuteEvents.Execute(pointerUpHandler, pointerEventData,
                                         RKExecuteEvents.rayPointerUpHandler);
                            if (sendGlobalEvent)
                            {
                                RKPointerListener.OnPointerUp?.Invoke(pointerEventData);
                                RKPointerListener.OnPhysicalPointerUp?.Invoke(pointerEventData);
                            }
                        }
                    }
                    if (pressTime < clickTime && selectedObj == result.gameObject && result.gameObject != null)
                    {
                        //发送点击事件
                        GameObject pointerClickHandler = ExecuteEvents.GetEventHandler<IRayPointerClick>(selectedObj);
                        if (pointerClickHandler)
                        {
                            RKExecuteEvents.Execute(pointerClickHandler, pointerEventData,
                                         RKExecuteEvents.rayPointerClickHandler);
                            if (sendGlobalEvent)
                            {
                                RKPointerListener.OnPointerClick?.Invoke(pointerEventData);
                            }
                        }
                    }
                    selectedObj = null;
                    pointerEventData.pointerDrag = null;
                }
            }

            //Process drag end
            if (dragging && !input.GetMouseButton(0))
            {
                dragging = false;
                if (selectedObj != null)
                {
                    GameObject pointerDragEndHandler = ExecuteEvents.GetEventHandler<IRayEndDrag>(selectedObj);
                    if (pointerDragEndHandler)
                    {
                        RKExecuteEvents.Execute(pointerDragEndHandler, pointerEventData,
                                     RKExecuteEvents.rayEndDragHandler);
                        if (sendGlobalEvent)
                            RKPointerListener.OnPointerDragEnd?.Invoke(pointerEventData);
                    }
                    selectedObj = null;
                    pointerEventData.pointerDrag = null;
                }
            }
            else
            {
                StatusRefresh();
            }
        }
        #region  Virtual Method
        protected virtual void StatusRefresh()
        {

        }
        protected virtual bool CanDrag(Vector3 delta)
        {
            return !dragging && Vector3.SqrMagnitude(delta) >= dragThreshold * dragThreshold;
        }
        protected virtual bool TriggerPointerDown()
        {
            return false;
        }
        protected virtual void OnFirstSelect()
        {
            oriHitPoint = result.worldPosition;
        }
        protected virtual Vector3 CalDragDelta()
        {
            return result.worldPosition - oriHitPoint;
        }
        protected virtual void OnBeginDrag()
        {
            oriHitPointDis = Vector3.Distance(result.worldPosition, ray.origin);
            oriHitPoint = result.worldPosition;
        }
        protected virtual bool ProcessDrag(Ray ray) { return false; }
        protected virtual void ProcessNothingUpEvent(PointerEventData eventData)
        {
            if (sendGlobalEvent)
                RKPointerListener.OnPointerNothingUp?.Invoke(eventData);
        }
        protected virtual void ProcessNothingDownEvent(PointerEventData eventData)
        {
            if (sendGlobalEvent)
                RKPointerListener.OnPointerNothingDown?.Invoke(eventData);
        }
        #endregion

        #region  Raycast Method
        private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                var lhsEventCamera = lhs.module.eventCamera;
                var rhsEventCamera = rhs.module.eventCamera;
                if (lhsEventCamera != null && rhsEventCamera != null && lhsEventCamera.depth != rhsEventCamera.depth)
                {
                    // need to reverse the standard compareTo
                    if (lhsEventCamera.depth < rhsEventCamera.depth)
                        return 1;
                    if (lhsEventCamera.depth == rhsEventCamera.depth)
                        return 0;

                    return -1;
                }

                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }

            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

            // comparing depth only makes sense if the two raycast results have the same root canvas (case 912396)
            if (lhs.module != null && rhs.module != null)
            {
                if (lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster)
                    return rhs.depth.CompareTo(lhs.depth);
            }

            if (lhs.distance != rhs.distance)
                return lhs.distance.CompareTo(rhs.distance);

            return lhs.index.CompareTo(rhs.index);
        }

        /// <summary> Raycasts. </summary>
        /// <param name="distance">       The distance.</param>
        /// <param name="raycastResults"> The raycast results.</param>
        public void Raycast(float distance, List<RaycastResult> raycastResults)
        {
            Raycast(ray, distance, raycastResults);
        }


        /// <summary> Raycasts. </summary>
        /// <param name="ray">            The ray.</param>
        /// <param name="distance">       The distance.</param>
        /// <param name="raycastResults"> The raycast results.</param>
        public void Raycast(Ray ray, float distance, List<RaycastResult> raycastResults)
        {
            raycastResults.Clear();
            if (raycastGraphic)
                GraphicRaycast(ignoreReversedGraphics, ray, distance, raycastResults);
            if (raycastPhysical)
                PhysicsRaycast(ray, distance, raycastResults);
            raycastResults.Sort(raycastComparer);
        }


        /// <summary> Physics raycast. </summary>
        /// <param name="ray">            The ray.</param>
        /// <param name="distance">       The distance.</param>
        /// <param name="raycastResults"> The raycast results.</param>
        public virtual void PhysicsRaycast(Ray ray, float distance, List<RaycastResult> raycastResults)
        {
            var hitCount = Physics.RaycastNonAlloc(ray, hits, distance, raycastMask);
            for (int i = 0; i < hitCount; ++i)
            {
                // RKLog.Debug("=====BaseRayCast==== Physical Raycast:" + hits[i].collider.gameObject.name);
                if (!hits[i].collider.GetComponent<RayInteractable>() && !hits[i].collider.GetComponentInParent<RayInteractable>())
                    continue;
                raycastResults.Add(new RaycastResult
                {
                    gameObject = hits[i].collider.gameObject,
                    distance = hits[i].distance,
                    worldPosition = hits[i].point,
                    worldNormal = hits[i].normal,
                    screenPosition = screenCenterPoint,
                    index = raycastResults.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                });
            }
        }

        public RaycastResult FirstRaycastResult()
        {
            for (int i = 0, imax = sortedRaycastResults.Count; i < imax; ++i)
            {
                return sortedRaycastResults[i];
            }
            return default(RaycastResult);
        }

        public RaycastResult FirstRaycastResult(List<RaycastResult> sortedRaycastResults)
        {
            for (int i = 0, imax = sortedRaycastResults.Count; i < imax; ++i)
            {
                return sortedRaycastResults[i];
            }
            return default(RaycastResult);
        }

        protected virtual Vector2 GetEventPosition(Ray ray, Camera eventCamera, Graphic graphic)
        {
            if (Utils.GetWorldPointInRectangle(graphic, ray, out Vector3 worldPoint))
            {
                return eventCamera.WorldToScreenPoint(worldPoint);
            }
            return screenCenterPoint;
        }

        /// <summary> Graphic raycast. </summary>
        /// <param name="canvas">                 The canvas.</param>
        /// <param name="ignoreReversedGraphics"> True to ignore reversed graphics.</param>
        /// <param name="ray">                    The ray.</param>
        /// <param name="distance">               The distance.</param>
        /// <param name="raycaster">              The raycaster.</param>
        /// <param name="raycastResults">         The raycast results.</param>
        public virtual void GraphicRaycast(bool ignoreReversedGraphics, Ray ray, float distance, List<RaycastResult> raycastResults)
        {
            foreach (Canvas canvas in CanvasRegister.canvasList)
            {
                var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
                for (int i = 0; i < graphics.Count; ++i)
                {
                    var graphic = graphics[i];

                    // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                    if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
                        continue;

                    Vector2 screenPoint = GetEventPosition(ray, eventCamera, graphic);

                    if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenPoint, eventCamera))
                        continue;

                    if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
                        continue;

                    if (eventCamera != null && ignoreReversedGraphics && Vector3.Dot(eventCamera.transform.forward, graphic.transform.forward) < 0)
                        continue;

                    float dist;
                    //The intersection of the plane and the ray
                    new Plane(graphic.transform.forward, graphic.transform.position).Raycast(ray, out dist);
                    if (float.IsNaN(dist) || dist > distance)
                        continue;

                    if (graphic.Raycast(screenPoint, eventCamera))
                    {
                        raycastResults.Add(new RaycastResult
                        {
                            gameObject = graphic.gameObject,
                            distance = dist,
                            worldPosition = ray.GetPoint(dist),
                            worldNormal = -graphic.transform.forward,
                            screenPosition = screenCenterPoint,
                            index = raycastResults.Count,
                            depth = graphic.depth,
                            sortingLayer = canvas.sortingLayerID,
                            sortingOrder = canvas.sortingOrder
                        });
                    }
                }
            }
        }
        #endregion
    }
}
