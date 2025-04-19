using UnityEngine;
using Rokid.UXR.Utility;
using UnityEngine.EventSystems;
using System;

namespace Rokid.UXR.Interaction
{
    public class TouchCursorVisual : MonoBehaviour, ICustomCursorVisual
    {
        [SerializeField]
        private RayInteractor rayInteractor;
        [SerializeField]
        private TouchRayCaster touchRayCaster;

        [Tooltip("光标Size")]
        [SerializeField]
        private float cursorAngularSize = 100;

        [SerializeField]
        private Transform focusCursor;

        [SerializeField]
        private Transform pressCursor;

        private Transform customCursorVisual;
        public Transform CustomCursorVisual { get { return customCursorVisual; } set { customCursorVisual = value; customCursorVisual?.SetParent(this.transform); } }

        private Pose customTargetPose = Pose.identity;
        public Pose CustomTargetPose { get => customTargetPose; set => customTargetPose = value; }


        private void Start()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += UpdateVisualState;
        }

        private void OnDestroy()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= UpdateVisualState;
        }

        private void OnEnable()
        {
            RKPointerListener.OnGraphicPointerEnter += OnPointerEnter;
            RKPointerListener.OnGraphicPointerExit += OnPointerExit;
            RKPointerListener.OnGraphicPointerHover += OnPointerHover;
        }


        private void OnDisable()
        {
            RKPointerListener.OnGraphicPointerEnter -= OnPointerEnter;
            RKPointerListener.OnGraphicPointerExit -= OnPointerExit;
            RKPointerListener.OnGraphicPointerHover -= OnPointerHover;
            customCursorVisual = null;
        }

        private Vector3 ComputeScaleWithAngularScale(Vector3 targetPoint)
        {
            if (MainCameraCache.mainCamera != null)
            {
                float cursorDistance = Vector3.Distance(MainCameraCache.mainCamera.transform.position, targetPoint);
                float desiredScale = Utils.ScaleFromAngularSizeAndDistance(cursorAngularSize, cursorDistance);
                return Vector3.one * desiredScale;
            }
            return Vector3.one;
        }

        private void UpdateVisual()
        {
            if (rayInteractor.State == InteractorState.Disabled)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);


            Vector3 EndPosition;
            Vector3 EndNormal;
            if (floatingUI != null)
            {
                EndPosition = hoverPosition;
                EndNormal = hoverNormal;
            }
            else
            {
                EndPosition = rayInteractor.End;
                EndNormal = rayInteractor.Forward;
            }

            if (rayInteractor.State == InteractorState.Select || rayInteractor.State == InteractorState.Hover)
            {
                if (rayInteractor.CollisionInfo != null)
                {
                    this.transform.position = customTargetPose != Pose.identity ? customTargetPose.position : EndPosition - EndNormal * 0.001f;
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -rayInteractor.CollisionInfo.Value.Normal);
                    this.transform.eulerAngles = customTargetPose != Pose.identity ? customTargetPose.rotation.eulerAngles : new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
                    this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
                }
                else
                {
                    this.transform.position = customTargetPose != Pose.identity ? customTargetPose.position : EndPosition - EndNormal * 0.001f;
                    this.transform.rotation = customTargetPose != Pose.identity ? customTargetPose.rotation : Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                    this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
                }
            }
            else if (rayInteractor.State == InteractorState.Normal)
            {
                this.transform.position = EndPosition;
                if (MainCameraCache.mainCamera != null)
                    this.transform.rotation = Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
            }

            bool press = rayInteractor.State == InteractorState.Select;
            focusCursor.gameObject.SetActive(!press);
            pressCursor.gameObject.SetActive(press);

            if (customCursorVisual != null)
            {
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
            }
        }
        private void UpdateVisualState(InteractorStateChangeArgs args) => UpdateVisual();

        #region For FloatingUI
        private Vector3 hoverPosition;
        private Vector3 hoverNormal;
        private IFloatingUI floatingUI;
        public event Action<HandType, bool> OnCustomFocusCursorActive;

        private void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (floatingUI == null)
                    floatingUI = eventData.pointerCurrentRaycast.gameObject.GetComponent<IFloatingUI>();
            }
        }
        private void OnPointerExit(PointerEventData eventData)
        {
            floatingUI = null;
        }
        private void OnPointerHover(PointerEventData eventData)
        {
            hoverPosition = eventData.pointerCurrentRaycast.worldPosition;
            hoverNormal = eventData.pointerCurrentRaycast.worldNormal;
        }

        private SpriteRenderer[] spriteRenders;
        public void CustomCursorAlpha(float alpha)
        {
            if (spriteRenders == null)
            {
                spriteRenders = GetComponentsInChildren<SpriteRenderer>(true);
            }
            if (spriteRenders != null)
            {
                for (int i = 0; i < spriteRenders.Length; i++)
                {
                    Color color = spriteRenders[i].color;
                    spriteRenders[i].color = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }
        #endregion
    }
}
