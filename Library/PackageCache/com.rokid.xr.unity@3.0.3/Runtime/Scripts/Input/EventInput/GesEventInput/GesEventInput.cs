using System.Text;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using Rokid.UXR.Native;

namespace Rokid.UXR.Interaction
{

    #region  Data

    /// <summary>
    ///  Types of Gesture Interaction
    /// </summary>
    public enum InteractorType
    {
        None,

        //near-field interaction
        Near,
        //far-field interaction
        Far
    }

    /// <summary>
    /// Hand bone node marker index
    /// </summary>
    public enum SkeletonIndexFlag
    {
        WRIST = 0,

        THUMB_CMC = 1,
        THUMB_MCP = 2,
        THUMB_IP = 3,
        THUMB_TIP = 4,

        INDEX_FINGER_MCP = 5,
        INDEX_FINGER_PIP = 6,
        INDEX_FINGER_DIP = 7,
        INDEX_FINGER_TIP = 8,

        MIDDLE_FINGER_MCP = 9,
        MIDDLE_FINGER_PIP = 10,
        MIDDLE_FINGER_DIP = 11,
        MIDDLE_FINGER_TIP = 12,

        RING_FINGER_MCP = 13,
        RING_FINGER_PIP = 14,
        RING_FINGER_DIP = 15,
        RING_FINGER_TIP = 16,

        PINKY_MCP = 17,
        PINKY_PIP = 18,
        PINKY_DIP = 19,
        PINKY_TIP = 20,

        PALM = 21,
        METACARPAL_INDEX = 22,
        METACARPAL_MIDDLE = 23,
        METACARPAL_RING = 24,
        METACARPAL_PINKY = 25
    }

    /// <summary>
    /// Gesture data class
    /// </summary>
    [Serializable]
    public class Gesture
    {
        /// <summary>
        /// Types of hands
        /// </summary>
        public HandType handType;
        /// <summary>
        /// Types of gestures
        /// </summary>
        public GestureType gesType;
        /// <summary>
        /// Pressed in a certain frame
        /// </summary>
        public bool handDown;
        /// <summary>
        /// Released in a certain frame
        /// </summary>
        public bool handUp;
        /// <summary>
        /// Hand is Click
        /// </summary>
        public bool handClick;
        /// <summary>
        /// Hand is Pressing
        /// </summary>
        public bool handPress;
        /// <summary>
        ///  Hand is tracking success
        /// </summary>
        public bool trackingSuccess;
        /// <summary>
        /// Hand Center Position
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// Hand delta position
        /// </summary>
        public Vector3 deltaPos;
        /// <summary>
        /// Hand orientation
        /// </summary>
        public HandOrientation handOrientation;
        /// <summary>
        /// Pinch distance
        /// </summary>
        public float pinchDistance;
        public void Reset()
        {
            gesType = GestureType.None;
            handDown = false;
            handUp = false;
            handClick = false;
            handPress = false;
        }
        public Gesture(HandType type)
        {
            this.handType = type;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HandType:").Append(handType.ToString())
            .Append(" GesType:").Append(gesType.ToString())
            .Append(" HandDown:").Append(handDown)
            .Append(" HandUp:").Append(handUp)
            .Append(" HandClick:").Append(handClick)
            .Append(" HandPress:").Append(handPress)
            .Append(" HandOrientation:").Append(handOrientation)
            .Append(" Position:").Append(position)
            .Append(" deltaPos:").Append(deltaPos);
            return builder.ToString();
        }
    }


    /// <summary>
    /// Types of gestures
    /// </summary>
    public enum GestureType
    {
        None = -1,
        Grip = 1,
        Palm = 2,
        Pinch = 3,
        OpenPinch = 4,
    }

    /// <summary>
    /// Types of hands
    /// </summary>
    public enum HandType
    {
        None = 0,

        LeftHand = 2,

        RightHand = 1
    }

    /// <summary>
    /// Types of handorientation
    /// </summary>
    public enum HandOrientation
    {
        None = -1,
        Palm = 0,
        Back = 1
    }

    /// <summary>
    /// NormalHand / HeadHand
    /// </summary>
    public enum HandOrHeadHandType
    {
        NormalHand,
        HeadHand
    }
    #endregion

    /// <summary>
    /// Gesture event input, provide all gesture external interface
    /// </summary>

