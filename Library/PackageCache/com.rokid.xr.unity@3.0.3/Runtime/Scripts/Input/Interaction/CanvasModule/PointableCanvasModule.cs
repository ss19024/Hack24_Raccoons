using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEngine.UI;
using Rokid.UXR.Utility;
using Rokid.UXR.Module;

namespace Rokid.UXR.Interaction
{
    public class PointableCanvasEventArgs
    {
        public readonly Canvas Canvas;
        public readonly GameObject Hovered;
        public readonly bool Dragging;

        public PointableCanvasEventArgs(Canvas canvas, GameObject hovered, bool dragging)
        {
            Canvas = canvas;
            Hovered = hovered;
            Dragging = dragging;
        }
    }

    /// <summary>
    /// IPointerInteractableModule manages all InteractableCanvas events in
    /// the scene and translates them into pointer events for Unity Canvas UIs.
    /// </summary>
    public class PointableCanvasModule : PointerInputModule
    {
        public static event Action<PointableCanvasEventArgs> WhenSelected;

        public static event Action<PointableCanvasEventArgs> WhenUnselected;

        public static event Action<PointableCanvasEventArgs> WhenSelectableHovered;

        public static event Action<PointableCanvasEventArgs> WhenSelectableUnhovered;

        [SerializeField]
        private bool _useInitialPressPositionForDrag = true;
        private bool childSendPointerHoverToParent = true;
        private Camera _pointerEventCamera;
        private static PointableCanvasModule _instance = null;
        [SerializeField]
        private GameObject m_PreOverGo;

        [SerializeField]
        private Text eventLogText;
        private List<int> draggingPointerData = new List<int>();

