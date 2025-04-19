using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rokid.UXR.Editor
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class XRIActionsFix : EditorWindow
    {
        private static XRIActionsFix window;
        private static string replaceActionResult;

        [MenuItem("Rokid/XRI/Replace Default Input Actions", false)]
        public static void ShowWindow()
        {
            ReplaceXRIInputActions();
            window = GetWindow<XRIActionsFix>(true);
            window.minSize = new Vector2(320, 240);
            window.maxSize = new Vector2(320, 360);
            window.titleContent = new GUIContent("Rokid OpenXR | Replace Input Actions");
        }

        private static void ReplaceXRIInputActions()
        {
            string InputActionsFilePath = "";
            replaceActionResult = "\nError: XRI Default Input Actions file not found\n";
            string[] paths = AssetDatabase.GetAllAssetPaths().Where(x => x.Contains("XRI Default Input Actions")).ToArray();
            if (paths.Length == 1)
            {
                InputActionsFilePath = paths[0];
            }
            else if (paths.Length > 1)
            {
                foreach (var path in paths)
                {
                    if (path.Contains("XR Interaction Toolkit"))
                    {
                        InputActionsFilePath = path;
                        break;
                    }
                }
            }
            Debug.Log("XRIActionsFix: ReplaceXRIInputActions() InputActionsFilePath="+InputActionsFilePath);
            if (!string.IsNullOrEmpty(InputActionsFilePath))
            {
                try {
                    string text = File.ReadAllText(InputActionsFilePath);
                    text = text.Replace("MetaAim", "RokidAim");
                    File.WriteAllText(InputActionsFilePath, text);
                    replaceActionResult = "\nReplace XRI default Input Actions successfully\n";
                } catch (System.Exception e) {
                    replaceActionResult = $"\nError: Unable to replace default XRI Actions due to {e.Message}\n";
                    Debug.LogError($"XRIActionsFix: Replace XRI default Input Actions {InputActionsFilePath} failed, Error is {e.ToString()}");
                }
            }
        }
        
        public void OnGUI()
        {
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(replaceActionResult, EditorStyles.textArea);
            GUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button("Close Window"))
                        Close();
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }

    }
#endif
}
