using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Native;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using Rokid.UXR.Arithmetic;

namespace Rokid.UXR.Interaction
{

    public enum ThreeDofType
    {
        StationPro,
        Phone,
        Station2
    }

    /// <summary>
    /// The 3dof event input class provides an external interface for 3dof interaction
    /// </summary>
    public class ThreeDofEventInput : MonoSingleton<ThreeDofEventInput>, IEventInput
    {
        public enum SleepActiveType
        {
            Shake,
            AnyKeyDown
        }
        public static event Action<Quaternion> OnPhoneRayRotation;
        public static event Action<Quaternion> OnOriRot;
        public static event Action OnActiveThreeDofModule;
        public static event Action OnReleaseThreeDofModule;
        public static event Action OnThreeDofReset;
        public static event Action<bool> OnThreeDofSleep;
        public static event Action OnSwipeTriggerSuccess;
        public Transform Interactor { get; set; }
        private float[] data = new float[4];
        private float[] oriData = new float[4];
        private bool initialize = false;
        private HandType hand = HandType.RightHand;
        public HandType HoldHandType { get { return hand; } set { hand = value; } }
        private int pixelDragThreshold = 60;
        public int PixelDragThreshold { get { return pixelDragThreshold; } set { pixelDragThreshold = value; } }

        public InputModuleType inputModuleType => InputModuleType.ThreeDof;

        private BaseRayCaster raycaster;
        public BaseRayCaster GetRayCaster(HandType hand = HandType.None)
        {
            if (raycaster == null && Interactor != null)
            {
                raycaster = Interactor.GetComponent<BaseRayCaster>();
            }
            return raycaster;
        }
        private ISelector raySelector;
        public ISelector GetRaySelector(HandType hand = HandType.None)
        {
            if (raySelector == null && Interactor != null)
            {
                raySelector = Interactor.GetComponentInChildren<ISelector>();
            }
            return raySelector;
        }
        private IRayPose rayPose;
        public IRayPose GetRayPose(HandType hand = HandType.None)
        {
            if (rayPose == null && Interactor != null)
            {
                rayPose = Interactor.GetComponentInChildren<IRayPose>();
            }
            return rayPose;
        }
        private Quaternion rayRotation = Quaternion.identity;
        private Quaternion oriRayRotation = Quaternion.identity;
        private Quaternion preRayRotation = Quaternion.identity;
        private ThreeDofType threeDofType = ThreeDofType.StationPro;
        private float raySleepTime = 10.0f;
        private float raySleepElasptime = 0;
        private float height = -0.5f;
        private bool raySleep;
        private bool dragging;
        private bool lockInput;
        private SwipeLogic horizontalUpSwipe;
        private SwipeLogic verticalLeftSwipe;
        private SwipeLogic verticalRightSwipe;
        private float deltaTime;
        private bool triggerSuccess;

        private RKCameraRig cameraRig;

        private SleepActiveType activeType = SleepActiveType.Shake;

        public void SetActiveType(SleepActiveType activeType)
        {
            this.activeType = activeType;
        }

        protected override void Awake()
        {
            cameraRig = MainCameraCache.mainCamera.transform.GetComponent<RKCameraRig>();
            NativeInterface.NativeAPI.OpenPhoneTracker();
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;

            horizontalUpSwipe = new SwipeLogic(0, Quaternion.identity, SwipeSuccess, () => { return !dragging && !IsTouchOperate(); });
            verticalLeftSwipe = new SwipeLogic(1, Quaternion.AngleAxis(-90, Vector3.up), SwipeSuccess, () => { return !dragging && !IsTouchOperate(); });
            // verticalRightSwipe = new SwipeLogic(2, Quaternion.AngleAxis(90, Vector3.up), SwipeSuccess, () => { return !dragging && !IsTouchOperate(); });
        }


        private void SwipeUpdate()
        {
            if (triggerSuccess == false)
            {
                horizontalUpSwipe.Update();
                verticalLeftSwipe.Update();
                // verticalRightSwipe.Update();
            }
            if (triggerSuccess)
            {
                deltaTime += Time.deltaTime;
                if (deltaTime > 0.7f)
                {
                    deltaTime = 0;
                    triggerSuccess = false;
                }
            }
        }

        private bool IsTouchOperate()
        {
            return false;
        }

        private void SwipeSuccess(SwipeSuccessInfo info)
        {
            if (!triggerSuccess)
            {
                triggerSuccess = true;
                if (!lockInput)
                {
                    ActiveModule();
                    ResetImuAxisY();
                    Sleep(false);
                }
                RKLog.KeyInfo($"====SwipeLogic====: SwipeSuccess {info.ToString()}");
                OnSwipeTriggerSuccess?.Invoke();
            }
        }

        private void OnPointerDragEnd(PointerEventData data)
        {
            dragging = false;
        }