        public bool whenDragMaskEnterAndExit = true;
        public static PointableCanvasModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PointableCanvasModule>();
                }
                return _instance;
            }
        }

        public static void RegisterPointableCanvas(IPointableCanvas pointerCanvas)
        {
            // RKLog.Info($"====PointableCanvasModule====: +RegisterPointableCanvas");
            Assert.IsNotNull(Instance, "A PointableCanvasModule is required in the scene.");
            Instance.AddPointerCanvas(pointerCanvas);
        }

        public static void UnregisterPointableCanvas(IPointableCanvas pointerCanvas)
        {
            Instance?.RemovePointerCanvas(pointerCanvas);
        }

        private Dictionary<int, Pointer> _pointerMap = new Dictionary<int, Pointer>();
        private List<RaycastResult> _raycastResultCache = new List<RaycastResult>();
        private Dictionary<int, Pointer> _pointersForDeletion = new Dictionary<int, Pointer>();
        private Dictionary<IPointableCanvas, Action<PointerEvent>> _pointerCanvasActionMap =
            new Dictionary<IPointableCanvas, Action<PointerEvent>>();

        private void AddPointerCanvas(IPointableCanvas pointerCanvas)
        {
            //1.New A PointerToCanvasAction
            //2.Add PointerCanvasAction To Map 
            //3.Regist PointerCanvasAction To pointerCanvas.WhenPointerEventRaised
            Action<PointerEvent> pointerCanvasAction = (args) =>
            {
                HandlePointerEvent(pointerCanvas.Canvas, args);
            };
            _pointerCanvasActionMap.Add(pointerCanvas, pointerCanvasAction);
            pointerCanvas.WhenPointerEventRaised += pointerCanvasAction;
        }

        private void RemovePointerCanvas(IPointableCanvas pointerCanvas)
        {
            // RKLog.Info("====PointableCanvasModule==== : RemovePointerCanvas");
            Action<PointerEvent> pointerCanvasAction = _pointerCanvasActionMap[pointerCanvas];
            _pointerCanvasActionMap.Remove(pointerCanvas);
            pointerCanvas.WhenPointerEventRaised -= pointerCanvasAction;

            List<int> pointerIDs = new List<int>(_pointerMap.Keys);
            foreach (int pointerID in pointerIDs)
            {
                Pointer pointer = _pointerMap[pointerID];
                if (pointer.Canvas != pointerCanvas.Canvas)
                {
                    continue;
                }
                ClearPointerSelection(pointer.PointerEventData);
                pointer.MarkForDeletion();
                _pointersForDeletion[pointerID] = pointer;
            }
        }

        private void HandlePointerEvent(Canvas canvas, PointerEvent evt)
        {
            Pointer pointer;
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                    pointer = new Pointer(canvas);
                    pointer.PointerEventData = new PointerEventData(eventSystem);
                    pointer.SetPosition(evt.Pose.position);
                    _pointerMap.Add(evt.Identifier, pointer);
                    pointer.PointerEventData.pointerId = evt.Identifier;
                    if (!m_PointerData.ContainsKey(evt.Identifier))
                        m_PointerData.Add(evt.Identifier, pointer.PointerEventData);
                    break;
                case PointerEventType.Unhover:
                    if (_pointerMap.ContainsKey(evt.Identifier))
                    {
                        pointer = _pointerMap[evt.Identifier];
                        _pointerMap.Remove(evt.Identifier);
                        pointer.MarkForDeletion();
                        _pointersForDeletion[evt.Identifier] = pointer;
                    }
                    break;
                case PointerEventType.Select:
                    if (_pointerMap.ContainsKey(evt.Identifier))
                    {
                        pointer = _pointerMap[evt.Identifier];
                        pointer.SetPosition(evt.Pose.position);
                        pointer.Press();
                    }
                    break;
                case PointerEventType.Unselect:
                    if (_pointerMap.ContainsKey(evt.Identifier))
                    {
                        pointer = _pointerMap[evt.Identifier];
                        pointer.SetPosition(evt.Pose.position);
                        pointer.Release();
                    }
                    break;
                case PointerEventType.Move:
                    if (_pointerMap.ContainsKey(evt.Identifier))
                    {
                        pointer = _pointerMap[evt.Identifier];
                        pointer.SetPosition(evt.Pose.position);
                        // RKLog.Debug($"====PointableCanvasModule====  Move:  {evt.Pose.position}");
                    }
                    break;
                case PointerEventType.Cancel:
                    if (_pointerMap.ContainsKey(evt.Identifier))
                    {
                        pointer = _pointerMap[evt.Identifier];
                        _pointerMap.Remove(evt.Identifier);
                        ClearPointerSelection(pointer.PointerEventData);
                        pointer.MarkForDeletion();
                        _pointersForDeletion[evt.Identifier] = pointer;
                    }
                    break;
            }
        }

        /// <summary>
        /// Pointer class that is used for state associated with IPointables that are currently
        /// tracked by any IPointableCanvases in the scene.
        /// </summary>
        private class Pointer
        {
            public PointerEventData PointerEventData { get; set; }

            public bool MarkedForDeletion { get; private set; }

            private Canvas _canvas;
            public Canvas Canvas => _canvas;

            private Vector3 _position;
            public Vector3 Position => _position;

            private GameObject _hoveredSelectable;
            public GameObject HoveredSelectable => _hoveredSelectable;


            private bool _pressing = false;
            private bool _pressed;
            private bool _released;

            public Pointer(Canvas canvas)
            {
                _canvas = canvas;
                _pressed = _released = false;
            }

            public void Press()
            {
                if (_pressing) return;
                _pressing = true;
                _pressed = true;
            }

            public void Release()
            {
                if (!_pressing) return;
                _pressing = false;
                _released = true;
            }

            public void ReadAndResetPressedReleased(out bool pressed, out bool released)
            {
                pressed = _pressed;
                released = _released;
                _pressed = _released = false;
            }

            public void MarkForDeletion()
            {
                MarkedForDeletion = true;
                Release();
            }

            public void SetPosition(Vector3 position)
            {
                _position = position;
            }

            public void SetHoveredSelectable(GameObject hoveredSelectable)
            {
                _hoveredSelectable = hoveredSelectable;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_pointerEventCamera == null)
            {
                _pointerEventCamera = gameObject.AddComponent<Camera>();
                _pointerEventCamera.nearClipPlane = 0.1f;
                _pointerEventCamera.enabled = false;
            }
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
        }

        private void OnPointerDragBegin(PointerEventData eventData)
        {
            // RKLog.KeyInfo($" ===PointableCanvas=== {eventData.pointerId} is begin dragging");
            if (whenDragMaskEnterAndExit)
                draggingPointerData.Add(eventData.pointerId);
        }

        private void OnPointerDragEnd(PointerEventData eventData)
        {
            // RKLog.KeyInfo($" ===PointableCanvas=== {eventData.pointerId} is end dragging");
            if (whenDragMaskEnterAndExit)
                draggingPointerData.Remove(eventData.pointerId);
        }

        protected override void OnDisable()
        {
            ProcessOnDisable();
            Destroy(_pointerEventCamera);
            _pointerEventCamera = null;
            base.OnDisable();
            RKPointerListener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd -= OnPointerDragEnd;
            draggingPointerData.Clear();
        }

        // Based On FindFirstRaycast
        protected static RaycastResult FindFirstRaycastWithinCanvas(List<RaycastResult> candidates, Canvas canvas)
        {
            GameObject candidateGameObject;
            Canvas candidateCanvas;
            for (var i = 0; i < candidates.Count; ++i)
            {
                candidateGameObject = candidates[i].gameObject;
                if (candidateGameObject == null) continue;

                candidateCanvas = candidateGameObject.GetComponentInParent<Canvas>();
                if (candidateCanvas == null) continue;
                if (candidateCanvas.rootCanvas != canvas) continue;

                return candidates[i];
            }
            return new RaycastResult();
        }

        private void UpdateRaycasts(Pointer pointer, out bool pressed, out bool released)
        {
            if (_pointerEventCamera == null)
            {
                _pointerEventCamera = gameObject.AddComponent<Camera>();
                _pointerEventCamera.nearClipPlane = 0.1f;
                _pointerEventCamera.enabled = false;
            }
            PointerEventData pointerEventData = pointer.PointerEventData;
            Vector2 prevPosition = pointerEventData.position;
            Canvas canvas = pointer.Canvas;
            if (canvas == null)
            {
                pressed = false;
                released = true;
                return;
            }
            canvas.worldCamera = _pointerEventCamera;

            pointerEventData.Reset();

            pointer.ReadAndResetPressedReleased(out pressed, out released);

            //计算射线和平面的交点的关键计算逻辑,计算Pointer和Canvas的交点,position 为世界坐标
            Vector3 position = Vector3.zero;
            var plane = new Plane(-1f * canvas.transform.forward, canvas.transform.position);
            var ray = new Ray(pointer.Position - canvas.transform.forward, canvas.transform.forward);

            float enter;
            if (plane.Raycast(ray, out enter))
            {
                position = ray.GetPoint(enter);
            }

            // We need to position our camera at an offset from the Pointer position or else
            // a graphic raycast may ignore a world canvas that's outside of our regular camera view(s)
            // 我们需要将我们的相机定位在与指针位置的偏移处，否则图形光线投射可能会忽略我们常规相机视图之外的世界画布
            // 所以这里重新设置了事件相机的位置和朝向
            _pointerEventCamera.transform.position = pointer.Position - canvas.transform.forward;
            _pointerEventCamera.transform.LookAt(pointer.Position, canvas.transform.up);

            //pointerPosition 为screenPosition
            Vector2 pointerPosition = _pointerEventCamera.WorldToScreenPoint(position);
            pointerEventData.position = pointerPosition;

            // RaycastAll raycasts against with every GraphicRaycaster in the scene,
            // including nested ones like in the case of a dropdown
            eventSystem.RaycastAll(pointerEventData, _raycastResultCache);

            RaycastResult firstResult = FindFirstRaycastWithinCanvas(_raycastResultCache, canvas);
            _raycastResultCache.Clear();

            // We use a static translation offset from the canvas for 2D position delta tracking
            _pointerEventCamera.transform.position = canvas.transform.position - canvas.transform.forward;
            _pointerEventCamera.transform.LookAt(canvas.transform.position, canvas.transform.up);

            pointerPosition = _pointerEventCamera.WorldToScreenPoint(position);
            pointerEventData.position = pointerPosition;

            firstResult.screenPosition = pointerPosition;
            pointer.PointerEventData.pointerCurrentRaycast = firstResult;

            if (pressed)
            {
                pointerEventData.delta = Vector2.zero;
            }
            else
            {
                pointerEventData.delta = pointerEventData.position - prevPosition;
            }

            pointerEventData.button = PointerEventData.InputButton.Left;
        }

        public override void Process()
        {
            if (eventLogText != null)
            {
                eventLogText.text = "====PointableCanvasModule==== _pointerMap.Count:" + _pointerMap.Count;
            }

            foreach (KeyValuePair<int, Pointer> pair in _pointersForDeletion)
            {
                ProcessPointer(pair.Value, true);
                // RKLog.Debug($"====PointableCanvasModule====: RemovePointer:{pair.Key}");
                _pointerMap.Remove(pair.Key);
            }
            _pointersForDeletion.Clear();

            foreach (Pointer pointer in _pointerMap.Values)
            {
                if (pointer.MarkedForDeletion)
                    continue;
                ProcessPointer(pointer);
            }
        }

        private void ProcessOnDisable()
        {
            Process();

            foreach (var pointer in _pointerMap.Values)
            {
                // RKLog.Debug($"====PointableCanvasModule==== : ProcessOnDisable HandlePointerExitAndEnter ");
#if UNITY_2021_1_OR_NEWER
                HandlePointerExitAndEnter1(pointer.PointerEventData, null);
#else
                HandlePointerExitAndEnter2(pointer.PointerEventData, null);
#endif
            }

            foreach (var pointer in _pointersForDeletion.Values)
            {
                // RKLog.Debug($"====PointableCanvasModule==== : ProcessOnDisable HandlePointerExitAndEnter ");
#if UNITY_2021_1_OR_NEWER
                HandlePointerExitAndEnter1(pointer.PointerEventData, null);
#else
                HandlePointerExitAndEnter2(pointer.PointerEventData, null);
#endif
            }
        }


        private void ProcessPointer(Pointer pointer, bool forceRelease = false)
        {
            RKPointerListener.OnGraphicPointerHover?.Invoke(pointer.PointerEventData);
            RKPointerListener.OnPointerHover?.Invoke(pointer.PointerEventData);

            bool pressed = false;
            bool released = false;
            bool wasDragging = pointer.PointerEventData.dragging;

            UpdateRaycasts(pointer, out pressed, out released);
            PointerEventData pointerEventData = pointer.PointerEventData;

#if UNITY_EDITOR
            pointerEventData.scrollDelta = Input.mouseScrollDelta;
#else           
                if (InputModuleManager.Instance.GetMouseActive())
                {
                    pointerEventData.scrollDelta = MouseEventInput.Instance.GetMouseScrollDelta();
                }
                else if (InputModuleManager.Instance.GetTouchPadActive())
                {
                    if (RKTouchInput.Instance.GetInsideTouchCount() == 2)
                    {
                        pointerEventData.scrollDelta = TouchPadEventInput.Instance.GetTouchDelta();
                    }
                    else
                    {
                        pointerEventData.scrollDelta = Vector2.zero;
                    }
                }
                else if (InputModuleManager.Instance.GetThreeDofActive())
                {
                    pointerEventData.scrollDelta = TouchPadEventInput.Instance.GetTouchDelta();
                }
#endif

            // RKLog.Debug("====PointableCanvasModule==== ProcessPointer IScrollHandler " + pointerEventData.scrollDelta.sqrMagnitude);
            if (!Mathf.Approximately(pointerEventData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(pointerEventData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, pointerEventData, ExecuteEvents.scrollHandler);
            }

            ProcessPress(pointerEventData, pressed, released);

            released |= forceRelease;

            if (!released)
            {
                // PointableCanvasModule Press
                if (!draggingPointerData.Contains(pointer.PointerEventData.pointerId))
                {
                    ProcessMove(pointerEventData);
                }
                ProcessDrag(pointerEventData);
            }
            else
            {
                // PointableCanvasModule Release
                if (pointer.MarkedForDeletion)
                {
#if UNITY_2021_1_OR_NEWER
                    HandlePointerExitAndEnter1(pointerEventData, null);
#else
                    HandlePointerExitAndEnter2(pointerEventData, null);
#endif
                }
                RemovePointerData(pointerEventData);
            }
            HandleSelectableHover(pointer, wasDragging);
            HandleSelectablePress(pointer, pressed, released, wasDragging);
        }

        protected override void ProcessMove(PointerEventData pointerEvent)
        {
            GameObject newEnterTarget = ((Cursor.lockState == CursorLockMode.Locked) ? null : pointerEvent.pointerCurrentRaycast.gameObject);

#if UNITY_2021_1_OR_NEWER
            HandlePointerExitAndEnter1(pointerEvent, newEnterTarget);
#else
            HandlePointerExitAndEnter2(pointerEvent, newEnterTarget);
#endif

        }

#if UNITY_2021_1_OR_NEWER
        private void HandlePointerExitAndEnter1(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                int count = currentPointerData.hovered.Count;
                for (int i = 0; i < count; i++)
                {
                    currentPointerData.fullyExited = true;
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerMoveHandler);
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);
                    if (currentPointerData.hovered[i] != null)
                    {
                        RKPointerListener.OnPointerExit?.Invoke(currentPointerData, currentPointerData.hovered[i]);
                        RKPointerListener.OnGraphicPointerExit?.Invoke(currentPointerData);
                    }
                }

                currentPointerData.hovered.Clear();
                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = null;
                    return;
                }
            }

            if (currentPointerData.pointerEnter == newEnterTarget && (bool)newEnterTarget)
            {
                if (currentPointerData.IsPointerMoving())
                {
                    int count2 = currentPointerData.hovered.Count;
                    for (int j = 0; j < count2; j++)
                    {
                        ExecuteEvents.Execute(currentPointerData.hovered[j], currentPointerData, ExecuteEvents.pointerMoveHandler);
                    }
                }

                return;
            }

            GameObject gameObject = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);
            GameObject gameObject2 = ((Component)newEnterTarget.GetComponentInParent<IPointerExitHandler>())?.gameObject;
            if (currentPointerData.pointerEnter != null)
            {
                Transform parent = currentPointerData.pointerEnter.transform;
                while (parent != null && (!childSendPointerHoverToParent || !(gameObject != null) || !(gameObject.transform == parent)) && (childSendPointerHoverToParent || !(gameObject2 == parent.gameObject)))
                {
                    currentPointerData.fullyExited = parent.gameObject != gameObject && currentPointerData.pointerEnter != newEnterTarget;
                    ExecuteEvents.Execute(parent.gameObject, currentPointerData, ExecuteEvents.pointerMoveHandler);
                    ExecuteEvents.Execute(parent.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    if (parent.gameObject != null)
                    {
                        RKPointerListener.OnPointerExit?.Invoke(currentPointerData, parent.gameObject);
                        RKPointerListener.OnGraphicPointerExit?.Invoke(currentPointerData);
                    }

                    currentPointerData.hovered.Remove(parent.gameObject);
                    if (childSendPointerHoverToParent)
                    {
                        parent = parent.parent;
                    }

                    if (gameObject != null && gameObject.transform == parent)
                    {
                        break;
                    }

                    if (!childSendPointerHoverToParent)
                    {
                        parent = parent.parent;
                    }
                }
            }

            GameObject pointerEnter = currentPointerData.pointerEnter;
            currentPointerData.pointerEnter = newEnterTarget;
            if (!(newEnterTarget != null))
            {
                return;
            }

            Transform parent2 = newEnterTarget.transform;
            while (parent2 != null)
            {
                currentPointerData.reentered = parent2.gameObject == gameObject && parent2.gameObject != pointerEnter;
                if (!childSendPointerHoverToParent || !currentPointerData.reentered)
                {
                    ExecuteEvents.Execute(parent2.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    RKPointerListener.OnPointerEnter?.Invoke(currentPointerData);
                    RKPointerListener.OnPointerEnterWithObj?.Invoke(currentPointerData, parent2.gameObject);
                    RKPointerListener.OnGraphicPointerEnter?.Invoke(currentPointerData);
                    ExecuteEvents.Execute(parent2.gameObject, currentPointerData, ExecuteEvents.pointerMoveHandler);
                    currentPointerData.hovered.Add(parent2.gameObject);
                    if (childSendPointerHoverToParent || parent2.gameObject.GetComponent<IPointerEnterHandler>() == null)
                    {
                        if (childSendPointerHoverToParent)
                        {
                            parent2 = parent2.parent;
                        }

                        if (!(gameObject != null) || !(gameObject.transform == parent2))
                        {
                            if (!childSendPointerHoverToParent)
                            {
                                parent2 = parent2.parent;
                            }

                            continue;
                        }

                        break;
                    }

                    break;
                }

                break;
            }
        }
#else
        private void HandlePointerExitAndEnter2(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // if we have no target / pointerEnter has been deleted
            // just send exit events to anything we are tracking
            // then exit
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                var hoveredCount = currentPointerData.hovered.Count;
                for (var i = 0; i < hoveredCount; ++i)
                {
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);
                    RKPointerListener.OnPointerExit?.Invoke(currentPointerData, currentPointerData.hovered[i]);
                    RKPointerListener.OnGraphicPointerExit?.Invoke(currentPointerData);
                }


                currentPointerData.hovered.Clear();

                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = null;
                    return;
                }
            }

            // if we have not changed hover target
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
                return;

            GameObject commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // and we already an entered object from last time
            if (currentPointerData.pointerEnter != null)
            {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                Transform t = currentPointerData.pointerEnter.transform;

                while (t != null)
                {
                    // if we reach the common root break out!
                    if (commonRoot != null && commonRoot.transform == t)
                        break;

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    RKPointerListener.OnPointerExit?.Invoke(currentPointerData, t.gameObject);
                    RKPointerListener.OnGraphicPointerExit?.Invoke(currentPointerData);
                    t = t.parent;
                }
            }

            // now issue the enter call up to but not including the common root
            currentPointerData.pointerEnter = newEnterTarget;
            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;

                while (t != null && t.gameObject != commonRoot)
                {
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);
                    RKPointerListener.OnPointerEnter?.Invoke(currentPointerData);
                    RKPointerListener.OnPointerEnterWithObj?.Invoke(currentPointerData, t.gameObject);
                    RKPointerListener.OnGraphicPointerEnter?.Invoke(currentPointerData);
                    t = t.parent;
                }
            }
        }
