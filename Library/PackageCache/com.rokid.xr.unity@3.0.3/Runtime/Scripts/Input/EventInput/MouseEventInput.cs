using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public class MouseEventInput : MonoSingleton<MouseEventInput>, IEventInput
    {
        public static Action<Vector2> OnMouseMove;
        public static Action OnActiveMouseModule;
        public static Action OnReleaseMouseModule;
        public static event Action<bool> OnMouseSleep;
        private Vector2 mouseDelta = Vector2.zero;
        private Vector2 scrollDelta = Vector2.zero;
        private bool scrollChange;
        private bool mouseChange;
        private bool scrollInverse = false;
        private float elapsedTime = 0;
        public Transform Interactor { get; set; }
        private int pixelDragThreshold = 10;
        public int PixelDragThreshold { get { return pixelDragThreshold; } set { pixelDragThreshold = value; } }
        public InputModuleType inputModuleType => InputModuleType.Mouse;

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
        private bool initialize = false;
        private float raySleepTime = 10.0f;
        private float raySleepElasptime = 0;
        private bool raySleep;
        private bool lockInput;
        public void Initialize(Transform parent)
        {
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("MouseRayInteractor");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/MouseRayInteractor"));
                }
                Interactor = go.transform.GetComponentInChildren<ModuleInteractor>().transform;
                Interactor.name = "MouseRayInteractor";
                Interactor.SetParent(transform);
            }
            Interactor.SetParent(transform);
            this.transform.SetParent(parent);
            initialize = true;
        }

        public void Release()
        {
            OnReleaseMouseModule?.Invoke();
            Destroy(this.gameObject);
        }

        private void Start()
        {
#if !USE_ROKID_OPENXR
            RKLog.Info("====MouseEventInput==== 注册android回调用");
            CallBridge.CallAndroid(Request.Build()
                .Name("VirtualController.setOnHoverListener").
                AndroidCallback(CallBridge.CreateCallback(this.gameObject.name, "OnHoverEvent")));

            CallBridge.CallAndroid(Request.Build()
                .Name("VirtualController.setOnTouchListener").
                AndroidCallback(CallBridge.CreateCallback(this.gameObject.name, "OnTouchEvent")));

            CallBridge.CallAndroid(Request.Build()
                .Name("VirtualController.setOnScrollListener").
                AndroidCallback(CallBridge.CreateCallback(this.gameObject.name, "OnScrollEvent")));
#endif
        }

        /// <summary>
        /// 鼠标滚轮输入事件
        /// Mouse Scroll Input Event
        /// </summary>
        /// <param name="delta"></param>
        private void OnScrollEvent(string delta)
        {
            float x = Convert.ToSingle(delta.Split(',')[1]);
            float y = Convert.ToSingle(delta.Split(',')[0]);
            scrollDelta[0] = scrollInverse ? -x : x;
            scrollDelta[1] = scrollInverse ? -y : y;
            scrollChange = true;
            elapsedTime = 0;
        }

        /// <summary>
        /// 获取鼠标滚轮事件
        /// Get Mouse Scroll Delta
        /// </summary>
        public Vector2 GetMouseScrollDelta()
        {
            return scrollDelta;
        }

        /// <summary>;
        /// Whether the mouse is inverted
        /// </summary>
        /// <param name="inverse"></param>
        public void SetScrollInverse(bool inverse)
        {
            scrollInverse = inverse;
        }

        /// <summary>
        /// Get the mouse down event
        /// </summary>
        /// <param name="button">0: left mouse button 1: right mouse button 2: middle mouse button</param>
        /// <returns></returns>
        public bool GetMouseButtonDown(int button)
        {
            mouseChange = true;
            if (button == 0)
            {
#if UNITY_EDITOR
                return Input.GetMouseButtonDown(0);
#else
                return RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_MOUSE_FIRST);
#endif
            }
            if (button == 1)
            {
#if UNITY_EDITOR
                return Input.GetMouseButtonDown(1);
#else
                return RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_MOUSE_SECONDARY);
