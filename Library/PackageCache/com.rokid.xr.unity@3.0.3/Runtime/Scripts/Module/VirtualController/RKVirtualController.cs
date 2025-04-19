using UnityEngine;

namespace Rokid.UXR.Module
{
    public class RKVirtualController : MonoSingleton<RKVirtualController>
    {
        public ControllerType currentControllerType = ControllerType.NONE;

        protected override void OnSingletonInit()
        {
            this.gameObject.name = "RKVirtualController";
            DontDestroyOnLoad(this.gameObject);
            this.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        public void Change(ControllerType type)
        {
#if !USE_ROKID_OPENXR
            // RKLog.Error("change fragment type");
            if (SystemInfo.deviceModel.Contains("station"))
            {
                type = ControllerType.Mouse;
            }
            if (currentControllerType == type || type == ControllerType.NONE)
                return;
            currentControllerType = type;
            CallBridge.CallAndroid(Request.Build()
            .Name("VirtualController.registerFrag")
            .Param("type", (int)type));
            RKLog.KeyInfo($"==== RKVirtualController ==== Change Type {type} {SystemInfo.deviceModel}");
            RegisterListener();
#endif
        }

        public int GetPhoneScreenHeight()
        {
            return CallBridge.CovertInt(CallBridge.CallAndroid(Request.Build().Name("VirtualController.getScreenHeight")));
        }

        public int GetPhoneScreenWidth()
        {
            return CallBridge.CovertInt(CallBridge.CallAndroid(Request.Build().Name("VirtualController.getScreenWidth")));
        }


        //Use RKInput instead of UnityEngine.Input for KeyEvent(Key ABXY) and Axis event, true as default.
        public void UseCustomGamePadEvent(bool isHook)
        {
            CallBridge.CallAndroid(Request.Build()
            .Name("VirtualController.setHookGamePad")
            .Param("isHook", isHook));
        }

        private static void RegisterListener()
        {
            //regist key event listener
            CallBridge.CallAndroid(
                Request.Build()
                .Name("VirtualController.setOnKeyListener")
                .AndroidCallback(CallBridge.CreateCallback("RKInput", "OnKeyEvent")));
            //regist key down event listener
            CallBridge.CallAndroid(
                Request.Build()
                .Name("VirtualController.setOnKeyDownListener")
                .AndroidCallback(CallBridge.CreateCallback("RKInput", "OnKeyDownEvent"))
                );
            //regist key up event listener
            CallBridge.CallAndroid(
                Request.Build()
                .Name("VirtualController.setOnKeyUpListener")
                .AndroidCallback(CallBridge.CreateCallback("RKInput", "OnKeyUpEvent"))
                );
            CallBridge.CallAndroid(
                Request.Build()
                .Name("VirtualController.setOnAxisListener")
                .AndroidCallback(CallBridge.CreateCallback("RKInput", "OnAxisEvent"))
                );
            //regist station2 key event listener
            CallBridge.CallAndroid(
                Request.Build()
                .Name("VirtualController.setOnS2EventListener")
                .AndroidCallback(CallBridge.CreateCallback("RKInput", "OnStation2KeyEvent")));
        }

        public void DestroyVirtualController()
        {
            CallBridge.CallAndroid(
            Request.Build()
            .Name("VirtualController.dismiss"));
        }

        public void LoadWebView(string url)
        {
            if (currentControllerType == ControllerType.WEBVIEW)
            {
                CallBridge.CallAndroid(Request.Build().Name("VirtualController.loadWebViewUrl").Param("url", url));
            }
            else
            {
                RKLog.Info("RKVirtualController Please to set ControllerType to WebView");
            }
        }

        public void AutoLoadWebView(string url)
        {
            Change(ControllerType.WEBVIEW);
            LoadWebView(url);
        }

        /// <summary>   
        /// 配置界面按钮的显示和隐藏
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="menuBtn1"></param>
        /// <param name="menuBtn2"></param>
        /// <param name="menuBtn3"></param>
        public void ConfigMenuView(bool menu, bool menuBtn1, bool menuBtn2, bool menuBtn3)
        {
            CallBridge.CallAndroid(Request.Build()
                .Name("VirtualController.showMenuView")
                .Param("menu", menu)
                .Param("menuBtn1", menuBtn1)
                .Param("menuBtn2", menuBtn2)
                .Param("menuBtn3", menuBtn3));
        }
    }

    public enum ControllerType
    {
        NONE = -1,
        NORMAL = 0,
        GAMEPAD = 1,
        IMUCTL = 2,
        PHONE3DOF = 3,
        WEBVIEW = 4,
        Mouse = 5,
        PHONE3DOF2 = 6
    }
}