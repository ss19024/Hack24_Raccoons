using System.Threading;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using System.Collections.Generic;
using Rokid.UXR.Native;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    ///  Gesture logic implementation class,
    ///  the interfaces provided by this class are not recommended
    /// </summary>
    internal class GesImplementation : MonoBehaviour
    {
        /// <summary>
        /// Left-hand cached data structure
        /// </summary>
        [SerializeField]
        private Gesture leftGesture;
        /// <summary>
        /// Right-hand cached data structure
        /// </summary>
        [SerializeField]
        private Gesture rightGesture;

        [SerializeField]
        private GestureType preLeftGesType = GestureType.None;
        [SerializeField]
        private GestureType preRightGesType = GestureType.None;

        /// <summary>
        /// Gesture type cache
        /// </summary>
        [SerializeField]
        protected GestureType[] leftGestureTypeCache;

        /// <summary>
        /// Gesture type cache
        /// </summary>
        [SerializeField]
        protected GestureType[] rightGestureTypeCache;


        /// <summary>
        /// HandOrientation cache
        /// </summary>
        [SerializeField]
        protected HandOrientation[] leftHandOrientationCache;

        /// <summary>
        /// HandOrientation cache
        /// </summary>
        [SerializeField]
        protected HandOrientation[] rightHandOrientationCache;


        /// <summary>
        /// Cache the number of frames for gestures
        /// </summary>
        [SerializeField]
        protected int gesCacheCount = 30;
        /// <summary>
        /// Determine the threshold for changing the gesture
        /// </summary>
        [SerializeField]
        protected int changeGesThreshold = 10;
        /// <summary>
        /// Whether to enable gesture type recording
        /// </summary>
        [SerializeField]
        protected bool useGesFrameRecord = true;
        /// <summary>
        /// Internal log text
        /// </summary>
        [SerializeField]
        private Text logText;
        /// <summary>
        /// Whether to display log
        /// </summary>
        [SerializeField]
        private bool showDebugLog;

        [Tooltip("Left-handed interaction type, default Far")]
        [SerializeField]
        private InteractorType rightHandInteractorType = InteractorType.Far;

        [Tooltip("Right-handed interaction type, default Far")]
        [SerializeField]
        private InteractorType leftHandInteractorType = InteractorType.Far;
        /// <summary>s
        /// Gesture data array
        /// </summary>
        [SerializeField]
        private GestureBean[] gestureData = new GestureBean[2] { new GestureBean(), new GestureBean() };

        /// <summary>s
        /// Cache data from the previous frame
        /// </summary>
        private GestureBean preLeftGesData = new GestureBean();
        private GestureBean preRightGesData = new GestureBean();

        /// <summary>
        /// Gesture frame
        /// </summary>
        protected int rightGesFrame = 0;
        /// <summary>
        /// Gesture frame
        /// </summary>
        protected int leftGesFrame = 0;

        private float clickTime = 1.25f;
        private float leftClickTime = 0;
        private float rightClickTime = 0;

        /// <summary>
        /// Initialize gesture module
        /// </summary>
        private bool initGesModule = false;
        /// <summary>
        /// 是否已经初始化缓存
        /// </summary>
        private bool initGesCache = false;

        #region  Algorithm data caching 
        /// <summary>
        /// Left hand joint data
        /// </summary>
        private float[] leftSkeletons;
        /// <summary>
        /// Right hand joint data
        /// </summary>
        private float[] rightSkeletons;
        /// <summary>
        /// Left hand rotation
        /// </summary>
        private float[] leftRootRotation;
        /// <summary>
        /// Right hand rotation
        /// </summary>
        private float[] rightRootRotation;

        /// <summary>
        /// Left skeletons rotation
        /// </summary>
        private float[] leftSkeletonRotation;
        /// <summary>
        /// Right skeletons rotation
        /// </summary>
        private float[] rightSkeletonRotation;
        private Quaternion[] leftSkeletonsRot;
        private Quaternion[] rightSkeletonsRot;
        private Vector3[] leftSkeletonsPos;
        private Vector3[] rightSkeletonsPos;
        /// <summary>
        /// Retrieve the number of tracked hands
        /// </summary>
        private int handNum;
        #endregion

        private Vector3 preLeftHandPos;
        private Vector3 preRightHandPos;
        private Vector3 preHandPos;
        private bool useCustomCalHandOrientation = false;

        /// <summary>
        /// Cache a Vector3 array to reduce garbage collection (GC).
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="Vector3[]"></typeparam>
        /// <returns></returns>
        private Dictionary<string, Vector3[]> vector3Dict = new Dictionary<string, Vector3[]>();

        /// <summary>
        /// Cache a quaternion to reduce garbage collection (GC).
        /// </summary>
        private Quaternion rotation = Quaternion.identity;

        /// <summary>
        /// Cache a vector to reduce garbage collection (GC)
        /// </summary>
        private Vector3 op = Vector3.zero;

        [SerializeField, Tooltip("手被主动过滤后是否弹出提示")]
        private bool showToastOnHandLost = true;
        [SerializeField, Tooltip("手被过滤的阈值")]
        private float handLostThreshold = 0.1f;
        private float lostTimeThreshold = 1.0f;
        private float elapsedTime = 0;
        private long gesPreImageTimeStamp = 0;
        private Pose cameraPose = Pose.identity;
        private Pose cameraParentPose = Pose.identity;

        private bool setLeftIndexFingerToStraight = false;
        private bool setRightIndexFingerToStraight = false;


        private Thread getDataThread;
        /// <summary>
        /// hand scale 
        /// </summary>
        private float handScale = 1.0f;

        internal void Initialize()
        {
            if (Utils.IsAndroidPlatform() && !FuncDeviceCheck.CheckFunc(FuncDeviceCheck.FuncEnum.HandTracking))
            {
                return;
            }
            InitCache();
            InitGesture();
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
        }

        internal void SetUseCustomCalHandOrientation(bool active)
        {
            this.useCustomCalHandOrientation = active;
        }

        private void InitCache()
        {
            //初始化缓存...
            leftGestureTypeCache = new GestureType[gesCacheCount];
            rightGestureTypeCache = new GestureType[gesCacheCount];
            leftHandOrientationCache = new HandOrientation[gesCacheCount];
            rightHandOrientationCache = new HandOrientation[gesCacheCount];
            leftGesture = new Gesture(HandType.LeftHand);
            rightGesture = new Gesture(HandType.RightHand);
            leftSkeletons = new float[26 * 3];
            rightSkeletons = new float[26 * 3];
            leftSkeletonRotation = new float[26 * 9];
            rightSkeletonRotation = new float[26 * 9];
            leftSkeletonsPos = new Vector3[26];
            rightSkeletonsPos = new Vector3[26];
            leftSkeletonsRot = new Quaternion[26];
            rightSkeletonsRot = new Quaternion[26];
            initGesCache = true;
        }

        private void OnTrackedFailed(HandType hand)
        {
            if (hand == HandType.LeftHand)
            {
                if (preLeftGesType == GestureType.Grip || preLeftGesType == GestureType.Pinch)
                {
                    // RKLog.KeyInfo($"====GesImplementation==== OnTrackedFailed To Process LeftHand Data:{preLeftGesData.ThreeGesKeyInfo()}");
                    preLeftGesData.gesture_type = (int)GestureType.None;
                    ProcessGesData(preLeftGesData);
                }
            }
            else if (hand == HandType.RightHand)
            {
                if (preRightGesType == GestureType.Grip || preRightGesType == GestureType.Pinch)
                {
                    // RKLog.KeyInfo($"====GesImplementation==== OnTrackedFailed To Process RightHand Data:{preRightGesData.ThreeGesKeyInfo()}");
                    preRightGesData.gesture_type = (int)GestureType.None;
                    ProcessGesData(preRightGesData);
                }
            }
        }

        protected void OnDestroy()
        {
            Release();
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
        }

        /// <summary>
        ///  Initialize  logic 
        /// </summary>
        private void InitGesture()
        {
            if (initGesCache && initGesModule == false && Utils.IsAndroidPlatform())
            {
                initGesModule = true;
                RKLog.KeyInfo("====GesImplementation==== Initialize");
                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    Permission.RequestUserPermission(Permission.Camera);
                }
                NativeInterface.NativeAPI.InitGesture();
                getDataThread = new Thread(() =>
                {
                    while (initGesModule && getDataThread.ThreadState != ThreadState.Aborted && getDataThread.ThreadState != ThreadState.Stopped)
                    {
                        GetData();
                        Thread.Sleep(20);
                    }
                });
                getDataThread.Start();
            }
        }

        private void Release()
        {
            if (initGesModule == true)
            {
                initGesModule = false;
                if (getDataThread.ThreadState != ThreadState.Aborted && getDataThread.ThreadState != ThreadState.Stopped)
                    getDataThread.Abort();
                NativeInterface.NativeAPI.ReleaseGesture();
                // RKLog.KeyInfo("====GesImplementation==== Release");
                // 释放事件
                if (leftGesture.trackingSuccess)
                {
                    // RKLog.KeyInfo($"====GesImplementation====: tracking failed {HandType.LeftHand}");
                    leftGesture.trackingSuccess = false;
                    GesEventInput.OnTrackedFailed?.Invoke(HandType.LeftHand);
                }
                if (rightGesture.trackingSuccess)
                {
                    // RKLog.KeyInfo($"====GesImplementation====: tracking failed {HandType.RightHand}");
                    rightGesture.trackingSuccess = false;
                    GesEventInput.OnTrackedFailed?.Invoke(HandType.RightHand);
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Release();
            }
            else
            {
                InitGesture();
            }
        }

        #region LogHandTrackCount
        private float logTime = 5;
        private float logElapsedTime = 0;
        private void LogHandKeyInfo()
        {
            logElapsedTime += Time.deltaTime;
            if (logElapsedTime > logTime)
            {
                logElapsedTime = 0;
                RKLog.KeyInfo($"====GesImplementation==== TrackingHandNum: {NativeInterface.NativeAPI.GetTrackingHandNum()}  TimeStamp:{NativeInterface.NativeAPI.GetCurrentFrameTimeStamp()}");
            }
        }
        #endregion

        internal void Update()
        {
            if (initGesModule)
            {
                UpdateCameraParentPose();
                ProcessData(gestureData, handNum);
            }
        }

        private void UpdateCameraParentPose()
        {
            if (MainCameraCache.mainCamera.transform.parent != null)
            {
                cameraParentPose.position = MainCameraCache.mainCamera.transform.parent.position;
                cameraParentPose.rotation = MainCameraCache.mainCamera.transform.parent.rotation;
            }
        }

        private void GetData()
        {
            long timeStamp = NativeInterface.NativeAPI.GetCurrentFrameTimeStamp();
            if (gesPreImageTimeStamp == timeStamp)
            {
                return;
            }
            gesPreImageTimeStamp = timeStamp;
            handNum = Mathf.Clamp(NativeInterface.NativeAPI.GetTrackingHandNum(), 0, 2);
            if (handNum > 0)
            {
                cameraPose = NativeInterface.NativeAPI.GetHistoryCameraPhysicsPose(timeStamp);
                cameraPose.position = cameraParentPose.rotation * cameraPose.position + cameraParentPose.position;
                cameraPose.rotation = cameraParentPose.rotation * cameraPose.rotation;
                if (float.IsNaN(cameraPose.position.x) || float.IsNaN(cameraPose.rotation.x))
                {
                    RKLog.Error($"====GesImplementation==== History Pose Data Error: {cameraPose.position},{cameraPose.rotation.eulerAngles},{timeStamp} ");
                    return;
                }
                for (int i = 0; i < handNum; i++)
                {
                    //0:左手 1:右手
                    int handType = NativeInterface.NativeAPI.GetTrackingHandLrHand(i);
                    gestureData[i].hand_type = handType == 0 ? 2 : handType;
                    int gesType = NativeInterface.NativeAPI.GetTrackingHandCombineGestureType(i);
                    gestureData[i].gesture_type = gesType == 0 ? -1 : gesType;
                    gestureData[i].pinchDistance = NativeInterface.NativeAPI.GetTrackingHandPinchDistance(i) / 1000;
                    if (gestureData[i].hand_type == 2)
                    {
                        //左手(骨骼数据)
                        NativeInterface.NativeAPI.GetTrackingHandSkeletonCAM(leftSkeletons, i);
                        gestureData[i].skeletons = GetVertices(leftSkeletons, false, cameraPose, handScale);
                        NativeInterface.NativeAPI.GetTrackingHandSkeletonRotationAll(leftSkeletonRotation, i, 0);
                        gestureData[i].skeletonsRot = HandUtils.GetSkeletonsQuaternion(leftSkeletonRotation, false, leftSkeletonsRot, cameraPose);
                        if (setLeftIndexFingerToStraight)
                            HandUtils.AdjustIndexToStraight(gestureData[i].skeletonsRot, gestureData[i].skeletons, HandType.LeftHand);
                    }
                    else
                    {
                        //右手(骨骼数据)
                        NativeInterface.NativeAPI.GetTrackingHandSkeletonCAM(rightSkeletons, i);
                        gestureData[i].skeletons = GetVertices(rightSkeletons, true, cameraPose, handScale);
                        NativeInterface.NativeAPI.GetTrackingHandSkeletonRotationAll(rightSkeletonRotation, i, 0);
                        gestureData[i].skeletonsRot = HandUtils.GetSkeletonsQuaternion(rightSkeletonRotation, true, rightSkeletonsRot, cameraPose);
                        if (setRightIndexFingerToStraight)
                            HandUtils.AdjustIndexToStraight(gestureData[i].skeletonsRot, gestureData[i].skeletons, HandType.RightHand);
                    }
                    gestureData[i].position = gestureData[i].skeletons[21];
                    gestureData[i].rotation = gestureData[i].skeletonsRot[21];
                    gestureData[i].hand_orientation = useCustomCalHandOrientation ? CalHandOrientation(gestureData[i].skeletonsRot[21] * Vector3.forward) : NativeInterface.NativeAPI.GetTrackingHandOrientation(i);
                }
                //将数组内的第二个数据置空
                if (handNum == 1)
                    gestureData[1].hand_type = (int)HandType.None;
                //排除两只手,类型相同的情况
                if (handNum == 2 && gestureData[0].hand_type == gestureData[1].hand_type)
                {
                    handNum = 1;
                    gestureData[1].hand_type = (int)HandType.None;
                }
            }
        }

        /// <summary>
        /// Process data for 3D gestures
        /// </summary>
        /// <param name="beans"></param>
        internal void ProcessData(GestureBean[] beans, int handNum)
        {
            if (showDebugLog && logText != null)
            {
                logText.text = $" handNum: {handNum}";
                logText.text += "\n" + $" HistoryPhysicalCameraPose: {cameraPose.position},{cameraPose.rotation.eulerAngles}";
                Pose data = NativeInterface.NativeAPI.GetCameraPhysicsPose(out long ts);
                logText.text += "\n" + $" CurrentPhysicalCameraPose: {data.position},{data.rotation.eulerAngles}";
                logText.text += "\n" + $" MainCameraPose: {MainCameraCache.mainCamera.transform.position},{MainCameraCache.mainCamera.transform.rotation.eulerAngles}";
            }

            int startIndex = 0;
#if !UNITY_EDITOR
            //增加手势过滤的阈值
            for (int i = 0; i < handNum; i++)
            {
                Vector3 handPosInCamera = MainCameraCache.mainCamera.transform.InverseTransformPoint(beans[i].position);
                if (handPosInCamera.z < handLostThreshold)
                {
                    // RKLog.Info($"====GesImplementation==== 过滤手势,手距离相机过近:{handPosInCamera},{handNum},{beans[i].ThreeGesKeyInfo()}");
                    if (showToastOnHandLost)
                    {
                        elapsedTime += Time.deltaTime;
                        if (elapsedTime > lostTimeThreshold)
                        {
                            HandType hand = (HandType)beans[i].hand_type;
                            GesEventInput.OnHandLostInCameraSpace?.Invoke(hand);
                        }
                    }
                    startIndex++;
                }
                else
                {
                    elapsedTime = 0;
                }
            }
#endif
            if (beans != null && handNum - startIndex > 0)
            {
                if (handNum - startIndex == 1)
                {
                    HandType handType = (HandType)beans[0].hand_type;
                    if (handType == HandType.RightHand)
                    {
                        if (leftGesture.trackingSuccess)
                        {
                            // RKLog.KeyInfo($"====GesImplementation====: tracking failed {HandType.LeftHand}");
                            leftGesture.trackingSuccess = false;
                            GesEventInput.OnTrackedFailed?.Invoke(HandType.LeftHand);
                        }
                    }
                    else if (handType == HandType.LeftHand)
                    {
                        if (rightGesture.trackingSuccess)
                        {
                            // RKLog.KeyInfo($"====GesImplementation====: tracking failed {HandType.RightHand}");
                            rightGesture.trackingSuccess = false;
                            GesEventInput.OnTrackedFailed?.Invoke(HandType.RightHand);
                        }
                    }
                }
                for (int i = startIndex; i < handNum; i++)
                {
                    HandType handType = (HandType)beans[i].hand_type;
                    switch (handType)
                    {
                        case HandType.LeftHand:
                            if (leftGesture.trackingSuccess == false)
                            {
                                // RKLog.KeyInfo($"====GesImplementation====: tracking success {HandType.LeftHand}");
                                leftGesture.trackingSuccess = true;
                                GesEventInput.OnTrackedSuccess?.Invoke(HandType.LeftHand);
                            }
                            break;
                        case HandType.RightHand:
                            if (rightGesture.trackingSuccess == false)
                            {
                                // RKLog.KeyInfo($"====GesImplementation====: tracking success {HandType.RightHand}");
                                rightGesture.trackingSuccess = true;
                                GesEventInput.OnTrackedSuccess?.Invoke(HandType.RightHand);
                            }
                            break;
                    }
                    ProcessGesData(beans[i]);
                }
            }
            else
            {
                //两只手丢失的情况
                if (leftGesture.trackingSuccess)
                {
                    // RKLog.KeyInfo($"====GesImplementation====: tracking failed {HandType.LeftHand}");
                    leftGesture.trackingSuccess = false;
                    GesEventInput.OnTrackedFailed?.Invoke(HandType.LeftHand);
                }

                if (rightGesture.trackingSuccess)
                {
                    // RKLog.KeyInfo($"====GesImplementation====: tracking failed {HandType.RightHand}");
                    rightGesture.trackingSuccess = false;
                    GesEventInput.OnTrackedFailed?.Invoke(HandType.RightHand);
                }
            }
        }


        /// <summary>
        /// Process gesture data
        /// </summary>
        /// <param name="gesData">gesture data</param>
        protected void ProcessGesData(GestureBean gesData)
        {
            if (showDebugLog && logText != null)
            {
                logText.text += gesData.ThreeGesKeyInfo();
            }

            HandType handType = (HandType)gesData.hand_type;
            GestureType gesType = (GestureType)gesData.gesture_type;

            //优化手势类型的判定
            if (JudgeGesType(handType) == GestureType.OpenPinch)
            {
                gesType = GestureType.OpenPinch;
            }
            //手表模式的特殊过滤
            if (InputModuleManager.Instance.GetWatchModuleActive(handType) && JudgeGesType(handType, GestureType.Pinch) == GestureType.Grip)
            {
                gesType = GestureType.Grip;
            }

            if (handType == HandType.RightHand)
            {
                if (rightGesFrame < rightGestureTypeCache.Length)
                {
                    rightGestureTypeCache[rightGesFrame] = gesType;
                    rightHandOrientationCache[rightGesFrame] = (HandOrientation)gesData.hand_orientation;
                    rightGesFrame++;
                }
                else
                {
                    rightGesFrame = 0;
                }
                //处理拖拽过程中Grip和Pinch互相误识别的问题
                if (preRightGesType == GestureType.Grip)
                {
                    if (gesType == GestureType.Pinch)
                    {
                        gesType = GestureType.Grip;
                    }

                    if (gesType == GestureType.None && !CanChangeToGesType(GestureType.None, HandType.RightHand))
                    {
                        gesType = GestureType.Grip;
                    }
                }
                //处理特殊角度手势Pinch和Grip误识别
                if (HasGesType(GestureType.OpenPinch, 20, HandType.RightHand) && gesType == GestureType.Grip)
                {
                    gesType = GestureType.Pinch;
                }
            }
            else if (handType == HandType.LeftHand)
            {
                if (leftGesFrame < leftGestureTypeCache.Length)
                {
                    leftGestureTypeCache[leftGesFrame] = gesType;
                    leftHandOrientationCache[leftGesFrame] = (HandOrientation)gesData.hand_orientation;
                    leftGesFrame++;
                }
                else
                {
                    leftGesFrame = 0;
                }
                //处理拖拽过程中Grip和Pinch互相误识别的问题
                if (preLeftGesType == GestureType.Grip)
                {
                    if (gesType == GestureType.Pinch)
                    {
                        gesType = GestureType.Grip;
                    }

                    if (gesType == GestureType.None && !CanChangeToGesType(GestureType.None, HandType.LeftHand))
                    {
                        gesType = GestureType.Grip;
                    }
                }
                //处理特殊角度手势Pinch和Grip误识别
                if (HasGesType(GestureType.OpenPinch, 20, HandType.LeftHand) && gesType == GestureType.Grip)
                {
                    gesType = GestureType.Pinch;
                }
            }

            if (showDebugLog && logText != null)
                logText.text += $"\n FilterGesData {handType},{gesType}";

            //初始化数据
            if (handType == HandType.LeftHand)
            {
                leftGesture.Reset();
                leftGesture.gesType = gesType;
                leftGesture.position = gesData.position;
                leftGesture.deltaPos = leftGesture.position - preLeftHandPos;
                preLeftHandPos = leftGesture.position;
                leftGesture.handOrientation = (HandOrientation)gesData.hand_orientation;
                leftGesture.pinchDistance = gesData.pinchDistance;
                //处理手心手背的判定
                // if (Vector3.Dot(gesData.rotation * Vector3.forward, MainCameraCache.mainCamera.transform.forward) < 0 && Vector3.Dot(gesData.rotation * Vector3.right, MainCameraCache.mainCamera.transform.right) > 0)
                // {
                //     leftGesture.handOrientation = HandOrientation.Back;
                // }
                GesEventInput.OnHandOrientationUpdate?.Invoke(HandType.LeftHand, leftGesture.handOrientation);
            }
            else
            {
                rightGesture.Reset();
                rightGesture.gesType = gesType;
                rightGesture.position = gesData.position;
                rightGesture.deltaPos = rightGesture.position - preRightHandPos;
                preRightHandPos = rightGesture.position;
                rightGesture.handOrientation = (HandOrientation)gesData.hand_orientation;
                rightGesture.pinchDistance = gesData.pinchDistance;
                // if (Vector3.Dot(gesData.rotation * Vector3.forward, MainCameraCache.mainCamera.transform.forward) < 0 && Vector3.Dot(gesData.rotation * Vector3.right, MainCameraCache.mainCamera.transform.right) > 0)
                // {
                //     rightGesture.handOrientation = HandOrientation.Back;
                // }
                GesEventInput.OnHandOrientationUpdate?.Invoke(HandType.RightHand, rightGesture.handOrientation);
            }

            if (gesType == GestureType.Pinch || gesType == GestureType.Grip)
            {
                if (handType == HandType.LeftHand)
                {
                    leftClickTime += Time.deltaTime;
                    leftGesture.handPress = true;
                }
                else
                {
                    rightClickTime += Time.deltaTime;
                    rightGesture.handPress = true;
                }
                GesEventInput.OnHandPress?.Invoke(handType);
            }
            else
            {
                if (handType == HandType.LeftHand)
                {
                    leftGesture.handPress = false;
                }
                else
                {
                    rightGesture.handPress = false;
                }
                GesEventInput.OnHandRelease?.Invoke(handType);
            }

            switch (gesType)
            {
                case GestureType.Grip:
                    if (handType == HandType.LeftHand && preLeftGesType != GestureType.Grip)
                    {
                        leftGesture.handDown = true;
                    }
                    if (handType == HandType.RightHand && preRightGesType != GestureType.Grip)
                    {
                        rightGesture.handDown = true;
                    }
                    break;
                case GestureType.Pinch:
                    if (handType == HandType.LeftHand && preLeftGesType != GestureType.Pinch)
                    {
                        leftGesture.handDown = true;
                    }
                    if (handType == HandType.RightHand && preRightGesType != GestureType.Pinch)
                    {
                        rightGesture.handDown = true;
                    }
                    break;
                default:
                    if (handType == HandType.LeftHand &&
                        (preLeftGesType == GestureType.Pinch &&
                        gesType != GestureType.Grip) ||
                        (leftHandInteractorType == InteractorType.Near ?
                        (preLeftGesType == GestureType.Grip && gesType == GestureType.Palm) :
                        preLeftGesType == GestureType.Grip && gesType != GestureType.Grip))
                    {
                        // RKLog.Info($"====GesImplementation====: Trigger LeftHandUp {preLeftGesType},{gesType}");
                        leftGesture.handUp = true;
                    }
                    if (handType == HandType.RightHand &&
                        (preRightGesType == GestureType.Pinch &&
                        gesType != GestureType.Grip) ||
                        (rightHandInteractorType == InteractorType.Near ?
                        (preRightGesType == GestureType.Grip && gesType == GestureType.Palm) :
                         preRightGesType == GestureType.Grip && gesType != GestureType.Grip))
                    {
                        // RKLog.Info($"====GesImplementation====: Trigger RightHandUp  {preRightGesType},{gesType}");
                        rightGesture.handUp = true;
                    }
                    break;
            }
            if (handType == HandType.LeftHand)
            {
                preLeftGesType = gesType;
                preLeftGesData = gesData;
                if (leftClickTime > 0 && leftClickTime < clickTime && leftGesture.handUp && leftGesture.gesType != GestureType.Palm)
                {
                    // RKLog.Info("====GesImplementation====: OnLeftHandClick");
                    GesEventInput.OnGesClick?.Invoke(handType);
                    leftGesture.handClick = true;
                    leftClickTime = 0;
                }
                else if (leftGesture.handUp)
                {
                    leftClickTime = 0;
                }
            }
            else
            {
                preRightGesType = gesType;
                preRightGesData = gesData;
                if (rightClickTime > 0 && rightClickTime < clickTime && rightGesture.handUp && rightGesture.gesType != GestureType.Palm)
                {
                    // RKLog.Info("====GesImplementation====: OnRightHandClick");
                    GesEventInput.OnGesClick?.Invoke(handType);
                    rightGesture.handClick = true;
                    rightClickTime = 0;
                }
                else if (rightGesture.handUp)
                {
                    rightClickTime = 0;
                }
            }
            GesEventInput.OnRenderHand?.Invoke(handType, gesData);
            GesEventInput.OnProcessGesData?.Invoke(handType, gesData);
            Vector3 pinchCenterOri = (GetSkeletonPose(SkeletonIndexFlag.THUMB_MCP, handType).position + GetSkeletonPose(SkeletonIndexFlag.INDEX_FINGER_MCP, handType).position) / 2;
            Vector3 handCenter = gesData.position;
            GesEventInput.OnRayPoseUpdate?.Invoke(handType, GetSkeletonPose(SkeletonIndexFlag.WRIST, handType).position, handCenter, pinchCenterOri, pinchCenterOri);
        }

        private int CalHandOrientation(Vector3 handForward)
        {
            float result = Vector3.Dot(handForward, NativeInterface.NativeAPI.GetHeadPose(out long ts).rotation * Vector3.forward);

            if (result > -1 && result < -0.7f)
            {
                return 0;
            }

            if (result > 0.7f && result < 1)
            {
                return 1;
            }

            return -1;
        }


        Matrix4x4 matrix = Matrix4x4.identity;
        internal Vector3[] GetVertices(float[] data, bool right, Pose cameraPose, float scale = 1.0f)
        {
            Vector3[] vertices;
            string key = right ? "right:" + data.Length / 3 : "left:" + data.Length / 3;
            if (vector3Dict.ContainsKey(key))
            {
                vertices = vector3Dict[key];
            }
            else
            {
                vertices = new Vector3[data.Length / 3];
                vector3Dict.Add(key, vertices);
            }
            Vector3 basePosition = Vector3.zero;
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            for (int i = 0; i < vertices.Length; i++)
            {
                op[0] = data[3 * i] / 1000.0f;
                op[1] = data[3 * i + 1] / 1000.0f;
                op[2] = -data[3 * i + 2] / 1000.0f;
                vertices[i] = cameraPose.rotation * op;
                vertices[i] += cameraPose.position;
                if (i == 0)
                {
                    basePosition = vertices[i];
                }
                else
                {
                    vertices[i] -= basePosition;
                    vertices[i] = matrix * vertices[i];
                    vertices[i] += basePosition;
                }
            }
            return vertices;
        }


        internal bool GetHandDown(HandType type, bool isPinch)
        {
            if (type == HandType.RightHand)
            {
                return isPinch ? GetHandDown(HandType.RightHand) && rightGesture.gesType == GestureType.Pinch : GetHandDown(HandType.RightHand) && rightGesture.gesType == GestureType.Grip;
            }
            else if (type == HandType.LeftHand)
            {
                return isPinch ? GetHandDown(HandType.LeftHand) && leftGesture.gesType == GestureType.Pinch : GetHandDown(HandType.LeftHand) && leftGesture.gesType == GestureType.Grip;
            }
            else
            {
                return false;
            }
        }

        internal void SetIndexFingerToStraight(bool active, HandType hand)
        {
            if (hand == HandType.LeftHand)
                this.setLeftIndexFingerToStraight = active;
            if (hand == HandType.RightHand)
                this.setRightIndexFingerToStraight = active;
        }

        internal bool GetHandUp(HandType type, bool isPinch)
        {
            if (type == HandType.RightHand)
            {
                return isPinch ? GetHandUp(HandType.RightHand) : GetHandUp(HandType.RightHand) && rightGesture.gesType == GestureType.Palm;
            }
            else if (type == HandType.LeftHand)
            {
                return isPinch ? GetHandUp(HandType.LeftHand) : GetHandUp(HandType.LeftHand) && leftGesture.gesType == GestureType.Palm;
            }
            else
            {
                return false;
            }
        }

        internal bool GetHandUp(HandType type)
        {
            if (type == HandType.RightHand)
            {
                return rightGesture.handUp;
            }
            else if (type == HandType.LeftHand)
            {
                return leftGesture.handUp;
            }
            else
            {
                return false;
            }
        }

        internal bool GetHandClick(HandType type)
        {
            if (type == HandType.RightHand)
            {
                return rightGesture.handClick;
            }
            else if (type == HandType.LeftHand)
            {
                return leftGesture.handClick;
            }
            else
            {
                return false;
            }
        }

        internal bool GetHandPress(HandType type)
        {
            if (type == HandType.RightHand)
            {
                return rightGesture.handPress;
            }
            else if (type == HandType.LeftHand)
            {
                return leftGesture.handPress;
            }
            else
            {
                return false;
            }
        }

        internal void SetHandScale(float handScale)
        {
            this.handScale = handScale;
        }

        internal float GetHandScale()
        {
            return handScale;
        }

        internal bool GetHandPress(HandType type, bool isPinch)
        {
            if (type == HandType.LeftHand)
            {
                return isPinch ? GetHandPress(HandType.LeftHand) && leftGesture.gesType == GestureType.Pinch : GetHandPress(HandType.LeftHand) && leftGesture.gesType == GestureType.Grip;
            }
            else if (type == HandType.RightHand)
            {
                return isPinch ? GetHandPress(HandType.RightHand) && rightGesture.gesType == GestureType.Pinch : GetHandPress(HandType.RightHand) && rightGesture.gesType == GestureType.Grip;
            }
            else
            {
                return false;
            }
        }

        internal Gesture GetGesture(HandType type)
        {
            if (type == HandType.RightHand)
            {
                return rightGesture;
            }
            else
            {
                return leftGesture;
            }
        }

        internal float GetPinchDistance(HandType type)
        {
            switch (type)
            {
                case HandType.LeftHand:
                    return leftGesture.pinchDistance;
                case HandType.RightHand:
                    return rightGesture.pinchDistance;
            }
            return 0;
        }


        internal GestureType GetGestureType(HandType hand)
        {
            return GetGesture(hand).gesType;
        }


        internal Vector3 GetHandPos(HandType handType)
        {
            switch (handType)
            {
                case HandType.LeftHand:
                    return leftGesture.position;
                case HandType.RightHand:
                    return rightGesture.position;
                case HandType.None:
                    return Vector3.zero;
            }
            return Vector3.zero;
        }

        internal Vector3 GetHandDeltaPos(HandType hand)
        {
            switch (hand)
            {
                case HandType.LeftHand:
                    return leftGesture.deltaPos;
                case HandType.RightHand:
                    return rightGesture.deltaPos;
            }
            return Vector3.zero;
        }

        internal Pose GetSkeletonPose(SkeletonIndexFlag flag, HandType type)
        {
            Pose pose = new Pose();
            if (gestureData.Length > 0)
            {
                for (int i = 0; i < gestureData.Length; i++)
                {
                    if (gestureData[i].hand_type == (int)type && gestureData[i].skeletons != null && gestureData[i].skeletons.Length > 0)
                    {
                        pose.position = gestureData[i].skeletons[(int)flag];
                        pose.rotation = gestureData[i].skeletonsRot[(int)flag];
                    }
                }
            }
            return pose;
        }


        internal Pose GetAlgorithmSkeletonPose(SkeletonIndexFlag flag, HandType type)
        {
            Pose pose = new Pose();
            if (gestureData.Length > 0)
            {
                for (int i = 0; i < gestureData.Length; i++)
                {
                    if (gestureData[i].hand_type == (int)type && gestureData[i].skeletons != null && gestureData[i].skeletons.Length > 0)
                    {
                        pose.position = type == HandType.LeftHand ? leftSkeletonsPos[(int)flag] : rightSkeletonsPos[(int)flag];
                        pose.rotation = gestureData[i].skeletonsRot[(int)flag];
                    }
                }
            }
            return pose;
        }

        /// <summary>
        /// Determine the filtered gesture types
        /// </summary>
        /// <param name="hand">左右手类型</param>
        /// <param name="gesType">输入的手势类型</param>
        /// <returns></returns>
        private GestureType JudgeGesType(HandType hand, GestureType gesType = GestureType.None)
        {
            //食指方向
            Vector3 indexForward = (GetSkeletonPose(SkeletonIndexFlag.INDEX_FINGER_TIP, hand).position - GetSkeletonPose(SkeletonIndexFlag.INDEX_FINGER_MCP, hand).position).normalized;
            //中指方向
            Vector3 middleForward = (GetSkeletonPose(SkeletonIndexFlag.MIDDLE_FINGER_TIP, hand).position - GetSkeletonPose(SkeletonIndexFlag.MIDDLE_FINGER_MCP, hand).position).normalized;
            //无名指方向
            Vector3 ringFingerForward = (GetSkeletonPose(SkeletonIndexFlag.RING_FINGER_TIP, hand).position - GetSkeletonPose(SkeletonIndexFlag.RING_FINGER_MCP, hand).position).normalized;
            //小拇指方向
            Vector3 pinkyForward = (GetSkeletonPose(SkeletonIndexFlag.PINKY_TIP, hand).position - GetSkeletonPose(SkeletonIndexFlag.PINKY_MCP, hand).position).normalized;
            //手方向
            Vector3 handForward = (GetSkeletonPose(SkeletonIndexFlag.MIDDLE_FINGER_MCP, hand).position - GetSkeletonPose(SkeletonIndexFlag.WRIST, hand).position).normalized;

            float dotHandIndex = Vector3.Dot(handForward, indexForward);
            float dotHandMiddle = Vector3.Dot(handForward, middleForward);
            float dotHandRing = Vector3.Dot(handForward, ringFingerForward);
            float dotHandPinky = Vector3.Dot(handForward, pinkyForward);

            if (showDebugLog && logText != null)
                logText.text += "\n" + $"JudgeGesType => dotHandIndex:{dotHandIndex},dotHandMiddle:{dotHandMiddle},dotHandRing:{dotHandRing},dotHandPinky:{dotHandPinky}";

            if (dotHandIndex > 0.5f && dotHandMiddle < 0f && dotHandRing < 0f && dotHandPinky < 0f)
            {
                return GestureType.OpenPinch;
            }

            if (dotHandIndex < 0.8f && dotHandMiddle < 0.8f && dotHandRing < 0.8f && dotHandPinky < 0.8f && gesType == GestureType.Pinch)
            {
                return GestureType.Grip;
            }

            return GestureType.None;
        }

        /// <summary>
        /// Determine if there is a target gesture type within the previous frame
        /// </summary>
        /// <param name="targetGes"></param>
        /// <param name="preFrame"></param>
        /// <returns></returns>
        internal bool HasGesType(GestureType targetGes, int preFrame, HandType hand)
        {
            if (hand == HandType.None)
                return false;
            GestureType[] gestureTypeCache = hand == HandType.LeftHand ? leftGestureTypeCache : rightGestureTypeCache;
            for (int i = 1; i <= preFrame; i++)
            {
                int index = hand == HandType.LeftHand ? leftGesFrame - i : rightGesFrame - i;
                if (index < 0)
                {
                    index = gestureTypeCache.Length + index;
                }
                if (gestureTypeCache[index] == targetGes)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine if there is a target gesture type and  target handorientation type within the previous frame
        /// </summary>
        /// <param name="targetHandOri"></param>
        /// <param name="preFrame"></param>
        /// <returns></returns>
        internal bool HasGesTypeAndHandOrientation(GestureType targetGes, HandOrientation targetHandOri, HandType hand, int preFrame, int count = 1)
        {
            int targetCount = 0;
            if (hand == HandType.None)
                return false;
            GestureType[] gestureTypeCache = hand == HandType.LeftHand ? leftGestureTypeCache : rightGestureTypeCache;
            HandOrientation[] handOrientationCache = hand == HandType.LeftHand ? leftHandOrientationCache : rightHandOrientationCache;
            for (int i = 1; i <= preFrame; i++)
            {
                int index = hand == HandType.LeftHand ? leftGesFrame - i : rightGesFrame - i;
                if (index < 0)
                {
                    index = gestureTypeCache.Length + index;
                }
                if (gestureTypeCache[index] == targetGes && handOrientationCache[index] == targetHandOri)
                {
                    targetCount++;
                }
                if (count == targetCount)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Determine if there is a target handorientation type within the previous frame
        /// </summary>
        /// <param name="targetHandOri"></param>
        /// <param name="preFrame"></param>
        /// <returns></returns>
        internal bool HasHandOrientationType(HandOrientation targetOrientation, int preFrame, HandType hand)
        {
            if (hand == HandType.None)
                return false;
            HandOrientation[] handOrientationCache = hand == HandType.LeftHand ? leftHandOrientationCache : rightHandOrientationCache;
            for (int i = 1; i <= preFrame; i++)
            {
                int index = hand == HandType.LeftHand ? leftGesFrame - i : rightGesFrame - i;
                if (index < 0)
                {
                    index = handOrientationCache.Length + index;
                }
                if (handOrientationCache[index] == targetOrientation)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Retrieve the hand pose
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Pose GetHandPose(HandType type)
        {
            for (int i = 0; i < gestureData.Length; i++)
            {
                if (gestureData[i].hand_type == (int)type)
                {
                    return new Pose(gestureData[i].position, gestureData[i].rotation);
                }
            }
            return new Pose();
        }

        internal InteractorType GetInteractorType(HandType hand)
        {
            switch (hand)
            {
                case HandType.LeftHand:
                    return leftHandInteractorType;
                case HandType.RightHand:
                    return rightHandInteractorType;
                default:
                    return InteractorType.None;
            }
        }

        internal HandOrientation GetHandOrientation(HandType hand)
        {
            switch (hand)
            {
                case HandType.LeftHand:
                    return leftGesture.handOrientation;
                case HandType.RightHand:
                    return rightGesture.handOrientation;
            }
            return default(HandOrientation);
        }

        internal void SetInteractorType(InteractorType type, HandType hand)
        {
            switch (hand)
            {
                case HandType.LeftHand:
                    leftHandInteractorType = type;
                    break;
                case HandType.RightHand:
                    rightHandInteractorType = type;
                    break;
            }
        }

        internal bool GetHandDown(HandType type)
        {
            if (type == HandType.RightHand)
            {
                return rightGesture.handDown;
            }
            else if (type == HandType.LeftHand)
            {
                return leftGesture.handDown;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Can switch to a specific gesture
        /// </summary>
        /// <param name="targetGes"></param>
        /// <returns></returns>
        internal virtual bool CanChangeToGesType(GestureType targetGes, HandType hand)
        {
            GestureType[] gestureTypeCache = hand == HandType.LeftHand ? leftGestureTypeCache : rightGestureTypeCache;
            int count = 0;
            for (int i = 0; i < gestureTypeCache.Length; i++)
            {
                if (targetGes == gestureTypeCache[i])
                {
                    count++;
                }
            }
            //超过百分之50%激活...
            return count > changeGesThreshold;
        }
    }
}
