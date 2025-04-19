using UnityEngine;
using System;
using Rokid.UXR.Utility;
using System.Runtime.InteropServices;

namespace Rokid.UXR.Native
{

    public enum HeadTrackingStatus
    {
        Unknown = 0,
        UnInit = 1,
        Detecting = 2,
        Tracking = 3,
        Track_Limited = 4,
        Tracking_Bad = 5,
        Tracking_Paused = 6,
        Tracking_Stopped = 7,
        Tracking_Error = 99
    }

    public enum SlamTrackingStatus
    {
        Success = 0,
        Bad = 1,
        Fail = 2,
        Unknown = 404
    }

    public enum SlamImageQuality
    {
        Good = 0,
        Weak = 1,
        Dark = 2,
        Bright = 3,
        Unknown = 404
    }

    public enum SlamKineticQuality
    {
        Good = 0,
        FootFast = 1,
        Unknown = 404
    }

    public partial class NativeInterface
    {
        public partial class NativeAPI
        {
            static float[] frustum_left = new float[6];
            static float[] frustum_right = new float[6];
            static float[] distortion_quad_left = new float[8];
            static float[] distortion_quad_right = new float[8];
            static float[] position = new float[3];
            static float[] rotation = new float[4];


            /// <summary>
            /// Get head tracking status
            /// </summary>
            /// <returns></returns>
            public static HeadTrackingStatus GetHeadTrackingStatus()
            {
                if (Utils.IsAndroidPlatform())
                    return (HeadTrackingStatus)getSlamState();
                return HeadTrackingStatus.Tracking;
            }


            /// <summary>
            /// Reset center
            /// </summary>
            public static void Recenter()
            {
                if (Utils.IsAndroidPlatform())
                    recenterHeadPose();
            }

            /// <summary>
            /// Get debug information
            /// </summary>
            public static string GetDebugInfo()
            {
                if (Utils.IsAndroidPlatform())
                {
                    IntPtr keyPtr = getDebugInfo();
                    string result = Marshal.PtrToStringAnsi(keyPtr);
                    return result;
                }
                return null;
            }

            /// <summary>
            /// Get left and right eye projection parameters
            /// </summary>
            /// <param name="frustum_left">float[4]  {hFov,vFov,near,far}</param>
            /// <param name="frustum_right">float[4] {hFov,vFov,near,far}</param>
            /// <returns></returns>
            public static void GetUnityFrustum(ref float[] frustum_left, ref float[] frustum_right)
            {
                if (!Utils.IsAndroidPlatform())
                    return;
                if (frustum_left.Length != 4 || frustum_right.Length != 4)
                {
                    RKLog.Error("====NativeAnd====:+ GetUnityFrustum 参数数组长度不正确");
                    return;
                }
                if (GetFrustum())
                {
                    frustum_left[0] = GetHFov(false);
                    frustum_left[1] = GetVFov(false);
                    frustum_left[2] = NativeAPI.frustum_left[4];
                    frustum_left[3] = NativeAPI.frustum_left[5];

                    frustum_right[0] = GetHFov(true);
                    frustum_right[1] = GetVFov(true);
                    frustum_right[2] = NativeAPI.frustum_right[4];
                    frustum_right[3] = NativeAPI.frustum_right[5];
                }
            }

            /// <summary>
            /// Get left and right eye fov parameters
            /// </summary>
            /// <returns></returns>
            public static void GetUnityEyeFrustumHalf(bool isRight, ref float[] fov)
            {
                if (!Utils.IsAndroidPlatform())
                    return;
                if (GetFrustum())
                {
                    RKLog.KeyInfo($"====GetUnityEyeFrustumHalf====: left fov {Newtonsoft.Json.JsonConvert.SerializeObject(frustum_left)},right fov {Newtonsoft.Json.JsonConvert.SerializeObject(frustum_right)} ");
                    float[] frustum = isRight ? frustum_right : frustum_left;
                    float left = frustum[0];
                    float right = frustum[1];
                    float bottom = frustum[2];
                    float top = frustum[3];
                    float near = frustum[4];
                    float RAD2DEG = 180.0f / 3.14159265358979323846f;
                    fov[0] = RAD2DEG * (float)Math.Atan(Math.Abs(left) / near);
                    fov[1] = RAD2DEG * (float)Math.Atan(Math.Abs(right) / near);
                    fov[2] = RAD2DEG * (float)Math.Atan(Math.Abs(top) / near);
                    fov[3] = RAD2DEG * (float)Math.Atan(Math.Abs(bottom) / near);
                }
            }