        private void OnPointerDragBegin(PointerEventData data)
        {
            if (InputModuleManager.Instance.GetThreeDofActive())
                dragging = true;
        }

        public ThreeDofType GetThreeDofType()
        {
            return threeDofType;
        }


        private void Update()
        {
            if (!initialize)
                return;
            if (Utils.IsAndroidPlatform())
            {
                UpdateThreeDofType();
                SwipeUpdate();
                if (lockInput)
                    return;
                if (InputModuleManager.Instance.GetThreeDofActive())
                {
                    if (CanResetImu() || IsDoubleClick())
                    {
                        ResetImuAxisY();
                    }
                    if (Input.anyKeyDown && raySleep)
                    {
                        raySleepElasptime = 0;
                        raySleep = false;
                        OnThreeDofSleep?.Invoke(false);
                    }
                    GetData();
                    ProcessData();
                }
                else
                {
                    //只有station pro 设备能直接双击+重置
                    if (IsDoubleClick() && threeDofType == ThreeDofType.StationPro)
                    {
                        ActiveModule();
                        ResetImuAxisY();
                    }
                    else if (CanActiveModule())
                    {
                        ActiveModule();
                    }
                }
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.H))
            {
                ActiveModule();
                ResetImuAxisY();
            }
#endif      
        }

