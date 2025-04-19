using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Utility;
using Rokid.UXR.Module;
using Rokid.UXR.Native;
using UnityEngine.Assertions;


namespace Rokid.UXR.Interaction
{
    public enum InputModuleType
    {
        None = 0,
        ThreeDof = 1 << 1, //手机3dof 射线
        Gesture = 1 << 2, //手势
        Mouse = 1 << 3, // 蓝牙鼠标
        TouchPad = 1 << 4, // 触摸屏
        All,// 所有交互
    }

    [Flags]
    public enum ActiveModuleType
    {
        ThreeDof = 1 << 0, //手机 3dof 射线
        Gesture = 1 << 1, //手势
        Mouse = 1 << 2, // 蓝牙鼠标
        TouchPad = 1 << 3, // 触摸屏
    }

    [Flags]
    public enum ActiveHandType
    {
        LeftHand = 1 << 0, //左手
        RightHand = 1 << 1, //右手
    }


    [Flags]
    public enum ActiveHandOrientationType
    {
        Back = 1 << 0, //左手
        Palm = 1 << 1, //右手
    }


    [Flags]
    public enum ActiveHandInteractorType
    {
        Far = 1 << 0, //远场交互
        Near = 1 << 1, //进场交互
    }


    [Flags]
    public enum ActiveWatchType
    {
        DisableWatch = 1 << 0,
        EnableWatch = 1 << 1,
    }


    [Flags]
    public enum ActiveHandRayType
    {
        LeftHandRay = 1 << 0,
        RightHandRay = 1 << 1,
    }


    [Flags]
    public enum ActiveHeadHandType
    {
        NormalHand = 1 << 0,
        HeadHand = 1 << 1,
    }


    public enum DeviceInputModule
    {
        ThreeDof,
        TouchPad
    }

    /// <summary>
    /// Input Module status 
    /// </summary>
    [Serializable]
    public class ActiveModuleStatus
    {
        public InputModuleType moduleType;
        public ActiveHandStatus leftHandStatus;
        public ActiveHandStatus rightHandStatus;
        public ActiveHeadHandType headHandType;
        public ActiveHandRayType handRayType;
        public bool threeDofSleep = false;
        public bool leftHandSleep = false;
        public bool rightHandSleep = false;
        public bool mouseSleep = false;
        public bool touchSleep = false;

        public override string ToString()
        {
            return $"ActiveInputModuleType: {moduleType} \r\n\r\nLeftHandStatus: {leftHandStatus}  \r\n\r\nRightHandStatus: {rightHandStatus} \r\n\r\n handRayType:{handRayType}";
        }
    }

    [Serializable]
    public class ActiveHandStatus
    {
        public bool active;
        public ActiveHandInteractorType handInteractorType;
        public ActiveHandOrientationType handOrientationType;
        public ActiveWatchType activeWatchType;
        public bool handDragging;

        public override string ToString()
        {
            return $" \r\n  Active:{active} \r\n  HandDragging:{handDragging}\r\n  HandInteractorType:{handInteractorType} \r\n  HandOrientationType:{handOrientationType} \r\n  ActiveWatchType:{activeWatchType}";
        }
    }


    /// <summary>
    /// This script implements the IInputModuleActive interface, which allows it to register its own activation status information to the InputModuleManager for centralized management and switching.
    /// </summary>
    public class InputModuleManager : MonoSingleton<InputModuleManager>
    {
        /// <summary>
        /// The default init module
        /// </summary>
        [SerializeField, Tooltip("默认初始化的模块")]
        private ActiveModuleType defaultInitModule;
        /// <summary>
        /// The default active module. 
        /// </summary>

        [SerializeField, Tooltip("默认激活的模块")]
        private InputModuleType defaultActiveModule;

        /// <summary>
        /// Whether to play a module switch sound.
        /// </summary>
        [HideInInspector, SerializeField, Tooltip("是否播放模块切换提示音")]
        private bool muteModuleActiveSound = false;
        /// <summary>
        /// Optional TextUI for debugging purposes.
        /// </summary>
        [Optional, SerializeField, Tooltip("调试Text")]
        private Text logText;
        [SerializeField,
        HideInInspector,
        Tooltip("模块的激活状态")]
        private ActiveModuleStatus activeModuleStatus = new ActiveModuleStatus();
        [SerializeField, Tooltip("当选择交互,设备不支持时,是否默认回退一个更为基础的交互")]
        private bool autoFallbackInput = true;

