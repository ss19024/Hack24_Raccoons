using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Module
{
    public class RKCameraSetting : MonoBehaviour
    {

        [Tooltip("设置日志等级")]
        public RKLog.LogLevel logLevel = RKLog.LogLevel.Info;
        private RKLog.LogLevel preLogLevel = RKLog.LogLevel.Info;

        [Tooltip("是否在编辑器模式激活相机控制")]
        public bool activeCameraCtrlInEditor = true;

        private void Start()
        {
            if (Utils.IsUnityEditor())
            {
                if (activeCameraCtrlInEditor)
                    gameObject.AddComponent<SimpleCameraController>();
            }
            RKLog.SetLogLevel(logLevel);
        }


        void Update()
        {
            if (logLevel != preLogLevel)
            {
                RKLog.SetLogLevel(logLevel);
                preLogLevel = logLevel;
            }
        }
    }
}