        private void UpdateThreeDofType()
        {
            if (threeDofType != ThreeDofType.StationPro && (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
                       || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_DOWN)
                       || Input.GetKeyUp(KeyCode.JoystickButton0)))
            {
                threeDofType = ThreeDofType.StationPro;
            }
            else if (threeDofType != ThreeDofType.Phone && Input.touchCount > 0 && Utils.IsPhone())
            {
                threeDofType = ThreeDofType.Phone;
            }
            else if (threeDofType != ThreeDofType.Station2 && Input.touchCount > 0 && !Utils.IsPhone())
            {
                threeDofType = ThreeDofType.Station2;
            }
        }

        private void GetData()
        {
            switch (threeDofType)
            {
                case ThreeDofType.Station2:
                case ThreeDofType.StationPro:
                case ThreeDofType.Phone:
                    NativeInterface.NativeAPI.GetPhonePose(data);
                    if (cameraRig.headTrackingType == RKCameraRig.HeadTrackingType.ZeroDofEiv)
                    {
                        NativeInterface.NativeAPI.GetOriPhonePose(oriData);
                    }
                    break;
            }
#if USE_ROKID_OPENXR
            rayRotation[0] = -data[0];
            rayRotation[1] = -data[1];
            rayRotation[2] = data[2];
            rayRotation[3] = data[3];
            if (cameraRig.headTrackingType == RKCameraRig.HeadTrackingType.ZeroDofEiv)
            {
                oriRayRotation[0] = -oriData[0];
                oriRayRotation[1] = -oriData[1];
                oriRayRotation[2] = oriData[2];
                oriRayRotation[3] = oriData[3];
            }
#else
            rayRotation[0] = data[0];
            rayRotation[1] = data[1];
            rayRotation[2] = -data[2];
            rayRotation[3] = data[3];
            if (cameraRig.headTrackingType == RKCameraRig.HeadTrackingType.ZeroDofEiv)
            {
                oriRayRotation[0] = oriData[0];
                oriRayRotation[1] = oriData[1];
                oriRayRotation[2] = -oriData[2];
                oriRayRotation[3] = oriData[3];
            }
#endif
        }

        private bool CanActiveModule()
        {
            switch (threeDofType)
            {
                case ThreeDofType.StationPro:
                    return Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
                      || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_DOWN)
                      || Input.GetKeyUp(KeyCode.JoystickButton0);
            }
            return false;
        }

        private bool CanResetImu()
        {
            return RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RESET_RAY);
        }

        public void Initialize(Transform parent)
        {
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("ThreeDofRayInteractor");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/ThreeDofRayInteractor"));
                }
                Interactor = go.transform.GetComponentInChildren<ModuleInteractor>().transform;
                Interactor.name = "ThreeDofRayInteractor";
                Interactor.SetParent(transform);
            }
            Interactor.SetParent(transform);
            this.transform.SetParent(parent);
            initialize = true;
        }

        public void Release()
        {
            OnReleaseThreeDofModule?.Invoke();
            horizontalUpSwipe.Release();
            verticalLeftSwipe.Release();
            // verticalRightSwipe.Release();
            Destroy(this.gameObject);
        }

        public void ActiveModule()
        {
            if (!InputModuleManager.Instance.GetThreeDofActive())
            {
                ThreeDofEventInput.OnActiveThreeDofModule?.Invoke();
                RKVirtualController.Instance.Change(ControllerType.PHONE3DOF);
                EventSystem.current.pixelDragThreshold = PixelDragThreshold;
                if (raySleep)
                {
                    raySleepElasptime = 0;
                    raySleep = false;
                    OnThreeDofSleep?.Invoke(false);
                }
                UnityPlayerAPI.Instance.SetUnityScreenOrientation(ScreenOrientation.Portrait);
            }
        }

        protected override void OnDestroy()
        {
            // station 设备 SDK不再关闭3dof 射线算法
            if (!SystemInfo.deviceModel.Contains("station"))
            {
                NativeInterface.NativeAPI.ClosePhoneTracker();
            }
            OnPhoneRayRotation = null;
            initialize = false;
        }

        void ProcessData()
        {
            OnPhoneRayRotation?.Invoke(rayRotation);
            OnOriRot?.Invoke(rayRotation);
            // LogThreeDofInfo(rayRotation.eulerAngles);
            if (InputModuleManager.Instance.GetThreeDofActive())
            {
                Vector3 preForward = preRayRotation * Vector3.forward;
                Vector3 forward = cameraRig.headTrackingType == RKCameraRig.HeadTrackingType.ZeroDofEiv ? oriRayRotation * Vector3.forward : rayRotation * Vector3.forward;
                if (raySleep == false && Vector3.Angle(preForward, forward) < 0.05f)
                {
                    raySleepElasptime += Time.deltaTime;
                    if (raySleepElasptime > raySleepTime)
                    {
                        raySleep = true;
                        OnThreeDofSleep.Invoke(true);
                    }
                }

                if (Vector3.Angle(preForward, forward) > 0.05f)
                {
                    raySleepElasptime = 0;
                }

                if ((activeType == SleepActiveType.Shake && Vector3.Angle(preForward, forward) > 0.05f) || Input.touchCount != 0 || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RESET_RAY) ||
               RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_LIGHT_SINGLE_TAP) ||
               RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_LIGHT_DOUBLE_TAP) ||
               RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_LIGHT_LONG_TAP))
                {
                    raySleepElasptime = 0;
                    if (raySleep)
                    {
                        raySleep = false;
                        OnThreeDofSleep.Invoke(false);
                    }
                }
                preRayRotation = cameraRig.headTrackingType == RKCameraRig.HeadTrackingType.ZeroDofEiv ? oriRayRotation : rayRotation;
            }
        }

        public void ResetImuAxisY()
        {
            NativeInterface.NativeAPI.RecenterPhonePose();
            OnThreeDofReset?.Invoke();
            // RKLog.KeyInfo("====ThreeDofEventInput==== 重置3dof射线");
        }

        #region IsDoubleClick
        float doubleClickTime = 0.7f;
        float clickTime = 0;
        int clickCount = 0;
        //Only for station pro
        Vector2 touchPos = Vector2.zero;
        private bool IsDoubleClick()
        {
            switch (threeDofType)
            {
                case ThreeDofType.StationPro:
                    if (Input.GetKeyDown(KeyCode.JoystickButton3))
                    {
                        clickCount++;
                    }
                    break;
            }
            if (clickCount == 1)
            {
                clickTime += Time.deltaTime;
            }
            if (clickTime < doubleClickTime)
            {
                if (clickCount == 2)
                {
                    clickTime = 0;
                    clickCount = 0;
                    touchPos = Vector2.zero;
                    return true;
                }
            }
            else
            {
                clickCount = 0;
                clickTime = 0;
                touchPos = Vector2.zero;
            }
            return false;
        }
        #endregion

        public float ForwardSpeed()
        {
            if (RKTouchInput.Instance.GetInsideTouchCount() > 0)
            {
                switch (threeDofType)
                {
                    case ThreeDofType.Station2:
                        return RKTouchInput.Instance.GetInsideTouchDeltaPosition().y * 0.02f;
                    case ThreeDofType.Phone:
                        return RKTouchInput.Instance.GetInsideTouchDeltaPosition().y * 0.02f;
                }
                return 0;
            }
            else
            {
                return (Input.GetKey(KeyCode.UpArrow) ? 1 : (Input.GetKey(KeyCode.DownArrow) ? -1 : 0)) * 0.05f;
            }
        }

        /// <summary>
        /// 设置射线相对头的高度,默认位-0.5,范围(-0.5f,0)
        /// </summary>
        /// <param name="height"></param>
        public void SetRayHeight(float height)
        {
            height = Mathf.Clamp(height, -0.5f, 0);
            this.height = height;
        }
        public float GetRayHeight()
        {
            return height;
        }

        public void Sleep(bool sleep)
        {
            RKLog.KeyInfo("====ThreeDofEventInput====: Sleep");
            OnThreeDofSleep?.Invoke(sleep);
        }

        public void Lock(bool isLock)
        {
            this.lockInput = isLock;
        }
    }
}
