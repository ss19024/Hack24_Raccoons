using UnityEngine;

namespace Rokid.UXR.Config
{
    [System.Serializable]
    public class UXRSDKConfig : ScriptableObject
    {

        #region 配置
        private static UXRSDKConfig _instance;
        private static void LoadInstance()
        {
            _instance = Resources.Load<UXRSDKConfig>("UXRSDKConfig");
            if (_instance == null)
            {
                RKLog.Error("Not Find SDK Config, Will Use Default App Config.");
            }
        }
        public static UXRSDKConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadInstance();
                }
                return _instance;
            }
        }



        #endregion

        #region 配置属性
        public bool LogActive = false;
        public bool MRCActive = true;
        public LayerMask MRCCameraRenderLayer;
        public RenderingPath MRCCameraRenderingPath;

        #endregion
    }
}
