using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace Rokid.UXR.Interaction
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [DisallowMultipleComponent]
    public class CanvasRenderTexture : MonoBehaviour
    {
        private class TransformChangeListener : MonoBehaviour
        {
            public event Action WhenRectTransformDimensionsChanged = delegate { };

            private void OnRectTransformDimensionsChange()
            {
                WhenRectTransformDimensionsChanged();
            }
        }

        public enum DriveMode
        {
            Auto,
            Manual
        }

        public const int DEFAULT_UI_LAYERMASK = 1 << 5; //Hardcoded as the UI layer in Unity.

        private static readonly Vector2Int DEFAULT_TEXTURE_RES = new Vector2Int(128, 128);

        [SerializeField]
        private Canvas _canvas;

        [Tooltip("If you need extra resolution, you can use this as a whole-integer multiplier of the final resolution used to render the texture.")]
        [Range(1, 100)]
        [Delayed]
        [SerializeField]
        private int _renderScale = 1;

        [SerializeField]
        private DriveMode _dimensionsDriveMode = DriveMode.Auto;

        [Tooltip("The exact pixel resolution of the texture used for interface rendering.  If set to auto this will take the size of the attached " +
        "RectTransform into consideration, in addition to the configured pixel-to-meter ratio.")]
        [Delayed]
        [SerializeField]
        private Vector2Int _resolution = DEFAULT_TEXTURE_RES;

        [Tooltip("Whether or not mip-maps should be auto-generated for the texture.  Can help aliasing if the texture can be " +
        "viewed from many difference distances.")]
        [SerializeField]
        private bool _generateMipMaps = false;

        [SerializeField]
        private int _pixelsPerUnit = 100;

        [Header("Rendering Settings")]
        [Tooltip("The layers to render when the rendering texture is created.  All child renderers should be part of this mask.")]
        [SerializeField]
        private LayerMask _renderingLayers = DEFAULT_UI_LAYERMASK;

        public LayerMask RenderingLayers => _renderingLayers;

        public Action<Texture> OnUpdateRenderTexture = delegate { };

        public void SetPixelsPerUnit(int value)
        {
            _pixelsPerUnit = value;
        }

        public int GetPixelsPerUnit()
        {
            return _pixelsPerUnit;
        }

        public int RenderScale
        {
            get
            {
                return _renderScale;
            }
            set
            {
                if (_renderScale < 1 || _renderScale > 3)
                {
                    throw new ArgumentException($"Render scale must be between 1 and 3, but was {value}");
                }

                if (_renderScale == value)
                {
                    return;
                }

                _renderScale = value;

                if (isActiveAndEnabled && Application.isPlaying)
                {
                    UpdateCamera();
                }
            }
        }

        public Camera OverlayCamera => _camera;

        public Texture Texture => _tex;

        private TransformChangeListener _listener;
        private RenderTexture _tex;
        private Camera _camera;


        public Vector2Int CalcAutoResolution()
        {
            // RKLog.Debug("====CanvasRenderTexture====: CalcAutoResolution");
            if (_canvas == null)
            {
                return DEFAULT_TEXTURE_RES;
            }

            var rectTransform = _canvas.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return DEFAULT_TEXTURE_RES;
            }

            Vector2 size = rectTransform.sizeDelta;
            size.x *= rectTransform.lossyScale.x;
            size.y *= rectTransform.lossyScale.y;

            int x = Mathf.RoundToInt(UnitsToPixels(size.x));
            int y = Mathf.RoundToInt(UnitsToPixels(size.y));

            // RKLog.Debug($"====CanvasRenderTexture====: CalcAutoResolution {x},{y}");
            return new Vector2Int(Mathf.Max(x, 1), Mathf.Max(y, 1));
        }

        public Vector2Int GetBaseResolutionToUse()
        {
            if (_dimensionsDriveMode == DriveMode.Auto)
            {
                return CalcAutoResolution();
            }
            else
            {
                return _resolution;
            }
        }

        public Vector2Int GetScaledResolutionToUse()
        {
            Vector2 resolution = GetBaseResolutionToUse();
            return Vector2Int.RoundToInt(resolution * _renderScale);
        }

        public float PixelsToUnits(float pixels)
        {
            return (1f / _pixelsPerUnit) * pixels;
        }

        public float UnitsToPixels(float units)
        {
            return _pixelsPerUnit * units;
        }


        protected void OnEnable()
        {
            if (_listener == null)
            {
                _listener = _canvas.gameObject.AddComponent<TransformChangeListener>();
            }
            _listener.WhenRectTransformDimensionsChanged += WhenCanvasRectTransformDimensionsChanged;
            UpdateCamera();
        }

        private void WhenCanvasRectTransformDimensionsChanged()
        {
            UpdateCamera();
        }

        protected void OnDisable()
        {
            if (_camera != null && _camera.gameObject != null)
            {
                Destroy(_camera.gameObject);
            }
            if (_tex != null)
            {
                DestroyImmediate(_tex);
            }
            if (_listener != null)
            {
                _listener.WhenRectTransformDimensionsChanged -= WhenCanvasRectTransformDimensionsChanged;
            }
        }

        public void UpdateCamera()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            Profiler.BeginSample("InterfaceRenderer.UpdateCamera");
            try
            {
                if (_camera == null)
                {
                    // RKLog.Debug("====CanvasRenderTexture====: UpdateCamera Camera is Null");
                    GameObject cameraObj = CreateChildObject("__Camera");
                    _camera = cameraObj.AddComponent<Camera>();

                    _camera.orthographic = true;
                    _camera.nearClipPlane = -5;
                    _camera.farClipPlane = 5f;
                    _camera.backgroundColor = new Color(0, 0, 0, 0);
                    _camera.clearFlags = CameraClearFlags.SolidColor;
                }

                UpdateRenderTexture();
                UpdateOrthoSize();
                UpdateCameraCullingMask();
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void Update()
        {
            UpdateRenderTexture();
        }

        protected void UpdateRenderTexture()
        {
            Profiler.BeginSample("InterfaceRenderer.UpdateRenderTexture");
            try
            {
                Vector2Int resolutionToUse = GetScaledResolutionToUse();

                if (_tex == null ||
                    _tex.width != resolutionToUse.x ||
                    _tex.height != resolutionToUse.y ||
                    _tex.autoGenerateMips != _generateMipMaps)
                {
                    if (_tex != null)
                    {
                        _camera.targetTexture = null;
                        DestroyImmediate(_tex);
                    }

                    _tex = new RenderTexture(resolutionToUse.x, resolutionToUse.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                    _tex.filterMode = FilterMode.Bilinear;
                    _tex.autoGenerateMips = _generateMipMaps;
                    _camera.targetTexture = _tex;

                    OnUpdateRenderTexture(_tex);
                    //RKLog.KeyInfo("====CanvasRenderTexture====: UpdateRenderTexture");
                }
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void UpdateOrthoSize()
        {
            if (_camera != null)
            {
                _camera.orthographicSize = PixelsToUnits(GetBaseResolutionToUse().y) * 0.5f;
            }
        }

        private void UpdateCameraCullingMask()
        {
            if (_camera != null)
            {
                _camera.cullingMask = _renderingLayers.value;
            }
        }

        protected GameObject CreateChildObject(string name)
        {
            GameObject obj = new GameObject(name);

            obj.transform.SetParent(_canvas.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            return obj;
        }

        public static class Properties
        {
            public static readonly string DimensionDriveMode = nameof(_dimensionsDriveMode);
            public static readonly string Resolution = nameof(_resolution);
            public static readonly string RenderScale = nameof(_renderScale);
            public static readonly string PixelsPerUnit = nameof(_pixelsPerUnit);
            public static readonly string RenderLayers = nameof(_renderingLayers);
            public static readonly string GenerateMipMaps = nameof(_generateMipMaps);
            public static readonly string Canvas = nameof(_canvas);
        }

    }
}
