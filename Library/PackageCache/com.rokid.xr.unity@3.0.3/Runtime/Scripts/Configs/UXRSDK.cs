using UnityEngine;
using Rokid.UXR.Module;
using Rokid.UXR.SubModule;
using Rokid.UXR.Native;

namespace Rokid.UXR.Config
{
    public class UXRSDK
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Load()
        {
            InitModule();
            InitSetting();
            InitFragment();
        }

        private static void InitFragment()
        {
            RKVirtualController.Instance.Change(ControllerType.NORMAL);
        }

        private static void InitSetting()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            RKLog.SetLogEnable(UXRSDKConfig.Instance.LogActive);
#if USE_ROKID_OPENXR 
            NativeInterface.NativeAPI.SetPersistValue("rokid.sdk.version", "openXR");
#else 
            NativeInterface.NativeAPI.SetPersistValue("rokid.sdk.version", "rokidXR");
#endif
        }

        private static void InitModule()
        {
#if !UNITY_EDITOR
            InitGlobalFPS();
            InitTracePoseListener();
#endif
            InitMRC();
        }

        private static void InitMRC()
        {
            if (UXRSDKConfig.Instance.MRCActive)
            {
                new GameObject("MRCLoader").AddComponent<MRCLoader>();
            }
        }

        private static void InitGlobalFPS()
        {
            if (RKGlobalFpsModule.Instance.IsEnableAPM())
            {
                RKGlobalFpsModule.Instance.SetActive(true);
            }
            else
            {
                RKGlobalFpsModule.Instance.DestroyInstance();
            }
        }

        private static void InitTracePoseListener()
        {
            RKTracePoseListener.Instance.Initialize();
        }
    }
}