#endif

        /// <summary>
        /// 处理可选物体的Hover问题
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="wasDragging"></param>
        private void HandleSelectableHover(Pointer pointer, bool wasDragging)
        {
            bool dragging = pointer.PointerEventData.dragging || wasDragging;

            GameObject currentOverGo = pointer.PointerEventData.pointerCurrentRaycast.gameObject;
            GameObject prevHoveredSelectable = pointer.HoveredSelectable;
            GameObject newHoveredSelectable = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
            pointer.SetHoveredSelectable(newHoveredSelectable);

            if (newHoveredSelectable != null && newHoveredSelectable != prevHoveredSelectable)
            {
                WhenSelectableHovered?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, pointer.HoveredSelectable, dragging));
            }
            else if (prevHoveredSelectable != null && newHoveredSelectable == null)
            {
                WhenSelectableUnhovered?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, pointer.HoveredSelectable, dragging));
            }
        }

        /// <summary>
        /// 处理可选物体的按压事件
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="pressed"></param>
        /// <param name="released"></param>
        /// <param name="wasDragging"></param>
        private void HandleSelectablePress(Pointer pointer, bool pressed, bool released, bool wasDragging)
        {
            bool dragging = pointer.PointerEventData.dragging || wasDragging;

            if (pressed)
            {
                WhenSelected?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, pointer.HoveredSelectable, dragging));
            }
            else if (released && !pointer.MarkedForDeletion)
            {
                // Unity handles UI selection on release, so we verify the hovered element has been selected
                bool hasSelectedHoveredObject = pointer.HoveredSelectable != null &&
                                                pointer.HoveredSelectable == pointer.PointerEventData.selectedObject;
                GameObject selectedObject = hasSelectedHoveredObject ? pointer.HoveredSelectable : null;
                WhenUnselected?.Invoke(new PointableCanvasEventArgs(pointer.Canvas, selectedObject, dragging));
            }
        }

        /// <summary>
        /// This method is based on ProcessTouchPoint in StandaloneInputModule,
        /// but is instead used for Pointer events
        /// </summary>
        protected void ProcessPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            // RKLog.Info($"====PointableCanvasModule==== : ProcessPress {pressed},{released}");
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
                if (newPressed)
                {
                    RKPointerListener.OnPointerDown?.Invoke(pointerEvent);
                    RKPointerListener.OnGraphicPointerDown?.Invoke(pointerEvent);
                }

                // didn't find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (released)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                if (pointerEvent.pointerPress)
                {
                    RKPointerListener.OnPointerUp?.Invoke(pointerEvent);
                    RKPointerListener.OnGraphicPointerUp?.Invoke(pointerEvent);
                }


                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    if (!TouchPadEventInput.Instance.TouchMove() && pointerEvent.pointerPress)
                    {
                        ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                        RKPointerListener.OnPointerClick?.Invoke(pointerEvent);
                    }
                }

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                    RKPointerListener.OnPointerDragEnd?.Invoke(pointerEvent);
                    RKPointerListener.OnGraphicDragEnd?.Invoke(pointerEvent);
                }

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // send exit events as we need to simulate this on touch up on touch device
                // ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                // pointerEvent.pointerEnter = null;
            }
        }

        /// <summary>
        /// Override of PointerInputModule's ProcessDrag to allow using the initial press position for drag begin.
        /// Set _useInitialPressPositionForDrag to false if you prefer the default behaviour of PointerInputModule.
        /// </summary>
        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            if (!pointerEvent.IsPointerMoving() ||
                Cursor.lockState == CursorLockMode.Locked ||
                pointerEvent.pointerDrag == null)
                return;

            if (!pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position,
                    eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
            {
                if (_useInitialPressPositionForDrag)
                {
                    pointerEvent.position = pointerEvent.pressPosition;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent,
                    ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
                // RKLog.Debug($"====PointableCanvasModule==== :  ProcessDrag");
                RKPointerListener.OnPointerDragBegin?.Invoke(pointerEvent);
                RKPointerListener.OnGraphicDragBegin?.Invoke(pointerEvent);
            }

            // Drag notification
            if (pointerEvent.dragging)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent,
                    ExecuteEvents.pointerUpHandler);
                    if (pointerEvent.pointerPress)
                    {
                        RKPointerListener.OnPointerUp?.Invoke(pointerEvent);
                        RKPointerListener.OnGraphicPointerUp?.Invoke(pointerEvent);
                    }
                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }

                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent,
                    ExecuteEvents.dragHandler);
                RKPointerListener.OnPointerDrag?.Invoke(pointerEvent);
            }
        }

        private void ClearPointerSelection(PointerEventData pointerEvent)
        {
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent,
                ExecuteEvents.pointerUpHandler);
            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;
        }

        /// <summary>
        /// Used in PointerInputModule's ProcessDrag implementation. Brought into this subclass with a protected
        /// signature (as opposed to the parent's private signature) to be used in this subclass's overridden ProcessDrag.
        /// </summary>
        protected static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (InputModuleManager.Instance.GetTouchPadActive())
            {
                // RKLog.Debug($"====PointableCanvasModule====:  ShouldStartDrag {TouchPadEventInput.Instance.ShouldStartDrag()}");
                return TouchPadEventInput.Instance.ShouldStartDrag();
            }
            else
            {
                if (!useDragThreshold)
                    return true;
                return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
            }
        }

        protected new void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
        {
            // if currentOverGo has added the component "DontGetFocus", eventSystem.currentSelectedGameObject will not do "OnDeselect".
            // we need to add "DontGetFocus" to the virtual keyboard,or inputField's focus will become false when click virtual key.
            if (currentOverGo == null || (currentOverGo != null && IsComponentInParent<DontGetFocus>(currentOverGo)))
            {
                return;
            }
            // Selection tracking
            var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
            // if we have clicked something new, deselect the old thing
            // leave 'selection handling' up to the press event though.
            if (selectHandlerGO != eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null, pointerEvent);
        }

        public static bool IsComponentInParent<T>(GameObject curGo) where T : MonoBehaviour
        {
            if (!curGo)
            {
                return false;
            }
            if (curGo.GetComponent<T>() != null)
            {
                return true;
            }

            if (curGo.transform.parent == null)
            {
                return false;
            }

            return IsComponentInParent<T>(curGo.transform.parent.gameObject);
        }
    }

}

