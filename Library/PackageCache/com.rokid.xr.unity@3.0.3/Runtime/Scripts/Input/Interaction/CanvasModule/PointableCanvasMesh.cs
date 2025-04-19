using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Rokid.UXR;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public class PointableCanvasMesh : PointableElement
    {
        [SerializeField]
        [FormerlySerializedAs("_canvasRenderTextureMesh")]
        private CanvasMesh _canvasMesh;

        [SerializeField]
        private CanvasRenderTexture _canvasRenderTexture;
        [SerializeField]
        private Cylinder _cylinder;
        [SerializeField, Range(0.3f, 10)]
        public float _cylinderRadius;

        [SerializeField, Tooltip("曲面屏横向放大倍数"), HideInInspector]
        public float _cylinderCanvasScaleWidth = 1.0f;
        [SerializeField, Tooltip("曲面屏纵向放大倍数"), HideInInspector]
        public float _cylinderCanvasScaleHeight = 1.0f;

        [SerializeField, Tooltip("是否修改显示半径")]
        public bool _changeViewRadius;

        [SerializeField]
        private Canvas _unityCanvas;
        private float ori_radius;
        private float ori_scale;
        private bool ori_changeViewRadius;
        public float CylinderRadius { get { return _cylinderRadius; } set { _cylinderRadius = value; } }

        protected override void Start()
        {
            base.Start();
            Assert.IsNotNull(_canvasMesh);

            ori_radius = _cylinder.Radius;
            ori_scale = _unityCanvas.transform.localScale.x;
            ori_changeViewRadius = _changeViewRadius;

            UpdateRadius();
        }

        public override void ProcessPointerEvent(PointerEvent evt)
        {
            // RKLog.Debug($"====PointableCanvasMesh==== Before ProcessPointerEvent {evt.Pose.position}");
            Vector3 transformPosition =
                _canvasMesh.ImposterToCanvasTransformPoint(evt.Pose.position);
            Pose transformedPose = new Pose(transformPosition, evt.Pose.rotation);
            // RKLog.Debug($"====PointableCanvasMesh==== After ProcessPointerEvent {transformedPose.position}");
            base.ProcessPointerEvent(new PointerEvent(evt.Identifier, evt.Type, transformedPose));
        }

        private void Update()
        {
            if (_cylinderRadius != ori_radius || _unityCanvas.transform.localScale.x != ori_scale || ori_changeViewRadius != _changeViewRadius)
            {
                UpdateRadius();
            }
        }

        private void UpdateRadius()
        {
            _cylinder.Radius = _cylinderRadius;

            _cylinder.CylinderCanvasScaleWidth = _cylinderCanvasScaleWidth;
            _cylinder.CylinderCanvasScaleHeight = _cylinderCanvasScaleHeight;

            if (_changeViewRadius)
            {
                transform.position = MainCameraCache.mainCamera.transform.TransformPoint(new Vector3(0, 0, _cylinderRadius));
            }
            _unityCanvas.transform.localPosition = Vector3.zero;
            _cylinder.transform.localPosition = new Vector3(0, 0, -_cylinderRadius);

            _canvasMesh.UpdateImposter();
            _canvasRenderTexture.UpdateCamera();

            ori_radius = _cylinderRadius;
            ori_scale = _unityCanvas.transform.localScale.x;
            ori_changeViewRadius = _changeViewRadius;
        }
    }
}
