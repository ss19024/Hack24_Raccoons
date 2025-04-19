using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

namespace Rokid.UXR.Editor
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class UXREnvFix : EditorWindow
    {
        #region  Config
        private const string ignorePrefix = "UXREnvFix";
        private static FixItem[] fixItems;
        private static UXREnvFix window;
        private Vector2 scrollPosition;
        private static string minUnityVersion = "2020.3.26";
        private static AndroidSdkVersions minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
        private static AndroidArchitecture targetArchitecture = AndroidArchitecture.ARM64;

        #endregion

        static UXREnvFix()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            bool show = false;

            if (fixItems == null) { RegisterItems(); }
            foreach (var item in fixItems)
            {
                if (!item.IsIgnored() && !item.IsValid() && item.Level > MessageType.Warning && item.IsAutoPop())
                {
                    show = true;
                }
            }

            if (show)
            {
                ShowWindow();
            }

            EditorApplication.update -= OnUpdate;
        }

        private static void RegisterItems()
        {
            fixItems = new FixItem[]
            {
                new CheckBuildTarget(MessageType.Error),
                new CheckUnityMinVersion(MessageType.Error), 
	            // new CkeckMTRendering(MessageType.Error),
                // new CkeckAndroidGraphicsAPI(MessageType.Error),
                new CkeckAndroidOrientation(MessageType.Warning),
	            // new CkeckColorSpace(MessageType.Warning),
	            new CheckOptimizedFramePacing(MessageType.Warning),
                new CheckMinmumAPILevel(MessageType.Error),
                new CheckArchitecture(MessageType.Error)
            };
        }

        [MenuItem("Rokid/Env/Project Environment Fix", false)]
        public static void ShowWindow()
        {
            RegisterItems();
            window = GetWindow<UXREnvFix>(true);
            window.minSize = new Vector2(320, 300);
            window.maxSize = new Vector2(320, 800);
            window.titleContent = new GUIContent("Rokid OpenXR | Environment Fix");
        }

        //[MenuItem("Rokid/Env/Delete Ignore", false)]
        public static void DeleteIgonre()
        {
            foreach (var item in fixItems)
            {
                item.DeleteIgonre();
            }
        }

        public void OnGUI()
        {
            string resourcePath = GetResourcePath();
            Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "RokidIcon.png");
            Rect rect = GUILayoutUtility.GetRect(position.width, 80, GUI.skin.box);
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

            string aboutText = "This window provides tips to help fix common issues with Rokid OpenXR and your project.";
            EditorGUILayout.LabelField(aboutText, EditorStyles.textArea);

            int ignoredCount = 0;
            int fixableCount = 0;
            int invalidNotIgnored = 0;

            for (int i = 0; i < fixItems.Length; i++)
            {
                FixItem item = fixItems[i];

                bool ignored = item.IsIgnored();
                bool valid = item.IsValid();
                bool fixable = item.IsFixable();

                if (!valid && !ignored && fixable)
                {
                    fixableCount++;
                }

                if (!valid && !ignored)
                {
                    invalidNotIgnored++;
                }

                if (ignored)
                {
                    ignoredCount++;
                }
            }

            Rect issuesRect = EditorGUILayout.GetControlRect();
            GUI.Box(new Rect(issuesRect.x - 4, issuesRect.y, issuesRect.width + 8, issuesRect.height), "Tips", EditorStyles.toolbarButton);

            if (invalidNotIgnored > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                {
                    for (int i = 0; i < fixItems.Length; i++)
                    {
                        FixItem item = fixItems[i];

                        if (!item.IsIgnored() && !item.IsValid())
                        {
                            invalidNotIgnored++;

                            GUILayout.BeginVertical("box");
                            {
                                item.DrawGUI();

                                EditorGUILayout.BeginHorizontal();
                                {
                                    // Aligns buttons to the right
                                    GUILayout.FlexibleSpace();

                                    if (item.IsFixable())
                                    {
                                        if (GUILayout.Button("Fix"))
                                            item.Fix();
                                    }

                                    //if (GUILayout.Button("Ignore"))
                                    //    check.Ignore();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                        }
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();

            if (invalidNotIgnored == 0)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("No issue found");

                        if (GUILayout.Button("Close Window"))
                            Close();
                    }
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.BeginHorizontal("box");
            {
                if (fixableCount > 0)
                {
                    if (GUILayout.Button("Accept All"))
                    {
                        if (EditorUtility.DisplayDialog("Accept All", "Are you sure?", "Yes, Accept All", "Cancel"))
                        {
                            for (int i = 0; i < fixItems.Length; i++)
                            {
                                FixItem item = fixItems[i];

                                if (!item.IsIgnored() &&
                                    !item.IsValid())
                                {
                                    if (item.IsFixable())
                                        item.Fix();
                                }
                            }
                        }
                    }
                }

            }
            GUILayout.EndHorizontal();
        }

        private string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path + "\\Textures\\";
        }

        private abstract class FixItem
        {
            protected string key;
            protected MessageType level;

            public MessageType Level
            {
                get
                {
                    return level;
                }
            }

            public FixItem(MessageType level)
            {
                this.level = level;
            }

            public void Ignore()
            {
                EditorPrefs.SetBool(ignorePrefix + key, true);
            }

            public bool IsIgnored()
            {
                return EditorPrefs.HasKey(ignorePrefix + key);
            }

            public void DeleteIgonre()
            {
                Debug.Log("DeleteIgnore" + ignorePrefix + key);
                EditorPrefs.DeleteKey(ignorePrefix + key);
            }

            public abstract bool IsValid();

            public abstract bool IsAutoPop();

            public abstract void DrawGUI();

            public abstract bool IsFixable();

            public abstract void Fix();

            protected void DrawContent(string title, string msg)
            {
                EditorGUILayout.HelpBox(title, level);
                EditorGUILayout.LabelField(msg, EditorStyles.textArea);
            }
        }

        // private class CkeckAndroidGraphicsAPI : FixItem
        // {
        //     public CkeckAndroidGraphicsAPI(MessageType level) : base(level)
        //     {
        //         key = this.GetType().Name;
        //     }
        //
        //     public override bool IsValid()
        //     {
        //         if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        //         {
        //             if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android))
        //             {
        //                 return false;
        //             }
        //             var graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        //             if (graphics != null && graphics.Length >= 1 &&
        //                 graphics[0] == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
        //             {
        //                 return true;
        //             }
        //             return false;
        //         }
        //         else
        //         {
        //             return false;
        //         }
        //     }
        //
        //     public override void DrawGUI()
        //     {
        //         string message = @"Graphics APIs should be set to OpenGLES.  Player Settings > Other Settings > Graphics APIs , choose 'OpenGLES3'.";
        //         DrawContent("Graphics APIs is not OpenGLES", message);
        //     }
        //
        //     public override bool IsFixable()
        //     {
        //         return true;
        //     }
        //
        //     public override void Fix()
        //     {
        //         if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        //         {
        //             PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        //             PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[1] { GraphicsDeviceType.OpenGLES3 });
        //         }
        //     }
        //
        //     public override bool IsAutoPop()
        //     {
        //         return true;
        //     }
        // }

 //        private class CkeckMTRendering : FixItem
 //        {
 //            public CkeckMTRendering(MessageType level) : base(level)
 //            {
 //                key = this.GetType().Name;
 //            }
 //
 //            public override bool IsValid()
 //            {
 //                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
 //                {
 //                    return !PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android);
 //                }
 //                else
 //                {
 //                    return false;
 //                }
 //            }
 //
 //            public override void DrawGUI()
 //            {
 //                string message = @"In order to run correct on mobile devices, the RenderingThreadingMode should be set. 
	// in dropdown list of Player Settings > Other Settings > Multithreaded Rendering, close toggle.";
 //                DrawContent("Multithreaded Rendering not close", message);
 //            }
 //
 //            public override bool IsFixable()
 //            {
 //                return true;
 //            }
 //
 //            public override void Fix()
 //            {
 //                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
 //                {
 //                    PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false);
 //                }
 //            }
 //
 //            public override bool IsAutoPop()
 //            {
 //                return true;
 //            }
 //        }

        private class CkeckAndroidOrientation : FixItem
        {
            public CkeckAndroidOrientation(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait;
            }

            public override void DrawGUI()
            {
                string message = @"In order to display correct on mobile devices, the orientation should be set to portrait. 
	in dropdown list of Player Settings > Resolution and Presentation > Default Orientation, choose 'Portrait'.";
                DrawContent("Orientation is not portrait", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        private class CkeckColorSpace : FixItem
        {
            public CkeckColorSpace(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                return PlayerSettings.colorSpace == ColorSpace.Gamma;
            }

            public override void DrawGUI()
            {
                string message = @"In order to display correct on mobile devices, the colorSpace should be set to gamma. 
	in dropdown list of Player Settings > Other Settings > Color Space, choose 'Gamma'.";
                DrawContent("ColorSpace is not Linear", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.colorSpace = ColorSpace.Gamma;
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        private class CkeckAndroidPermission : FixItem
        {
            public CkeckAndroidPermission(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return PlayerSettings.Android.forceInternetPermission;
                }
                else
                {
                    return false;
                }
            }

            public override void DrawGUI()
            {
                string message = @"In order to run correct on mobile devices, the internet access premission should be set. 
	in dropdown list of Player Settings > Other Settings > Internet Access, choose 'Require'.";
                DrawContent("internet access permission not available", message);
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.Android.forceInternetPermission = true;
                }
            }

            public override bool IsAutoPop()
            {
                return true;
            }
        }

        //todo...添加最低版本号的检查
        private class CheckUnityMinVersion : FixItem
        {
            string unityVersion;//

            public CheckUnityMinVersion(MessageType level) : base(level)
            {
                key = this.GetType().Name;
                unityVersion = Application.unityVersion;
            }

            public override void DrawGUI()
            {
                string message = @"The minimum Unity version required is 2020.3.36";
                DrawContent("Unity version not valid ", message);
            }

            public override void Fix()
            {

            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return unityVersion.CompareTo(minUnityVersion) == 1;
            }

            public override bool IsValid()
            {
                return unityVersion.CompareTo(minUnityVersion) == 1;
            }
        }

        private class CheckOptimizedFramePacing : FixItem
        {
            public CheckOptimizedFramePacing(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The optimizedFramePacing need to unselect";
                DrawContent("OptimizedFramePacing", message);
            }

            public override void Fix()
            {
                PlayerSettings.Android.optimizedFramePacing = false;
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                return PlayerSettings.Android.optimizedFramePacing == false;
            }
        }

        private class CheckMinmumAPILevel : FixItem
        {
            public CheckMinmumAPILevel(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The minSdkVersion needs to be " + minSdkVersion.ToString();
                DrawContent("MinSDKVersion", message);
            }

            public override void Fix()
            {
                PlayerSettings.Android.minSdkVersion = minSdkVersion;
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                return PlayerSettings.Android.minSdkVersion >= minSdkVersion;
            }
        }

        private class CheckArchitecture : FixItem
        {
            public CheckArchitecture(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The Target Architecture should be " + targetArchitecture;
                DrawContent("Target Architecture", message);
            }

            public override void Fix()
            {
#if UNITY_6000_0_OR_NEWER
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
#else
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#endif
                PlayerSettings.Android.targetArchitectures = targetArchitecture;
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
#if UNITY_6000_0_OR_NEWER
                return PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP &&
                       PlayerSettings.Android.targetArchitectures == targetArchitecture;
#else
                return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP &&
                    PlayerSettings.Android.targetArchitectures == targetArchitecture;
#endif
            }
        }


        private class CheckBuildTarget : FixItem
        {
            public CheckBuildTarget(MessageType level) : base(level)
            {
                key = this.GetType().Name;
            }

            public override void DrawGUI()
            {
                string message = @"The Build Target should be Android";
                DrawContent("Build Target", message);
            }

            public override void Fix()
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            public override bool IsAutoPop()
            {
                return true;
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            }
        }
    }
#endif
}
