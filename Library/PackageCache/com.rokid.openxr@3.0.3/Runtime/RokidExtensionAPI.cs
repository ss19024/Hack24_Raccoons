using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.Features
{
    public static class RokidExtensionAPI
    {
        /**
         * 重置控制器射线Pose
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_RecenterPhonePose();

        /**
         * param out Returns the camera's focal length in pixels,长度为2,[fx,fy]
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_GetFocalLength(float[] data);

        /**
         * param out Returns the principal pointin pixels,长度为2,[cx,cy]
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_GetPrincipalPoint(float[] data);

        /**
         * param out Returns the image's width and height in pixels,长度为2,[width,height]
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_GetImageDimensions(int[] data);

        /**
         * param out Returns the camera's distortion parameters,长度为5,pinhole:[k1 k2 k3 p1 p2],fisheye:[alpha,k1,k2,k3,k4]
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_GetDistortion(float[] data);

        /**
         * 获取相机历史位姿
         * param timeStamp 历史时间点, 纳秒
         * param out 输出位姿数据, position为位置三维坐标, 长度为3; orientation为四元数表示朝向, 长度为4
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_GetHistoryCameraPhysicsPose(long timeStamp, float[] position, float[] orientation);
        
        /**
         * 获取当前时间戳的相机位姿
         * param out 输出位姿数据, position为位置三维坐标, 长度为3; orientation为四元数表示朝向, 长度为4
         * return 当前时间戳, 纳秒
         */
        [DllImport("rokid_openxr_api")]
        public static extern long RokidOpenXR_API_GetCameraPhysicsPose(float[] position, float[] orientation);

        /**
         * 获取跟踪状态
         * Unknow = 0,
         * UnInit = 1,
         * Detecting = 2,//RESERVED
         * Tracking = 3,
         * Track_Limited = 4,//RESERVED
         * Tracking_Bad = 5,//RESERVED
         * Tracking_Paused = 6,
         * Tracking_Stopped = 7,
         * Tracking_Error = 99
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_GetHeadTrackingStatus();

        /**
         * 开启相机预览，通过callback获取相机数据
         */
        public delegate void OnCameraDataUpdateC(IntPtr ptr, int size, ushort width, ushort height, long timestamp);

        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_OpenCameraPreview(OnCameraDataUpdateC callback);

        /**
         * 关闭相机预览
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_CloseCameraPreview();

        /**
         * 获取跟踪状态
         *  * 获取SLAM置信度
         * @param trackingStatus SLAM跟踪算法的状态（短时的）
         * AR_TS_SUCCESS = 0,
         * AR_TS_BAD = 1,
         * AR_TS_FAIL = 2
         * @param imageQuality SLAM图像质量
         * AR_IQ_GOOD = 0,
         * AR_IQ_WEAK = 1,
         * AR_IQ_DARK = 2,
         * AR_IQ_BRIGHT = 3,
         * @param kineticQuality SLAM运动质量
         * AR_KQ_GOOD = 0,
         * AR_KQ_TOOFAST = 1,
         */
        [DllImport("rokid_openxr_api")]
        public static extern int RokidOpenXR_API_GetSLAMQuality(int[] trackingStatus, int[] imageQuality, int[] kineticQuality);

        [DllImport("rokid_openxr_api")]
        private static extern int getSlamState();

        [DllImport("rokid_openxr_api")]
        public static extern IntPtr getGlassName();

        [DllImport("rokid_openxr_api")]
        public static extern bool isUsbConnect();

        [DllImport("rokid_openxr_api")]
        public static extern int getGlassProductId();

        [DllImport("rokid_openxr_api")]
        public static extern IntPtr getGlassTypeId();

        [DllImport("rokid_openxr_api")]
        public static extern IntPtr getGlassSeed();

        [DllImport("rokid_openxr_api")]
        public static extern IntPtr getGlassSn();

        [DllImport("rokid_openxr_api")]
        public static extern IntPtr getGlassFirmwareVersion();
    }
}