            /// <summary>
            /// Get left and right eye projection parameters
            /// </summary>
            /// <param name="frustum_lest">float[6] {left,right,bottom,top,near,far}</param>
            /// <param name="frustum_right">{left,right,bottom,top,near,far}</param>
            /// <returns></returns>
            static bool GetFrustum()
            {
                if (!Utils.IsAndroidPlatform())
                    return false;
                return get_frustum(frustum_left, frustum_right);
            }

            public static float[] GetFrustum(bool useLeftEyeFov)
            {
                if (Utils.IsAndroidPlatform())
                {
                    if (get_frustum(frustum_left, frustum_right))
                    {
                        return useLeftEyeFov ? frustum_left : frustum_right;
                    }
                }
                return null;
            }

            public static float[] GetDistortionQuad(bool useLeftEyeFov)
            {
                if (Utils.IsAndroidPlatform())
                {
                    if (get_distortion_quad(distortion_quad_left, distortion_quad_right))
                    {
                        return useLeftEyeFov ? distortion_quad_left : distortion_quad_right;
                    }
                    else
                    {
                        RKLog.KeyInfo("distortion_quad: error!!!");
                    }
                }
                return null;
            }

            /// <summary>
            /// Get vertical field of view
            /// </summary>
            /// <param name="frustum"></param>
            /// <returns></returns>
            public static float GetVFov(bool isRight)
            {
                if (!Utils.IsAndroidPlatform())
                    return 0;
                float[] frustum = isRight ? frustum_right : frustum_left;
                float near = frustum[4];
                float top = frustum[3];
                float bottom = frustum[2];
                float RAD2DEG = 180.0f / 3.14159265358979323846f;
                float fov = RAD2DEG * (2.0f * (float)Math.Atan((top - bottom) / (2.0f * near)));
                return fov;
            }

            /// <summary>
            /// Get horizontal field of view
            /// </summary>
            /// <param name="frustum"></param>
            /// <returns></returns>
            public static float GetHFov(bool isRight)
            {
                if (!Utils.IsAndroidPlatform())
                    return 0;
                float[] frustum = isRight ? frustum_right : frustum_left;
                float near = frustum[4];
                float left = frustum[0];
                float right = frustum[1];
                float RAD2DEG = 180.0f / 3.14159265358979323846f;
                float fov = RAD2DEG * (2.0f * (float)Math.Atan((right - left) / (2.0f * near)));
                return fov;
            }

            /// <summary>
            /// Get current time head pose
            /// </summary>
            /// <returns></returns>
            public static Pose GetHeadPose(out long timeStamp)
            {
                if (Utils.IsAndroidPlatform())
                {
                    timeStamp = getHeadPoseRHS(position, rotation);
                    Pose pose = new Pose(new Vector3(position[0], position[1], -position[2]),
                        new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]));
                    return pose;
                }
                timeStamp = 0;
                return Pose.identity;
            }

