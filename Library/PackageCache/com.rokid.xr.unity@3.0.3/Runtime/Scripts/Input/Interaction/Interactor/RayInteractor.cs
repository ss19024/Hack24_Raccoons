using System.Collections.Generic;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class RayInteractor : PointerInteractor<RayInteractor, RayInteractable>, IHeadHandDriver
    {
        [SerializeField]
        private HandType hand = HandType.None;
        [SerializeField, Interface(typeof(ISelector))]
        private MonoBehaviour _selector;

        [SerializeField]
        private Transform _rayOrigin;

        [SerializeField]
        private float _maxRayLength = 5f;

        [SerializeField]
        private float _noHoverCursorDistance = 5f;

        [SerializeField]
        [Tooltip("(Meters, World) The threshold below which distances to a surface " +
                 "are treated as equal for the purposes of ranking.")]
        private float _equalDistanceThreshold = 0.001f;

        private RayCandidateProperties _rayCandidateProperties = null;

        private IMovement _movement;
        private SurfaceHit _movedHit;
        private Pose _movementHitDelta = Pose.identity;

        private float _currentRayLength = 1.0f;

        public Vector3 Origin { get; protected set; }
        public Quaternion Rotation { get; protected set; }
        public Vector3 Forward { get; protected set; }
        public Vector3 End { get; set; }

        public Transform GetRayOriginTsf()
        {
            return _rayOrigin;
        }


        public float CurrentRayLength
        {
            get
            {
                return _currentRayLength;
            }
        }
        public float NoHoverCursorDistance
        {
            get
            {
                return _noHoverCursorDistance;
            }
            set
            {
                _noHoverCursorDistance = value;
            }
        }

        public float MaxRayLength
        {
            get
            {
                return _maxRayLength;
            }
            set
            {
                _maxRayLength = value;
            }
        }

        private float _ProjectNoHoverCursorDistance;

        public SurfaceHit? CollisionInfo { get; protected set; }
        public Ray Ray { get; protected set; }

        public static List<RayInteractor> RayInteractors = new List<RayInteractor>();

        protected override void Awake()
        {
            base.Awake();
            Selector = _selector as ISelector;
        }

        protected override void Start()
        {
            base.Start();
            RayInteractors.Add(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RayInteractors.Remove(this);
        }

        public static Ray GetRayByIdentifier(int identity)
        {
            for (int i = 0; i < RayInteractors.Count; i++)
            {
                if (RayInteractors[i].realId == identity)
                {
                    return RayInteractors[i].Ray;
                }
            }
            return default(Ray);
        }

        public static HandType GetHandTypeByIdentifier(int identity)
        {
            for (int i = 0; i < RayInteractors.Count; i++)
            {
                if (RayInteractors[i].realId == identity)
                {
                    return RayInteractors[i].hand;
                }
            }
            return HandType.None;
        }

        public static RayInteractor GetRayInteractorByIdentifier(int identity)
        {
            for (int i = 0; i < RayInteractors.Count; i++)
            {
                if (RayInteractors[i].realId == identity)
                {
                    return RayInteractors[i];
                }
            }
            return null;
        }

        protected override void DoPreprocess()
        {
            Origin = _rayOrigin.transform.position;
            Rotation = _rayOrigin.transform.rotation;
            Forward = Rotation * Vector3.forward;
            Ray = new Ray(Origin, Forward);
        }

        public class RayCandidateProperties : ICandidatePosition
        {
            public RayInteractable ClosestInteractable { get; set; }
            public Vector3 CandidatePosition { get; set; }
            public RayCandidateProperties(RayInteractable closestInteractable, Vector3 candidatePosition)
            {
                ClosestInteractable = closestInteractable;
                CandidatePosition = candidatePosition;
            }
        }

        public override object CandidateProperties => _rayCandidateProperties;


        protected override RayInteractable ComputeCandidate()
        {
            CollisionInfo = null;

            RayInteractable closestInteractable = null;
            float closestDist = float.MaxValue;
            Vector3 candidatePosition = Vector3.zero;
            var interactables = RayInteractable.Registry.List(this);
            //使用射线检测关键的碰撞物体
            foreach (RayInteractable interactable in interactables)
            {
                if (interactable.Raycast(Ray, out SurfaceHit hit, MaxRayLength, false))
                {
                    bool equal = Mathf.Abs(hit.Distance - closestDist) < _equalDistanceThreshold;
                    if ((!equal && hit.Distance < closestDist) ||
                        (equal && interactable.TiebreakerScore > closestInteractable.TiebreakerScore))
                    {
                        closestDist = hit.Distance;
                        closestInteractable = interactable;
                        CollisionInfo = hit;
                        candidatePosition = hit.Point;
                    }
                }
            }

            _ProjectNoHoverCursorDistance = NoHoverCursorDistance;
            if (closestInteractable == null && MainCameraCache.mainCamera != null)
            {
                Vector3 ProjectDir = Vector3.ProjectOnPlane(MainCameraCache.mainCamera.transform.forward, Vector3.up).normalized;
                float AngleY = Vector3.SignedAngle(MainCameraCache.mainCamera.transform.forward.normalized, ProjectDir, Vector3.up) * Mathf.Deg2Rad;
                _ProjectNoHoverCursorDistance = NoHoverCursorDistance / Mathf.Cos(AngleY);
            }

            float rayDist = (closestInteractable != null ? closestDist : _ProjectNoHoverCursorDistance);
            End = Origin + rayDist * Forward;

            if (_rayCandidateProperties == null)
            {
                _rayCandidateProperties = new RayCandidateProperties(closestInteractable, candidatePosition);
            }
            else
            {
                _rayCandidateProperties.ClosestInteractable = closestInteractable;
                _rayCandidateProperties.CandidatePosition = candidatePosition;
            }
            return closestInteractable;
        }

        protected override void InteractableSelected(RayInteractable interactable)
        {
            if (interactable != null)
            {
                _movedHit = CollisionInfo.Value;
                Pose hitPose = new Pose(_movedHit.Point, Quaternion.LookRotation(_movedHit.Normal));
                Pose backHitPose = new Pose(_movedHit.Point, Quaternion.LookRotation(-_movedHit.Normal));
                _movement = interactable.GenerateMovement(_rayOrigin.GetPose(), backHitPose);
                if (_movement != null)
                {
                    _movementHitDelta = PoseUtils.Delta(_movement.Pose, hitPose);
                }
            }
            base.InteractableSelected(interactable);
        }

        protected override void InteractableUnselected(RayInteractable interactable)
        {
            if (_movement != null)
            {
                _movement.StopAndSetPose(_movement.Pose);
            }
            base.InteractableUnselected(interactable);
            _movement = null;
        }

        protected override void DoSelectUpdate()
        {
            RayInteractable interactable = _selectedInteractable;

            if (_movement != null)
            {
                _movement.UpdateTarget(_rayOrigin.GetPose());
                _movement.Tick();
                Pose hitPoint = PoseUtils.Multiply(_movement.Pose, _movementHitDelta);
                _movedHit.Point = hitPoint.position;
                _movedHit.Normal = hitPoint.forward;
                CollisionInfo = _movedHit;
                End = _movedHit.Point;
                return;
            }

            CollisionInfo = null;
            if (interactable != null &&
                interactable.Raycast(Ray, out SurfaceHit hit, MaxRayLength, true))
            {
                End = hit.Point;
                CollisionInfo = hit;
            }
            else
            {
                // End = Origin + MaxRayLength * Forward;
                End = Origin + _ProjectNoHoverCursorDistance * Forward;
            }
        }

        protected override Pose ComputePointerPose()
        {
            if (_movement != null)
            {
                return _movement.Pose;
            }

            if (CollisionInfo != null)
            {
                Vector3 position = CollisionInfo.Value.Point;
                Quaternion rotation = Quaternion.LookRotation(CollisionInfo.Value.Normal);
                return new Pose(position, rotation);
            }
            return new Pose(Vector3.zero, Quaternion.identity);
        }

        public void OnChangeHoldHandType(HandType hand)
        {
            this.hand = hand;
        }

        public void OnHandPress(HandType hand)
        {

        }

        public void OnHandRelease()
        {

        }

        public void OnBeforeChangeHoldHandType(HandType hand)
        {

        }


        protected override void Update()
        {
            base.Update();
            switch (State)
            {
                case InteractorState.Hover:
                    _currentRayLength = Mathf.Clamp(transform.InverseTransformPoint(End).z, 0.5f, _maxRayLength);
                    break;
                case InteractorState.Normal:
                    NoHoverCursorDistance = _currentRayLength;
                    break;
            }
        }
    }
}