    public class GesEventInput : MonoSingleton<GesEventInput>, IEventInput
    {
        #region  Event
        /// <summary>
        /// Hand click callback
        /// </summary>
        /// <typeparam  HandType >Hand type</typeparam>
        public static Action<HandType> OnGesClick;
        /// <summary>
        /// hand press callback
        /// </summary>
        /// <typeparam  HandType >Hand type</typeparam>
        public static Action<HandType> OnHandPress;
        /// <summary>
        /// Hand release callback
        /// </summary>
        /// <typeparam  HandType >Hand type</typeparam>
        public static Action<HandType> OnHandRelease;
        /// <summary>
        /// Gesture tracked failed callback
        /// </summary>
        /// <typeparam  HandType >handtype=leftHand lefthand lost, handtype=rightHand righthand lost</typeparam>
        public static Action<HandType> OnTrackedFailed;
        /// <summary>
        /// Hand track success callback update invoke
        /// </summary> 
        /// <typeparam  HandType >hand type</typeparam>
        public static Action<HandType> OnTrackedSuccess;
        /// <summary>
        ///  Process Gesture Data 
        /// </summary>
        /// <typeparam  GestureResults >The result of the gesture data returned </typeparam>
        public static Action<HandType, GestureBean> OnProcessGesData;
        /// <summary>
        /// Use log ges fps
        /// </summary>
        /// <typeparam  Vector3 >wrist pos</typeparam>
        public static Action<float> OnGesDataUpdate;
        /// <summary>
        /// Use to process ges log
        /// </summary>
        /// <typeparam  float >The time difference between the upper and lower frames of the gesture</typeparam>
        public static Action<string> OnLogGesData;

        /// <summary>
        /// Use to process ray pose
        /// </summary>
        /// <typeparam  HandType >Hand type lefthand/righthand</typeparam>
        /// <typeparam  Vector3 >Wrist pos</typeparam>
        /// <typeparam  Vector3 >HandCenter pos</typeparam>
        /// <typeparam  Vector3 >PinchCenter base pos</typeparam>
        /// <typeparam  Vector3 >PinchCenter  pos</typeparam>
        internal static Action<HandType, Vector3, Vector3, Vector3, Vector3> OnRayPoseUpdate;
        /// <summary>
        /// Use to render hand update invoke
        /// </summary>
        /// <typeparam  HandType >hand type</typeparam>
        public static Action<HandType, GestureBean> OnRenderHand;
        /// <summary>
        /// On hand lost in camera space  callback
        /// </summary>
        /// <typeparam  HandType >hand type</typeparam>
        public static Action<HandType> OnHandLostInCameraSpace;

        /// <summary>
        /// On hand orientation change callback update invoke
        /// </summary>
        public static Action<HandType, HandOrientation> OnHandOrientationUpdate;
        /// <summary>
        /// Gesture module action callback
        /// </summary>
        public static Action OnActiveGesModule;
        /// <summary>
        /// Gesture module release callback 
        /// </summary>
        public static Action OnReleaseGesModule;
        /// <summary>
        /// Gesture module init callback
        /// </summary>
        public static Action OnInitializeGesModule;

        /// <summary>
        /// Hand or headhand type change callback
        /// </summary>
        public static Action<HandOrHeadHandType> OnHandOrHeadHandTypeChange;

        /// <summary>
        /// Hand is sleep
        /// </summary>
        public static Action<HandType, bool> OnHandSleep;
        /// <summary>
        ///  When Hand Active callback
        /// </summary>
        public static Action<HandType> OnHandRayActive;

        #endregion

        [SerializeField]
        private GesImplementation gesInput;

        /// <summary>
        /// Interactor
        /// </summary>
        /// <value></value>
        public Transform Interactor { get; set; }

        /// <summary>
        /// Auto change headhand or handtype
        /// </summary>
        /// <value></value>
        public bool autoChangeHeadHandOrHandType = false;

        private bool initialize = false;

        private HandType holdHandRayType = HandType.None;

        private bool lockInput;


        private GesImplementation GesInput
        {
            get
            {
                if (gesInput == null)
                {
                    if (gameObject.GetComponent<GesImplementation>() != null)
                    {
                        gesInput = gameObject.GetComponent<GesImplementation>();
                    }
                    else
                    {
                        gesInput = gameObject.AddComponent<GesImplementation>();
                    }
                }
                return gesInput;
            }
        }

        private int pixelDragThreshold = 60;
        public int PixelDragThreshold { get { return pixelDragThreshold; } set { pixelDragThreshold = value; } }

        public InputModuleType inputModuleType => InputModuleType.Gesture;

