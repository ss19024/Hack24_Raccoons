using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Rokid.UXR.Utility
{
    public class ZipUtils
    {
        public static void AsyncCompressFolder(string folderPath, string zipPath, Action success, Action<string> failed = null)
        {
            Loom.RunAsync(() =>
            {
                if (Directory.Exists(folderPath))
                {
                    ZipFile.CreateFromDirectory(folderPath, zipPath);
                }
                else
                {
                    Loom.QueueOnMainThread(() => { failed?.Invoke($"====ZipUtils==== CompressFolder Directory is not exists:{folderPath}"); });
                }
            }, () => { success?.Invoke(); });
        }


        public static void AsyncDeCompressZip(string zipFile, string extractPath, Action success, Action<string> failed = null)
        {
            Loom.RunAsync(() =>
            {
                if (File.Exists(zipFile))
                {
                    ZipFile.ExtractToDirectory(zipFile, extractPath);
                }
                else
                {
                    Loom.QueueOnMainThread(() => { failed?.Invoke($"====ZipUtils==== DeCompressZip File is not exists: {zipFile}"); });
                }
            }, () => { success?.Invoke(); });
        }
    }
}