        [SerializeField, Tooltip("是否激活全局事件Log")]
        private bool enableGlobalEventLog = false;
        public bool EnableGlobalEventLog { get { return enableGlobalEventLog; } set { enableGlobalEventLog = value; } }
        private bool stateChanged, mouseInputLock, threeDofInputLock, touchPadInputLock, gestureInputLock;
        private List<IInputModuleActive> moduleActives = new List<IInputModuleActive>();
        public static event Action<IInputModuleActive, bool> OnObjectActive;
        public static event Action<InputModuleType> OnModuleActive;
        public static event Action<InputModuleType, InputModuleType> OnModuleChange;
        public static event Action OnInitialize;
        private bool initialize = false;
        private GlobalEventsLogUtils globalEventLog;
        private List<IEventInput> eventInputs = new List<IEventInput>();
        public bool GetInitialize()
        {
            return initialize;
        }

        public ActiveModuleType GetDefaultInitModule()
        {
            return defaultInitModule;
        }

        public bool GetMouseActive()
        {
            return activeModuleStatus.moduleType == InputModuleType.Mouse;
        }

        public bool GetGesActive()
        {
            return activeModuleStatus.moduleType == InputModuleType.Gesture;
        }

        public bool GetThreeDofActive()
        {
            return activeModuleStatus.moduleType == InputModuleType.ThreeDof;
        }

        public bool GetButtonMouseActive()
        {
            return false;// activeModuleStatus.moduleType == InputModuleType.ButtonMouse;
        }

        public bool GetTouchPadActive()
        {
            return activeModuleStatus.moduleType == InputModuleType.TouchPad;
        }

