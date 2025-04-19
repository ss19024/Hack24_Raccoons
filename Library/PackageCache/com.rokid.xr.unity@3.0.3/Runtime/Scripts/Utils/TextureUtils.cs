using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Rokid.UXR.Utility
{
    public class TextureUtils
    {
        /// <summary>
        /// 根据指定的Texture生成唯一的GUID。
        /// </summary>
        public static string GenerateGUIDForTexture(Texture2D texture, string texturePath)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture), "Texture cannot be null");
            }

            // 获取Texture的基本信息
            int width = texture.width;
            int height = texture.height;
            TextureFormat format = texture.format;
            string textureName = texture.name;

            // 获取Texture的像素数据
            byte[] pixelData = texture.GetRawTextureData();

            // 构建哈希输入数据：包括名称、尺寸、格式和像素数据
            using (SHA256 sha256 = SHA256.Create())
            {
                // 使用StringBuilder构建包含所有属性的字符串
                StringBuilder hashInput = new StringBuilder();
                hashInput.Append(textureName)
                         .Append(width)
                         .Append(height)
                         .Append(format.ToString())
                         .Append(Convert.ToBase64String(pixelData))
                         .Append(texturePath);

                // 生成哈希字节数组
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput.ToString()));

                // 将哈希字节转换为GUID格式
                string guid = new Guid(hashBytes.Take(16).ToArray()).ToString("N");
                RKLog.KeyInfo($"====TextureUtils==== GenerateGUIDForTexture {textureName},{Convert.ToBase64String(pixelData)},{pixelData.Length},{guid}");
                return guid;
            }
        }

        public static IEnumerator LoadTextureFromURL(string url, Action<Texture2D> success)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    success?.Invoke(texture);
                }
                else
                {
                    RKLog.Error($"====TextureUtils==== request failed: {url}");
                }
            }
        }

        public static void AsyncLoadTexture(string path, Action<Texture2D> success)
        {
            byte[] data = new byte[0];
            Texture2D texture = new Texture2D(1, 1);
            Loom.RunAsync(() =>
            {
                data = File.ReadAllBytes(path);
            }, () =>
            {
                RKLog.KeyInfo($"====TextureUtils==== LoadTexture Success: {data.Length}");
                texture.LoadImage(data);
                success?.Invoke(texture);
            });
        }
    }
}

