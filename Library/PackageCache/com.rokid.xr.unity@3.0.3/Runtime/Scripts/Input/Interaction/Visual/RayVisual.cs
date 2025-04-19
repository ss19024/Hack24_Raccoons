using System;
using Rokid.UXR.Utility;
using Rokid.UXR.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class RayVisual : MonoBehaviour
    {
        [SerializeField]
        private RayInteractor rayInteractor;

        [SerializeField]
        private LineRenderer rayRenderer;

        [Tooltip("射线最小长度")]
        [SerializeField]
        protected float minRayLength;
        [Tooltip("射线最长长度")]
        [SerializeField]
        protected float maxRayLength;

        [Tooltip("射线默认长度")]
        [SerializeField]
        protected float normalRayLength;
        [SerializeField]
        private Transform rayOrigin;

        private Material rayMat;

        private bool dragging;

        [SerializeField]
        private HandType hand;

        private bool raySleep;


        private void Start()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += HandleStateChanged;

            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;

            InteractorStateChange.OnHandDragStatusChangedWithData += OnHandDragStatusChanged;
            ThreeDofEventInput.OnThreeDofSleep += OnRaySleep;
            ThreeDofEventInput.OnSwipeTriggerSuccess += OnSwipeTriggerSuccess;
            rayMat = rayRenderer.material;
        }


        private void OnDestroy()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= HandleStateChanged;

            RKPointerListener.OnPointerDragEnd -= OnPointerDragEnd;
            RKPointerListener.OnPointerDragBegin -= OnPointerDragBegin;
            ThreeDofEventInput.OnThreeDofSleep -= OnRaySleep;
            InteractorStateChange.OnHandDragStatusChangedWithData -= OnHandDragStatusChanged;
            ThreeDofEventInput.OnSwipeTriggerSuccess -= OnSwipeTriggerSuccess;
        }

        private void OnRaySleep(bool sleeping)
        {
            raySleep = sleeping;
        }


        private void OnEnable()
        {
            RKPointerListener.OnGraphicPointerEnter += OnPointerEnter;
            RKPointerListener.OnGraphicPointerExit += OnPointerExit;
            RKPointerListener.OnGraphicPointerHover += OnPointerHover;
            AdsorbArea.OnPointerChanged += OnPointerChanged;
        }

        private void OnDisable()
        {
            RKPointerListener.OnGraphicPointerEnter -= OnPointerEnter;
            RKPointerListener.OnGraphicPointerExit -= OnPointerExit;
            RKPointerListener.OnGraphicPointerHover -= OnPointerHover;
            AdsorbArea.OnPointerChanged -= OnPointerChanged;
        }

        private void HandleStateChanged(InteractorStateChangeArgs args)
        {
            UpdateVisual();
        }


        private void Update()
        {
            PlayRayAnimation();
            if (doAnimation)
            {
                rayRenderer.enabled = true;
                DrawRay(maxRayLength, true);
            }
            else if (Input.touchCount == 0)
            {
                // if (rayInteractor.enabled == false)
                //     rayInteractor.enabled = true;
                cursorVisual?.gameObject.SetActive(true);
            }
        }
        private void UpdateVisual()
        {
            rayRenderer.enabled = !raySleep;
            if (bezierForAdsorb != null)
            {
                if (bezierForAdsorb.ActiveAdsorb() && bezierForAdsorb.IsEnableBezierCurve(rayInteractor.realId))
                {
                    SetLineEndPoints(bezierForAdsorb.GetBezierAdsorbPoint(rayInteractor.realId));
                    DrawBezierRay(rayInteractor.State == InteractorState.Select);
                    return;
                }
                else
                {
                    bezierForAdsorb = null;
                }
            }
            if (IsBezierCurveDragging())
            {
                SetLineEndPoints(bezierCurveDrag.GetBezierCurveEndPoint(rayInteractor.realId));
                DrawBezierRay(rayInteractor.State == InteractorState.Select);
                return;
            }
            if (rayInteractor.State == InteractorState.Disabled)
            {
                rayRenderer.enabled = false;
                return;
            }
            switch (rayInteractor.State)
            {
                case InteractorState.Normal:
                    DrawRay(normalRayLength, false);
                    break;
                case InteractorState.Hover:
                    if (floatingUI != null)
                    {
                        DrawRay(transform.InverseTransformPoint(hoverPosition), false);
                    }
                    else
                    {
                        if (rayInteractor.CollisionInfo != null)
                        {
                            DrawRay(transform.InverseTransformPoint(rayInteractor.End).z, false);
                        }
                        else
                        {
                            DrawRay(normalRayLength, false);
                        }
                    }
                    break;
                case InteractorState.Select:
                    if (floatingUI != null)
                    {
                        DrawRay(transform.InverseTransformPoint(hoverPosition), true);
                    }
                    else
                    {
                        DrawRay(transform.InverseTransformPoint(rayInteractor.End).z, true);
                    }
                    break;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSwipeTriggerSuccess();
            }
        }

        protected void DrawRay(float length, bool isPress)
        {
            currentRayLength = length;
            rayRenderer.positionCount = 2;
            rayRenderer.useWorldSpace = false;
            rayRenderer.textureMode = LineTextureMode.RepeatPerSegment;
            rayRenderer.SetPosition(0, rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0));
            rayRenderer.SetPosition(1, new Vector3(0, 0, length > 0 ? length : 0));
            rayMat?.SetFloat("_Length", length);
            rayMat?.SetFloat("_IsPress", isPress ? 1 : 0);
        }

        protected void DrawRay(Vector3 endPosition, bool isPress)
        {
            float length = Vector3.Distance(rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0), endPosition);
            currentRayLength = length;
            rayRenderer.positionCount = 2;
            rayRenderer.useWorldSpace = false;
            rayRenderer.textureMode = LineTextureMode.RepeatPerSegment;
            rayRenderer.SetPosition(0, rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0));
            rayRenderer.SetPosition(1, length > 0 ? endPosition : rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0));
            rayMat?.SetFloat("_Length", length);
            rayMat?.SetFloat("_IsPress", isPress ? 1 : 0);
        }

        #region DrawBezierCurve
        [SerializeField]
        private float lineStartClamp = 0.0001f;
        [SerializeField]
        private float lineEndClamp = 0.8028384f;
        private float startPointLerp = 0.33f;
        private float endPointLerp = 0.66f;
        private Vector3[] Points = new Vector3[4];
        private Vector3[] positions;
        private int lineStepCount = 16;
        private int LineStepCount
        {
            get => lineStepCount;
            set => lineStepCount = Mathf.Clamp(value, 2, 2048);
        }


        private int PointCount { get { return 4; } }

        public Vector3 FirstPoint
        {
            get => GetPoint(0);
            set => SetPoint(0, value);
        }

        public Vector3 LastPoint
        {
            get => GetPoint(PointCount - 1);
            set => SetPoint(PointCount - 1, value);
        }

        private void SetLinePoints(Vector3 startPoint, Vector3 endPoint)
        {
            FirstPoint = startPoint;
            LastPoint = endPoint;
        }

        private void SetLineEndPoints(Vector3 endPoint)
        {
            LastPoint = endPoint;
        }

        private Vector3 GetPoint(int pointIndex)
        {
            if (pointIndex < 0 || pointIndex >= PointCount)
            {
                Debug.LogError("Invalid point index");
                return Vector3.zero;
            }

            Vector3 point = Points[pointIndex];
            point = transform.TransformPoint(point);
            return point;
        }

        public Vector3 GetPoint(float normalizedLength)
        {
            normalizedLength = Mathf.Lerp(lineStartClamp, lineEndClamp, Mathf.Clamp01(normalizedLength));
            Vector3 point = GetPointInternal(normalizedLength);
            point = transform.TransformPoint(point);
            return point;
        }

        private void SetPoint(int pointIndex, Vector3 point)
        {
            if (pointIndex < 0 || pointIndex >= PointCount)
            {
                Debug.LogError("Invalid point index");
                return;
            }

            point = transform.InverseTransformPoint(point);
            Points[pointIndex] = point;
        }

        private Vector3 InterpolateBezierPoints(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4, float normalizedLength)
        {
            float invertedDistance = 1f - normalizedLength;
            return invertedDistance * invertedDistance * invertedDistance * point1 +
                3f * invertedDistance * invertedDistance * normalizedLength * point2 +
                3f * invertedDistance * normalizedLength * normalizedLength * point3 +
                normalizedLength * normalizedLength * normalizedLength * point4;
        }

        private Vector3 GetPointInternal(float normalizedDistance)
        {
            return InterpolateBezierPoints(Points[0], Points[1], Points[2], Points[3], normalizedDistance);
        }

        private void DrawBezierRay(bool isPress)
        {
            float distance = Vector3.Distance(transform.position, LastPoint);
            Vector3 startPoint = FirstPoint;
            Vector3 expectedPoint = startPoint + transform.forward * distance;

            SetPoint(1, Vector3.Lerp(startPoint, expectedPoint, startPointLerp));
            expectedPoint = Vector3.Lerp(expectedPoint, LastPoint, endPointLerp);
            SetPoint(2, Vector3.Lerp(startPoint, expectedPoint, endPointLerp));

            rayRenderer.positionCount = LineStepCount;
            rayRenderer.useWorldSpace = false;
            rayRenderer.textureMode = LineTextureMode.Stretch;
            if (positions == null || positions.Length != rayRenderer.positionCount)
            {
                positions = new Vector3[rayRenderer.positionCount];
            }

            for (int i = 0; i < positions.Length; i++)
            {
                float normalizedDistance = (1f / (LineStepCount - 1)) * i;
                positions[i] = transform.InverseTransformPoint(GetPoint(normalizedDistance));
            }

            // Set positions
            rayRenderer.positionCount = positions.Length;
            rayRenderer.SetPositions(positions);
            rayMat?.SetFloat("_IsPress", isPress ? 1 : 0);
        }
        #endregion

        #region For BezierCurveDrag
        private IBezierCurveDrag bezierCurveDrag;
        private void OnPointerDragBegin(PointerEventData pointerEventData)
        {
            if (hand == HandType.None)
            {
                dragging = true;
                bezierCurveDrag = pointerEventData.pointerDrag?.GetComponent<IBezierCurveDrag>();
            }
        }

        private void OnPointerDragEnd(PointerEventData pointerEventData)
        {
            if (hand == HandType.None)
            {
                bezierCurveDrag = null;
                dragging = false;
            }
        }
        private bool IsBezierCurveDragging()
        {
            return dragging && bezierCurveDrag != null && bezierCurveDrag.IsInBezierCurveDragging() && (bezierCurveDrag.IsEnablePinchBezierCurve() || bezierCurveDrag.IsEnableGripBezierCurve());
        }

        private void OnHandDragStatusChanged(HandType hand, PointerEventData data, bool dragging)
        {
            if (hand == this.hand)
            {
                if (dragging)
                {
                    this.dragging = true;
                    bezierCurveDrag = data.pointerDrag?.GetComponent<IBezierCurveDrag>();
                }
                else
                {
                    bezierCurveDrag = null;
                    this.dragging = false;
                }
            }
        }

        #endregion

        #region For Floating Or Adsorb UI
        private Vector3 hoverPosition;
        private Vector3 hoverNormal;
        private IBezierForAdsorb bezierForAdsorb;
        private IFloatingUI floatingUI;

        private void OnPointerChanged(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand && eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (bezierForAdsorb == null)
                    bezierForAdsorb = eventData.pointerCurrentRaycast.gameObject.GetComponent<IBezierForAdsorb>();
            }
        }
        private void OnPointerEnter(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand && eventData.pointerCurrentRaycast.gameObject != null)
            {
                hoverPosition = eventData.pointerCurrentRaycast.worldPosition;
                hoverNormal = eventData.pointerCurrentRaycast.worldNormal;
                if (bezierForAdsorb == null)
                    bezierForAdsorb = eventData.pointerCurrentRaycast.gameObject.GetComponent<IBezierForAdsorb>();
                if (floatingUI == null)
                    floatingUI = eventData.pointerCurrentRaycast.gameObject.GetComponent<IFloatingUI>();
            }
        }
        private void OnPointerExit(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand)
            {
                floatingUI = null;
                bezierForAdsorb = null;
            }
        }
        private void OnPointerHover(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand)
            {
                hoverPosition = eventData.pointerCurrentRaycast.worldPosition;
                hoverNormal = eventData.pointerCurrentRaycast.worldNormal;
            }
        }

        #endregion


        #region RayAnimation

        float rayMask = 1;
        bool doAnimation = false;
        float smoothStep = 1;
        float currentRayLength;

        CursorVisual cursorVisual;

        private void OnSwipeTriggerSuccess()
        {
            rayMask = 0;
            if (cursorVisual == null)
                cursorVisual = transform.parent.GetComponentInChildren<CursorVisual>();
            cursorVisual?.gameObject.SetActive(false);
            // rayInteractor.enabled = false;
            doAnimation = true;
            smoothStep = 0.12f / currentRayLength;
        }

        private void PlayRayAnimation()
        {
            if (doAnimation)
            {
                rayMask += smoothStep;
                if (rayMask > 0.9)
                {
                    rayMask = 1;
                    doAnimation = false;
                }
                rayMat?.SetFloat("_Mask", rayMask);
            }
        }

        #endregion
    }
}