        public bool GetWatchModuleActive(HandType hand)
        {
            if (activeModuleStatus.moduleType == InputModuleType.Gesture)
            {
                if (hand == HandType.LeftHand)
                {
                    return activeModuleStatus.leftHandStatus.activeWatchType == ActiveWatchType.EnableWatch;
                }
                if (hand == HandType.RightHand)
                {
                    return activeModuleStatus.rightHandStatus.activeWatchType == ActiveWatchType.EnableWatch;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取当前激活的模块
        /// Gets the currently active module
        /// </summary>
        /// <returns></returns>
        public ActiveModuleStatus GetActiveModule()
        {
            return activeModuleStatus;
        }

        /// <summary>
        /// set default active module
        /// </summary>
        /// <param name="moduleType"></param>
        public void SetDefaultActiveModule(InputModuleType moduleType)
        {
            if (initialize)
            {
                RKLog.Error($"InputModuleManager is initialize can not to set default active module:{moduleType}");
                return;
            }
            defaultActiveModule = moduleType;
        }

        public void SetMuteModuleActiveSound(bool isMute)
        {
            muteModuleActiveSound = isMute;
        }

        public bool GetMuteModuleActiveSound()
        {
            return muteModuleActiveSound;
        }

        public void Initialize()
        {
            if (HasInputModuleType(defaultInitModule, ActiveModuleType.ThreeDof))
            {
                ThreeDofEventInput.Instance.Initialize(transform);
                eventInputs.Add(ThreeDofEventInput.Instance.GetComponent<IEventInput>());
            }

            if (HasInputModuleType(defaultInitModule, ActiveModuleType.Mouse))
            {
                MouseEventInput.Instance.Initialize(transform);
                eventInputs.Add(MouseEventInput.Instance.GetComponent<IEventInput>());
            }

            if (HasInputModuleType(defaultInitModule, ActiveModuleType.Gesture))
            {
                if (Utils.IsAndroidPlatform())
                {
                    if (autoFallbackInput && FuncDeviceCheck.CheckHandTrackingFunc())
                    {
                        GesEventInput.Instance.Initialize(transform);
                        eventInputs.Add(GesEventInput.Instance.GetComponent<IEventInput>());
                    }
                    else
                    {
                        string msg = Utils.IsChineseLanguage() ? "检测到您的设备不支持手势交互,已经将您的交互回退到3dof射线" : Utils.IsJapaneseLanguage() ? "お使いのデバイスがジェスチャー操作をサポートしていないことが検出されました。操作を3DOFレイに戻しました。" : "Detected that your device does not support gesture interaction, and your interaction has been reverted to 3DOF ray";
                        RKLog.Warning(msg);
                        ThreeDofEventInput.Instance.Initialize(transform);
                        eventInputs.Add(ThreeDofEventInput.Instance.GetComponent<IEventInput>());
                    }
                }
                else
                {
                    GesEventInput.Instance.Initialize(transform);
                    eventInputs.Add(GesEventInput.Instance.GetComponent<IEventInput>());
                }
            }

            if (HasInputModuleType(defaultInitModule, ActiveModuleType.TouchPad))
            {
                TouchPadEventInput.Instance.Initialize(transform);
                eventInputs.Add(TouchPadEventInput.Instance.GetComponent<IEventInput>());
            }

            switch (defaultActiveModule)
            {
                case InputModuleType.ThreeDof:
                    ThreeDofEventInput.Instance.ActiveModule();
                    break;
                case InputModuleType.Gesture:
                    if (Utils.IsAndroidPlatform())
                    {
                        if (autoFallbackInput && FuncDeviceCheck.CheckHandTrackingFunc())
                        {
                            GesEventInput.Instance.ActiveModule();
                        }
                        else
                        {
                            string msg = Utils.IsChineseLanguage() ? "检测到您的设备不支持手势交互,已经将您的交互回退到3dof射线" : Utils.IsJapaneseLanguage() ? "お使いのデバイスがジェスチャー操作をサポートしていないことが検出されました。操作を3DOFレイに戻しました。" : "Detected that your device does not support gesture interaction, and your interaction has been reverted to 3DOF ray";
                            RKLog.Warning(msg);
                            ThreeDofEventInput.Instance.ActiveModule();
                        }
                    }
                    else
                    {
                        GesEventInput.Instance.ActiveModule();
                    }
                    break;
                case InputModuleType.Mouse:
                    MouseEventInput.Instance.ActiveModule();
                    break;
                case InputModuleType.TouchPad:
                    TouchPadEventInput.Instance.ActiveModule();
                    break;
            }

            if (EventSystem.current == null)
            {
                GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Events/RKEventSystem"));
                go.name = "RKEventSystem";
                go.transform.SetParent(transform);
            }

            InitModuleChangeAudio();
            InitGlobalLog();
        }

        private void InitGlobalLog()
        {
            globalEventLog = GameObject.Find("GlobalEventLogUI(Clone)")?.GetComponent<GlobalEventsLogUtils>();
            if (globalEventLog == null)
                globalEventLog = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Utils/GlobalEventLogUI")).GetComponent<GlobalEventsLogUtils>();
            globalEventLog.transform.SetParent(this.transform);
            Assert.IsNotNull(globalEventLog);
#if UNITY_EDITOR
            string value = UnityEngine.PlayerPrefs.GetString("rokid.globaldebug.show");
            if (!string.IsNullOrEmpty(value))
            {
                this.enableGlobalEventLog = value == "1";
            }
            globalEventLog.gameObject.SetActive(enableGlobalEventLog);
#else
            //read sys prop
            string value = NativeInterface.NativeAPI.GetPersistValue("rokid.globaldebug.show");
            if (!string.IsNullOrEmpty(value))
            {
                this.enableGlobalEventLog = value == "1";
            }
            globalEventLog.gameObject.SetActive(enableGlobalEventLog);
#endif
        }


        /// <summary>
        /// 交互锁定后无法强制切换
        /// </summary>
        /// <param name="moduleType"></param>
        public void ForceActiveModule(InputModuleType moduleType)
        {
            if (!initialize)
            {
                RKLog.Error($"InputModuleManager is not initialize can not to force active module:{moduleType}");
                return;
            }
            if (this.activeModuleStatus.moduleType != moduleType && HasInputModuleType(defaultInitModule, ActiveModuleType.TouchPad))
            {
                switch (moduleType)
                {
                    case InputModuleType.ThreeDof:
                        if (HasInputModuleType(defaultInitModule, ActiveModuleType.ThreeDof))
                        {
                            stateChanged = true;
                            ThreeDofEventInput.Instance.ActiveModule();
                        }
                        break;
                    case InputModuleType.TouchPad:
                        if (HasInputModuleType(defaultInitModule, ActiveModuleType.TouchPad))
                        {
                            stateChanged = true;
                            TouchPadEventInput.Instance.ActiveModule();
                        }
                        break;
                    case InputModuleType.Gesture:
                        if (HasInputModuleType(defaultInitModule, ActiveModuleType.Gesture))
                        {
                            stateChanged = true;
                            GesEventInput.Instance.ActiveModule();
                        }
                        break;
                }
            }
        }

        private void Start()
        {
            InteractorStateChange.OnInteractorTypeChange += OnInteractorTypeChange;
            InteractorStateChange.OnHandDragStatusChanged += OnGestureDragStatusChanged;

            RKHandWatch.OnActiveWatch += OnActiveWatch;

            GesEventInput.OnHandOrHeadHandTypeChange += OnHandOrHeadHandTypeChange;

            MouseEventInput.OnActiveMouseModule += OnActiveMouseModule;
            ThreeDofEventInput.OnActiveThreeDofModule += OnActiveThreeDofModule;
            GesEventInput.OnActiveGesModule += OnActiveGesModule;
            TouchPadEventInput.OnActiveTouchPadModule += OnActiveTouchPadModule;

            MouseEventInput.OnReleaseMouseModule += OnReleaseMouseModule;
            ThreeDofEventInput.OnReleaseThreeDofModule += OnReleaseThreeDofModule;
            GesEventInput.OnReleaseGesModule += OnReleaseGesModule;
            TouchPadEventInput.OnReleaseTouchPadModule += OnReleaseTouchPadModule;

            GesEventInput.OnTrackedSuccess += OnTrackedSuccess;
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            GesEventInput.OnHandOrientationUpdate += OnHandOrientationUpdate;

            ThreeDofEventInput.OnThreeDofSleep += OnThreeDofSleep;
            GesEventInput.OnHandSleep += OnHandSleep;
            TouchPadEventInput.OnTouchPadSleep += OnTouchPadSleep;
            MouseEventInput.OnMouseSleep += OnMouseSleep;
            GesEventInput.OnHandRayActive += OnHandRayActive;

            OnModuleChange += _OnModuleChange;

            GesEventInput.OnHandRayActive += OnHandRayActive;

            OnModuleChange += _OnModuleChange;

            Initialize();
            initialize = true;
            OnInitialize?.Invoke();
        }

        private void _OnModuleChange(InputModuleType oldType, InputModuleType newType)
        {
            RKLog.KeyInfo("====OnModuleChange===  " + oldType + " " + newType);
            if (oldType != InputModuleType.None)
            {
                switch (newType)
                {
                    case InputModuleType.ThreeDof:
                    case InputModuleType.TouchPad:
                        NativeInterface.NativeAPI.Vibrate(2);
                        break;
                }
            }
        }



        private void OnHandRayActive(HandType type)
        {
            if (type == HandType.LeftHand && activeModuleStatus.handRayType != ActiveHandRayType.LeftHandRay)
            {
                activeModuleStatus.handRayType = ActiveHandRayType.LeftHandRay;
                stateChanged = true;
                // PlayModuleChangeAudio();
            }
            if (type == HandType.RightHand && activeModuleStatus.handRayType != ActiveHandRayType.RightHandRay)
            {
                activeModuleStatus.handRayType = ActiveHandRayType.RightHandRay;
                stateChanged = true;
                // PlayModuleChangeAudio();
            }
        }

        private void OnTouchPadSleep(bool sleeping)
        {
            if (activeModuleStatus.touchSleep != sleeping)
            {
                activeModuleStatus.touchSleep = sleeping;
                stateChanged = true;
            }
        }

        private void OnHandSleep(HandType handType, bool sleeping)
        {
            if (handType == HandType.LeftHand)
            {
                if (activeModuleStatus.leftHandSleep != sleeping)
                {
                    activeModuleStatus.leftHandSleep = sleeping;
                    stateChanged = true;
                }
            }

            if (handType == HandType.RightHand)
            {
                if (activeModuleStatus.rightHandSleep != sleeping)
                {
                    activeModuleStatus.rightHandSleep = sleeping;
                    stateChanged = true;
                }
            }
        }

        private void OnMouseSleep(bool sleeping)
        {
            if (activeModuleStatus.mouseSleep != sleeping)
            {
                activeModuleStatus.mouseSleep = sleeping;
                stateChanged = true;
            }
        }
        private void OnThreeDofSleep(bool sleeping)
        {
            if (activeModuleStatus.threeDofSleep != sleeping)
            {
                activeModuleStatus.threeDofSleep = sleeping;
                stateChanged = true;
            }
        }

        /// <summary>
        /// 获取当前激活的事件
        /// </summary>
        /// <returns></returns>
        public IEventInput GetActiveEventInput()
        {
            switch (GetActiveModule().moduleType)
            {
                case InputModuleType.ThreeDof:
                    return ThreeDofEventInput.Instance.GetComponent<IEventInput>();
                case InputModuleType.Gesture:
                    return GesEventInput.Instance.GetComponent<IEventInput>();
                case InputModuleType.Mouse:
                    return MouseEventInput.Instance.GetComponent<IEventInput>();
                case InputModuleType.TouchPad:
                    return TouchPadEventInput.Instance.GetComponent<IEventInput>();
            }
            return null;
        }

        public void LockAllEventInput(bool isLock)
        {
            LockEventInput(isLock, InputModuleType.All);
        }


        public void LockEventInput(bool isLock, InputModuleType inputModuleType = InputModuleType.All)
        {
            for (int i = 0; i < eventInputs.Count; i++)
            {
                if (inputModuleType == InputModuleType.All || inputModuleType == eventInputs[i].inputModuleType)
                    eventInputs[i].Lock(isLock);
            }
        }

        public void LockNativeInput(bool isLock)
        {
            RKNativeInput.Instance.Lock(isLock);
            RKTouchInput.Instance.Lock(isLock);
        }

        private void OnGestureDragStatusChanged(HandType hand, bool dragging)
        {
            if (hand == HandType.LeftHand)
            {
                if (activeModuleStatus.leftHandStatus.handDragging != dragging)
                {
                    activeModuleStatus.leftHandStatus.handDragging = dragging;
                    stateChanged = true;
                }
            }

            if (hand == HandType.RightHand)
            {
                if (activeModuleStatus.rightHandStatus.handDragging != dragging)
                {
                    activeModuleStatus.rightHandStatus.handDragging = dragging;
                    stateChanged = true;
                }
            }
        }

        private void OnActiveWatch(HandType hand, bool active)
        {
            if (hand == HandType.LeftHand)
            {
                activeModuleStatus.leftHandStatus.activeWatchType = active ? ActiveWatchType.EnableWatch : ActiveWatchType.DisableWatch;
                stateChanged = true;
            }
            if (hand == HandType.RightHand)
            {
                activeModuleStatus.rightHandStatus.activeWatchType = active ? ActiveWatchType.EnableWatch : ActiveWatchType.DisableWatch;
                stateChanged = true;
            }
        }

        private void OnHandOrientationUpdate(HandType hand, HandOrientation handOrientation)
        {
            if (hand == HandType.LeftHand)
            {
                if (activeModuleStatus.leftHandStatus.handOrientationType != ConvertType(handOrientation) && activeModuleStatus.leftHandStatus.handDragging == false)
                {
                    activeModuleStatus.leftHandStatus.handOrientationType = ConvertType(handOrientation);
                    stateChanged = true;
                }
            }

            if (hand == HandType.RightHand)
            {
                if (activeModuleStatus.rightHandStatus.handOrientationType != ConvertType(handOrientation) && activeModuleStatus.rightHandStatus.handDragging == false)
                {
                    activeModuleStatus.rightHandStatus.handOrientationType = ConvertType(handOrientation);
                    stateChanged = true;
                }
            }
        }

        private void OnTrackedFailed(HandType hand)
        {
            if (hand == HandType.LeftHand || hand == HandType.None)
            {
                if (activeModuleStatus.leftHandStatus.active == true)
                {
                    activeModuleStatus.leftHandStatus.active = false;
                    stateChanged = true;
                }
            }
            if (hand == HandType.RightHand || hand == HandType.None)
            {
                if (activeModuleStatus.rightHandStatus.active == true)
                {
                    activeModuleStatus.rightHandStatus.active = false;
                    stateChanged = true;
                }
            }
        }

        protected override void OnDestroy()
        {
            InteractorStateChange.OnInteractorTypeChange -= OnInteractorTypeChange;
            InteractorStateChange.OnHandDragStatusChanged -= OnGestureDragStatusChanged;

            RKHandWatch.OnActiveWatch -= OnActiveWatch;

            GesEventInput.OnHandOrHeadHandTypeChange -= OnHandOrHeadHandTypeChange;

            MouseEventInput.OnActiveMouseModule -= OnActiveMouseModule;
            ThreeDofEventInput.OnActiveThreeDofModule -= OnActiveThreeDofModule;
            GesEventInput.OnActiveGesModule -= OnActiveGesModule;
            TouchPadEventInput.OnActiveTouchPadModule -= OnActiveTouchPadModule;

            MouseEventInput.OnReleaseMouseModule -= OnReleaseMouseModule;
            ThreeDofEventInput.OnReleaseThreeDofModule -= OnReleaseThreeDofModule;
            GesEventInput.OnReleaseGesModule -= OnReleaseGesModule;
            TouchPadEventInput.OnReleaseTouchPadModule -= OnReleaseTouchPadModule;

            GesEventInput.OnTrackedSuccess -= OnTrackedSuccess;
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
            GesEventInput.OnHandOrientationUpdate -= OnHandOrientationUpdate;

            ThreeDofEventInput.OnThreeDofSleep -= OnThreeDofSleep;
            GesEventInput.OnHandSleep -= OnHandSleep;
            TouchPadEventInput.OnTouchPadSleep -= OnTouchPadSleep;
            MouseEventInput.OnMouseSleep -= OnMouseSleep;

            GesEventInput.OnHandRayActive -= OnHandRayActive;

            OnModuleChange -= _OnModuleChange;
        }



        private void OnInteractorTypeChange(HandType hand, InteractorType interactorType)
        {
            if (hand == HandType.LeftHand)
            {
                activeModuleStatus.leftHandStatus.handInteractorType = interactorType == InteractorType.Near ? ActiveHandInteractorType.Near : ActiveHandInteractorType.Far;
                stateChanged = true;
            }
            if (hand == HandType.RightHand)
            {
                activeModuleStatus.rightHandStatus.handInteractorType = interactorType == InteractorType.Near ? ActiveHandInteractorType.Near : ActiveHandInteractorType.Far;
                stateChanged = true;
            }
        }

        private void OnHandOrHeadHandTypeChange(HandOrHeadHandType handOrHeadHandType)
        {
            if (handOrHeadHandType == HandOrHeadHandType.NormalHand)
            {
                activeModuleStatus.headHandType = ActiveHeadHandType.NormalHand;
                stateChanged = true;
            }
            else
            {
                activeModuleStatus.headHandType = ActiveHeadHandType.HeadHand;
                stateChanged = true;
            }
        }

        private void OnActiveMouseModule()
        {
            if (!GetMouseActive() && !mouseInputLock)
            {
                OnModuleChange?.Invoke(activeModuleStatus.moduleType, InputModuleType.Mouse);
                if (activeModuleStatus.moduleType != InputModuleType.TouchPad)
                    PlayModuleChangeAudio();
                activeModuleStatus.moduleType = InputModuleType.Mouse;
                stateChanged = true;
                OnModuleActive?.Invoke(InputModuleType.Mouse);
            }
        }

        private void OnActiveThreeDofModule()
        {
            if (!GetThreeDofActive() && !threeDofInputLock)
            {
                OnModuleChange?.Invoke(activeModuleStatus.moduleType, InputModuleType.ThreeDof);
                activeModuleStatus.moduleType = InputModuleType.ThreeDof;
                stateChanged = true;

                // PlayModuleChangeAudio();

                OnModuleActive?.Invoke(InputModuleType.ThreeDof);
            }
        }

        private void OnActiveGesModule()
        {
            if (!GetGesActive() && !gestureInputLock)
            {
                OnModuleChange?.Invoke(activeModuleStatus.moduleType, InputModuleType.Gesture);
                activeModuleStatus.moduleType = InputModuleType.Gesture;

                activeModuleStatus.leftHandStatus.handInteractorType = ConvertType(GesEventInput.Instance.GetInteractorType(HandType.LeftHand));
                activeModuleStatus.leftHandStatus.handOrientationType = ConvertType(GesEventInput.Instance.GetHandOrientation(HandType.LeftHand));

                activeModuleStatus.rightHandStatus.handInteractorType = ConvertType(GesEventInput.Instance.GetInteractorType(HandType.RightHand));
                activeModuleStatus.rightHandStatus.handOrientationType = ConvertType(GesEventInput.Instance.GetHandOrientation(HandType.RightHand));

                stateChanged = true;

                PlayModuleChangeAudio();

                OnModuleActive?.Invoke(InputModuleType.Gesture);
            }
        }

        private void OnActiveTouchPadModule()
        {
            if (!GetTouchPadActive() && !touchPadInputLock)
            {
                OnModuleChange?.Invoke(activeModuleStatus.moduleType, InputModuleType.TouchPad);
                if (activeModuleStatus.moduleType != InputModuleType.Mouse)
                    PlayModuleChangeAudio();
                activeModuleStatus.moduleType = InputModuleType.TouchPad;
                stateChanged = true;
                OnModuleActive?.Invoke(InputModuleType.TouchPad);
            }
        }

        private void OnReleaseGesModule()
        {
            if (GetGesActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnReleaseThreeDofModule()
        {
            if (GetThreeDofActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnReleaseMouseModule()
        {
            if (GetMouseActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }


        private void OnReleaseTouchPadModule()
        {
            if (GetTouchPadActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnTrackedSuccess(HandType hand)
        {
            if (hand == HandType.LeftHand)
            {
                if (activeModuleStatus.leftHandStatus.active == false)
                {
                    activeModuleStatus.leftHandStatus.active = true;
                    stateChanged = true;
                }
            }
            if (hand == HandType.RightHand)
            {
                if (activeModuleStatus.rightHandStatus.active == false)
                {
                    activeModuleStatus.rightHandStatus.active = true;
                    stateChanged = true;
                }
            }
        }

        public void RegisterActive(IInputModuleActive moduleActive)
        {
            this.moduleActives.Add(moduleActive);
            stateChanged = true;
        }

        public void UnRegisterActive(IInputModuleActive moduleActive)
        {
            this.moduleActives.Remove(moduleActive);
        }

        private void SetActiveEnable(IInputModuleActive active, bool enabled)
        {
            if (active.Behaviour != null)
            {
                active.Behaviour.enabled = enabled;
            }
            else
            {
                active.Go.SetActive(enabled);
                OnObjectActive?.Invoke(active, enabled);
            }
        }

        public void LockInputModuleChange(InputModuleType inputModuleType = InputModuleType.All)
        {
            LockInput(true, inputModuleType);
        }

        public void ReleaseInputModuleChange(InputModuleType inputModuleType = InputModuleType.All)
        {
            LockInput(false, inputModuleType);
        }

        public void LockInput(bool isLock, InputModuleType inputModuleType = InputModuleType.All)
        {
            switch (inputModuleType)
            {
                case InputModuleType.ThreeDof:
                    threeDofInputLock = isLock;
                    break;
                case InputModuleType.Gesture:
                    gestureInputLock = isLock;
                    break;
                case InputModuleType.TouchPad:
                    touchPadInputLock = isLock;
                    break;
                case InputModuleType.Mouse:
                    mouseInputLock = isLock;
                    break;
                case InputModuleType.All:
                    threeDofInputLock = isLock;
                    gestureInputLock = isLock;
                    touchPadInputLock = isLock;
                    mouseInputLock = isLock;
                    break;
            }
            stateChanged = !isLock;
        }

        public void LockAndDisableAllInput(InputModuleType excludeInput = InputModuleType.None)
        {
            //Lock All  input
            LockInput(true);
            LockEventInput(true);
            LockNativeInput(true);
            //UnLock exclude input
            ReleaseInputModuleChange(excludeInput);
            LockEventInput(false, excludeInput);
            if (GetActiveEventInput().inputModuleType != excludeInput)
                GetActiveEventInput().Sleep(true);
        }

        public void UnLockAndEnableAllInput()
        {
            LockInput(false);
            LockEventInput(false);
            LockNativeInput(false);
            GetActiveEventInput().Sleep(false);
        }

        private void UpdateStateChange()
        {
            // RKLog.KeyInfo($"====InputModuleManager==== Current Active Status: {activeModuleStatus}");
            stateChanged = false;
            for (int i = 0; i < moduleActives.Count; i++)
            {
                IInputModuleActive active = moduleActives[i];

                bool enabled = false;

                switch (activeModuleStatus.moduleType)
                {
                    case InputModuleType.Mouse:
                        if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.Mouse) && !activeModuleStatus.mouseSleep)
                            enabled = HasInputModuleType(active.ActiveModuleType, ActiveModuleType.Mouse);
                        break;
                    case InputModuleType.ThreeDof:
                        if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.ThreeDof) && !activeModuleStatus.threeDofSleep)
                            enabled = HasInputModuleType(active.ActiveModuleType, ActiveModuleType.ThreeDof);
                        break;
                    case InputModuleType.Gesture:
                        if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.Gesture))
                        {
                            if (!activeModuleStatus.leftHandSleep && JudgeHandLost(active.DisableOnHandLost, activeModuleStatus.leftHandStatus.active) && HasHandType(active.ActiveHandType, ActiveHandType.LeftHand) && HasHandInteractorType(active.ActiveHandInteractorType, activeModuleStatus.leftHandStatus.handInteractorType) && HasHandOrientationType(active.ActiveHandOrientationType, activeModuleStatus.leftHandStatus.handOrientationType) &&
                            HasWatchType(active.ActiveWatchType, activeModuleStatus.leftHandStatus.activeWatchType) && HasHeadHandType(active.ActiveHeadHandType, activeModuleStatus.headHandType) && HasHandRayType(active.ActiveHandRayType, activeModuleStatus.handRayType))
                            {
                                enabled = true;
                            }
                            if (!activeModuleStatus.rightHandSleep && JudgeHandLost(active.DisableOnHandLost, activeModuleStatus.rightHandStatus.active) && HasHandType(active.ActiveHandType, ActiveHandType.RightHand) && HasHandInteractorType(active.ActiveHandInteractorType, activeModuleStatus.rightHandStatus.handInteractorType) && HasHandOrientationType(active.ActiveHandOrientationType, activeModuleStatus.rightHandStatus.handOrientationType) &&
                            HasWatchType(active.ActiveWatchType, activeModuleStatus.rightHandStatus.activeWatchType) && HasHeadHandType(active.ActiveHeadHandType, activeModuleStatus.headHandType) && HasHandRayType(active.ActiveHandRayType, activeModuleStatus.handRayType))
                            {
                                enabled = true;
                            }
                        }
                        break;
                    case InputModuleType.TouchPad:
                        if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.TouchPad) && !activeModuleStatus.touchSleep)
                            enabled = HasInputModuleType(active.ActiveModuleType, ActiveModuleType.TouchPad);
                        break;
                    default:
                        enabled = false;
                        break;
                }
                SetActiveEnable(active, enabled);
            }
        }

        private void Update()
        {
            if (stateChanged)
            {
                UpdateStateChange();
            }
            if (logText != null)
            {
                logText.text = activeModuleStatus.ToString() + $"\r\n\r\nDragThreshold: {EventSystem.current.pixelDragThreshold}";
            }
            UpdatePlayTime();
            if (globalEventLog != null)
                globalEventLog.gameObject.SetActive(enableGlobalEventLog);
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.I))
            {
                this.enableGlobalEventLog = !this.enableGlobalEventLog;
                UnityEngine.PlayerPrefs.SetString("rokid.globaldebug.show", this.enableGlobalEventLog ? "1" : "0");
            }
#endif
        }

