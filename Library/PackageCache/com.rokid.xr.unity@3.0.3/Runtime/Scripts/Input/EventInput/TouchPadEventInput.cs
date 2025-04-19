using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Module;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine.UI;

namespace Rokid.UXR.Interaction
{
    public enum TouchOperateOrientation
    {
        Auto,
        LandscapeLeft,
        Portrait
    }

    public class TouchPadEventInput : MonoSingleton<TouchPadEventInput>, IEventInput
    {
        [SerializeField]
        private Text logText;
        public static Action<Vector2> OnMouseMove;
        public static Action OnActiveTouchPadModule;
        public static Action OnReleaseTouchPadModule;
        public static Action OnTriggerTouchPadSuccess;
        public static event Action<bool> OnTouchPadSleep;
        private Vector2 touchDelta = Vector2.zero;
        private float X_MOVE_SCALE = 0.035f;
        private float Y_MOVE_SCALE = 0.035f;
        private float DRAG_LONG_PRESS_TIME = 0.6f;
        public Transform Interactor { get; set; }
        private TouchOperateOrientation touchOperateOrientation = TouchOperateOrientation.Auto;
        public TouchOperateOrientation TouchOperateOrientation { get { return touchOperateOrientation; } set { touchOperateOrientation = value; } }
        private int pixelDragThreshold = 0;
        public int PixelDragThreshold { get { return pixelDragThreshold; } set { pixelDragThreshold = value; } }
        public InputModuleType inputModuleType { get => InputModuleType.TouchPad; }

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
                rayPose = Interactor?.GetComponentInChildren<IRayPose>();
            }
            return rayPose;
        }
        private bool initialize = false;
        private float elapsedTime = 0;
        private bool touchMove = false;
        private bool mouseMove = false, mouseMove1 = false;
        private bool shouldStartDrag = false;
        private bool triggerLongPress = false;
        private bool longPress = false;
        private bool lightLongPress = false;
        private bool pointerDown = false;
        private bool pointerUp = false;
        private float raySleepTime = 5.0f;
        private float raySleepElasptime = 0;
        private bool raySleep;
        private bool canTriggerDrag = true;
        private bool touchInputActive = true;
        private bool lockInput;

        protected override void OnSingletonInit()
        {
            RKTouchInput.OnUpdate += OnUpdate;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnMouseMove = null;
            RKTouchInput.OnUpdate -= OnUpdate;
        }

        public void Initialize(Transform parent)
        {
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("TouchRayInteractor");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/TouchRayInteractor"));
                }
                Interactor = go.transform.GetComponentInChildren<ModuleInteractor>().transform;
                Interactor.name = "TouchRayInteractor";
                Interactor.SetParent(transform);
            }
            Interactor.SetParent(transform);
            this.transform.SetParent(parent);
            initialize = true;
            RKTouchInput.Instance.Init();
            UnityPlayerAPI.Instance.SetUnityScreenOrientation(ScreenOrientation.Portrait);
        }

        public void Release()
        {
            OnReleaseTouchPadModule?.Invoke();
            Destroy(this.gameObject);
        }

        private void Start()
        {
            RKLog.Info("====TouchPadEventInput==== 注册android回调用");
        }

        private float deltaTime = 0;
        private Vector2 touchForward = Vector2.zero;
        private Vector2 startMidPoint = Vector2.zero;
        private void ActiveLogic2(ref bool active)
        {
            if (RKTouchInput.Instance.GetInsideTouchCount() == 2 && active == false)
            {
                if (RKTouchInput.Instance.GetInsideTouch(0).phase == TouchPhase.Began || RKTouchInput.Instance.GetInsideTouch(1).phase == TouchPhase.Began)
                {
                    deltaTime = 0;
                    startMidPoint = (RKTouchInput.Instance.GetInsideTouch(0).position + RKTouchInput.Instance.GetInsideTouch(1).position) / 2;
                    touchForward = Vector3.Normalize(RKTouchInput.Instance.GetInsideTouch(0).position - RKTouchInput.Instance.GetInsideTouch(1).position);
                    // RKLog.KeyInfo($"====RKTouchInput====: touch begin {touchForward},{RKTouchInput.Instance.GetInsideTouch(0).position},{RKTouchInput.Instance.GetInsideTouch(1).position}");
                }
                deltaTime += Time.deltaTime;
                Vector2 currMidPoint = (RKTouchInput.Instance.GetInsideTouch(0).position + RKTouchInput.Instance.GetInsideTouch(1).position) / 2;
                if (deltaTime > 0.1f && Vector3.SqrMagnitude(currMidPoint - startMidPoint) > 300 * 300)
                {
                    OnTriggerTouchPadSuccess?.Invoke();
                    switch (touchOperateOrientation)
                    {
                        case TouchOperateOrientation.LandscapeLeft:
                            UnityPlayerAPI.Instance.SetUnityScreenOrientation(ScreenOrientation.LandscapeLeft);
                            break;
                        case TouchOperateOrientation.Portrait:
                            UnityPlayerAPI.Instance.SetUnityScreenOrientation(ScreenOrientation.Portrait);
                            break;
                        case TouchOperateOrientation.Auto:
                            if (Mathf.Abs(Vector2.Dot(touchForward, Vector2.right)) > 0.7f)
                            {
                                UnityPlayerAPI.Instance.SetUnityScreenOrientation(ScreenOrientation.Portrait);
                            }
                            else
                            {
                                UnityPlayerAPI.Instance.SetUnityScreenOrientation(ScreenOrientation.LandscapeLeft);
                            }
                            break;
                    }
                    active = true;
                }
            }
        }

        private void OnUpdate()
        {
            if (lockInput) return;
            if (initialize == true)
            {
                if (!InputModuleManager.Instance.GetTouchPadActive())
                {
                    bool active = false;
                    if (!Utils.IsAndroidPlatform())
                    {
                        active = Input.GetKeyDown(KeyCode.T);
                    }
                    else
                    {
                        ActiveLogic2(ref active);
                    }
                    if (active)
                    {
                        ActiveModule();
                    }
                }
            }
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            if (mouseDelta.magnitude > 0.0001f && RKTouchInput.Instance.FingerOperation == FingerOperation.None)
            {
                touchInputActive = false;
            }

            if (RKTouchInput.Instance.FingerOperation != FingerOperation.None)
            {
                touchInputActive = true;
            }

            if (touchInputActive)
            {
                pointerDown = false;
                pointerUp = false;
                if (RKTouchInput.Instance.FingerOperation != FingerOperation.None)
                {
                    Touch touch = RKTouchInput.Instance.GetInsideTouch(0);
                    if (RKTouchInput.Instance.TryGetSmoothInsideTouchDeltaPosition(0, out Vector2 delta))
                    {
                        touchDelta.x = delta.x * X_MOVE_SCALE;
                        touchDelta.y = -delta.y * Y_MOVE_SCALE;
                    }
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            touchMove = false;
                            if (RKTouchInput.Instance.FingerOperation == FingerOperation.OneFinger)
                            {
                                pointerDown = true;
                                longPress = false;
                                lightLongPress = false;
                                triggerLongPress = false;
                            }
                            break;
                        case TouchPhase.Stationary:
                            if (touchMove == false && RKTouchInput.Instance.FingerOperation == FingerOperation.OneFinger)
                            {
                                elapsedTime += Time.deltaTime;
                                if (elapsedTime > DRAG_LONG_PRESS_TIME)
                                {
                                    if (shouldStartDrag == false && InputModuleManager.Instance.GetTouchPadActive())
                                    {
                                        NativeInterface.NativeAPI.Vibrate(1);
                                    }
                                    shouldStartDrag = true;
                                    if (triggerLongPress == false)
                                    {
                                        triggerLongPress = true;
                                        longPress = true;
                                        if (touch.position.y < UnityPlayerAPI.Instance.PhoneScreenHeight * 0.2f)
                                        {
                                            lightLongPress = true;
                                        }
                                    }
                                    else
                                    {
                                        lightLongPress = false;
                                    }
                                }
                            }
                            else
                            {
                                elapsedTime = 0;
                            }
                            break;
                        case TouchPhase.Ended:
                            Loom.QueueOnMainThread(() => { touchMove = false; }, 0.1f);
                            shouldStartDrag = false;
                            elapsedTime = 0;
                            if (RKTouchInput.Instance.FingerOperation == FingerOperation.OneFinger)
                                pointerUp = true;
                            break;
                        case TouchPhase.Canceled:
                            touchMove = false;
                            shouldStartDrag = false;
                            elapsedTime = 0;
                            if (RKTouchInput.Instance.FingerOperation == FingerOperation.OneFinger)
                                pointerUp = true;
                            break;
                        case TouchPhase.Moved:
                            if (RKTouchInput.Instance.FingerOperation == FingerOperation.OneFinger)
                            {
                                OnMouseMove?.Invoke(touchDelta);
                            }
                            touchMove = true;
                            break;
                    }
                    raySleepElasptime = 0;
                    if (raySleep)
                    {
                        raySleep = false;
                        OnTouchPadSleep?.Invoke(false);
                    }
                }
                else
                {
                    if (raySleep == false)
                    {
                        raySleepElasptime += Time.deltaTime;
                        if (raySleepElasptime > raySleepTime)
                        {
                            raySleep = true;
                            OnTouchPadSleep?.Invoke(true);
                        }
                    }
                    touchDelta = Vector2.zero;
                }
            }
            else
            {
                pointerDown = false;
                pointerUp = false;
                if (Input.GetMouseButtonDown(0))
                {
                    shouldStartDrag = false;
                    mouseMove = false;
                    elapsedTime = 0;
                    pointerDown = true;
                    longPress = false;
                    mouseMove1 = false;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    pointerUp = true;
                }
                if (Input.GetMouseButton(0) && mouseDelta.x < 0.01f && mouseDelta.y < 0.01f && mouseMove == false)
                {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime > DRAG_LONG_PRESS_TIME)
                    {
                        shouldStartDrag = true;
                        longPress = true;
                    }
                }
                else
                {
                    mouseMove = true;
                }
            }
        }

        public void ActiveModule()
        {
            OnActiveTouchPadModule?.Invoke();
            Input.ResetInputAxes();
            RKVirtualController.Instance.Change(ControllerType.Mouse);
            EventSystem.current.pixelDragThreshold = 0;
        }

        public bool ShouldStartDrag()
        {
            return shouldStartDrag && canTriggerDrag;
        }

        public void CanTriggerDrag(bool active)
        {
            canTriggerDrag = active;
        }

        public bool LongPress()
        {
            return longPress;
        }

        public void SetLongPress(bool press)
        {
            this.longPress = press;
        }

        public bool GetLightLongPress()
        {
            return this.lightLongPress;
        }

        public bool PointerDown()
        {
            return pointerDown;
        }

        public bool PointerUp()
        {
            return pointerUp;
        }

        public bool TouchMove()
        {
            return touchMove;
        }

        public float ForwardSpeed()
        {
            if (RKTouchInput.Instance.GetInsideTouchCount() == 2)
            {
                Debug.Log($"TouchPadEventInput==> {touchDelta}");
                return touchDelta.y * -0.2f;
            }
            return 0;
        }

        public Vector2 GetTouchDelta()
        {
            return touchDelta;
        }

        internal bool GetTouchInputActive()
        {
            return touchInputActive;
        }

        public void Sleep(bool sleep)
        {
            OnTouchPadSleep?.Invoke(sleep);
        }

        public void Lock(bool isLock)
        {
            lockInput = isLock;
        }
    }
}

