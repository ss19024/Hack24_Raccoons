using System;
using System.Runtime.InteropServices;
using AOT;
using Rokid.UXR.Module;
using UnityEngine;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Native
{
    public partial class NativeInterface
    {
        /// <summary>
        /// The update of camera preview data.
        /// width, height, result, timestamp
        /// </summary>
        public static partial class NativeAPI
        {

            /// <summary>
            /// The update of camera preview data.
            /// width, height, result, timestamp
            /// </summary>
            public static event Action<int, int, byte[], long> OnCameraDataUpdate;

            /// <summary>
            /// Start camera preview
            /// </summary>
            public static void StartCameraPreview()
            {
                if (Utils.IsAndroidPlatform())
                    startCameraPreview();
            }

            /// <summary>
            /// Sets the type of camera preview data type
            /// </summary>
            /// <param name="dataType">1-ARGB,2-NV21 3-RGBA32</param>
            public static void SetCameraPreviewDataType(int dataType)
            {
                if (Utils.IsAndroidPlatform())
                    setOnCameraDataUpdate(OnCameraDataUpdateCallByC, dataType);
            }

            /// <summary>
            /// Stop camera preview
            /// </summary>
            public static void StopCameraPreview()
            {
                if (Utils.IsAndroidPlatform())
                    stopCameraPreview();
            }

            /// <summary>
            /// Clean up camera data updates
            /// </summary>
            public static void ClearCameraDataUpdate()
            {
                if (Utils.IsAndroidPlatform())
                    clearOnCameraDataUpdate();
            }

            /// <summary>
            /// Whether the glasses support the camera
            /// </summary>
            /// <returns></returns>
            public static bool IsSupportCamera()
            {
                return FuncDeviceCheck.CheckCameraFunc();
            }

            /// <summary>
            /// Preview or not
            /// </summary>
            /// <returns></returns>
            public static bool IsPreviewing()
            {
                if (Application.platform == RuntimePlatform.Android)
                    return isPreviewing();
                return false;
            }

            /// <summary>
            /// Get preview width
            /// </summary>
            /// <returns></returns>
            public static int GetPreviewWidth()
            {
                if (Utils.IsAndroidPlatform())
                {
                    int[] dimen = new int[2];
                    getPreviewDimen(dimen);
                    return dimen[0];
                }
                return 0;
            }

            /// <summary>
            /// Get preview height
            /// </summary>
            /// <returns></returns>
            public static int GetPreviewHeight()
            {
                if (Utils.IsAndroidPlatform())
                {
                    int[] dimen = new int[2];
                    getPreviewDimen(dimen);
                    return dimen[1];
                }
                return 0;
            }

            /// <summary>
            /// Get fx,fy
            /// </summary>
            /// <param name="data"></param>
            public static void GetFocalLength(float[] data)
            {
                if (Utils.IsAndroidPlatform())
                    getFocalLength(data);
            }

            /// <summary>
            /// Get cx,cy
            /// </summary>
            /// <param name="data"></param>
            public static void GetPrincipalPoint(float[] data)
            {
                if (Utils.IsAndroidPlatform())
                    getPrincipalPoint(data);
            }

            /// <summary>
            /// Get width,height
            /// </summary>
            /// <param name="data"></param>
            public static void GetImageDimensions(int[] data)
            {
                if (Utils.IsAndroidPlatform())
                    getImageDimensions(data);
            }

            /// <summary>
            /// pinhole:k1,k2,k3,p1,p2
            /// fisheye:alpha,k1,k2,k3,k4;
            /// </summary>
            /// <param name="data"></param>
            public static void GetDistortion(float[] data)
            {
                if (Utils.IsAndroidPlatform())
                    getDistortion(data);
            }

            /// <summary>
            /// Get the YPR angles of the camera, 
            /// represented as an array of length 3 in the order [yaw, pitch, roll].
            /// </summary>
            /// <param name="data"></param>
            public static void GetCameraYPR(float[] data)
            {
                if (Utils.IsAndroidPlatform())
                    getCameraYPR(data);
            }

            #region NativeInterface
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void startCameraPreview();
            delegate void OnCameraDataUpdateC(IntPtr ptr, int size, ushort width, ushort height, long timestamp);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void setOnCameraDataUpdate(OnCameraDataUpdateC cb, int type);
            [MonoPInvokeCallback(typeof(OnCameraDataUpdateC))]
            static void OnCameraDataUpdateCallByC(IntPtr ptr, int size, ushort width, ushort height, long timestamp)
            {
                byte[] result = new byte[size];
                Marshal.Copy(ptr, result, 0, size);
                OnCameraDataUpdate?.Invoke(width, height, result, timestamp);
            }
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void stopCameraPreview();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void clearOnCameraDataUpdate();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getFocalLength(float[] data);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getPrincipalPoint(float[] data);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getImageDimensions(int[] data);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getDistortion(float[] data);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getCameraYPR(float[] data);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern bool isPreviewing();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getPreviewDimen(int[] data);

            #endregion
        }
    }
}

