using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class PlaneSurface : MonoBehaviour, ISurface, IBounds
    {
        public enum NormalFacing
        {
            /// <summary>
            /// Normal faces along the transform's negative Z axis
            /// </summary>
            Backward,

            /// <summary>
            /// Normal faces along the transform's positive Z axis
            /// </summary>
            Forward,
        }

        [SerializeField]
        private NormalFacing _facing = NormalFacing.Backward;

        [SerializeField, Tooltip("Raycasts hit either side of plane, but hit normal " +
        "will still respect plane facing.")]
        private bool _doubleSided = false;

        public NormalFacing Facing
        {
            get => _facing;
            set => _facing = value;
        }

        public bool DoubleSided
        {
            get => _doubleSided;
            set => _doubleSided = value;
        }

        public Vector3 Normal
        {
            get
            {
                return _facing == NormalFacing.Forward ?
                                  transform.forward :
                                  -transform.forward;
            }
        }

        private bool IsPointAboveSurface(Vector3 point)
        {
            Plane plane = GetPlane();
            return plane.GetSide(point);
        }

        public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance)
        {
            hit = new SurfaceHit();
            Plane plane = GetPlane();

            float hitDistance = plane.GetDistanceToPoint(point);
            if (maxDistance > 0 && Mathf.Abs(hitDistance) > maxDistance)
            {
                return false;
            }

            hit.Point = plane.ClosestPointOnPlane(point);
            hit.Distance = IsPointAboveSurface(point) ? hitDistance : -hitDistance;
            hit.Normal = plane.normal;

            return true;
        }

        public Transform Transform => transform;

        public Bounds Bounds
        {
            get
            {
                Vector3 size = new Vector3(
                    Mathf.Abs(Normal.x) == 1f ? float.Epsilon : float.PositiveInfinity,
                    Mathf.Abs(Normal.y) == 1f ? float.Epsilon : float.PositiveInfinity,
                    Mathf.Abs(Normal.z) == 1f ? float.Epsilon : float.PositiveInfinity);
                return new Bounds(transform.position, size);
            }
        }

        public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
        {
            hit = new SurfaceHit();
            Plane plane = GetPlane();

            if (!_doubleSided && !IsPointAboveSurface(ray.origin))
            {
                return false;
            }

            if (plane.Raycast(ray, out float hitDistance))
            {
                if (maxDistance > 0 && hitDistance > maxDistance)
                {
                    RKLog.Warning($"hit distance over max distance: {hitDistance}");
                    return false;
                }

                hit.Point = ray.GetPoint(hitDistance);
                hit.Normal = plane.normal;
                hit.Distance = hitDistance;
                return true;
            }

            return false;
        }

        public Plane GetPlane()
        {
            return new Plane(Normal, transform.position);
        }
    }
}
