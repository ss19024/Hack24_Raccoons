using System;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Processing the pose information of the ray
    /// </summary>
    public class HeadHandRayPose : BaseRayPose, IHeadHandDriver
    {
        private RayInteractor rayInteractor;

        [SerializeField]
        private HandType handType;
        [SerializeField, Tooltip("Shoulder position, symmetrically based on neck space")]
        private Vector3 shoulder = new Vector3(-0.2f, -0.1f, 0);
        [SerializeField, Tooltip("Neck position estimation, calculated based on camera space.")]
        private Vector3 neck = new Vector3(0, -0.1f, -0.1f);
        [SerializeField, Tooltip("The strength of the influence of palm direction on the ray direction.")]
        private float palmForwardInfluencePow = 0f;
        [SerializeField, Tooltip("The intensity of ray upward bending.")]
        private float upForwardInfluencePow = 0.0f;

        private Vector3 shoulderPos;
        private Vector3 delta = Vector3.zero;
        private bool rayFirstShow = true;
        private InteractorType interactorType = InteractorType.Far;

        /// <summary>
        /// 颈部位置的tsf
        /// </summary>
        private Transform neckTsf;
        private Vector3 preWristPos = Vector3.zero;
        private float preCamRotY = 0;
        private Transform rayOrigin;

        private Vector3 startRayPos;
        private Vector3 startRayForward;

        private Vector3 preRayPos;
        private Vector3 preRayForward;

        private Vector3 deltaRayPos;
        private Vector3 deltaRayForward;

        private Vector3 allDeltaRayPos;
        private Vector3 allDeltaRayForward;
        private int handFrame = 0;
        private int headFrame = 0;
        private float noHoverCursorDistance;


        protected override void Start()
        {
            base.Start();
            if (rayInteractor == null)
            {
                rayInteractor = GetComponent<RayInteractor>();
            }
            GesEventInput.OnRayPoseUpdate += OnRayPoseUpdate;
            neckTsf = new GameObject("neck").transform;
            neckTsf.SetParent(transform);
            rayOrigin = rayInteractor.GetRayOriginTsf();
            noHoverCursorDistance = rayInteractor.NoHoverCursorDistance;
        }


        private void OnDestroy()
        {
            GesEventInput.OnRayPoseUpdate -= OnRayPoseUpdate;
        }

        public void OnChangeHoldHandType(HandType hand)
        {
            if (hand != HandType.None)
            {
                handType = hand;
                shoulder = handType == HandType.RightHand ? new Vector3(0.2f, -0.1f, 0) : new Vector3(-0.2f, 0.1f, 0);
                startRayPos = rayOrigin.position;
                startRayForward = rayOrigin.forward;
            }
        }

        public void OnHandPress(HandType hand)
        {
            if (handFrame > 10)
            {
                allDeltaRayPos += deltaRayPos;
                allDeltaRayForward += deltaRayForward;
                rayOrigin.rotation = Quaternion.Slerp(rayOrigin.rotation, Quaternion.FromToRotation(Vector3.forward, startRayForward + allDeltaRayForward), Time.deltaTime * 10);
                rayOrigin.position = Vector3.Lerp(rayOrigin.position, startRayPos + allDeltaRayPos, Time.deltaTime * 30);
            }
        }

        public void OnHandRelease()
        {
            headFrame++;
            if (Utils.IsAndroidPlatform() && headFrame > 1)
            {
                rayOrigin.SetPositionAndRotation(MainCameraCache.mainCamera.transform.position + new Vector3(0, 0.05f, 0), Quaternion.FromToRotation(Vector3.forward, MainCameraCache.mainCamera.transform.forward + new Vector3(0, 0.05f, 0)));
            }
        }

        private void OnRayPoseUpdate(HandType hand, Vector3 wrist, Vector3 handCenter, Vector3 pinchCenterOri, Vector3 pinchCenter)
        {
            if (Utils.IsUnityEditor())
                return;
            if (hand == handType)
            {
                float deltaWrist = Vector3.SqrMagnitude(wrist - preWristPos);
                float deltaCamPos = Vector3.SqrMagnitude(MainCameraCache.mainCamera.transform.position - preWristPos);
                neckTsf.position = MainCameraCache.mainCamera.transform.TransformPoint(neck);
                neckTsf.rotation = Quaternion.Euler(0, deltaWrist > 0.1f ? MainCameraCache.mainCamera.transform.eulerAngles.y : preCamRotY, 0);
                shoulderPos = neckTsf.TransformPoint(shoulder);
                preWristPos = wrist;
                preCamRotY = MainCameraCache.mainCamera.transform.eulerAngles.y;

                //胳膊朝向
                Vector3 armForward = (wrist - shoulderPos).normalized;
                //手腕朝向
                Vector3 palmForWard = (handCenter - wrist).normalized;

                Vector3 curForward = armForward + palmForWard * palmForwardInfluencePow + Vector3.up * upForwardInfluencePow;
                Vector3 curPos = pinchCenterOri;

                deltaRayForward = curForward - preRayForward;
                deltaRayPos = curPos - preRayPos;

                preRayForward = curForward;
                preRayPos = curPos;

                handFrame++;
            }
        }

        public void OnBeforeChangeHoldHandType(HandType hand)
        {
            if (hand == HandType.None)
            {
                rayInteractor.NoHoverCursorDistance = noHoverCursorDistance;
                handType = HandType.None;
                allDeltaRayForward = Vector3.zero;
                allDeltaRayPos = Vector3.zero;
                handFrame = 0;
                headFrame = 0;
            }
        }
    }
}
