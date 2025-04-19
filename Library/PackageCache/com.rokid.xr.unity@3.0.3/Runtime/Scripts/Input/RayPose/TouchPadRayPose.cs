using UnityEngine;
using Rokid.UXR.Utility;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class TouchPadRayPose : BaseRayPose
    {
        private float yaw;
        private float pitch;
        private bool dragging = false;
        private float minSpeed = 0.7f, maxSpeed = 1.0f;
        private RayInteractor rayInteractor;

        private void Awake()
        {
            rayInteractor = GetComponent<RayInteractor>();
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
            InputModuleManager.OnModuleChange += OnModuleChange;
        }

        private void OnModuleChange(InputModuleType oldType, InputModuleType currentType)
        {
            if (currentType == InputModuleType.TouchPad)
            {
                if (oldType == InputModuleType.Mouse)
                {
                    transform.SetPose(InteractorPose[InputModuleType.Mouse]);
                }
                else
                {
                    ResetPose();
                }
            }
        }

        private void OnPointerDragEnd(PointerEventData data)
        {
            dragging = false;
        }

        private void OnPointerDragBegin(PointerEventData data)
        {
            dragging = true;
        }


        private void OnDestroy()
        {
            InputModuleManager.OnModuleChange -= OnModuleChange;
        }

        private void OnEnable()
        {
            TouchPadEventInput.OnMouseMove += OnMouseMove;
        }

        private void OnDisable()
        {
            TouchPadEventInput.OnMouseMove -= OnMouseMove;
        }


        /// <summary>
        /// 重置射线位姿
        /// </summary>
        private void ResetPose()
        {
            transform.position = MainCameraCache.mainCamera.transform.position;
            Vector3 localCenter = Utils.GetCameraCenter(rayInteractor.CurrentRayLength);
            Vector3 worldCenter = MainCameraCache.mainCamera.transform.localToWorldMatrix * localCenter;
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, worldCenter - transform.position);
            SavePose();
        }

        private void SavePose()
        {
            InteractorPose[InputModuleType.TouchPad] = transform.GetPose();
        }

        private void OnMouseMove(Vector2 delta)
        {
            if (updateType == PoseUpdateType.Auto)
            {
                float speed = Mathf.Clamp(delta.magnitude / (Time.deltaTime * 25), minSpeed, maxSpeed);

                Vector3 targetUp = Vector3.ProjectOnPlane(Vector3.up, transform.forward);

                if (Vector3.Angle(Vector3.down, MainCameraCache.mainCamera.transform.up) <= 45)
                {
                    targetUp = Vector3.ProjectOnPlane(Vector3.down, transform.forward);
                }

                if (Vector3.Angle(Vector3.up, MainCameraCache.mainCamera.transform.forward) <= 30 || Vector3.Angle(Vector3.down, MainCameraCache.mainCamera.transform.forward) <= 30)
                {
                    targetUp = Vector3.ProjectOnPlane(MainCameraCache.mainCamera.transform.up, transform.forward);
                }

                Vector3 targetRight = Vector3.ProjectOnPlane(MainCameraCache.mainCamera.transform.right, targetUp);
                transform.rotation = Quaternion.AngleAxis(delta.y * speed, targetRight) * Quaternion.AngleAxis(delta.x * speed, targetUp) * transform.rotation;
                if (!dragging)
                {
                    LimitInViewField();
                }
                SavePose();
            }
        }

        public override void UpdateTargetPoint(Vector3 point)
        {
            if (updateType == PoseUpdateType.FollowTargetPoint)
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.forward, point - transform.position);
                SavePose();
            }
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            LimitInViewField();
        }
#endif
    }
}