        #region  Judge Has Type
        private bool HasInputModuleType(ActiveModuleType inType, ActiveModuleType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHandType(ActiveHandType inType, ActiveHandType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHandOrientationType(ActiveHandOrientationType inType, ActiveHandOrientationType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHandInteractorType(ActiveHandInteractorType inType, ActiveHandInteractorType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasWatchType(ActiveWatchType inType, ActiveWatchType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHeadHandType(ActiveHeadHandType inType, ActiveHeadHandType targetType)
        {
            return (inType & targetType) == targetType;
        }


        private bool HasHandRayType(ActiveHandRayType inType, ActiveHandRayType targetType)
        {
            return (inType & targetType) == targetType;
        }


        private bool JudgeHandLost(bool disableOnHandLost, bool handLost)
        {
            if (disableOnHandLost == false)
            {
                return true;
            }
            else
            {
                return handLost;
            }
        }

        #endregion

        #region  Convert
        private ActiveHandInteractorType ConvertType(InteractorType interactorType)
        {
            switch (interactorType)
            {
                case InteractorType.Far:
                    return ActiveHandInteractorType.Far;
                case InteractorType.Near:
                    return ActiveHandInteractorType.Near;
            }
            return default(ActiveHandInteractorType);
        }

        private ActiveHandOrientationType ConvertType(HandOrientation handOrientation)
        {
            switch (handOrientation)
            {
                case HandOrientation.Back:
                    return ActiveHandOrientationType.Back;
                case HandOrientation.Palm:
                    return ActiveHandOrientationType.Palm;
            }
            return default(ActiveHandOrientationType);
        }
        #endregion


        #region PlayAudio
        private int moduleChangeAudio = -1;
        private bool canPlayVideo = false;
        private float elapsedTime = 0;

        private void UpdatePlayTime()
        {
            if (canPlayVideo == false)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > 0.3f)
                {
                    canPlayVideo = true;
                    elapsedTime = 0;
                }
            }
        }
        public void InitModuleChangeAudio()
        {
            try
            {
                NativeInterface.NativeAPI.makeSoundPool();
                moduleChangeAudio = NativeInterface.NativeAPI.loadSound("ModuleChangeAudio.wav");
            }
            catch (System.Exception e)
            {
                RKLog.Error("====InputModuleManager==== InitModuleChangeAudio:" + e.ToString());
            }
        }

        public void PlayModuleChangeAudio()
        {
            if (!muteModuleActiveSound && moduleChangeAudio != -1 && canPlayVideo)
            {
                canPlayVideo = false;
                NativeInterface.NativeAPI.playSound(moduleChangeAudio);
            }
        }
        #endregion

    }
}

