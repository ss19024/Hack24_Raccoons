using System;
using UnityEngine;

namespace Rokid.UXR.Utility
{
    /// <summary>
    /// To use the following interface, you need to set Unity's Graphics API to OpenGL3 and disable multi-threaded rendering.
    /// </summary>
    public class AndroidTextureLoader
    {
        public const string classString = "com.rokid.textureloader.AndroidTextureLoader";
        /// <summary>
        /// Get Image Dimensions
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static int[] GetImageDimensions(string imagePath)
        {
            int[] dimensions = new int[2];
            using (AndroidJavaClass textureLoaderClass = new AndroidJavaClass(classString))
            {
                dimensions = textureLoaderClass.CallStatic<int[]>("getImageDimensions", imagePath);
            }
            return dimensions;
        }

        /// <summary>
        /// Get All Image Paths User Json Format
        /// </summary>
        /// <returns></returns>
        public static string GetAllImagePaths()
        {
            AndroidJavaObject activity = null;
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            // 调用 Java 异步加载纹理方法
            using (AndroidJavaClass textureLoaderClass = new AndroidJavaClass(classString))
            {
                return textureLoaderClass.CallStatic<string>("getAllImagePaths", activity);
            }
        }

        /// <summary>
        /// Load Texture Async
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="success"></param>
        public static void LoadTextureAsync(string imagePath, Action<Texture2D> success)
        {
            RKLog.KeyInfo($"LoadTextureAsync imagePath:{imagePath}");
            // 获取 Unity 的 Android Java Activity
            AndroidJavaObject activity = null;
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            // 先获取图片的宽高
            int[] dimensions = GetImageDimensions(imagePath);
            int width = dimensions[0];
            int height = dimensions[1];

            if (width <= 0 || height <= 0)
            {
                RKLog.Error("Failed to get image dimensions");
                return;
            }

            // 创建一个回调对象，用于接收纹理 ID
            TextureLoadCallback callback = new TextureLoadCallback((textureId) =>
            {
                // 在 Unity 主线程上创建 Texture2D
                if (textureId != 0)
                {
                    Texture2D sharedTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, new IntPtr(textureId));
                    success?.Invoke(sharedTexture);
                }
                else
                {
                    RKLog.Error("Failed to load texture from Android");
                }
            });

            // 调用 Java 异步加载纹理方法
            using (AndroidJavaClass textureLoaderClass = new AndroidJavaClass(classString))
            {
                textureLoaderClass.CallStatic("loadTextureAsync", activity, imagePath, callback);
            }
        }

        /// <summary>
        /// Load Texture Thumbnail Async
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="success"></param>
        public static void LoadTextureThumbnailAsync(string imagePath, Action<Texture2D> success)
        {
            RKLog.KeyInfo($"LoadTextureThumbnailAsync  imagePath:{imagePath}");
            // 获取 Unity 的 Android Java Activity
            AndroidJavaObject activity = null;
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            // 先获取图片的宽高
            int[] dimensions = GetImageDimensions(imagePath);
            int width = dimensions[0];
            int height = dimensions[1];

            if (width <= 0 || height <= 0)
            {
                RKLog.Error("Failed to get image dimensions");
                return;
            }

            // 创建一个回调对象，用于接收纹理 ID
            TextureLoadCallback callback = new TextureLoadCallback((textureId) =>
            {
                // 在 Unity 主线程上创建 Texture2D
                if (textureId != 0)
                {
                    Texture2D sharedTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, new IntPtr(textureId));
                    success?.Invoke(sharedTexture);
                }
                else
                {
                    RKLog.Error("Failed to load texture from Android");
                }
            });

            // 调用 Java 异步加载纹理方法
            using (AndroidJavaClass textureLoaderClass = new AndroidJavaClass(classString))
            {
                textureLoaderClass.CallStatic("loadTextureThumbnailAsync", activity, imagePath, callback);
            }
        }


        private class TextureLoadCallback : AndroidJavaProxy
        {
            private Action<int> callbackAction;

            public TextureLoadCallback(Action<int> callback) : base(classString + "$TextureLoadCallback")
            {
                callbackAction = callback;
            }

            public void onTextureLoaded(int textureId)
            {
                callbackAction?.Invoke(textureId);
            }
        }
    }
}

