using System.Collections;
using UnityEngine;
using System.Linq;
using Rokid.UXR.Config;
using Rokid.UXR.Native;

namespace Rokid.UXR.Utility
{
    public class CanvasUtils
    {
        public static IEnumerator FitCanvasToCameraFov(Canvas canvas, Camera camera, float distance, float fov, bool followToCamera)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = distance;
            camera.fieldOfView = fov;
            if (followToCamera)
            {
                canvas.transform.SetParent(camera.transform);
            }
            yield return new WaitForSeconds(1.0f);
            canvas.renderMode = RenderMode.WorldSpace;
        }

        /// <summary>
        /// 获取眼镜的显示fov
        /// </summary>
        /// <returns></returns>
        public static float GetGlassViewFov()
        {
            string glassName = NativeInterface.NativeAPI.GetGlassName();
            glassName = glassName.Contains("Air") ? "Air" : "Max";
            DeviceInfo deviceInfo = DeviceInfos.GetInfos().Where(info => info.DeviceName.Contains(glassName)).FirstOrDefault();
            if (deviceInfo != null)
            {
                return deviceInfo.CameraFov;
            }
            return 20.3f;
        }

        public static IEnumerator FitCanvasToCameraFov(Canvas canvas, Camera camera, float distance, bool followToCamere)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = distance;
            camera.fieldOfView = GetGlassViewFov();
            if (followToCamere)
            {
                canvas.transform.SetParent(camera.transform);
            }
            yield return new WaitForSeconds(1.0f);
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = null;
        }
    }
}
