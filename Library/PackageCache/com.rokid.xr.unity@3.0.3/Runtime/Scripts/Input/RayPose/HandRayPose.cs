using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Native;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Processing the pose information of the ray
    /// </summary>

    public class HandRayPose : BaseRayPose
    {
        [Tooltip("手的类型")]
        [SerializeField]
        public HandType hand;
        [Tooltip("肩膀位置,基于脖子空间左右对称")]
        [SerializeField]
        private Vector3 shoulder = new Vector3(-0.2f, -0.1f, 0);
        [Tooltip("脖子位置估算,基于相机空间计算")]
        [SerializeField]
        private Vector3 neck = new Vector3(0, -0.1f, -0.1f);

        [Tooltip("手掌方向对射线方向影响的强度")]
        [SerializeField]
        private float palmForwardInfluencePow = 0.7f;

        [SerializeField]
        private RayInteractor rayInteractor;
        [SerializeField]
        private RayVisual rayVisual;

        [SerializeField]
        private Text logText;
        [SerializeField, Tooltip("是否打印日志")]
        private bool log = false;

        private float upForwardInfluencePow = 0.06f;
        private Vector3 shoulderPos;
        private Vector3 delta = Vector3.zero;
        private bool rayFirstShow = true;
        private InteractorType interactorType = InteractorType.Far;

        /// <summary>
        /// 颈部位置的tsf
        /// </summary>
        private Transform neckTsf;
        private Vector3 preWristPos = Vector3.zero;
        private Vector3 wristPos = Vector3.zero;
        private float preCamRotY = 0;


        protected override void Start()
        {
            base.Start();
            neckTsf = new GameObject("neck").transform;
            neckTsf.SetParent(transform);
            GesEventInput.OnRayPoseUpdate += OnRayPoseUpdate;
            InteractorStateChange.OnInteractorTypeChange += OnInteractorTypeChange;
            upForwardInfluencePow = NativeInterface.NativeAPI.GetUpForwardInfluencePow();
        }

        private void OnInteractorTypeChange(HandType hand, InteractorType interactorType)
        {
            if (this.hand == hand)
            {
                rayFirstShow = true;
                this.interactorType = interactorType;
            }
        }

        private void OnEnable()
        {
            rayFirstShow = true;
        }

        private void OnDestroy()
        {
            GesEventInput.OnRayPoseUpdate -= OnRayPoseUpdate;
            InteractorStateChange.OnInteractorTypeChange -= OnInteractorTypeChange;
        }

        private void OnRayPoseUpdate(HandType hand, Vector3 wrist, Vector3 handCenter, Vector3 pinchCenterOri, Vector3 pinchCenter)
        {
            if (Utils.IsUnityEditor())
                return;
            if (this.hand == hand)
            {
                float deltaWrist = Vector3.SqrMagnitude(wrist - preWristPos);
                neckTsf.position = MainCameraCache.mainCamera.transform.TransformPoint(neck);
                neckTsf.rotation = Quaternion.Euler(0, deltaWrist > 0.1f ? MainCameraCache.mainCamera.transform.eulerAngles.y : preCamRotY, 0);
                shoulderPos = neckTsf.TransformPoint(shoulder);
                preWristPos = wrist;
                preCamRotY = MainCameraCache.mainCamera.transform.eulerAngles.y;
                //手势关键信息的更新
                delta = rayFirstShow ? pinchCenter - pinchCenterOri : Vector3.Lerp(delta, pinchCenter - pinchCenterOri, Time.deltaTime * 3);
                rayFirstShow = false;
                //胳膊朝向
                Vector3 armForward = (wrist - shoulderPos).normalized;
                //手腕朝向
                Vector3 palmForWard = (handCenter - wrist).normalized;
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.FromToRotation(Vector3.forward, armForward + palmForWard * palmForwardInfluencePow + Vector3.up * upForwardInfluencePow), Time.deltaTime * 10);
                if (float.IsNaN(this.transform.position.x) || float.IsNaN(this.transform.position.y) || float.IsNaN(this.transform.position.z) || float.IsNaN(this.transform.rotation.x) || float.IsNaN(this.transform.rotation.y) || float.IsNaN(this.transform.rotation.z) || float.IsNaN(this.transform.rotation.w))
                {
                    RKLog.Error($"====HandRayPose==== Data Error {MainCameraCache.mainCamera.transform.position}cameraRot:{MainCameraCache.mainCamera.transform.rotation.eulerAngles},{wrist},{handCenter},{pinchCenterOri},{pinchCenter}");
                    transform.position = Vector3.zero;
                    transform.rotation = Quaternion.identity;
                    neckTsf.position = Vector3.zero;
                    neckTsf.rotation = Quaternion.identity;
                }
                this.transform.position = Vector3.Lerp(this.transform.position, pinchCenterOri + delta, Time.deltaTime * 20);
                // LogHandRayInfo(wrist, handCenter);
            }
        }


        #region LogHandRayInfo
        private float logTime = 5;
        private float logElapsedTime = 0;
        private void LogHandRayInfo(Vector3 wrist, Vector3 handCenter)
        {
            logElapsedTime += Time.deltaTime;
            if (logElapsedTime > logTime)
            {
                logElapsedTime = 0;
                string logInfo = $"====HandRayPose====\n camPos:{MainCameraCache.mainCamera.transform.position},\n camRot:{MainCameraCache.mainCamera.transform.rotation.eulerAngles}\n neckPos: {neckTsf.position}\n shoulderPos: {shoulderPos}\n wristPos:{wrist}\n wristPosInCamera:{MainCameraCache.mainCamera.transform.InverseTransformPoint(wrist)}\n handPos:{handCenter} \n rayPos:{this.transform.position} \n rayPos:{this.transform.rotation.eulerAngles} \n rayRot:{this.transform.rotation.eulerAngles} ";
                if (logText != null)
                    logText.text = logInfo;
                RKLog.KeyInfo(logInfo);
            }
        }
        #endregion
    }
}
