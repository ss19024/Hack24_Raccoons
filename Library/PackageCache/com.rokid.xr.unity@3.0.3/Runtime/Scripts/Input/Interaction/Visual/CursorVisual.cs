using System;
using Rokid.UXR.Components;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class CursorVisual : AutoInjectBehaviour, ICustomCursorVisual
    {

        [SerializeField]
        private HandType hand;
        [SerializeField]
        private RayInteractor rayInteractor;

        [Tooltip("CursorSize")]
        [SerializeField]
        private float cursorAngularSize = 100;

        [Autowrited("Cursor_Focus")]
        private Transform focusCursor;

        [Autowrited("Cursor_Press")]
        private Transform pressCursor;
        private Transform customCursorVisual;

        public Transform CustomCursorVisual { get { return customCursorVisual; } set { customCursorVisual = value; customCursorVisual?.SetParent(this.transform); } }
        private Pose customTargetPose = Pose.identity;
        public Pose CustomTargetPose { get => customTargetPose; set => customTargetPose = value; }
        private bool dragging;
        private float scaleRelease = 1.5f;
        private float scalePress = 1.0f;
        private float startSmoothPinchDistance = 0.06f;
        private bool saveActive;
        public event Action<HandType, bool> OnCustomFocusCursorActive;

        private void Start()
        {
            InteractorStateChange.OnHandDragStatusChangedWithData += OnHandDragStatusChanged;
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
        }


        private void OnDestroy()
        {
            InteractorStateChange.OnHandDragStatusChangedWithData -= OnHandDragStatusChanged;
            RKPointerListener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd -= OnPointerDragEnd;
        }

        private void OnEnable()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += UpdateVisualState;
            RKPointerListener.OnGraphicPointerEnter += OnPointerEnter;
            RKPointerListener.OnGraphicPointerExit += OnPointerExit;
            RKPointerListener.OnGraphicPointerHover += OnPointerHover;
            AdsorbArea.OnPointerChanged += OnPointerChanged;
        }
        private void OnDisable()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= UpdateVisualState;
            RKPointerListener.OnGraphicPointerEnter -= OnPointerEnter;
            RKPointerListener.OnGraphicPointerExit -= OnPointerExit;
            RKPointerListener.OnGraphicPointerHover -= OnPointerHover;
            customCursorVisual = null;
            AdsorbArea.OnPointerChanged -= OnPointerChanged;
        }


        private bool HandDragging(GestureType GesType)
        {
            if (dragging && hand != HandType.None)
            {
                if (GesEventInput.Instance.GetGestureType(hand) == GesType)
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 ComputeScaleWithAngularScale(Vector3 targetPoint)
        {
            if (MainCameraCache.mainCamera != null)
            {
                float cursorDistance = Vector3.Distance(MainCameraCache.mainCamera.transform.position, targetPoint);
                float desiredScale = Utils.ScaleFromAngularSizeAndDistance(cursorAngularSize, cursorDistance);
                return Vector3.one * desiredScale;
            }
            return Vector3.zero;
        }

        private int GetPointerId()
        {
            // RKLog.KeyInfo($"====CursorVisual==== GetPointerId {rayInteractor.realId}");
            return rayInteractor.realId;
        }

        private void UpdateVisual()
        {
            if (rayInteractor.CollisionInfo == null || rayInteractor.State == InteractorState.Disabled || IsBezierCurveDragging())
            {
                if (saveActive)
                {
                    saveActive = false;
                    OnCustomFocusCursorActive?.Invoke(this.hand, saveActive);
                }
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
                return;
            }

            if (bezierForAdsorb != null)
            {
                if (bezierForAdsorb.ActiveAdsorb() && bezierForAdsorb.IsEnableBezierCurve(rayInteractor.realId))
                {
                    // RKLog.KeyInfo($"====CursorVisual====:{hand} Adsorb UI");
                    //For Adsorb UI
                    this.transform.position = bezierForAdsorb.GetBezierAdsorbPoint(rayInteractor.realId);
                    this.transform.rotation = Quaternion.LookRotation(bezierForAdsorb.GetBezierAdsorbNormal(rayInteractor.realId), Vector3.up);
                    this.transform.localScale = ComputeScaleWithAngularScale(this.transform.position);
                }
                else
                {
                    bezierForAdsorb = null;
                }
            }
            else if (floatingUI != null)
            {
                // RKLog.KeyInfo($"====CursorVisual====:{hand} Floating UI");
                //For Floating UI
                this.transform.position = hoverPosition;
                this.transform.rotation = Quaternion.LookRotation(hoverNormal, Vector3.up);
                this.transform.localScale = ComputeScaleWithAngularScale(hoverPosition);
            }
            else
            {
                // RKLog.KeyInfo($"====CursorVisual====:{hand} Normal UI");
                Vector3 collisionNormal = customTargetPose != Pose.identity ? customTargetPose.rotation * Vector3.forward : IsBezierCurveDragging() ? bezierCurveDrag.GetBezierCurveEndNormal(GetPointerId()) : rayInteractor.CollisionInfo.Value.Normal;
                Vector3 collisionPosition = customTargetPose != Pose.identity ? customTargetPose.position : IsBezierCurveDragging() ? bezierCurveDrag.GetBezierCurveEndPoint(GetPointerId()) : rayInteractor.End;
                this.transform.position = collisionPosition + collisionNormal * 0.001f;
                this.transform.rotation = Quaternion.LookRotation(collisionNormal, Vector3.up);
                this.transform.localScale = ComputeScaleWithAngularScale(collisionPosition);
            }

            bool press = rayInteractor.State == InteractorState.Select || IsBezierCurveDragging();
            if (saveActive != !press)
            {
                saveActive = !press;
                OnCustomFocusCursorActive?.Invoke(this.hand, saveActive);
            }

            focusCursor.gameObject.SetActive(!press);
            pressCursor.gameObject.SetActive(press);
            if (customCursorVisual != null)
            {
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
            }

            if (hand != HandType.None)
            {
                if (!press)
                {
                    Vector3 index_tip = GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.INDEX_FINGER_TIP, hand).position;
                    Vector3 thumb_tip = GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.THUMB_TIP, hand).position;
                    float sqrDistance = Vector3.SqrMagnitude(index_tip - thumb_tip);
                    if (sqrDistance > startSmoothPinchDistance * startSmoothPinchDistance)
                    {
                        this.transform.localScale *= scaleRelease;
                    }
                    else
                    {
                        float pow = Mathf.Max(scalePress, scaleRelease * (sqrDistance / (startSmoothPinchDistance * startSmoothPinchDistance)));
                        this.transform.localScale *= pow;
                    }
                }
                else
                {
                    this.transform.localScale *= scalePress;
                }
            }
        }

        private void UpdateVisualState(InteractorStateChangeArgs args) => UpdateVisual();


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
                    bezierCurveDrag = data?.pointerDrag?.GetComponent<IBezierCurveDrag>();
                }
                else
                {
                    bezierCurveDrag = null;
                    this.dragging = false;
                }
            }
        }

        #endregion


        #region ForCursorHoverPose
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

        private SpriteRenderer[] spriteRenderers;
        private MeshRenderer[] meshRenderers;
        public void CustomCursorAlpha(float alpha)
        {
            if (spriteRenderers == null)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
            if (spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    Color color = spriteRenderers[i].color;
                    spriteRenderers[i].color = new Color(color.r, color.g, color.b, alpha);
                }
            }

            if (meshRenderers == null)
            {
                meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            }
            if (meshRenderers != null)
            {
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    meshRenderers[i].material.SetFloat("_Alpha", alpha);
                }
            }
        }
        #endregion
    }
}