        [SerializeField]
        private BaseRayCaster leftHandRaycaster;
        [SerializeField]
        private BaseRayCaster rightHandRaycaster;
        public BaseRayCaster GetRayCaster(HandType hand = HandType.None)
        {
            switch (hand)
            {
                case HandType.LeftHand:
                    if (leftHandRaycaster == null)
                    {
                        leftHandRaycaster = Interactor?.Find("LeftHandInteractors/RayInteractor").GetComponent<BaseRayCaster>();
                    }
                    return leftHandRaycaster;
                case HandType.RightHand:
                    if (rightHandRaycaster == null)
                    {
                        rightHandRaycaster = Interactor?.Find("RightHandInteractors/RayInteractor").GetComponent<BaseRayCaster>();
                    }
                    return rightHandRaycaster;
            }
            return null;
        }
        private ISelector leftHandSelector;
        private ISelector rightHandSelector;
        public ISelector GetRaySelector(HandType hand = HandType.None)
        {
            switch (hand)
            {
                case HandType.LeftHand:
                    if (leftHandSelector == null)
                    {
                        leftHandSelector = GetRayCaster(hand)?.GetComponentInChildren<ISelector>();
                    }
                    return leftHandSelector;
                case HandType.RightHand:
                    if (rightHandSelector == null)
                    {
                        rightHandSelector = GetRayCaster(hand)?.GetComponentInChildren<ISelector>();
                    }
                    return rightHandSelector;
            }
            return null;
        }
        private IRayPose leftHandRayPose;
        private IRayPose rightHandRayPose;
        public IRayPose GetRayPose(HandType hand = HandType.None)
        {
            switch (hand)
            {
                case HandType.LeftHand:
                    if (leftHandRayPose == null)
                    {
                        leftHandRayPose = GetRayCaster(hand)?.GetComponent<IRayPose>();
                    }
                    return leftHandRayPose;
                case HandType.RightHand:
                    if (rightHandRayPose == null)
                    {
                        rightHandRayPose = GetRayCaster(hand)?.GetComponent<IRayPose>();
                    }
                    return rightHandRayPose;
            }
            return null;
        }
        protected override void OnSingletonInit()
        {
            base.OnSingletonInit();
        }


        /// <summary>
        /// Gesture Module Initialize
        /// </summary>
        /// <param name="parent">Generates the parent of the interactor</param>
        public void Initialize(Transform parent)
        {
            if (Utils.IsAndroidPlatform() && !FuncDeviceCheck.CheckHandTrackingFunc())
            {
                return;
            }
#if UNITY_EDITOR
            if (GetComponent<GestureMockInEditor>() == null)
                gameObject.AddComponent<GestureMockInEditor>();
#endif
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("RKHand");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/RKHand"));
                }
                go.name = "RKHand";
                Interactor = go.transform.GetComponentInChildren<ModuleInteractor>().transform;
                Interactor.SetParent(transform);
            }
            Interactor.SetParent(transform);
            if (parent != null)
                this.transform.SetParent(parent);
            OnInitializeGesModule?.Invoke();
            ActiveHandOrHeadHand(HandOrHeadHandType.NormalHand);
            GesInput.Initialize();
            initialize = true;

            GesEventInput.OnTrackedFailed += OnGesTrackedFailed;
            GesEventInput.OnTrackedSuccess += OnGesTrackedSuccess;

