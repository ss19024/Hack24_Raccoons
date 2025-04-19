using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace Rokid.UXR.Interaction
{
    using OverlayType = RKOverlay.OverlayType;
    using OverlayShape = RKOverlay.OverlayShape;

    public enum RKRenderingMode
    {
        [InspectorName("Alpha-Blended")]
        AlphaBlended = RenderingMode.AlphaBlended,
        [InspectorName("Alpha-Cutout")]
        AlphaCutout = RenderingMode.AlphaCutout,
        [InspectorName("Opaque")]
        Opaque = RenderingMode.Opaque,

        [InspectorName("RK/Overlay")]
        Overlay = 100,
        [InspectorName("RK/Underlay")]
        Underlay,
    }

    /// <summary>
    /// Uses <see cref="RKOverlay"/> to enable Underlay and Overlay
    /// rendering of a UI canvas.
    /// </summary>
    public class RKCanvasMeshRenderer : CanvasMeshRenderer
    {
        [SerializeField]
        protected CanvasMesh _canvasMesh;

        [Tooltip("If non-zero it will cause the position of the overlay to be offset by this amount at runtime, while " +
        "the renderer will remain where it was at edit time. This can be used to prevent the two representations from overlapping.")]
        [SerializeField]
        protected Vector3 _runtimeOffset = new Vector3(0, 0, 0);

        [Tooltip(
        "Uses a more expensive image sampling technique for improved quality at the cost of performance.")]
        [SerializeField]
        protected bool _enableSuperSampling = true;

        [Tooltip(
    "Attempts to anti-alias the edges of the underlay by using alpha blending.  Can cause borders of " +
        "darkness around partially transparent objects.")]
        [SerializeField]
        private bool _doUnderlayAntiAliasing = false;
        private bool _emulateWhileInEditor = true;

        protected RKOverlay _overlay;

        private RKRenderingMode RenderingMode => (RKRenderingMode)_renderingMode;

        public bool ShouldUseOVROverlay
        {
            get
            {
                switch (RenderingMode)
                {
                    case RKRenderingMode.Underlay:
                    case RKRenderingMode.Overlay:
                        return !UseEditorEmulation();
                    default:
                        return false;
                }
            }
        }

        protected override string GetShaderName()
        {
            switch (RenderingMode)
            {
                case RKRenderingMode.Overlay:
                    return "Hidden/Imposter_AlphaCutout";
                case RKRenderingMode.Underlay:
                    if (UseEditorEmulation())
                    {
                        return "Hidden/Imposter_AlphaCutout";
                    }
                    else if (_doUnderlayAntiAliasing)
                    {
                        return "Hidden/Imposter_Underlay_AA";
                    }
                    else
                    {
                        return "Hidden/Imposter_Underlay";
                    }
                default:
                    return base.GetShaderName();
            }
        }

        protected override float GetAlphaCutoutThreshold()
        {
            switch (RenderingMode)
            {
                case RKRenderingMode.Overlay:
                    return 1f;
                case RKRenderingMode.Underlay:
                    return UseEditorEmulation() ? 0.5f : 1f;
                default:
                    return base.GetAlphaCutoutThreshold();
            }
        }

        protected override void HandleUpdateRenderTexture(Texture texture)
        {
            base.HandleUpdateRenderTexture(texture);
            UpdateOverlay(texture);
        }

        private bool UseEditorEmulation()
        {
            return Application.isEditor ? _emulateWhileInEditor : false;
        }

        private bool GetOverlayParameters(out OverlayShape shape,
                                          out Vector3 position,
                                          out Vector3 scale)
        {
            if (_canvasMesh is CanvasCylinder canvasCylinder)
            {
                shape = OverlayShape.Cylinder;
                Vector2Int resolution = _canvasRenderTexture.GetBaseResolutionToUse();
                position = new Vector3(0, 0, -canvasCylinder.Radius) - _runtimeOffset;
                scale = new Vector3(_canvasRenderTexture.PixelsToUnits(resolution.x) /
                                        canvasCylinder.transform.lossyScale.x,
                                    _canvasRenderTexture.PixelsToUnits(resolution.y) /
                                        canvasCylinder.transform.lossyScale.y,
                                    canvasCylinder.Radius);
                return true;
            }
            else if (_canvasMesh is CanvasRect canvasRect)
            {
                shape = OverlayShape.Quad;
                Vector2Int resolution = _canvasRenderTexture.GetBaseResolutionToUse();
                position = -_runtimeOffset;
                scale = new Vector3(_canvasRenderTexture.PixelsToUnits(resolution.x),
                                    _canvasRenderTexture.PixelsToUnits(resolution.y),
                                    1);
                return true;
            }
            else // Unsupported
            {
                shape = OverlayShape.Quad;
                position = Vector3.zero;
                scale = Vector3.zero;
                return false;
            }
        }

        protected override void Start()
        {
            this.BeginStart(ref _started, () => base.Start());
            Assert.IsNotNull(_canvasMesh);
            Assert.IsTrue(GetOverlayParameters(out _, out _, out _),
                $"Unsupported {nameof(CanvasMesh)} type");
            this.EndStart(ref _started);
        }

        protected void UpdateOverlay(Texture texture)
        {
            Profiler.BeginSample("InterfaceRenderer.UpdateOverlay");
            try
            {
                if (!ShouldUseOVROverlay)
                {
                    _overlay?.gameObject?.SetActive(false);
                    return;
                }

                if (_overlay == null)
                {
                    GameObject overlayObj = CreateChildObject("__Overlay");
                    _overlay = overlayObj.AddComponent<RKOverlay>();
                    _overlay.isAlphaPremultiplied = !Application.isMobilePlatform;
                }
                else
                {
                    _overlay.gameObject.SetActive(true);
                }

                if (!GetOverlayParameters(out OverlayShape shape,
                                          out Vector3 pos,
                                          out Vector3 scale))
                {
                    _overlay.gameObject.SetActive(false);
                    return;
                }

                bool useUnderlayRendering = RenderingMode == RKRenderingMode.Underlay;
                _overlay.textures = new Texture[1] { texture };
                _overlay.noDepthBufferTesting = useUnderlayRendering;
                _overlay.currentOverlayType = useUnderlayRendering ? OverlayType.Underlay : OverlayType.Overlay;
                _overlay.currentOverlayShape = shape;
                _overlay.useExpensiveSuperSample = _enableSuperSampling;
                _overlay.transform.localPosition = pos;
                _overlay.transform.localScale = scale;
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        protected GameObject CreateChildObject(string name)
        {
            GameObject obj = new GameObject(name);

            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            return obj;
        }

        public static new class Properties
        {
            public static readonly string CanvasRenderTexture = nameof(_canvasRenderTexture);
            public static readonly string CanvasMesh = nameof(_canvasMesh);
            public static readonly string EnableSuperSampling = nameof(_enableSuperSampling);
            public static readonly string EmulateWhileInEditor = nameof(_emulateWhileInEditor);
            public static readonly string DoUnderlayAntiAliasing = nameof(_doUnderlayAntiAliasing);
            public static readonly string RuntimeOffset = nameof(_runtimeOffset);
        }

    }
}
