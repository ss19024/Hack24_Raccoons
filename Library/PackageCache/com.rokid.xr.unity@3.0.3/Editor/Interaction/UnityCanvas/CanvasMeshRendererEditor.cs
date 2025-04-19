
using UnityEditor;
using Rokid.UXR.Interaction;
namespace Rokid.UXR.Editor
{
    using props = CanvasMeshRenderer.Properties;
    [CustomEditor(typeof(CanvasMeshRenderer))]
    public class CanvasMeshRendererEditor : EditorBase
    {
        public new CanvasMeshRenderer target
        {
            get
            {
                return base.target as CanvasMeshRenderer;
            }
        }

        protected override void OnEnable()
        {
            var renderingModeProp = serializedObject.FindProperty(props.RenderingMode);

            Draw(props.RenderingMode, (modeProp) =>
            {
                RenderingMode value = (RenderingMode)modeProp.intValue;
                value = (RenderingMode)EditorGUILayout.EnumPopup("Rendering Mode", value);
                modeProp.intValue = (int)value;
            });

            Draw(props.UseAlphaToMask, props.AlphaCutoutThreshold, (maskProp, cutoutProp) =>
            {
                if (renderingModeProp.intValue == (int)RenderingMode.AlphaCutout)
                {
                    EditorGUILayout.PropertyField(maskProp);

                    if (maskProp.boolValue == false)
                    {
                        EditorGUILayout.PropertyField(cutoutProp);
                    }
                }
            });
        }
    }
}
