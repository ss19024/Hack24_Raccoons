using UnityEngine;
using UnityEngine.Assertions;
using System;

namespace Rokid.UXR.Interaction
{
    public class PokeInteractable : PointerInteractable<PokeInteractor, PokeInteractable>
    {
        [SerializeField, Interface(typeof(ISurface))]
        public MonoBehaviour _surface;
        public ISurface Surface;

        [SerializeField, Interface(typeof(ISurface))]
        private MonoBehaviour _colliderSurface;
        public ISurface ColliderSurface;

        [SerializeField, Interface(typeof(IProximityField))]
        private MonoBehaviour _proximityField;
        public IProximityField ProximityField;

        [SerializeField]
        public float _maxDistance = 0.1f;
        public float MaxDistance => _maxDistance;

        [SerializeField]
        private float _enterHoverDistance = 0f;

        [SerializeField]
        private float _releaseDistance = 0.25f;

        [HideInInspector, SerializeField, Optional]
        private int _tiebreakerScore = 0;

        [HideInInspector, SerializeField, Optional]
        private Collider _volumeMask = null;

        [HideInInspector, SerializeField, Optional]
        private Transform hitPoint;

        [HideInInspector, SerializeField, Optional]
        private Transform proximityFieldPoint;


        public Collider VolumeMask { get => _volumeMask; }

        /// <summary>
        /// 拖拽阈值的配置
        /// </summary>
        [HideInInspector, Serializable]
        public class DragThresholdingConfig
        {
            public bool Enabled;
            public float SurfaceThreshold;
            public float ZThreshold;
            public ProgressCurve DragEaseCurve;
        }

        [HideInInspector, SerializeField]
        private DragThresholdingConfig _dragThresholding =
            new DragThresholdingConfig()
            {
                Enabled = true,
                SurfaceThreshold = 0.01f,
                ZThreshold = 0.01f,
                DragEaseCurve = new ProgressCurve(AnimationCurve.EaseInOut(0, 0, 1, 1), 0.05f)
            };

        /// <summary>
        /// 位置锁定的配置
        /// </summary>
        [Serializable]
        public class PositionPinningConfig
        {
            public bool Enabled;
            public float MaxPinDistance;
        }

        /// <summary>
        /// 固定位置
        /// </summary>
        /// <returns></returns>
        [HideInInspector, SerializeField]
        private PositionPinningConfig _positionPinning =
            new PositionPinningConfig()
            {
                Enabled = false,
                MaxPinDistance = 0f
            };

        #region Properties
        /// <summary>
        /// 进入悬停的位置
        /// </summary>
        public float EnterHoverDistance => _enterHoverDistance;

        /// <summary>
        /// 释放距离
        /// </summary>
        public float ReleaseDistance => _releaseDistance;

        public int TiebreakerScore
        {
            get
            {
                return _tiebreakerScore;
            }
            set
            {
                _tiebreakerScore = value;
            }
        }

        public DragThresholdingConfig DragThresholding
        {
            get
            {
                return _dragThresholding;
            }

            set
            {
                _dragThresholding = value;
            }
        }

        public PositionPinningConfig PositionPinning
        {
            get
            {
                return _positionPinning;
            }

            set
            {
                _positionPinning = value;
            }
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();
            ProximityField = _proximityField as IProximityField;
            Surface = _surface as ISurface;
            ColliderSurface = _colliderSurface as ISurface;
        }

        protected override void Start()
        {
            Assert.IsNotNull(ProximityField);
            Assert.IsNotNull(Surface);
            if (_enterHoverDistance > 0f)
            {
                _enterHoverDistance = Mathf.Min(_enterHoverDistance, _maxDistance);
            }
        }

        public Vector3 ComputeClosestPoint(Vector3 point)
        {
            //获取邻场近点
            Vector3 proximityFieldPoint = ProximityField.ComputeClosestPoint(point);
            if (this.proximityFieldPoint != null)
            {
                this.proximityFieldPoint.position = proximityFieldPoint;
            }
            //这里应该有点问题,这里是计算最接近平面的点
            Surface.ClosestSurfacePoint(proximityFieldPoint, out SurfaceHit hit);
            if (hitPoint != null && Input.GetMouseButton(0))
            {
                hitPoint.position = hit.Point;
            }
            return hit.Point;
        }

        public Vector3 ClosestSurfacePoint(Vector3 point)
        {
            Surface.ClosestSurfacePoint(point, out SurfaceHit hit);
            return hit.Point;
        }

        public Vector3 ClosestSurfaceNormal(Vector3 point)
        {
            Surface.ClosestSurfacePoint(point, out SurfaceHit hit);
            return hit.Normal;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}
