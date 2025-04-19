using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;

namespace Rokid.UXR.Module
{
    public class RKTracePoseListener : MonoSingleton<RKTracePoseListener>
    {
        private TrackedPoseDriver mCurrentTrackedPoseDriver;
        private RKCameraRig mCameraRig;
        private bool IsSceneChanged;

        private RKCameraRig.HeadTrackingType m_HeadTrackingType;

        protected override void OnSingletonInit()
        {
            base.OnSingletonInit();
            this.gameObject.name = "RKTracePoseListener";
            DontDestroyOnLoad(transform);
            SceneManager.activeSceneChanged += OnSceneChanged;
            RKLog.Info("RKTracePoseListener OnSingletonInit");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.activeSceneChanged -= OnSceneChanged;
            RKLog.Info("RKTracePoseListener OnDestroy");
        }

        public void Initialize()
        {
            IsSceneChanged = false;
            m_HeadTrackingType = RKCameraRig.HeadTrackingType.ZeroDof;
        }

        private void Start()
        {
            if (MainCameraCache.mainCamera != null)
            {
                mCurrentTrackedPoseDriver = MainCameraCache.mainCamera.GetComponent<TrackedPoseDriver>();
                mCameraRig = MainCameraCache.mainCamera.GetComponent<RKCameraRig>();
            }

            if (mCurrentTrackedPoseDriver != null && mCameraRig == null)
            {
                m_HeadTrackingType = (RKCameraRig.HeadTrackingType)(mCurrentTrackedPoseDriver.trackingType);
                RKLog.Info($"====RKTracePoseListener====: SetHeadTrackingType in Start :{m_HeadTrackingType}");
                SetTrackingType(m_HeadTrackingType);
            }
            else
            {
                RKLog.Info($"====RKTracePoseListener====: No found the TracedPoseDriver or found RKCameraRig in Start");
            }
        }

        /// <summary>
        ///  listener the scene change
        /// </summary>
        /// <param name="previousScene"></param>
        /// <param name="changedScene"></param>
        private void OnSceneChanged(Scene previousScene, Scene changedScene)
        {
            IsSceneChanged = true;
            RKLog.Info($"====RKTracePoseListener====: OnSceneChanged changedScene previousScene : {previousScene.name} , newScene : {changedScene.name}");
        }

        private void LateUpdate()
        {
            // reacquire the TrackedPoseDriver when scene changed
            if (IsSceneChanged)
            {
                if (MainCameraCache.mainCamera != null)
                {
                    mCurrentTrackedPoseDriver = MainCameraCache.mainCamera.GetComponent<TrackedPoseDriver>();
                    mCameraRig = MainCameraCache.mainCamera.GetComponent<RKCameraRig>();
                }

                if (mCurrentTrackedPoseDriver != null && mCameraRig == null)
                {
                    m_HeadTrackingType = (RKCameraRig.HeadTrackingType)mCurrentTrackedPoseDriver.trackingType;
                    IsSceneChanged = false;
                    RKLog.Info($"====RKTracePoseListener====: SetHeadTrackingType in SceneChanged :{m_HeadTrackingType}");
                    SetTrackingType(m_HeadTrackingType);
                }
                else
                {
                    IsSceneChanged = false;
                    RKLog.Info($"====RKTracePoseListener====: No found the TracedPoseDriver in SceneChanged");
                }
            }

            // check the type change
            if (mCameraRig == null && mCurrentTrackedPoseDriver != null && m_HeadTrackingType != (RKCameraRig.HeadTrackingType)mCurrentTrackedPoseDriver.trackingType)
            {
                m_HeadTrackingType = (RKCameraRig.HeadTrackingType)mCurrentTrackedPoseDriver.trackingType;
                RKLog.Info($"====RKTracePoseListener====: SetHeadTrackingType in Update :{m_HeadTrackingType}");
                SetTrackingType(m_HeadTrackingType);
            }
        }

        /// <summary>
        /// Call Native Api set the trace type.
        /// </summary>
        /// <param name="type"></param>
        private void SetTrackingType(RKCameraRig.HeadTrackingType type)
        {
            if (Utils.IsAndroidPlatform())
            {
                NativeInterface.NativeAPI.SetTrackingType((int)type);
            }
        }
    }
}