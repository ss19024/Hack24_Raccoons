using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rokid.UXR.Interaction;
using UnityEngine.Assertions;
using Rokid.UXR.Module;
using System;

namespace Rokid.UXR.Utility
{
    public class GlobalEventsLogUtils : AutoInjectBehaviour
    {

        [Autowrited, SerializeField]
        private Text pointerLogText;
        [Autowrited, SerializeField]
        private Text inputLogText;
        [Autowrited("ScrollRect_PointerLog"), SerializeField]
        private ScrollRect pointerScrollRect;
        [Autowrited("ScrollRect_InputLog"), SerializeField]
        private ScrollRect inputScrollRect;
        [SerializeField]
        private int maxLogCount = 50;
        private int logCount = 0;
        private bool logChanged = false;
        private bool dragging = false;
        private PointerEventData currentEventData = new PointerEventData(EventSystem.current);

        private int enterCount, exitCount, downCount, upCount, beginDragCount, endDragCount;

        private void Start()
        {
            Assert.IsNotNull(pointerLogText);
            Assert.IsNotNull(inputLogText);
            Assert.IsNotNull(pointerScrollRect);
            Assert.IsNotNull(inputScrollRect);
            pointerScrollRect.verticalNormalizedPosition = 1;
            inputScrollRect.verticalNormalizedPosition = 1;
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnEnable()
        {
            RKPointerListener.OnPointerDown += OnPointerDown;
            RKPointerListener.OnPointerUp += OnPointerUp;
            RKPointerListener.OnPointerClick += OnPointerClick;
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
            RKPointerListener.OnPointerEnterWithObj += OnPointerEnter;
            RKPointerListener.OnPointerExit += OnPointerExit;
        }

        private void OnDisable()
        {
            RKPointerListener.OnPointerDown -= OnPointerDown;
            RKPointerListener.OnPointerUp -= OnPointerUp;
            RKPointerListener.OnPointerClick -= OnPointerClick;
            RKPointerListener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd -= OnPointerDragEnd;
            RKPointerListener.OnPointerEnterWithObj -= OnPointerEnter;
            RKPointerListener.OnPointerExit -= OnPointerExit;
        }

        private void OnPointerEnter(PointerEventData data, GameObject @object)
        {
            currentEventData = data;
            pointerLogText.text += $"OnPointerEnter {@object?.name}\r\n";
            logChanged = true;
            enterCount++;
        }

        private void OnPointerExit(PointerEventData data, GameObject @object)
        {
            currentEventData = data;
            pointerLogText.text += $"OnPointerExit {@object?.name}\r\n";
            logChanged = true;
            exitCount++;
        }

        private void OnPointerDragBegin(PointerEventData data)
        {
            dragging = true;
            currentEventData = data;
            pointerLogText.text += $"OnPointerDragBegin {data.pointerDrag.name}\r\n";
            logChanged = true;
            beginDragCount++;
            endDragCount++;
        }

        private void OnPointerDragEnd(PointerEventData data)
        {
            dragging = false;
            currentEventData = data;
            pointerLogText.text += $"OnPointerDragEnd {data.pointerDrag.name}\r\n";
            logChanged = true;
        }
        private void OnPointerDown(PointerEventData data)
        {
            currentEventData = data;
            pointerLogText.text += $"OnPointerDown {data.pointerCurrentRaycast.gameObject?.name}\r\n";
            logChanged = true;
            downCount++;
        }

        private void OnPointerUp(PointerEventData data)
        {
            currentEventData = data;
            pointerLogText.text += $"OnPointerUp {data.pointerCurrentRaycast.gameObject?.name}\r\n";
            logChanged = true;
            upCount++;
        }

        private void OnPointerClick(PointerEventData data)
        {
            currentEventData = data;
            pointerLogText.text += $"OnPointerClick {data.pointerPress?.name}\r\n\r\n";
            logChanged = true;
        }

        private void Update()
        {
            if (logChanged)
            {
                logCount++;
                if (logCount > maxLogCount)
                {
                    logCount = 0;
                    pointerLogText.text = "";
                }
                logChanged = false;
            }
            IRayPose rayPose = InputModuleManager.Instance.GetActiveEventInput()?.GetRayPose();
            inputLogText.text = $"InteractorType:{InputModuleManager.Instance.GetActiveModule().moduleType}\r\nEventSystem.pixelDragThreshold:{EventSystem.current.pixelDragThreshold}\r\nInteractor.DragThreshold:{InputModuleManager.Instance.GetActiveEventInput()?.GetRayCaster()?.dragThreshold} \r\nTouch.FingerOperation:{RKTouchInput.Instance.FingerOperation} \r\nTouch.count:{RKTouchInput.Instance.GetInsideTouchCount()}\r\nTouch.position:{RKTouchInput.Instance.GetInsideTouch(0).position}\r\nRayPose:{rayPose?.RayPose.position},{rayPose?.RayPose.rotation.eulerAngles}\r\nRayPoseUpdateType:{rayPose?.GetPoseUpdateType()} \r\nDown.count:{downCount},Up.count:{upCount}\r\nEnter.count:{enterCount},Exit.count:{exitCount}\r\nBeginDrag.count:{beginDragCount},EndDrag.count:{endDragCount}\r\n{EventSystem.current.currentInputModule}\r\n";
            inputLogText.text += currentEventData.ToString();


            if (RKTouchInput.Instance.FingerOperation == FingerOperation.ThreeFinger)
            {
                if (RKTouchInput.Instance.TryGetSmoothInsideTouchDeltaPosition(0, out Vector2 delta))
                {
                    inputScrollRect.verticalNormalizedPosition -= delta.y * 0.001f;
                }
            }

            if (TouchPadEventInput.Instance.GetLightLongPress() || Input.GetKeyDown(KeyCode.Space) && !dragging)
            {
                pointerLogText.text = "";
                enterCount = exitCount = downCount = upCount = 0;
            }
#if UNITY_EDITOR
            inputScrollRect.verticalNormalizedPosition += Input.mouseScrollDelta.y * 0.005f;
#endif
            #region  KeyCodeLog
            foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    pointerLogText.text += $"UnityKeyCode Down:{kcode}\r\n";
                    logCount++;
                    logChanged = true;
                }

                if (Input.GetKeyUp(kcode))
                {
                    pointerLogText.text += $"UnityKeyCode Up:{kcode}\r\n";
                    logCount++;
                    logChanged = true;
                }
            }

            foreach (RKStation2KeyEvent kcode in System.Enum.GetValues(typeof(RKStation2KeyEvent)))
            {
                if (RKNativeInput.Instance.GetStation2EventTrigger(kcode))
                {
                    pointerLogText.text += $"Station2 Key Code Trigger:{kcode}\r\n";
                    logCount++;
                    logChanged = true;
                }
            }

            foreach (RKKeyEvent kcode in System.Enum.GetValues(typeof(RKKeyEvent)))
            {
                if (RKNativeInput.Instance.GetKeyDown(kcode))
                {
                    pointerLogText.text += $"RKKey Code Down:{kcode}\r\n";
                    logCount++;
                    logChanged = true;
                }

                if (RKNativeInput.Instance.GetKeyUp(kcode))
                {
                    pointerLogText.text += $"RKKey Code Up:{kcode}\r\n";
                    logCount++;
                    logChanged = true;
                }
            }
            #endregion

            pointerScrollRect.verticalNormalizedPosition = 0;
        }
    }
}
