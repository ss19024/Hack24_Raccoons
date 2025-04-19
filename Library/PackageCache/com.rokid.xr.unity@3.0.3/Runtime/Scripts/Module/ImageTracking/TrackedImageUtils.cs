using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace Rokid.UXR.Module
{
    public class TrackedImageUtils
    {
        static string GetTempFileNameWithoutExtension() => Guid.NewGuid().ToString("N");
        public static IEnumerator ReadAsset(MarkerDBPath dbPath, Action<byte[], string> success, Action<string> failed)
        {
            string path;
            bool needDeCompressZip = false;
            if (dbPath == MarkerDBPath.PersistentData)
            {
                RKLog.KeyInfo("====ImageUtils==== dbPath is PersistentData:" + Application.persistentDataPath);
                path = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER, ARTrackedImageConstant.DB_CORE);
                if (File.Exists(path))
                {
                    path = "file://" + path;
                }
                else
                {
                    path = "file://" + Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_ZIP_FILE);
                    needDeCompressZip = true;
                }
            }
            else
            {
                RKLog.KeyInfo("====ImageUtils==== dbPath is StreamingAssets:" + Application.streamingAssetsPath);
                path = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER, ARTrackedImageConstant.DB_CORE);
                if (File.Exists(path))
                {
                    path = "file://" + path;
                }
                else
                {
                    path = Path.Combine(Application.streamingAssetsPath, ARTrackedImageConstant.DB_ZIP_FILE);
                    needDeCompressZip = true;
                }
            }
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    failed?.Invoke($"Failed to read assets: {request.error} " + path);
                }
                else
                {
                    if (needDeCompressZip)
                    {
                        byte[] data = request.downloadHandler.data;
                        path = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER, ARTrackedImageConstant.DB_CORE);
                        var tempDirectory = Path.Combine(Application.temporaryCachePath, GetTempFileNameWithoutExtension());
                        Directory.CreateDirectory(tempDirectory);
                        var tempFile = Path.Combine(tempDirectory, GetTempFileNameWithoutExtension());
                        Loom.RunAsync(() =>
                        {
                            File.Create(tempFile).Close();
                            File.WriteAllBytes(tempFile, data);
                        }, () =>
                        {
                            ZipUtils.AsyncDeCompressZip(tempFile, Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER), () =>
                            {
                                Loom.RunAsync(() =>
                                {
                                    try
                                    {
                                        data = File.ReadAllBytes(path);
                                    }
                                    finally
                                    {
                                        File.Delete(tempFile);
                                        Directory.Delete(tempDirectory);
                                    }
                                }, () =>
                                {
                                    success?.Invoke(data, path);
                                });
                            });
                        });
                    }
                    else
                    {
                        success?.Invoke(request.downloadHandler.data, path);
                    }
                }
            }
        }

        public static bool TryGetARTrackedImages(RokidMarkerArray data, ref ARTrackedImage[] trackedImages)
        {
            if (data.size == 0)
                return false;
            if (trackedImages.Length != data.size)
            {
                trackedImages = new ARTrackedImage[data.size];
            }
            for (uint i = 0; i < data.size; i++)
            {
                IntPtr markerPtr = Marshal.ReadIntPtr(data.elements, (int)i * IntPtr.Size);
                if (markerPtr == IntPtr.Zero)
                    return false;
                trackedImages[i] = new ARTrackedImage(NativeInterface.NativeAPI.GetMarkerId(markerPtr), NativeInterface.NativeAPI.GetMarkerPose(markerPtr), NativeInterface.NativeAPI.GetMarkerSize(markerPtr));
            }
            return true;
        }
    }
}