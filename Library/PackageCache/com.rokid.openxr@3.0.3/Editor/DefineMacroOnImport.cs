using UnityEditor;
using UnityEngine;
using System.IO;

namespace Rokid.UXR.Editor
{
    [InitializeOnLoad]
    public class DefineMacroOnImport
    {
        private const string PackageName = "com.rokid.openxr"; // 替换为你的包名
        private const string DefineSymbol = "USE_ROKID_OPENXR"; // 替换为你的宏定义

        static DefineMacroOnImport()
        {
            // 检测包是否已存在
            EditorApplication.update += CheckForPackage;
        }

        private static void CheckForPackage()
        {
            // 获取 Packages 目录下是否有指定的包
            string packagePath = Path.Combine("Packages", PackageName);

            if (Directory.Exists(packagePath))
            {
                // 如果包存在，添加宏定义
                AddDefineSymbol(DefineSymbol);
                EditorApplication.update -= CheckForPackage; // 检测完后停止更新
            }
        }

        private static void AddDefineSymbol(string symbol)
        {
            // 获取当前构建目标组
            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            if (targetGroup == BuildTargetGroup.Unknown)
                return;

            // 获取现有的宏定义
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            // 如果宏定义已存在，则跳过
            if (defines.Contains(symbol))
                return;

            // 添加新的宏定义
            if (!string.IsNullOrEmpty(defines))
            {
                defines += ";";
            }
            defines += symbol;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }
    }

}
