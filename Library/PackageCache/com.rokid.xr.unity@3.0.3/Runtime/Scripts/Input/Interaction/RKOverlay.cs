using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Rokid.UXR.Interaction
{

    [ExecuteInEditMode]
    public class RKOverlay : MonoBehaviour
    {
        #region Interface

        /// <summary>
        /// Determines the on-screen appearance of a layer.
        /// </summary>
        public enum OverlayShape
        {
            Quad = 0,
            Cylinder = 1,
            Cubemap = 2,
            OffcenterCubemap = 3,
            Equirect = 4,
            ReconstructionPassthrough = 5,
            SurfaceProjectedPassthrough = 6,
            Fisheye = 7,
            KeyboardHandsPassthrough = 8,
            KeyboardMaskedHandsPassthrough = 9,
        }

        /// <summary>
        /// Whether the layer appears behind or infront of other content in the scene.
        /// </summary>
        public enum OverlayType
        {
            None,
            Underlay,
            Overlay,
        };

        /// <summary>
        /// Specify overlay's type
        /// </summary>
        [Tooltip("Specify overlay's type")]
        public OverlayType currentOverlayType = OverlayType.Overlay;

        /// <summary>
        /// If true, the texture's content is copied to the compositor each frame.
        /// </summary>
        [Tooltip("If true, the texture's content is copied to the compositor each frame.")]
        public bool isDynamic = false;

        /// <summary>
        /// If true, the layer would be used to present protected content (e.g. HDCP). The flag is effective only on PC.
        /// </summary>
        [Tooltip("If true, the layer would be used to present protected content (e.g. HDCP). The flag is effective only on PC.")]
        public bool isProtectedContent = false;

        //Source and dest rects
        public Rect srcRectLeft = new Rect();
        public Rect srcRectRight = new Rect();
        public Rect destRectLeft = new Rect();
        public Rect destRectRight = new Rect();

        // Used to support legacy behavior where the top left was considered the origin
        public bool invertTextureRects = false;


        public bool overrideTextureRectMatrix = false;

        public bool overridePerLayerColorScaleAndOffset = false;

        public Vector4 colorScale = Vector4.one;

        public Vector4 colorOffset = Vector4.zero;

        //Warning: Developers should only use this supersample setting if they absolutely have the budget and need for it. It is extremely expensive, and will not be relevant for most developers.
        public bool useExpensiveSuperSample = false;

        //Warning: Developers should only use this sharpening setting if they absolutely have the budget and need for it. It is extremely expensive, and will not be relevant for most developers.
        public bool useExpensiveSharpen = false;

        //Property that can hide overlays when required. Should be false when present, true when hidden.
        public bool hidden = false;


        /// <summary>
        /// If true, the layer will be created as an external surface. externalSurfaceObject contains the Surface object. It's effective only on Android.
        /// </summary>
        [Tooltip("If true, the layer will be created as an external surface. externalSurfaceObject contains the Surface object. It's effective only on Android.")]
        public bool isExternalSurface = false;

        /// <summary>
        /// The width which will be used to create the external surface. It's effective only on Android.
        /// </summary>
        [Tooltip("The width which will be used to create the external surface. It's effective only on Android.")]
        public int externalSurfaceWidth = 0;

        /// <summary>
        /// The height which will be used to create the external surface. It's effective only on Android.
        /// </summary>
        [Tooltip("The height which will be used to create the external surface. It's effective only on Android.")]
        public int externalSurfaceHeight = 0;

        /// <summary>
        /// The compositionDepth defines the order of the OVROverlays in composition. The overlay/underlay with smaller compositionDepth would be composited in the front of the overlay/underlay with larger compositionDepth.
        /// </summary>
        [Tooltip("The compositionDepth defines the order of the OVROverlays in composition. The overlay/underlay with smaller compositionDepth would be composited in the front of the overlay/underlay with larger compositionDepth.")]
        public int compositionDepth = 0;
        private int layerCompositionDepth = 0;

        /// <summary>
        /// The noDepthBufferTesting will stop layer's depth buffer compositing even if the engine has "Depth buffer sharing" enabled on Rift.
        /// </summary>
        [Tooltip("The noDepthBufferTesting will stop layer's depth buffer compositing even if the engine has \"Shared Depth Buffer\" enabled. The layer's ordering will be used instead which is determined by it's composition depth and overlay/underlay type.")]
        public bool noDepthBufferTesting = true;

        /// <summary>
        /// Specify overlay's shape
        /// </summary>
        [Tooltip("Specify overlay's shape")]
        public OverlayShape currentOverlayShape = OverlayShape.Quad;
        private OverlayShape prevOverlayShape = OverlayShape.Quad;

        /// <summary>
        /// The left- and right-eye Textures to show in the layer.
        /// \note If you need to change the texture on a per-frame basis, please use OverrideOverlayTextureInfo(..) to avoid caching issues.
        /// </summary>
        [Tooltip("The left- and right-eye Textures to show in the layer.")]
        public Texture[] textures = new Texture[] { null, null };

        [Tooltip("When checked, the texture is treated as if the alpha was already premultiplied")]
        public bool isAlphaPremultiplied = false;

        [Tooltip("When checked, the layer will use bicubic filtering")]
        public bool useBicubicFiltering = false;

        [Tooltip("When checked, the cubemap will retain the legacy rotation which was rotated 180 degrees around the Y axis comapred to Unity's definition of cubemaps. This setting will be deprecated in the near future, therefore it is recommended to fix the cubemap texture instead.")]
        public bool useLegacyCubemapRotation = false;

        [Tooltip("When checked, the layer will use efficient super sampling")]
        public bool useEfficientSupersample = false;

        [Tooltip("When checked, the layer will use efficient sharpen.  Must have anisotropic filtering and mipmaps enabled.")]
        public bool useEfficientSharpen = false;

        /// <summary>
        /// Preview the overlay in the editor using a mesh renderer.
        /// </summary>
        public bool previewInEditor
        {
            get
            {
                return _previewInEditor;
            }
            set
            {
                if (_previewInEditor != value)
                {
                    _previewInEditor = value;
                }
            }
        }

        [SerializeField]
        internal bool _previewInEditor = false;

#if UNITY_EDITOR
        private GameObject previewObject;
#endif

        protected IntPtr[] texturePtrs = new IntPtr[] { IntPtr.Zero, IntPtr.Zero };

        /// <summary>
        /// The Surface object (Android only).
        /// </summary>
        public System.IntPtr externalSurfaceObject;

        public delegate void ExternalSurfaceObjectCreated();
        /// <summary>
        /// Will be triggered after externalSurfaceTextueObject get created.
        /// </summary>
        public ExternalSurfaceObjectCreated externalSurfaceObjectCreated;

        /// <summary>
        /// Use this function to set texture and texNativePtr when app is running
        /// GetNativeTexturePtr is a slow behavior, the value should be pre-cached
        /// </summary>
        public void OverrideOverlayTextureInfo(Texture srcTexture, IntPtr nativePtr, UnityEngine.XR.XRNode node)
        {
            int index = (node == UnityEngine.XR.XRNode.RightEye) ? 1 : 0;

            if (textures.Length <= index)
                return;

            textures[index] = srcTexture;
            texturePtrs[index] = nativePtr;

            isOverridePending = true;
        }

        protected bool isOverridePending;

        internal const int maxInstances = 15;
        public static RKOverlay[] instances = new RKOverlay[maxInstances];

        public int layerId { get; private set; } = 0; // The layer's internal handle in the compositor.

        #endregion

        private static Material tex2DMaterial;
        private static Material cubeMaterial;


        private struct LayerTexture
        {
            public Texture appTexture;
            public IntPtr appTexturePtr;
            public Texture[] swapChain;
            public IntPtr[] swapChainPtr;
        };
        private LayerTexture[] layerTextures;

        private int stageCount = -1;

        private int layerIndex = -1; // Controls the composition order based on wake-up time.
        private GCHandle layerIdHandle;
        private IntPtr layerIdPtr = IntPtr.Zero;

        private int frameIndex = 0;
        private int prevFrameIndex = -1;

        private Renderer rend;


    }
}
