using UnityEditor;
using UnityEngine;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Editor
{
    [CustomEditor(typeof(CircleProximityField))]
    public class CircleProximityFieldEditor : UnityEditor.Editor
    {
        private SerializedProperty _transformProperty;
        private SerializedProperty _radiusProperty;

        private void Awake()
        {
            _transformProperty = serializedObject.FindProperty("_transform");
            _radiusProperty = serializedObject.FindProperty("_radius");
        }

        public void OnSceneGUI()
        {
            Handles.color = EditorConstants.PRIMARY_COLOR;

            Transform transform = _transformProperty.objectReferenceValue as Transform;
            float radius = _radiusProperty.floatValue * transform.lossyScale.x;
#if UNITY_2020_2_OR_NEWER
            Handles.DrawWireDisc(transform.position, -transform.forward, radius, EditorConstants.LINE_THICKNESS);
#else
            Handles.DrawWireDisc(transform.position, -transform.forward, radius);
#endif
        }
    }
}