            string handScale = NativeInterface.NativeAPI.GetPersistValue("rokid.hand.scale");
            if (!string.IsNullOrEmpty(handScale))
            {
                SetHandScale(Convert.ToSingle(handScale));
            }
            RKLog.KeyInfo("====GesEventInit==== Init");
        }

        private void OnGesTrackedSuccess(HandType type)
        {
            if (holdHandRayType == HandType.None)
            {
                holdHandRayType = type;
                if (InputModuleManager.Instance.GetGesActive())
                    OnHandRayActive?.Invoke(type);
            }
        }

        private void OnGesTrackedFailed(HandType type)
        {
            if (holdHandRayType == type)
            {
                holdHandRayType = HandType.None;
            }
        }

        /// <summary>
        /// Activate hand interaction or head-hand interaction
        /// </summary>
        /// <param name="handOrHeadHandType"></param>
        public void ActiveHandOrHeadHand(HandOrHeadHandType handOrHeadHandType)
        {
            OnHandOrHeadHandTypeChange?.Invoke(handOrHeadHandType);
        }

        /// <summary>
        /// Release Gesture
        /// </summary>
        public void Release()
        {
            OnReleaseGesModule?.Invoke();
            Destroy(this.gameObject);
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            OnGesClick = null;
            OnTrackedFailed = null;
            OnHandPress = null;
            OnHandRelease = null;
            OnTrackedSuccess = null;
            OnProcessGesData = null;
            OnGesDataUpdate = null;
            OnLogGesData = null;
            OnRenderHand = null;
            initialize = false;
        }

        /// <summary>
        /// Active Gesture
        /// </summary>
        public void ActiveModule()
        {
            RKLog.KeyInfo("====GesEventInput==== : ActiveModule");
            OnActiveGesModule?.Invoke();
            RKVirtualController.Instance.UseCustomGamePadEvent(false);
            EventSystem.current.pixelDragThreshold = PixelDragThreshold;
        }

        /// <summary>
        /// Get Hand Pose 
        /// </summary>
        /// <param name="hand">handtype lefthand righthand</param>
        /// <returns></returns>
        public Vector3 GetHandDeltaPos(HandType hand)
        {
            if (!initialize) { return Vector3.zero; };
            return GesInput.GetHandDeltaPos(hand);
        }


        /// <summary>
        /// GetHandDown
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <param name="isPinch">true pinch / false  grip</param>
        /// <returns></returns>
        public bool GetHandDown(HandType type, bool isPinch)
        {
            if (!initialize) { return false; };
            return GesInput.GetHandDown(type, isPinch);
        }

        /// <summary>
        /// Get Hand Up
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <param name="isPinch">true pinch / false grip</param>
        /// <returns>When hand up return true or return false </returns>
        public bool GetHandUp(HandType type, bool isPinch)
        {
            if (!initialize) { return false; };
            return GesInput.GetHandUp(type, isPinch);
        }

        /// <summary>
        /// Get Hand Press
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <param name="isPinch">true pinch / false grip</param>
        /// <returns>When hand press return true or return false </returns>
        public bool GetHandPress(HandType type, bool isPinch)
        {
            if (!initialize) { return false; };
            return GesInput.GetHandPress(type, isPinch);
        }

        /// <summary>
        /// Get hand down
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public bool GetHandDown(HandType type)
        {
            if (!initialize) { return false; };
            return GesInput.GetHandDown(type);
        }


        /// <summary>
        /// Get hand press
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public bool GetHandPress(HandType type)
        {
            if (!initialize) { return false; };
            return GesInput.GetHandPress(type);
        }



        /// <summary>
        /// Get hand up
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public bool GetHandUp(HandType type)
        {
            if (!initialize) { return false; };
            return GesInput.GetHandUp(type);
        }

        /// <summary>
        /// Are custom palm and back of hand detection criteria used
        /// </summary>
        /// <param name="active"></param>
        public void SetUseCustomCalHandOrientation(bool active)
        {
            if (!initialize) { return; }
            GesInput.SetUseCustomCalHandOrientation(active);
        }

        /// <summary>
        /// Get hand click
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public bool GetHandClick(HandType type)
        {
            if (!initialize) { return false; };
            return GesInput.GetHandClick(type);
        }

        /// <summary>
        ///  Get current gesture info 
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public Gesture GetGesture(HandType type)
        {
            if (!initialize) { return default(Gesture); };
            return GesInput.GetGesture(type);
        }

        /// <summary>
        /// Get current gesture type
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public GestureType GetGestureType(HandType type)
        {
            if (!initialize) { return GestureType.None; };
            return GesInput.GetGestureType(type);
        }

        /// <summary>
        /// Get current hand center position
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public Vector3 GetHandPos(HandType handType)
        {
            if (!initialize) { return Vector3.zero; };
            return GesInput.GetHandPos(handType);
        }

        /// <summary>
        /// Get current hand pose
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns>hand pose </returns>
        public Pose GetHandPose(HandType handType)
        {
            if (!initialize) { return Pose.identity; };
            return GesInput.GetHandPose(handType);
        }


        /// <summary>
        /// Get Skeleton Pose
        /// </summary>
        /// <param name="flag">Skeleton index flag</param>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public Pose GetSkeletonPose(SkeletonIndexFlag flag, HandType type)
        {
            if (!initialize) { return Pose.identity; };
            return GesInput.GetSkeletonPose(flag, type);
        }

        /// <summary>
        /// Get GetAlgorithmSkeletonPose
        /// </summary>
        /// <param name="flag">Skeleton index flag</param>
        /// <param name="type">left/right hand</param>
        /// <returns></returns>
        public Pose GetAlgorithmSkeletonPose(SkeletonIndexFlag flag, HandType type)
        {
            if (!initialize) { return Pose.identity; };
            return GesInput.GetAlgorithmSkeletonPose(flag, type);
        }
        /// <summary>
        /// Get hand interactor type
        /// </summary>
        /// <param name="type">left/right hand</param>
        /// <returns>InteractorType Near/Far </returns>
        public InteractorType GetInteractorType(HandType hand)
        {
            if (!initialize) { return InteractorType.None; };
            return GesInput.GetInteractorType(hand);
        }

        /// <summary>
        /// Set interactor type
        /// </summary>
        /// <param name="type">InteractorType Near/Far</param>
        /// <param name="hand">left/right hand</param>
        public void SetInteractorType(InteractorType type, HandType hand)
        {
            if (!initialize) { return; };
            GesInput.SetInteractorType(type, hand);
        }

        /// <summary>
        /// Get hand orientation
        /// </summary>
        /// <param name="hand">left/right hand</param>
        /// <returns></returns>
        public HandOrientation GetHandOrientation(HandType hand)
        {
            if (!initialize) { return HandOrientation.None; };
            return GesInput.GetHandOrientation(hand);
        }

        /// <summary>
        /// Get pinch distance
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public float GetPinchDistance(HandType hand)
        {
            if (!initialize) return 0;
            return GesInput.GetPinchDistance(hand);
        }

        /// <summary>
        /// Get hand scale 
        /// </summary>
        /// <returns></returns>
        public float GetHandScale()
        {
            if (!initialize) return 1;
            return GesInput.GetHandScale();
        }

        /// <summary>
        /// Set hand scale 
        /// </summary>
        public void SetHandScale(float handScale)
        {
            if (initialize)
            {
                GesInput.SetHandScale(handScale);
                Native.NativeInterface.NativeAPI.SetPersistValue("rokid.hand.scale", handScale.ToString());
            }
        }

        public void SetIndexFingerToStraight(HandType hand, bool active)
        {
            if (initialize)
                GesInput.SetIndexFingerToStraight(active, hand);
        }


        /// <summary>
        /// Unity Update
        /// </summary>
        private void Update()
        {
            if (!initialize || lockInput)
                return;
            if (!Utils.IsAndroidPlatform())
            {
                if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.F))
                {
                    this.ActiveModule();
                }
                if (Input.GetKeyDown(KeyCode.O))
                {
                    ActiveHandOrHeadHand(HandOrHeadHandType.HeadHand);
                }
                if (Input.GetKeyDown(KeyCode.I))
                {
                    ActiveHandOrHeadHand(HandOrHeadHandType.NormalHand);
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    OnHandRayActive?.Invoke(HandType.LeftHand);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    OnHandRayActive?.Invoke(HandType.RightHand);
                }
            }
            else
            {
                if (!InputModuleManager.Instance.GetGesActive())
                {
                    if (CanActiveHand(HandType.LeftHand))
                    {
                        ActiveModule();
                        if (autoChangeHeadHandOrHandType)
                            ActiveHandOrHeadHand(HandOrHeadHandType.HeadHand);
                    }
                    if (CanActiveHand(HandType.RightHand))
                    {
                        ActiveModule();
                        if (autoChangeHeadHandOrHandType)
                            ActiveHandOrHeadHand(HandOrHeadHandType.NormalHand);
                    }
                }
                else
                {
                    if (CanActiveHand(HandType.LeftHand))
                    {
                        OnHandRayActive?.Invoke(HandType.LeftHand);
                    }

                    if (CanActiveHand(HandType.RightHand))
                    {
                        OnHandRayActive?.Invoke(HandType.RightHand);
                    }
                    JudgeHandSleep(HandType.LeftHand);
                    JudgeHandSleep(HandType.RightHand);
                }
            }
        }

        #region  JudgeHandSleep
        private Vector3 oriLeftHandWristPos = Vector3.zero;
        private Vector3 oriRightHandWristPos = Vector3.zero;
        private float sleepWristPosThreshold = 0.02f;
        private float sleepTimeThreshold = 10.0f;
        private float leftHandSleepElapsedTime = 0, rightHandSleepElapsedTime = 0;
        private bool activeSleepJudge = false, leftHandSleep = false, rightHandSleep = false;

        public void SetActiveHand(HandType hand)
        {
            if (hand == HandType.LeftHand)
            {
                leftHandSleep = false;
                leftHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.LeftHand, false);

                rightHandSleep = true;
                rightHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.RightHand, true);
            }

            if (hand == HandType.RightHand)
            {
                leftHandSleep = true;
                leftHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.LeftHand, true);

                rightHandSleep = false;
                rightHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.RightHand, false);
            }
        }

        public HandType GetActiveHand()
        {
            if (leftHandSleep)
            {
                return HandType.RightHand;
            }
            else
            {
                return HandType.LeftHand;
            }
        }

        public void SetActiveSleepJudge(bool active)
        {
            this.activeSleepJudge = active;
            if (active == false)
            {
                leftHandSleep = false;
                leftHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.LeftHand, false);

                rightHandSleep = false;
                rightHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.RightHand, false);
            }
        }


        private void JudgeHandSleep(HandType handType)
        {
            if (handType == HandType.LeftHand && CanActiveHand(handType))
            {
                leftHandSleep = false;
                leftHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.LeftHand, false);
            }

            if (handType == HandType.RightHand && CanActiveHand(handType))
            {
                rightHandSleep = false;
                rightHandSleepElapsedTime = 0;
                OnHandSleep?.Invoke(HandType.RightHand, false);
            }

            if (activeSleepJudge)
            {
                Vector3 curWritsPos = GetSkeletonPose(SkeletonIndexFlag.WRIST, handType).position;

                if (handType == HandType.LeftHand && !leftHandSleep)
                {
                    float sqrDis = Vector3.SqrMagnitude(curWritsPos - oriLeftHandWristPos);
                    oriLeftHandWristPos = curWritsPos;
                    if (sqrDis < sleepWristPosThreshold * sleepWristPosThreshold && !InCameraTargetView(handType, 20) && !GetHandClick(handType))
                    {
                        leftHandSleepElapsedTime += Time.deltaTime;
                        if (leftHandSleepElapsedTime > sleepTimeThreshold)
                        {
                            leftHandSleep = true;
                            OnHandSleep?.Invoke(handType, true);
                        }
                    }
                    else
                    {
                        leftHandSleepElapsedTime = 0;
                    }
                }

                if (handType == HandType.RightHand && !rightHandSleep)
                {
                    float sqrDis = Vector3.SqrMagnitude(curWritsPos - oriRightHandWristPos);
                    oriRightHandWristPos = curWritsPos;
                    if (sqrDis < sleepWristPosThreshold * sleepWristPosThreshold && !InCameraTargetView(handType, 20) && !GetHandClick(handType))
                    {
                        rightHandSleepElapsedTime += Time.deltaTime;
                        if (rightHandSleepElapsedTime > sleepTimeThreshold)
                        {
                            rightHandSleep = true;
                            OnHandSleep?.Invoke(handType, true);
                        }
                    }
                    else
                    {
                        rightHandSleepElapsedTime = 0;
                    }
                }
            }
        }
        #endregion

        #region InputTypeChange

        public bool CanActiveHand(HandType handType)
        {
            if (GetHandOrientation(handType) == HandOrientation.Palm && GetGestureType(handType) == GestureType.Palm && gesInput.HasGesTypeAndHandOrientation(GestureType.Grip, HandOrientation.Palm, handType, 20, 5) && InCameraTargetView(handType, 40))
            {
                return true;
            }
            return false;
        }

        private bool InCameraTargetView(HandType hand, int halfFov)
        {
            Vector3 handPos = GesEventInput.Instance.GetHandPose(hand).position;
            Vector3 cameraSpaceHandPos = MainCameraCache.mainCamera.transform.InverseTransformPoint(handPos);
            float hFov = Vector3.Angle(Vector3.forward, new Vector3(cameraSpaceHandPos.x, 0, cameraSpaceHandPos.z));
            float vFov = Vector3.Angle(Vector3.forward, new Vector3(0, cameraSpaceHandPos.y, cameraSpaceHandPos.z));
            if (hFov < halfFov && vFov < halfFov)
            {
                return true;
            }
            return false;
        }

        public void Sleep(bool sleep)
        {
            OnHandSleep?.Invoke(HandType.LeftHand, sleep);
            OnHandSleep?.Invoke(HandType.RightHand, sleep);
        }

        public void Lock(bool isLock)
        {
            this.lockInput = isLock;
        }

        #endregion
    }
}
