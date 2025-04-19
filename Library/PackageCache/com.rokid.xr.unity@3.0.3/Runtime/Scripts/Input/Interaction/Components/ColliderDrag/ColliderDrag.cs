using System.Collections.Generic;
using Rokid.UXR.Components;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Plane = Rokid.UXR.Components.Plane;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Drag-and-drop component based on the collider, drag-and-drop objects will be dragged along the collider surface, the component interaction does not support gestures for the time being
    /// </summary>
    public class ColliderDrag : MonoBehaviour, IBezierCurveDrag, IRayBeginDrag, IRayEndDrag, IRayPointerDown, IRayPointerUp, IRayPointerEnter, IRayPointerExit
    {
        [SerializeField, Tooltip("The target drag obj")]
        private Transform dragObj;
        public Transform DragObj { get { return this.dragObj; } set { dragObj = value; } }
        [SerializeField, Tooltip("The area where the non-drag-and-drop state follows")]
        private Transform followToDragArea;
        [SerializeField, Tooltip("The ray to cast level")]
        private LayerMask raycastMask = (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6 | 1 << 7);
        [SerializeField]
        private float minYAngle = -45;
        [SerializeField]
        private float maxYAngle = 45;
        [SerializeField]
        private float sphereRadius = 1.4f;
        [SerializeField]
        private bool processDragLogic = true;
        [SerializeField, Tooltip("The drag obj is look at camera")]
        private bool lookAtCamera = true;
        [SerializeField, Tooltip("The drag obj is clamp in target filed only collider mesh is sphere can use")]
        private bool enableSphereClampInTargetFiled = true;
        [SerializeField, Tooltip("The drag smooth speed")]
        private float smoothSpeed = 10;

        [SerializeField, Tooltip("The max look at change threshold")]
        private float maxLookAtChangeThreshold = 0.3f;

        [SerializeField, Tooltip("The min look at change threshold")]
        private float minLookAtChangeThreshold = -0.3f;
        [SerializeField]
        private Transform debugPoint;

        private IEventInput eventInput;
        private Vector3 oriHitPoint, curHitPoint, oriDragObjPos;
        private Vector3 allDelta;
        private Vector3 delta;
        internal bool dragging { get; set; }
        private IShaper plane;
        private IShaper capsule;
        private ISelector selector;
        private FollowCamera followCamera;
        private Dictionary<int, BezierPointerData> bezierPointerDatas = new Dictionary<int, BezierPointerData>();
        private List<RaycastResult> sortedRaycastResults = new List<RaycastResult>();
        private bool registInit;
        private bool hovering;
        private int dragCount = 0;
        private LayerMask baseRaycastMask;
        private float oldDragThreshold = 0;


        #region  UNITY_EVENT
        public UnityEvent<PointerEventData> OnBeginDrag;
        public UnityEvent<Vector3> OnDrag;
        public UnityEvent<PointerEventData> OnEndDrag;
        public UnityEvent<PointerEventData> OnPointerDown;
        public UnityEvent<PointerEventData> OnPointerUp;
        public UnityEvent<PointerEventData> OnPointerEnter;
        public UnityEvent<PointerEventData> OnPointerExit;
        #endregion


        private void Awake()
        {
            if (InputModuleManager.Instance.GetInitialize())
            {
                OnInitialize();
            }
            else
            {
                InputModuleManager.OnInitialize += OnInitialize;
                registInit = true;
            }
            InputModuleManager.OnModuleActive += OnModuleActive;
        }

        private void OnModuleActive(InputModuleType type)
        {
            eventInput = InputModuleManager.Instance.GetActiveEventInput();
            oldDragThreshold = eventInput.GetRayCaster().dragThreshold;
            baseRaycastMask = eventInput.GetRayCaster().raycastMask;
            selector = eventInput.GetRaySelector();
        }

        private void OnDestroy()
        {
            if (registInit)
                InputModuleManager.OnInitialize -= OnInitialize;
            InputModuleManager.OnModuleActive -= OnModuleActive;
        }

        private void OnInitialize()
        {
            eventInput = InputModuleManager.Instance.GetActiveEventInput();
            oldDragThreshold = eventInput.GetRayCaster().dragThreshold;
            baseRaycastMask = eventInput.GetRayCaster().raycastMask;
            selector = eventInput.GetRaySelector();
            plane = GetComponent<Plane>();
            capsule = GetComponent<Capsule>();
            followCamera = GetComponent<FollowCamera>();
            Assert.IsNotNull(eventInput);
            Assert.IsNotNull(plane);
            Assert.IsNotNull(capsule);
            Assert.IsNotNull(followCamera);
            // Assert.IsNotNull(dragObj);
            Assert.IsNotNull(followToDragArea);
            Loom.QueueOnMainThread(() => { plane.RefreshMesh(); }, 0.1f);
        }

        public void OnRayBeginDrag(PointerEventData eventData)
        {
            if (Raycast(out RaycastResult hitInfo, 1000, raycastMask))
            {
                dragCount = 0;
                transform.parent = null;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                oriHitPoint = hitInfo.worldPosition;
                dragging = true;
                oriDragObjPos = dragObj.position;
                allDelta = Vector3.zero;
                capsule.RefreshMesh();
                //Set the deviation value because there is a deviation value between the drag point and the dragged object itself
                followCamera.enabled = true;
                followCamera.UpdateOffsetPosition(-oriDragObjPos + oriHitPoint, false);
                AddBezierPointerData(eventData);
                OnBeginDrag?.Invoke(eventData);
                RKLog.KeyInfo($"====ColliderDrag==== OnBeginDrag");
            }
        }

        public void OnRayEndDrag(PointerEventData eventData)
        {
            RemoveBezierPointerData(eventData);
            dragging = false;
            followCamera.enabled = false;
            transform.position = followToDragArea.position;
            transform.rotation = followToDragArea.rotation;
            plane.RefreshMesh();
            OnEndDrag?.Invoke(eventData);
            RKLog.KeyInfo($"====ColliderDrag==== OnEndDrag");
        }

        private void OnDisable()
        {
            if (eventInput?.GetRayCaster() != null)
            {
                eventInput.GetRayCaster().raycastMask = baseRaycastMask;
                eventInput.GetRayCaster().dragThreshold = oldDragThreshold;
            }
            bezierPointerDatas.Clear();
            dragging = false;
            followCamera.enabled = false;
            plane.RefreshMesh();
        }

        private bool Raycast(out RaycastResult hitInfo, float distance, LayerMask layerMask)
        {
            if (eventInput != null)
            {
                eventInput.GetRayCaster().Raycast(distance, sortedRaycastResults);
                if (sortedRaycastResults.Count > 0)
                {
                    hitInfo = eventInput.GetRayCaster().FirstRaycastResult(sortedRaycastResults);
                    return true;
                }
            }
            hitInfo = default(RaycastResult);
            return false;
        }

        private void FixedUpdate()
        {
            if (dragging)
            {
                if (Raycast(out RaycastResult hitInfo, 1000, raycastMask))
                {
                    curHitPoint = hitInfo.worldPosition;
                    delta = curHitPoint - oriHitPoint;
                    oriHitPoint = curHitPoint;
                    dragCount++;
                    //Filter out abnormal data due to shape changes
                    int beginDragThreshold = 3;
                    if (dragCount > beginDragThreshold)
                    {
                        allDelta += delta;
                        OnDrag?.Invoke(delta);
                        if (debugPoint != null)
                        {
                            debugPoint.position = curHitPoint;
                        }
                        // RKLog.KeyInfo($"====ColliderDrag==== OnDrag collider.position:{transform.position},oriDragObjPos:{oriDragObjPos},curHitPoint:{curHitPoint},delta:{delta},allDelta:{allDelta},colliderObj:{hitInfo.gameObject.name}");
                    }
                    else if (dragCount == beginDragThreshold)
                    {
                        oriDragObjPos = dragObj.position;
                    }
                }
            }

        }

        private void LateUpdate()
        {
            if (dragging)
            {
                if (processDragLogic)
                {
                    Vector3 targetPos = oriDragObjPos + allDelta;
                    Vector3 cameraPos = MainCameraCache.mainCamera.transform.position;
                    if (enableSphereClampInTargetFiled)
                    {
                        Vector3 targetForward = (targetPos - cameraPos).normalized;
                        // Debug.DrawRay(cameraPos, targetForward, Color.green);
                        float dot = Vector3.Dot(targetForward, Vector3.up);
                        if (dot > 0)
                        {
                            Vector3 ruleForward = Vector3.ProjectOnPlane(targetForward, Vector3.up).normalized;
                            // Debug.DrawRay(cameraPos, ruleForward, Color.red);
                            float angle = Vector3.Angle(targetForward, ruleForward);
                            if (angle > maxYAngle)
                            {
                                Vector3 P = cameraPos + Mathf.Cos(Mathf.Deg2Rad * maxYAngle) * sphereRadius * ruleForward;
                                float height = Mathf.Sin(Mathf.Deg2Rad * maxYAngle) * sphereRadius;
                                P.y += height;
                                targetPos = P;
                            }
                        }
                        else
                        {
                            Vector3 ruleForward = Vector3.ProjectOnPlane(targetForward, Vector3.down).normalized;
                            // Debug.DrawRay(cameraPos, ruleForward, Color.red);
                            float angle = Vector3.Angle(targetForward, ruleForward) * -1;
                            if (angle < minYAngle)
                            {
                                Vector3 P = cameraPos + Mathf.Cos(Mathf.Deg2Rad * Mathf.Abs(minYAngle)) * sphereRadius * ruleForward.normalized;
                                float height = Mathf.Sin(Mathf.Deg2Rad * Mathf.Abs(minYAngle)) * sphereRadius;
                                P.y -= height;
                                targetPos = P;
                            }
                        }
                    }
                    dragObj.position = Vector3.Lerp(dragObj.position, targetPos, Time.deltaTime * smoothSpeed);
                    if (lookAtCamera)
                    {
                        if (dragObj.position.y > maxLookAtChangeThreshold + cameraPos.y || dragObj.position.y < minLookAtChangeThreshold + cameraPos.y)
                        {
                            Vector3 forward = dragObj.position - cameraPos;
                            dragObj.rotation = Quaternion.Slerp(dragObj.rotation, Quaternion.LookRotation(forward), Time.deltaTime * smoothSpeed);
                        }
                        else
                        {
                            Vector3 forward = dragObj.position - cameraPos;
                            forward.y = 0;
                            dragObj.rotation = Quaternion.Slerp(dragObj.rotation, Quaternion.LookRotation(forward), Time.deltaTime * smoothSpeed);
                        }
                    }
                }
            }
            else
            {
                transform.position = followToDragArea.position;
                transform.rotation = followToDragArea.rotation;
            }
        }

        public bool IsEnablePinchBezierCurve()
        {
            return true;
        }

        public bool IsEnableGripBezierCurve()
        {
            return true;
        }

        public bool IsInBezierCurveDragging()
        {
            return dragging;
        }

        public Vector3 GetBezierCurveEndPoint(int pointerId)
        {
            BezierPointerData pointerData = GetBezierPointerData(pointerId);
            if (pointerData != null)
            {
                return dragObj.TransformPoint(pointerData.hitLocalPos);
            }
            else
            {
                return dragObj.TransformPoint(Vector3.zero);
            }
        }

        public Vector3 GetBezierCurveEndNormal(int pointerId)
        {
            BezierPointerData pointerData = GetBezierPointerData(pointerId);
            if (pointerData != null)
            {
                return dragObj.TransformPoint(pointerData.hitLocalNormal);
            }
            else
            {
                return dragObj.TransformPoint(Vector3.zero);
            }
        }

        private BezierPointerData GetBezierPointerData(int pointerId)
        {
            if (bezierPointerDatas.TryGetValue(pointerId, out BezierPointerData bezierPointerData))
            {
                return bezierPointerData;
            }
            else
            {
                return null;
            }
        }

        private void RemoveBezierPointerData(PointerEventData eventData)
        {
            bezierPointerDatas.Remove(eventData.pointerId);
        }

        private void AddBezierPointerData(PointerEventData eventData)
        {
            BezierPointerData bezierPointerData = new BezierPointerData
            {
                pointerId = eventData.pointerId,
                hitLocalNormal = dragObj.InverseTransformVector(eventData.pointerCurrentRaycast.worldNormal),
                hitLocalPos = dragObj.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition)
            };
            if (bezierPointerDatas.ContainsKey(bezierPointerData.pointerId))
            {
                bezierPointerDatas[bezierPointerData.pointerId] = bezierPointerData;
            }
            else
            {
                bezierPointerDatas.Add(bezierPointerData.pointerId, bezierPointerData);
            }
        }

        public void OnRayPointerDown(PointerEventData eventData)
        {
            eventInput.GetRayCaster().dragThreshold = 0;
            OnPointerDown?.Invoke(eventData);
            RKLog.KeyInfo($"====ColliderDrag==== OnPointerDown");
        }

        public void OnRayPointerUp(PointerEventData eventData)
        {
            eventInput.GetRayCaster().dragThreshold = oldDragThreshold;
            OnPointerUp?.Invoke(eventData);
            RKLog.KeyInfo($"====ColliderDrag==== OnPointerUp");
        }

        public void OnRayPointerEnter(PointerEventData eventData)
        {
            eventInput.GetRayCaster().dragThreshold = 0;
            eventInput.GetRayCaster().raycastMask = raycastMask;
            if (!dragging && selector.Selecting)
                return;
            hovering = true;
            OnPointerEnter?.Invoke(eventData);
            RKLog.KeyInfo($"====ColliderDrag==== OnPointerEnter");
        }

        public void OnRayPointerExit(PointerEventData eventData)
        {
            eventInput.GetRayCaster().dragThreshold = oldDragThreshold;
            eventInput.GetRayCaster().raycastMask = baseRaycastMask;
            hovering = false;
            OnPointerExit?.Invoke(eventData);
            RKLog.KeyInfo($"====ColliderDrag==== OnPointerExit");
        }
    }
}

