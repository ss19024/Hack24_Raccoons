
namespace Rokid.UXR.Interaction
{
    using UnityEngine;
    using UnityEngine.Profiling;
    using UnityEngine.Assertions;
    public enum RenderingMode
    {
        [InspectorName("Alpha-Blended")]
        AlphaBlended = 0,
        [InspectorName("Alpha-Cutout")]
        AlphaCutout,
        [InspectorName("Opaque")]
        Opaque,
    }

    public class CanvasMeshRenderer : MonoBehaviour
    {
        private static readonly int MainTexShaderID = Shader.PropertyToID("_MainTex");

        [SerializeField]
        protected CanvasRenderTexture _canvasRenderTexture;

        [SerializeField]
        protected MeshRenderer _meshRenderer;

        [SerializeField]
        protected int _renderingMode = (int)RenderingMode.AlphaCutout;

        [Tooltip("Requires MSAA.  Provides limited transparency useful for " +
                 "anti-aliasing soft edges of UI elements.")]
        [SerializeField]
        private bool _useAlphaToMask = true;

        [Tooltip("Select the alpha cutoff used for the cutout rendering.")]
        [Range(0, 1)]
        [SerializeField]
        private float _alphaCutoutThreshold = 0.5f;

        private RenderingMode RenderingMode => (RenderingMode)_renderingMode;

        protected virtual string GetShaderName()
        {
            switch (RenderingMode)
            {
                case RenderingMode.AlphaBlended:
                    return "Hidden/Imposter_AlphaBlended";
                case RenderingMode.AlphaCutout:
                    if (_useAlphaToMask)
                    {
                        return "Hidden/Imposter_AlphaToMask";
                    }
                    else
                    {
                        return "Hidden/Imposter_AlphaCutout";
                    }
                default:
                case RenderingMode.Opaque:
                    return "Hidden/Imposter_Opaque";
            }
        }

        protected virtual void SetAdditionalProperties(MaterialPropertyBlock block)
        {
            block.SetFloat("_Cutoff", GetAlphaCutoutThreshold());
        }

        protected virtual float GetAlphaCutoutThreshold()
        {
            if (RenderingMode == RenderingMode.AlphaCutout &&
                !_useAlphaToMask)
            {
                return _alphaCutoutThreshold;
            }
            return 1f;
        }

        protected Material _material;
        protected bool _started;

        protected virtual void HandleUpdateRenderTexture(Texture texture)
        {
            // RKLog.Debug($"====CanvasMeshRenderer====:+ HandleUpdateRenderTexture");
            _meshRenderer.material = _material;
            var block = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(block);
            block.SetTexture(MainTexShaderID, texture);
            SetAdditionalProperties(block);
            _meshRenderer.SetPropertyBlock(block);
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            Assert.IsNotNull(_meshRenderer);
            Assert.IsNotNull(_canvasRenderTexture);
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                Profiler.BeginSample("InterfaceRenderer.UpdateMaterial");
                try
                {
                    _material = new Material(Shader.Find(GetShaderName()));
                }
                finally
                {
                    Profiler.EndSample();
                }

                _canvasRenderTexture.OnUpdateRenderTexture += HandleUpdateRenderTexture;
                if (_canvasRenderTexture.Texture != null)
                {
                    HandleUpdateRenderTexture(_canvasRenderTexture.Texture);
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                if (_material != null)
                {
                    Destroy(_material);
                    _material = null;
                }
                _canvasRenderTexture.OnUpdateRenderTexture -= HandleUpdateRenderTexture;
            }
        }

        public static partial class Properties
        {
            public static readonly string RenderingMode = nameof(_renderingMode);
            public static readonly string UseAlphaToMask = nameof(_useAlphaToMask);
            public static readonly string AlphaCutoutThreshold = nameof(_alphaCutoutThreshold);
        }

    }

}
