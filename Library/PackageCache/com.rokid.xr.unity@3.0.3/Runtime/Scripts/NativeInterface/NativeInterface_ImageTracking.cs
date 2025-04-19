

using System;
using System.IO;
using System.Runtime.InteropServices;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RokidMarkerArray
    {
        public IntPtr elements; // RokidMarker** in C++
        public uint size;
    }

    public struct RokidMarkerChanges
    {
        public IntPtr value;
    }


    public enum ImageFormat
    {
        GRAY,
        RGB
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MarkerImage
    {
        public IntPtr image_name;
        public int image_id;
        public IntPtr image_pixels;
        public int pixels_size;
        public int width_in_pixels;
        public int height_in_pixels;
        public int stride_in_pixels;
        public float width_in_meters;
        public float height_in_meters;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MarkerImageArray
    {
        public IntPtr elements; // intptr_t* in C++    // MarkerImage** elements -> MarkerImage[]
        public uint size;
    }


    public partial class NativeInterface
    {
        public static partial class NativeAPI
        {
            /// <summary>
            /// Try open image tracker
            /// </summary>
            /// <param name="imageDatabase">image db</param>
            /// <param name="size">image db size</param>
            /// <returns></returns>
            public static unsafe bool TryOpenImageTracker(byte[] imageDatabase, int size)
            {
                if (Utils.IsAndroidPlatform())
                {
                    int result = openMarker(imageDatabase, size);
                    RKLog.KeyInfo("====openMarker====:" + result);
                    return result == 0;
                }
                return false;
            }


            /// <summary>
            /// Close image tracker
            /// </summary>
            /// <returns></returns>
            public static void CloseImageTracker()
            {
                if (Utils.IsAndroidPlatform())
                {
                    closeMarker();
                }
            }


            /// <summary>
            /// Create Image DB
            /// </summary>
            /// <returns></returns>
            public static int CreateImageDB()
            {
                if (Utils.IsAndroidPlatform())
                {
                    return createMarkerDb();
                }
                return 0;
            }

            /// <summary>
            /// Destroy Image DB
            /// </summary>
            /// <returns></returns>
            public static int DestroyImageDB()
            {
                if (Utils.IsAndroidPlatform())
                {
                    return destroyMarkerDb();
                }
                return 0;
            }

            /// <summary>
            /// Save and close image db
            /// </summary>
            /// <param name="path">The image database saved path</param>
            public static int SaveImageDB(string path)
            {
                if (Utils.IsAndroidPlatform())
                {
                    if (!File.Exists(path))
                        File.Create(path).Close();
                    int result = saveMarkerDBToFile(path);
                    if (result == 0)
                        result = destroyMarkerDb();
                    return result;
                }
                return 0;
            }

            /// <summary>
            /// Add image data to database
            /// </summary>
            /// <param name="arDBImage">The data obj in image database</param>
            public static int AddDBImage(ARDBImage arDBImage)
            {
                if (Utils.IsAndroidPlatform())
                {
                    if (!string.IsNullOrEmpty(arDBImage.imagePath))
                    {
                        RKLog.Info("====addMarkerImageWithPath====:" + arDBImage);
                        return addMarkerImageWithPathAndPhysSize(arDBImage.imagePath, Convert.ToString(arDBImage.index), arDBImage.index, arDBImage.physicalWidth, arDBImage.physicalHeight, (int)ImageFormat.GRAY);
                    }
                    else
                    {
                        RKLog.Error("====addMarkerImageWithData====:" + arDBImage);
                    }
                }
                return 0;
            }

            public static void QualityDestroy()
            {
                if (Utils.IsAndroidPlatform())
                {
                    int result = qualityDestroy();
                    RKLog.KeyInfo("====qualityDestroy====:" + result);
                }
            }

            public static void QualityCreate(IntPtr context)
            {
                if (Utils.IsAndroidPlatform())
                {
                    int result = qualityCreate(context);
                    RKLog.KeyInfo("====qualityCreate====:" + result);
                }
            }

            public static RokidMarkerChanges TrackedImage_AcquireChanges(ref RokidMarkerArray added,
                ref RokidMarkerArray updated,
                ref RokidMarkerArray removed)
            {
                if (Utils.IsAndroidPlatform())
                {
                    added.size = updated.size = removed.size = 0;
                    added.elements = updated.elements = removed.elements = IntPtr.Zero;
                    return markerAcquireChanges(ref added, ref updated, ref removed);
                }
                return default;
            }

            public static Vector2 GetMarkerSize(IntPtr marker)
            {
                getMarkerSize(marker, out float x, out float y);
                return new Vector2(x, y);
            }

            public static Pose GetMarkerPose(IntPtr marker)
            {
                float[] pose = new float[7];
                getMarkerPose(marker, pose);
                return new Pose(new Vector3(pose[4], pose[5], -pose[6]), new Quaternion(-pose[0], -pose[1], pose[2], pose[3]));
            }

            public static string GetMarkerId(IntPtr marker)
            {
                return Marshal.PtrToStringAnsi(getMarkerId(marker));
            }

            internal static int CheckImageQuality(string imagePath)
            {
                int result = qualityPreprocess(imagePath, 0, 0);
                RKLog.KeyInfo("====GetImageQuality====:" + imagePath + " result:" + result);
                return result;
            }

            internal static int RemoveImageQualityCheck(string imagePath)
            {
                int result = qualityDelete(imagePath);
                return result;
            }

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "openMarker2")]
            static unsafe extern int openMarker(byte[] imageDatabase, int size);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "closeMarker2")]
            static extern void closeMarker();

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "marker2AcquireChanges", CallingConvention = CallingConvention.Cdecl)]
            static extern RokidMarkerChanges markerAcquireChanges(
                ref RokidMarkerArray added,
                ref RokidMarkerArray updated,
                ref RokidMarkerArray removed);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "marker2GetCenterPose")]
            static extern void getMarkerPose(IntPtr marker, float[] pose);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "marker2GetExtent")]
            static extern void getMarkerSize(IntPtr marker, out float x, out float y);

            [DllImport(ApiConstants.ROKID_UXR_PLUGIN, EntryPoint = "marker2GetId")]
            static extern IntPtr getMarkerId(IntPtr marker);
            [DllImport("RokidMarkerApi")]
            static extern int createMarkerDb();

            [DllImport("RokidMarkerApi", EntryPoint = "writeMarkerDbToFile")]
            static extern int saveMarkerDBToFile(string path);

            [DllImport("RokidMarkerApi")]
            static extern int addMarkerImageWithPhysSize(
                                    string image_name,
                                    int image_id,
                                    byte[] pixels,
                                    int width_in_pixels,
                                    int height_in_pixels,
                                    int stride_in_pixels,
                                    float width_in_meters,
                                    float height_in_meters,
                                    int image_format);

            [DllImport("RokidMarkerApi")]
            static extern int addMarkerImageWithPathAndPhysSize(
                                    string image_path,
                                    string image_name,
                                    int image_id,
                                    float width_in_meters,
                                    float height_in_meters,
                                    int image_format);

            [DllImport("RokidMarkerApi")]
            static extern int qualityCreate(IntPtr context);

            [DllImport("RokidMarkerApi")]
            static extern int qualityPreprocess(string image_path, float width_in_meters, float height_in_meters);

            [DllImport("RokidMarkerApi")]
            static extern int qualityDelete(string image_path);

            [DllImport("RokidMarkerApi")]
            static extern int qualityDestroy();

            [DllImport("RokidMarkerApi")]
            static extern int destroyMarkerDb();

        }
    }
}