#endif
            }
            if (button == 2)
            {
#if UNITY_EDITOR
                return Input.GetMouseButtonDown(2);
#else
                return RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_MOUSE_THERIARY);
#endif
            }
            return false;
        }

        /// <summary>
        /// Get the Up event of the mouse
        /// </summary>
        /// <param name="button">0: left mouse button 1: right mouse button 2: middle mouse button</param>
        /// <returns></returns>
        public bool GetMouseButtonUp(int button)
        {
            mouseChange = false;
            if (button == 0)
            {
#if UNITY_EDITOR
                return Input.GetMouseButtonUp(0);
#else
                return RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_MOUSE_FIRST);
#endif
            }
            if (button == 1)
            {
#if UNITY_EDITOR
                return Input.GetMouseButtonUp(1);
#else
                return  RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_MOUSE_SECONDARY);
#endif
            }
            if (button == 2)
            {
#if UNITY_EDITOR
                return Input.GetMouseButtonUp(2);
#else
                return RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_MOUSE_THERIARY);
#endif
            }
            return false;
        }

        /// <summary>
        /// Android mouse input event
        /// </summary>
        private void OnHoverEvent(string delta)
        {
            float x = Convert.ToSingle(delta.Split(',')[0]);
            float y = Convert.ToSingle(delta.Split(',')[1]);
            mouseDelta[0] = x;
            mouseDelta[1] = y;
            if (InputModuleManager.Instance.GetMouseActive())
            {
                OnMouseMove?.Invoke(mouseDelta);
            }
            mouseChange = true;
        }

        /// <summary>
        /// Android mouse press input event
        /// </summary>
        private void OnTouchEvent(string screenPos)
        {
            float x = Convert.ToSingle(screenPos.Split(',')[0]);
            float y = Convert.ToSingle(screenPos.Split(',')[1]);
            mouseDelta[0] = x;
            mouseDelta[1] = y;
            if (InputModuleManager.Instance.GetMouseActive())
            {
                OnMouseMove?.Invoke(mouseDelta);
            }
            mouseChange = true;
        }

        private void Update()
        {
            if (lockInput) return;
            if (InputModuleManager.Instance.GetMouseActive() == false && initialize == true)
            {
                bool active = false;
#if UNITY_EDITOR
                active = Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.M);
#else
                active = mouseDelta.magnitude > 10.0f;
#endif
                if (active)
                {
                    ActiveModule();
                }
            }

            if (Input.GetMouseButton(0))
            {
                raySleepElasptime = 0;
            }

            if (Utils.IsUnityEditor())
            {
                Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                if (mouseDelta.x > 0.001f || mouseDelta.y > 0.001f)
                {
                    mouseChange = true;
                }
            }


            //判断鼠标是否一直在滑动
            if (mouseChange)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > 0.01f)
                {
                    mouseChange = false;
                    mouseDelta[0] = 0;
                    mouseDelta[1] = 0;
                }

                if (raySleep)
                {
                    raySleepElasptime = 0;
                    raySleep = false;
                    OnMouseSleep?.Invoke(false);
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
                        OnMouseSleep?.Invoke(true);
                    }
                }
            }
        }

        void LateUpdate()
        {
            //判断滚轮值是否一直在变化
            if (scrollChange)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > 0.01f)
                {
                    scrollChange = false;
                    scrollDelta[0] = 0;
                    scrollDelta[1] = 0;
                }
                raySleepElasptime = 0;
            }
        }

        public void ActiveModule()
        {
            RKLog.KeyInfo("====MouseEventInput==== : ActiveModule");
            OnActiveMouseModule?.Invoke();
            Input.ResetInputAxes();
            RKVirtualController.Instance.Change(ControllerType.Mouse);
            EventSystem.current.pixelDragThreshold = PixelDragThreshold;
        }

        protected override void OnSingletonInit()
        {
            this.gameObject.name = "MouseEventInput";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnMouseMove = null;
            initialize = false;
        }

        public void Sleep(bool sleep)
        {
            OnMouseSleep?.Invoke(sleep);
        }

        public void Lock(bool isLock)
        {
            lockInput = isLock;
        }
    }
}

