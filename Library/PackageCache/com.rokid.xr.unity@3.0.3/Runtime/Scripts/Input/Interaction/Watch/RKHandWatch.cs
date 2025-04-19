using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// RKWatch 
    /// </summary>
    public class RKHandWatch : MonoBehaviour
    {
        [SerializeField, Tooltip("跟随的手")]
        private HandType followhand = HandType.LeftHand;
        [SerializeField, Tooltip("跟随手的骨骼")]
        private SkeletonIndexFlag followSkeletonIndex = SkeletonIndexFlag.WRIST;
        private bool active = false;
        private bool handActive = false;
        private bool dragging = false;


        #region Event
        public static Action<HandType, bool> OnActiveWatch;
        public static Action<HandType, Pose> OnWatchPoseUpdate;

        #endregion

        private void Start()
        {
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            GesEventInput.OnTrackedSuccess += OnTrackedSuccess;
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
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
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
            GesEventInput.OnTrackedSuccess -= OnTrackedSuccess;
            RKPointerListener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd -= OnPointerDragEnd;
        }

        private void OnTrackedFailed(HandType hand)
        {
            if (hand == followhand || hand == HandType.None)
            {
                handActive = false;
                if (active)
                {
                    active = false;
                    OnActiveWatch?.Invoke(followhand, false);
                }
            }
        }

        private void OnTrackedSuccess(HandType hand)
        {
            if (hand == followhand)
            {
                handActive = true;
            }
        }

        private void Update()
        {
            if (handActive)
            {
                Pose pose = GesEventInput.Instance.GetHandPose(followhand);
                Pose skeletonPose = GesEventInput.Instance.GetSkeletonPose(followSkeletonIndex, followhand);

                //Process Enable Logic
                if (!active)
                {
                    if (Utils.InCameraView(skeletonPose.position, MainCameraCache.mainCamera) && !IsPalm() && GesEventInput.Instance.GetHandPress(followhand, false))
                    {
                        active = true;
                        OnActiveWatch?.Invoke(followhand, true);
                    }
                }

                //Process Disable Logic
                if (active)
                {
                    OnWatchPoseUpdate?.Invoke(followhand, new Pose(skeletonPose.position, pose.rotation));
                    if (!Utils.InCameraView(skeletonPose.position, MainCameraCache.mainCamera) || (GesEventInput.Instance.GetHandOrientation(followhand) == HandOrientation.Palm && IsPalm()))
                    {
                        active = false;
                        OnActiveWatch?.Invoke(followhand, false);
                    }
                }
            }
        }

        private bool IsPalm()
        {
            Vector3 handForward = (GesEventInput.Instance.GetHandPose(followhand).rotation * Vector3.forward).normalized;
            Vector3 cameraForward = MainCameraCache.mainCamera.transform.forward;
            return Vector3.Dot(handForward, cameraForward) < -0.7f;
        }
    }
}
