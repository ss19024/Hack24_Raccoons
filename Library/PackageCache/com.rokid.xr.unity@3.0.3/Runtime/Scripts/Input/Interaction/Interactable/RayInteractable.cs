using UnityEngine;
using UnityEngine.Assertions;
namespace Rokid.UXR.Interaction
{
    public class RayInteractable : PointerInteractable<RayInteractor, RayInteractable>
    {
        [SerializeField, Interface(typeof(ISurface))]
        private MonoBehaviour _surface;
        public ISurface Surface { get; private set; }

        [SerializeField, Optional, Interface(typeof(ISurface))]
        private MonoBehaviour _selectSurface = null;
        private ISurface SelectSurface;

        [HideInInspector, SerializeField, Optional, Interface(typeof(IMovementProvider))]
        private MonoBehaviour _movementProvider;
        private IMovementProvider MovementProvider { get; set; }

        [HideInInspector, SerializeField, Optional]
        private int _tiebreakerScore = 0;

        #region Properties
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
        #endregion

        protected override void Awake()
        {
            base.Awake();
            Surface = _surface as ISurface;
            SelectSurface = _selectSurface as ISurface;
            MovementProvider = _movementProvider as IMovementProvider;
        }

        protected override void Start()
        {
            Assert.IsNotNull(Surface);
            if (_selectSurface != null)
            {
                Assert.IsNotNull(SelectSurface);
            }
            else
            {
                SelectSurface = Surface;
                _selectSurface = SelectSurface as MonoBehaviour;
            }
        }

        /// <summary>
        /// 射线和平面检测的关键逻辑
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="hit"></param>
        /// <param name="maxDistance"></param>
        /// <param name="selectSurface"></param>
        /// <returns></returns>
        public bool Raycast(Ray ray, out SurfaceHit hit, in float maxDistance, bool selectSurface)
        {
            hit = new SurfaceHit();
            ISurface surface = selectSurface ? SelectSurface : Surface;
            return surface.Raycast(ray, out hit, maxDistance);
        }

        public IMovement GenerateMovement(in Pose to, in Pose source)
        {
            if (MovementProvider == null)
            {
                return null;
            }
            IMovement movement = MovementProvider.CreateMovement();
            movement.StopAndSetPose(source);
            movement.MoveTo(to);
            return movement;
        }
    }
}
