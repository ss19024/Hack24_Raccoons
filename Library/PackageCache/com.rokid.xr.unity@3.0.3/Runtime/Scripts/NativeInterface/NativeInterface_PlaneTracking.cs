using Rokid.UXR.Utility;
using System.Runtime.InteropServices;
using Rokid.UXR.Module;
using Unity.Collections;
using UnityEngine;
using System;

namespace Rokid.UXR.Native
{
    public partial class NativeInterface
    {
        public static partial class NativeAPI
        {
            public static void OpenPlaneTracker(PlaneDetectMode detectMode)
            {
                if (Utils.IsAndroidPlatform())
                    openPlaneTracker((int)detectMode);
            }

            public static void ClosePlaneTracker()
            {
                if (Utils.IsAndroidPlatform())
                    closePlaneTracker();
            }


            public static void SetPlaneDetectMode(PlaneDetectMode detectMode)
            {
                if (Utils.IsAndroidPlatform())
                    setPlaneDetectMode((int)detectMode);
            }

            public static PlaneDetectMode GetPlaneDetectMode()
            {
                int mode = 1;
                if (Utils.IsAndroidPlatform())
                {
                    getPlaneDetectMode(out mode);
                }
                if (mode == 0)
                    mode = 1;
                return (PlaneDetectMode)mode;
            }


            public static unsafe TrackableChanges<long> GetChanges(Allocator allocator)
            {
                if (Utils.IsAndroidPlatform())
                {
                    acquireChanges(0, out void* nochangedPtr, out void* updatedPtr, out void* addedPtr, out void* removedPtr, out int nochangeLength, out int updatedLength, out int addedLength, out int removedLength);
                    try
                    {
                        return new TrackableChanges<long>(
                           addedPtr, addedLength,
                           updatedPtr, updatedLength,
                           removedPtr, removedLength,
                           0, sizeof(long),
                           allocator);
                    }
                    finally
                    {
                        //Release
                        release(nochangedPtr);
                        release(updatedPtr);
                        release(addedPtr);
                        release(removedPtr);
                    }
                }
                return new TrackableChanges<long>(
                          null, 0,
                          null, 0,
                          null, 0,
                          0, sizeof(long),
                          allocator);
            }


            public static unsafe bool TryGetBoundedPlane(long plane, ref BoundedPlane boundedPlane)
            {
                if (getPlaneType(plane, out int type) == 0 && getBoundary(plane, out void* boundaryPtr, out int length) == 0 && getPlanePose(plane, out void* posePtr, out void* centerPtr, out void* normalPtr) == 0)
                {
                    try
                    {
                        boundedPlane.planeType = (PlaneType)type;
                        float[] poseArray = new float[7];
                        Marshal.Copy((IntPtr)posePtr, poseArray, 0, 7);
                        boundedPlane.pose = new Pose(new Vector3(poseArray[4], poseArray[5], -poseArray[6]), new Quaternion(-poseArray[0], -poseArray[1], poseArray[2], poseArray[3]));
                        if (boundedPlane.boundary == null || boundedPlane.boundary?.Length != length)
                        {
                            boundedPlane.boundary = new Vector2[length];
                            boundedPlane.boundary3D = new Vector3[length];
                        }
                        float[] boundaryArray = new float[length * 3];
                        Marshal.Copy((IntPtr)boundaryPtr, boundaryArray, 0, length * 3);
                        for (int i = 0; i < length; i++)
                        {
                            Vector3 point = new Vector3(boundaryArray[3 * i], boundaryArray[3 * i + 1], -boundaryArray[3 * i + 2]);
                            Vector3 localPointInCenterSpace = Quaternion.Inverse(boundedPlane.pose.rotation) * (point - boundedPlane.pose.position);
                            boundedPlane.boundary3D[i] = point;
                            boundedPlane.boundary[i] = Math.Abs(localPointInCenterSpace.x) < 0.001f ? new Vector2(localPointInCenterSpace.y, localPointInCenterSpace.z) : Math.Abs(localPointInCenterSpace.y) < 0.001f ? new Vector2(localPointInCenterSpace.x, localPointInCenterSpace.z) : new Vector2(localPointInCenterSpace.x, localPointInCenterSpace.y);
                        }
                    }
                    finally
                    {
                        release(boundaryPtr);
                        release(normalPtr);
                        release(centerPtr);
                        release(posePtr);
                    }
                    return true;
                }
                return false;
            }



            /// <summary>
            /// open plane tracker
            /// </summary>
            /// <param name="mode">0-禁用,1-仅平面,2-仅竖直,3-平面和竖直</param>
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int openPlaneTracker(int mode);
            /// <summary>
            /// close plane tracker
            /// </summary>
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int closePlaneTracker();


            /// <summary>
            /// acquire changes
            /// </summary>
            /// <param name="type"></param>
            /// <param name="noChangePtr"></param>
            /// <param name="updatePtr"></param>
            /// <param name="addPtr"></param>
            /// <param name="removePtr"></param>
            /// <param name="nochangeLength"></param>
            /// <param name="updateLength"></param>
            /// <param name="addLength"></param>
            /// <param name="removeLength"></param>  
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "getUpdatePlanes")]
            static unsafe extern int acquireChanges(int type, out void* noChangePtr, out void* updatePtr, out void* addPtr, out void* removePtr, out int nochangeLength, out int updateLength, out int addLength, out int removeLength);

            /// <summary>
            /// get plane boundary
            /// </summary>
            /// <param name="plane"></param>
            /// <param name="boundaryPtr"></param>
            /// <param name="boundaryLength"></param>
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "getPlanePolygon")]
            static unsafe extern int getBoundary(long plane, out void* boundaryPtr, out int boundaryLength);

            /// <summary>
            /// get plane type 
            /// </summary>
            /// <param name="index"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getPlaneType(long plane, out int type);

            /// <summary>
            /// get plane  pose
            /// </summary>
            /// <param name="plane"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "getPlaneCenterPose")]
            static unsafe extern int getPlanePose(long plane, out void* posePtr, out void* centerPtr, out void* normalPtr);
            /// <summary>
            /// release ptr
            /// </summary>
            /// <param name="ptr"></param>
            /// <returns></returns>

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "releaseMemory")]
            static unsafe extern int release(void* ptr);

            /// <summary>
            /// set plane detect mode
            /// </summary>
            /// <param name="mode">0-禁用,1-仅平面,2-仅竖直,3-平面和竖直</param>
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int setPlaneDetectMode(int mode);

            /// <summary>
            /// get plane detect mode
            /// </summary>
            /// <param name="mode">0-禁用,1-仅平面,2-仅竖直,3-平面和竖直</param>
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static extern int getPlaneDetectMode(out int mode);

            /// <summary>
            /// print polygon
            /// <returns></returns>
            [DllImport(ApiConstants.ROKID_UXR_PLUGIN)]
            static unsafe extern int printPolygon(ref void* boundary, int boundaryLength);
        }
    }
}
