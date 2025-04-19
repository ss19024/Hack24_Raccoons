using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using Rokid.UXR.UI;
using Rokid.UXR.Interaction;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rokid.UXR.Module
{
    public class DeviceEventHandler : MonoBehaviour
    {
        /// <summary>
        /// 当usb断开的时候是否自动退出应用
        /// </summary>
        [SerializeField]
        private bool quitWhenUsbDisconnect = true;

        /// <summary>
        /// 触发系统返回键的时候退出
        /// </summary>
        [SerializeField]
        private bool responseToEscape = true;
        private float deltaTime;
        private bool waitExit;
        public static event Action OnSystemEnvCheckFailed;
        private void Start()
        {
            RKLog.Debug("====DeviceEventHandler==== Initd Usb Devices!!! ");
            NativeInterface.NativeAPI.RegisterUSBStatusCallback();
            NativeInterface.NativeAPI.OnUSBConnect += OnUSBConnect;
            NativeInterface.NativeAPI.OnUSBDisConnect += OnUSBDisConnect;
            if (!NativeInterface.NativeAPI.SystemEnvCheck())
            {
                if (OnSystemEnvCheckFailed != null)
                {
                    OnSystemEnvCheckFailed?.Invoke();
                }
                else
                {
                    string msg = Utils.IsChineseLanguage() ? "当前系统版本低,请升级到最新版本" : "The current system version is low, please upgrade to the latest version.";
                    RKLog.Error($"====DeviceEventHandler==== {msg} ");
                    UIManager.Instance.CreatePanel<TipPanel>(true).Init(msg, TipLevel.Error, 5, () =>
                    {
                        Quit();
                    });
                }
            }
        }
        private void OnDestroy()
        {
            NativeInterface.NativeAPI.UnRegisterUSBStatusCallback();
            NativeInterface.NativeAPI.OnUSBConnect -= OnUSBConnect;
            NativeInterface.NativeAPI.OnUSBDisConnect -= OnUSBDisConnect;
        }

        private void OnUSBConnect()
        {
            RKLog.Debug("====UsbEventHandler==== USBConnect !!!");
        }


        private void OnUSBDisConnect()
        {
            RKLog.Debug("====UsbEventHandler==== USB Disconnect !!!");
            if (quitWhenUsbDisconnect)
                Quit();
        }

        public void Quit()
        {
            RKLog.Debug("====DeviceEventHandler==== Quit");
#if !UNITY_EDITOR
            Application.Quit();
#else
            EditorApplication.isPlaying = false;
#endif
        }


        private void Update()
        {
            if (responseToEscape)
            {
                bool trigger = Input.GetKeyDown(KeyCode.Escape) || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_BACK) || Input.GetKeyDown(KeyCode.JoystickButton2) || RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_LIGHT_SINGLE_TAP) || (!InputModuleManager.Instance.GetTouchPadActive() && (RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_SWIPE_FROM_LEFT_TAP) || RKNativeInput.Instance.GetStation2EventTrigger(RKStation2KeyEvent.KEY_SWIPE_FROM_RIGHT_TAP)));
                if (waitExit)
                {
                    deltaTime += Time.deltaTime;
                    if (trigger)
                    {
                        Quit();
                    }
                    if (deltaTime > 3.0f)
                    {
                        deltaTime = 0;
                        waitExit = false;
                    }
                }
                else if (trigger)
                {
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        waitExit = true;
                        string msg = Utils.IsChineseLanguage() ? "再次点击将退出应用" :
                         "Clicking again will exit the app.";
                        UIManager.Instance.CreatePanel<TipPanel>(true).Init(msg, TipLevel.Warning, 3);
                    }
                    else
                    {
                        SceneManager.LoadScene(0);
                    }
                }
            }
        }
    }
}