            /// <summary>
            /// Input a timestamp to retrieve the head pose at that specific timestamp only for ges.
            /// </summary>
            /// <param name="timestamp"></param> (t-1000ms ~ t-10ms)
            /// <returns></returns>
            public static Pose GetHistoryHeadPose(long timestamp)
            {
                Pose pose = Pose.identity;
                if (Utils.IsAndroidPlatform())
                {
                    getHistoryHeadPoseRHS(timestamp, position, rotation);
                    pose.position = new Vector3(position[0], position[1], -position[2]);
                    pose.rotation = new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]);
                }
                return pose;
            }


            /// <summary>
            ///  It is current physics camera pose
            /// </summary>
            /// <returns></returns>
            public static Pose GetCameraPhysicsPose(out long timeStamp)
            {
                Pose pose = Pose.identity;
                if (Utils.IsAndroidPlatform())
                {
                    timeStamp = getHeadPosePysRHS(position, rotation);
                    pose.position = new Vector3(position[0], position[1], -position[2]);
                    pose.rotation = new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]);
                    return pose;
                }
                timeStamp = 0;
                return pose;
            }

            /// <summary>
            ///  It is history physics camera pose
            /// </summary>
            /// <param name="timestamp"></param>
            /// <returns></returns>
            public static Pose GetHistoryCameraPhysicsPose(long timestamp)
            {
                Pose pose = Pose.identity;
                if (Utils.IsAndroidPlatform())
                {
                    getHistoryHeadPosePysRHS(timestamp, position, rotation);
                    pose.position = new Vector3(position[0], position[1], -position[2]);
                    pose.rotation = new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]);
                }
                return pose;
            }

            /// <summary>
            /// SetTrackingType
            /// </summary>
            /// <param name="type"></param>
            public static void SetTrackingType(int type)
            {
                if (Utils.IsAndroidPlatform())
                {
                    RKLog.Info($"====NativeAPI====: SetHeadTrackingType :{type}");
                    setHeadTrackingType(type);
                }
            }

            /// <summary>
            /// Get ost info
            /// </summary>
            /// <returns></returns>
            public static string GetOSTInfo()
            {
                IntPtr calPtr = getGlassCalFile();
                if (calPtr != null)
                {
                    string cal = Marshal.PtrToStringAuto(calPtr);
                    return cal;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// [type:0-SLAM_CONFIG_COORDINATE
            ///  value:0-PiEngine坐标系,1-OpenGL坐标系]
            /// [type:1-SLAM_CONFIG_3DOF_FOLLOW_SLAM
            ///  value:0-纯3dof,1-跟随slam 3dof]
            /// </summary>
            /// <param name="configParam"></param>
            internal static void ConfigSlamParam(int type, int value)
            {
                if (Utils.IsAndroidPlatform())
                {
                    configSlamParam(type, value);
                }
            }

            /// <summary>
            /// Get Slam Quality
            /// </summary>
            /// <param name="trackingStatus"></param>
            /// <param name="imageQuality"></param>
            /// <param name="kineticQuality"></param>
            public static void GetSLAMQuality(out SlamTrackingStatus trackingStatus, out SlamImageQuality imageQuality, out SlamKineticQuality kineticQuality)
            {
                if (Utils.IsAndroidPlatform())
                {
                    int[] t = new int[] { 0 }, i = new int[] { 0 }, k = new int[] { 0 };
                    if (getSLAMQuality(t, i, k) == 0)
                    {
                        trackingStatus = (SlamTrackingStatus)t[0];
                        imageQuality = (SlamImageQuality)i[0];
                        kineticQuality = (SlamKineticQuality)k[0];
                    }
                    else
                    {
                        trackingStatus = SlamTrackingStatus.Unknown;
                        imageQuality = SlamImageQuality.Unknown;
                        kineticQuality = SlamKineticQuality.Unknown;
                    }
                }
                else
                {
                    trackingStatus = SlamTrackingStatus.Unknown;
                    imageQuality = SlamImageQuality.Unknown;
                    kineticQuality = SlamKineticQuality.Unknown;
                }
            }

            /// <summary>
            /// Reset Slam
            /// </summary>
            public static void ResetSlam()
            {
                if (Utils.IsAndroidPlatform())
                {
                    resetSlam();
                }
            }

            /// <summary>
            /// Reset Slam
            /// </summary>
            public static void ResetHead3DofCenter()
            {
                if (Utils.IsAndroidPlatform())
                {
                    resetHead3DofCenter();
                }
            }

            internal static void RotationLock(int type)
            {
                if (Utils.IsAndroidPlatform())
                {
                    RKLog.KeyInfo("RotationLock type:" + type);
                    headTrackingFix(type);
                }
            }

            internal static int Set3DofAlpha(float alpha)
            {
                if (Utils.IsAndroidPlatform())
                {
                    return set3DofAlpha(alpha);
                }
                return 0;
            }

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern long getHeadPoseRHS(float[] position, float[] orientation);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getHistoryHeadPoseRHS(long timestamp, float[] position, float[] orientation);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern long getHeadPosePysRHS(float[] position, float[] orientation);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void configSlamParam(int type, int value);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getSLAMQuality(int[] trackingStatus, int[] imageQuality, int[] kineticQuality);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getSlamState();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void recenterHeadPose();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern IntPtr getDebugInfo();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern bool get_frustum(float[] frustum_left, float[] frustum_right);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern bool get_distortion_quad(float[] quad_left, float[] quad_right);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void setHeadTrackingType(int type);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern IntPtr getGlassCalFile();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void resetSlam();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void resetHead3DofCenter();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void headTrackingFix(int type);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int set3DofAlpha(float alpha);
        }
    }
}
