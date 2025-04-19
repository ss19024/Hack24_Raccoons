using UnityEngine;

namespace Rokid.UXR.Utility
{
    /// <summary>
    /// 全局常量配置
    /// </summary>
    public class MainCameraCache
    {
        private static Camera cameraCache;
        public static Camera mainCamera
        {
            get
            {
                if (cameraCache == null)
                {
                    cameraCache = Camera.main;
                }
                if (cameraCache == null)
                {
                    RKLog.Warning("Please Create a Main Camera!!!");
                }
                return cameraCache;
            }
        }

        /// <summary>
        /// Manually update the cached main camera 
        /// </summary>
        public static void UpdateCachedMainCamera(Camera camera)
        {
            if (camera != cameraCache)
            {
                GameObject.Destroy(cameraCache);
                cameraCache = camera;
            }
        }
    }
}
