
using System;
using System.Linq;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Module
{
    public enum DeviceType
    {
        UnKnow,
        PHONE,
        StationPro,
        Station2
    }
    public class UnityPlayerAPI : MonoSingleton<UnityPlayerAPI>
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        private ScreenOrientation systemOrientation = ScreenOrientation.Portrait;
        private ScreenOrientation unityOrientation = ScreenOrientation.Portrait;
        public static event Action<ScreenOrientation> OnUnityScreenOrientation;
        private int phoneScreenHeight = 0, phoneScreenWidth = 0;
        public int PhoneScreenHeight
        {
            get
            {
                if (phoneScreenHeight == 0)
                {
                    phoneScreenHeight = GetPhoneScreenHeight();
                }
                return phoneScreenHeight;
            }
        }

        public int PhoneScreenWidth
        {
            get
            {
                if (phoneScreenWidth == 0)
                {
                    phoneScreenWidth = GetPhoneScreenWidth();
                }
                return phoneScreenWidth;
            }
        }

        private int glassScreenHeight = 0, glassScreenWidth = 0;
        public int GlassScreenHeight
        {
            get
            {
                if (glassScreenHeight == 0)
                {
                    glassScreenHeight = GetGlassScreenHeight();
                }
                return glassScreenHeight;
            }
        }

        public int GlassScreenWidth
        {
            get
            {
                if (glassScreenWidth == 0)
                {
                    glassScreenWidth = GetGlassScreenWidth();
                }
                return glassScreenWidth;
            }
        }

        public DeviceType DeviceType { get; private set; }

        protected override void OnSingletonInit()
        {
            base.OnSingletonInit();
            string deviceModel = SystemInfo.deviceModel;
            if (deviceModel.Contains("station2") || deviceModel.Contains("SM-W2022"))
            {
                DeviceType = DeviceType.Station2;
            }
            else if (deviceModel.Contains("stationPro"))
            {
                DeviceType = DeviceType.StationPro;
            }
            else if (!deviceModel.Contains("station"))
            {
                DeviceType = DeviceType.PHONE;
            }
            else
            {
                DeviceType = DeviceType.UnKnow;
            }
            this.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        public ScreenOrientation GetSystemScreenOrientation()
        {
            return systemOrientation;
        }

        public void SetSystemScreenOrientation(ScreenOrientation orientation)
        {
            systemOrientation = orientation;
            if (Utils.IsAndroidPlatform())
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                switch (orientation)
                {
                    case ScreenOrientation.Portrait:
                        currentActivity.Call("setRequestedOrientation", 1);
                        break;
                    case ScreenOrientation.LandscapeLeft:
                        currentActivity.Call("setRequestedOrientation", 0);
                        break;
                    case ScreenOrientation.PortraitUpsideDown:
                        currentActivity.Call("setRequestedOrientation", 9);
                        break;
                    case ScreenOrientation.LandscapeRight:
                        currentActivity.Call("setRequestedOrientation", 8);
                        break;
                }
            }
        }

        public void SetUnityScreenOrientation(ScreenOrientation orientation)
        {
            unityOrientation = orientation;
            OnUnityScreenOrientation?.Invoke(orientation);
        }

        public ScreenOrientation GetUnityScreenOrientation()
        {
            return unityOrientation;
        }

        private int GetPhoneScreenHeight()
        {
#if USE_ROKID_OPENXR
            return NativeInterface.NativeAPI.GetPhoneScreenHeight();
#else
            return CallBridge.CovertInt(CallBridge.CallAndroid(Request.Build().Name("VirtualController.getScreenHeight")));
#endif
        }

        private int GetPhoneScreenWidth()
        {
#if USE_ROKID_OPENXR
            return NativeInterface.NativeAPI.GetPhoneScreenWidth();
#else
            return CallBridge.CovertInt(CallBridge.CallAndroid(Request.Build().Name("VirtualController.getScreenWidth")));
#endif
        }



        private int GetGlassScreenHeight()
        {
            if (Display.displays.Length >= 2)
                return Display.displays[1].renderingHeight;
            RKLog.Error($"Display count error :{Display.displays.Length}");
            return 0;
        }

        private int GetGlassScreenWidth()
        {
            if (Display.displays.Length >= 2)
                return Display.displays[1].renderingWidth / 2;
            RKLog.Error($"Display count error :{Display.displays.Length}");
            return 0;
        }
    }
}


