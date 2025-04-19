using System.IO;
using UnityEditor;
using UnityEngine;
using Rokid.UXR.Config;

namespace Rokid.UXR.Editor
{
    public class UXRSDKConfigEditor : MonoBehaviour
    {
        //[MenuItem("Assets/UXR/Config/Build SDKConfig")]
        public static void BuildSDKConfig()
        {
            BuildConfig<UXRSDKConfig>("UXRSDKConfig");
        }

        public static void BuildConfig<T>(string name) where T : ScriptableObject
        {
            T data = null;
            string folderPath = GetSelectedDirAssetsPath();
            string spriteDataPath = folderPath + string.Format("/{0}.asset", name);

            data = AssetDatabase.LoadAssetAtPath<T>(spriteDataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(data, spriteDataPath);
            }
            RKLog.Info("Create Config In Folder:" + spriteDataPath);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        public static string GetSelectedDirAssetsPath()
        {
            string path = string.Empty;

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }

            return path;
        }
    }
}
