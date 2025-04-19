using System;
using System.Runtime.InteropServices;
using AOT;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Native
{
    public partial class NativeInterface
    {

        public partial class NativeAPI
        {
            /// <summary>
            ///   0 成功
            ///  -1 手势未开启
            ///  -2 socket错误
            ///  -3 PiEngine未开启
            ///  -4 超时
            /// <summary>
            public static event Action<int> OnGesCalibStateChange;
            /// <summary>
            /// Init gesture
            /// </summary>
            public static void InitGesture()
            {
                if (Utils.IsAndroidPlatform())
                    openGestureTracker();
            }

            /// <summary>
            /// Release gesture
            /// </summary>
            public static void ReleaseGesture()
            {
                if (Utils.IsAndroidPlatform())
                    closeGestureTracker();
            }


            /// <summary>
            /// Retrieve the number of tracked hand vertices.
            /// </summary>
            /// <returns></returns>
            [Obsolete("This Interface Is Obsolete", true)]
            public static int GetTrackingHandVertsNum()
            {
                if (Utils.IsAndroidPlatform())
                    return getTrackingHandVertsNum();
                return 0;
            }

            /// <summary>
            /// Retrieve the number of tracked skeletons.
            /// </summary>
            /// <returns></returns>
            public static int GetTrackingHandSkeletonNum()
            {
                if (Utils.IsAndroidPlatform())
                    return getTrackingHandSkeletonNum();
                return 0;
            }

            /// <summary>
            /// Retrieve the number of tracked hands.
            /// </summary>
            /// <returns></returns>
            public static int GetTrackingHandNum()
            {
                if (Utils.IsAndroidPlatform())
                    return getTrackingHandNum();
                return 0;
            }

            /// <summary>
            /// Retrieve the type of tracked hands
            /// </summary>
            /// <param name="index">hand index</param>
            /// <returns> 0:lefthand 1:righthand</returns>
            public static int GetTrackingHandLrHand(int index)
            {
                if (Utils.IsAndroidPlatform())
                    return getTrackingHandLrHand(index);
                return 0;
            }

            /// <summary>
            /// Retrieve the type of tracked gestures.
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static int GetTrackingGestureType(int index)
            {
                if (Utils.IsAndroidPlatform())
                    return getTrackingGestureType(index);
                return 0;
            }

            /// <summary>
            /// Retrieve the type of tracked gestures.
            /// When there is a conflict between "grip" and "pinch" in the combine type, prioritize selecting "grip."
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static int GetTrackingHandCombineGestureType(int index)
            {
                if (Utils.IsAndroidPlatform())
                    return getTrackingHandCombineGestureType(index);
                return 0;
            }

            /// <summary>
            /// Retrieve the skeleton information in camera space.
            /// </summary>
            /// <param name="skeletonCAM"></param>
            /// <param name="index"></param>
            public static void GetTrackingHandSkeletonCAM(float[] skeletonCAM, int index)
            {
                if (Utils.IsAndroidPlatform())
                    getTrackingHandSkeletonCAM_GLAxis(skeletonCAM, index, 4);
            }

            /// <summary>
            /// Retrieve the skeleton information in NDC (Normalized Device Coordinates) space.
            /// </summary>
            /// <param name="skeletonNDC"></param>
            /// <param name="index"></param>
            [Obsolete("Use GetTrackingHandSkeletonCAM Instead")]
            public static void GetTrackingHandSkeletonNDC(float[] skeletonNDC, int index)
            {
                if (Utils.IsAndroidPlatform())
                    getTrackingHandSkeletonNDC(skeletonNDC, index);
            }

            /// <summary>
            /// Retrieve the mesh points of the hand in NDC (Normalized Device Coordinates) space.
            /// </summary>
            /// <param name="vertsNDC"></param>
            /// <param name="index"></param>
            [Obsolete("This Interface Is Obsolete", true)]
            public static void GetTrackingHandVertsNDC(float[] vertsNDC, int index)
            {
                if (Utils.IsAndroidPlatform())
                    getTrackingHandVertsNDC(vertsNDC, index);
            }

            /// <summary>
            /// Retrieve the mesh points of the hand in camera space.
            /// </summary>
            /// <param name="vertsCAM"></param>
            /// <param name="index"></param>
            [Obsolete("This Interface Is Obsolete", true)]
            public static void GetTrackingHandVertsCAM(float[] vertsCAM, int index)
            {
                if (Utils.IsAndroidPlatform())
                    getTrackingHandVertsCAM(vertsCAM, index);
            }


            /// <summary>
            /// Retrieve the rotation of the hand as a quaternion.
            /// </summary>
            /// <param name="rotation">quaternion</param>
            /// <param name="index">hand index</param>
            public static void GetTrackingHandRootRotation(float[] rotation, int index)
            {
                if (Utils.IsAndroidPlatform())
                    getTrackingHandRootRotation_GLAxis(rotation, index);
            }

            /// <summary>
            /// Retrieve the type of palm and back of the hand.
            /// </summary>
            /// <param name="index">hand index</param>
            /// <returns></returns>
            public static int GetTrackingHandOrientation(int index)
            {
                if (Utils.IsAndroidPlatform())
                    return getTrackingHandOrientation(index);
                return 0;
            }

            /// <summary>
            /// Retrieve the coordinate axes of the hand in camera space.
            /// </summary>
            /// <param name="axis"></param>
            /// <param name="index"></param>
            public static void GetTrackingHandRootRotationAxisCAM(float[] axis, int index)
            {
                if (Utils.IsAndroidPlatform())
                    getTrackingHandRootRotationAxisCAM_GLAxis(axis, index);
            }


            /// <summary>
            /// Retrieve the hand velocity tracking.
            /// </summary>
            /// <param name="data"></param>
            /// <param name="index"></param>
            // public static void GetTrackingHandVelocity(float[] data, int index)
            // {
            //     if (Utils.IsAndroidPlatform())
            //         getTrackingHandVelocity(data, index);
            // }

            /// <summary>
            /// Retrieve the timestamp of the gesture image moment.
            /// </summary>
            /// <returns></returns>
            public static long GetCurrentFrameTimeStamp()
            {
                if (Utils.IsAndroidPlatform())
                    return getCurrentFrameTimeStamp();
                return 0;
            }

            /// <summary>
            /// Retrieve the timestamp of the completed gesture image processing.
            /// </summary>
            /// <returns></returns>
            public static long GetFinishProcessTimeStamp()
            {
                if (Utils.IsAndroidPlatform())
                    return getFinishProcessTimeStamp();
                return 0;
            }

            /// <summary>
            /// Set the tracking count for hands.
            /// </summary>
            /// <param name="maxHandNum"></param>
            public static void SetMaxHandNum(int maxHandNum)
            {
                if (Utils.IsAndroidPlatform())
                    setMaxHandNum(maxHandNum);
            }

            /// <summary>
            /// Enable DSP
            /// <param name = "useDsp" >
            /// 0 means neither detection nor tracking DSP is enabled
            /// 1 means detection is enabled, but tracking is disabled.
            /// 2 means detection is disabled, but tracking is enabled.
            /// 3 means both detection and tracking are enabled.
            /// </ param >
            /// </summary>
            public static void SetUseDsp(int useDsp)
            {
                if (Utils.IsAndroidPlatform())
                    setUseDsp(useDsp);
            }

            /// <summary>
            /// Enable fisheye distortion correction
            /// </summary>
            /// <param name="useFishEyeDistort">,0-false,1-true</param>
            public static void SetUseFishEyeDistort(int useFishEyeDistort)
            {
                if (Utils.IsAndroidPlatform())
                    setUseFishEyeDistort(useFishEyeDistort);
            }

            /// <summary>
            /// Retrieve skeleton rotation data
            /// </summary>
            /// <param name="data"></param>
            /// <param name="index">hand index</param>
            /// <param name="type">0-matrix(26*9),1-quaternion(26*4),2-euler(26*3</param>
            public static void GetTrackingHandSkeletonRotationAll(float[] data, int index, int type)
            {
                if (Utils.IsAndroidPlatform())
                    getTrackingHandSkeletonRotationAll_GLAxis(data, index, 4, type);
            }

            /// <summary>
            /// Set gesture loglevel
            /// </summary>
            /// <param name="logLevel"> "debug", "info", "warn", "err", "fatal", "none"</param>
            public static void SetGestureLogLevel(string logLevel)
            {
                if (Utils.IsAndroidPlatform())
                {
                    setGestureLogLevel(logLevel);
                }
            }

            /// <summary>
            ///  It is physical camera pose
            /// </summary>
            /// <param name="timestamp"></param>
            /// <returns></returns>
            internal static Pose GetHeadPoseForGes(long timestamp)
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
            /// Get up forward influencePow
            /// </summary>
            /// <returns></returns>

            public static float GetUpForwardInfluencePow()
            {
                if (Utils.IsAndroidPlatform())
                    return getUpForwardInfluencePow();
                return 0;
            }

            /// <summary>
            /// Get pinch distance
            /// </summary>
            /// <returns></returns>
            public static float GetTrackingHandPinchDistance(int index)
            {
                return getTrackingHandPinchDistance(index);
            }

            /// <summary>
            /// Begin gesture calibrate
            /// </summary>
            public static void BeginGestureCalibrate()
            {
                if (Utils.IsAndroidPlatform())
                {
                    gestureCalibrate(OnGesCalibUpdateCallByC);
                }
            }

            /// <summary>
            /// Stop gesture calibrate
            /// </summary>
            public static void StopGestureCalibrate()
            {
                if (Utils.IsAndroidPlatform())
                {
                    stopGestureCalibrate();
                }
            }

            /// <summary>
            /// Reset gesture calibrate
            /// </summary>
            public static void ResetGestureCalibrate()
            {
                if (Utils.IsAndroidPlatform())
                {
                    resetGestureCalibrate();
                }
            }


            #region NativeInterface

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void openGestureTracker();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void closeGestureTracker();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getTrackingHandVertsNum();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getTrackingHandSkeletonNum();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getTrackingHandNum();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getTrackingHandLrHand(int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getTrackingGestureType(int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getTrackingHandCombineGestureType(int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandSkeletonCAM(float[] skeletonCAM, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandVertsNDC(float[] vertsNDC, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandVertsCAM(float[] vertsCAM, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandSkeletonNDC(float[] skeletonNDC, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandRootRotation(float[] rotation, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getTrackingHandOrientation(int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandRootRotationAxisCAM(float[] axis, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN), Obsolete]
            static extern void getTrackingHandStableAnchorPoint(float[] stableAnchorPoint, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandSkeletonRotationAll(float[] data, int index, int type);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandVelocity(float[] data, int index);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern long getCurrentFrameTimeStamp();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern long getFinishProcessTimeStamp();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void setMaxHandNum(int maxHandNum);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void setUseDsp(int useDsp);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void setUseFishEyeDistort(int useFishEyeDistort);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, CharSet = CharSet.Ansi)]
            static extern void setGestureLogLevel(string logLevel);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getHistoryHeadPosePysRHS(long timestamp, float[] position, float[] orientation);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern float getUpForwardInfluencePow();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern float getTrackingHandPinchDistance(int index);

            #region OPEN_GL_INTERFACE 
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "getGestureSkeletonPosition")]
            static extern void getTrackingHandSkeletonCAM_GLAxis(float[] data, int index, int mode);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandRootRotation_GLAxis(float[] data, int index);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void getTrackingHandRootRotationAxisCAM_GLAxis(float[] data, int index);
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "getGestureSkeletonRotation")]
            static extern void getTrackingHandSkeletonRotationAll_GLAxis(float[] data, int index, int mode, int type);
            #endregion


            #region  手势校准
            delegate void OnGesCalibUpdateC(int value);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void gestureCalibrate(OnGesCalibUpdateC cb);

            [MonoPInvokeCallback(typeof(OnGesCalibUpdateC))]
            static void OnGesCalibUpdateCallByC(int value)
            {
                OnGesCalibStateChange?.Invoke(value);
            }
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void stopGestureCalibrate();
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern void resetGestureCalibrate();

            #endregion

            #endregion
        }
    }


}
