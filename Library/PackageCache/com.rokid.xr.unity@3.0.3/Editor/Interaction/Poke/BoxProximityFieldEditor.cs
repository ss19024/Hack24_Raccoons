using UnityEditor;
using UnityEngine;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Editor
{
    [CustomEditor(typeof(BoxProximityField))]
    public class BoxProximityFieldEditor : UnityEditor.Editor
    {
        private SerializedProperty _boxTransformProperty;

        private void Awake()
        {
            _boxTransformProperty = serializedObject.FindProperty("_boxTransform");
        }

        public void OnSceneGUI()
        {
            Handles.color = EditorConstants.PRIMARY_COLOR;

            Transform boxTransform = _boxTransformProperty.objectReferenceValue as Transform;

            if (boxTransform != null)
            {
                using (new Handles.DrawingScope(boxTransform.localToWorldMatrix))
                {
                    Handles.DrawWireCube(Vector3.zero, Vector3.one);
                }
            }
        }
    }
}
