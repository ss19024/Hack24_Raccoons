
using System;
using UnityEngine;
using UnityEditor;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Editor
{
    using props = RKCanvasMeshRenderer.Properties;
    using baseProps = CanvasMeshRenderer.Properties;
    using rtprops = CanvasRenderTexture.Properties;
    [CustomEditor(typeof(RKCanvasMeshRenderer))]
    public class RKCanvasMeshRendererEditor : EditorBase
    {
        public new RKCanvasMeshRenderer target
        {
            get
            {
                return base.target as RKCanvasMeshRenderer;
            }
        }

        protected override void OnEnable()
        {
            Defer(baseProps.UseAlphaToMask, baseProps.AlphaCutoutThreshold);
            var renderingMode = serializedObject.FindProperty(baseProps.RenderingMode);

            bool CheckIsOVR()
            {
                return renderingMode.intValue == (int)RKRenderingMode.Underlay ||
                       renderingMode.intValue == (int)RKRenderingMode.Overlay;
            }

            Draw(props.RuntimeOffset, (offsetProp) =>
            {
                if (CheckIsOVR())
                {
                    EditorGUILayout.PropertyField(offsetProp);
                }
            });

            Draw(baseProps.RenderingMode, props.CanvasMesh, (modeProp, meshProp) =>
            {
                EditorGUILayout.PropertyField(meshProp);
                RKRenderingMode value = (RKRenderingMode)modeProp.intValue;
                value = (RKRenderingMode)EditorGUILayout.EnumPopup("Rendering Mode", value);
                modeProp.intValue = (int)value;
            });

            Draw(props.EnableSuperSampling, props.EmulateWhileInEditor, props.DoUnderlayAntiAliasing, (sampleProp, emulateProp, aaProp) =>
            {
                if (CheckIsOVR())
                {
                    EditorGUILayout.PropertyField(sampleProp);
                    if (renderingMode.intValue == (int)RKRenderingMode.Underlay)
                    {
                        EditorGUILayout.PropertyField(aaProp);
                    }
                    EditorGUILayout.PropertyField(emulateProp);
                }
            });

            Draw(baseProps.UseAlphaToMask, baseProps.AlphaCutoutThreshold, (maskProp, cutoutProp) =>
            {
                if (renderingMode.intValue == (int)RKRenderingMode.AlphaCutout)
                {
                    EditorGUILayout.PropertyField(maskProp);

                    if (maskProp.boolValue == false)
                    {
                        EditorGUILayout.PropertyField(cutoutProp);
                    }
                }
            });
        }

        protected override void OnBeforeInspector()
        {
            base.OnBeforeInspector();
            AutoFix(AutoFixIsUsingMipMaps(), AutoFixDisableMipMaps, $"{nameof(CanvasRenderTexture)} " +
            $"is generating mip maps, but these are ignored when using OVR Overlay/Underlay rendering.");
        }


        private bool AutoFix(bool needsFix, Action fixAction, string message)
        {
            if (needsFix)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(message, MessageType.Warning);
                    if (GUILayout.Button("Auto-Fix", GUILayout.ExpandHeight(true)))
                    {
                        fixAction();
                    }
                }
            }

            return needsFix;
        }

        private bool AutoFixIsUsingMipMaps()
        {
            var modeProp = serializedObject.FindProperty(baseProps.RenderingMode);
            RKRenderingMode mode = (RKRenderingMode)modeProp.intValue;
            if (mode != RKRenderingMode.Overlay && mode != RKRenderingMode.Underlay)
            {
                return false;
            }

            var rtProp = serializedObject.FindProperty(props.CanvasRenderTexture);
            CanvasRenderTexture canvasRT = rtProp.objectReferenceValue as CanvasRenderTexture;
            if (canvasRT == null)
            {
                return false;
            }

            var mipProp = new SerializedObject(canvasRT).FindProperty(rtprops.GenerateMipMaps);
            return mipProp.boolValue;
        }

        private void AutoFixDisableMipMaps()
        {
            var rtProp = serializedObject.FindProperty(props.CanvasRenderTexture);
            CanvasRenderTexture canvasRT = rtProp.objectReferenceValue as CanvasRenderTexture;
            var rtSO = new SerializedObject(canvasRT);
            var mipProp = rtSO.FindProperty(rtprops.GenerateMipMaps);
            mipProp.boolValue = false;
            rtSO.ApplyModifiedProperties();
        }
    }

}
