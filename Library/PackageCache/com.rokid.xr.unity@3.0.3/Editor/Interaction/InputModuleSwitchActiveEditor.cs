using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Editor
{
    [CustomEditor(typeof(InputModuleSwitchActive))]
    public class InputModuleSwitchActiveEditor : UnityEditor.Editor
    {
        SerializedProperty activeModuleType;
        SerializedProperty handActiveDetail;
        SerializedProperty behaviour;
        SerializedProperty autoRegister;
        AnimBool showHandInfo;
        private void OnEnable()
        {
            activeModuleType = serializedObject.FindProperty("activeModuleType");
            handActiveDetail = serializedObject.FindProperty("handActiveDetail");
            behaviour = serializedObject.FindProperty("behaviour");
            autoRegister = serializedObject.FindProperty("autoRegisterOnStart");
            showHandInfo = new AnimBool(Repaint);
#if UNITY_2021_3_OR_NEWER
            SetAnimBools(true);
#endif
        }

        void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
                a.value = value;
            else
                a.target = value;
        }

#if UNITY_2021_3_OR_NEWER
        void SetAnimBools(bool instant)
        {
            SetAnimBool(showHandInfo, !activeModuleType.hasMultipleDifferentValues && Contain(activeModuleType.enumValueFlag, (int)ActiveModuleType.Gesture), instant);
        }
#endif

        private bool Contain(int inData, int targetData)
        {
            return (inData & targetData) == targetData;
        }



        public override void OnInspectorGUI()
        {
#if UNITY_2021_3_OR_NEWER
            SetAnimBools(false);
            serializedObject.Update();
            EditorGUILayout.PropertyField(activeModuleType);
            if (EditorGUILayout.BeginFadeGroup(showHandInfo.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(handActiveDetail);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.PropertyField(behaviour);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(autoRegister);
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
#else
	        base.OnInspectorGUI();
#endif
        }
    }
}
