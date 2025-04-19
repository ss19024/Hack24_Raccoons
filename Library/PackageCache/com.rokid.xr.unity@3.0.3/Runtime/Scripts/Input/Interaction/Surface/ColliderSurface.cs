using UnityEngine;
using UnityEngine.Assertions;
using Vector3 = UnityEngine.Vector3;

namespace Rokid.UXR.Interaction
{
    public class ColliderSurface : MonoBehaviour, ISurface, IBounds
    {

        [SerializeField]
        private Collider _collider;

        protected virtual void Start()
        {
            Assert.IsNotNull(_collider);
        }

        public Transform Transform => transform;

        public Bounds Bounds => _collider.bounds;

        public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
        {
            hit = new SurfaceHit();

            RaycastHit hitInfo;
            if (_collider.Raycast(ray, out hitInfo, maxDistance))
            {
                hit.Point = hitInfo.point;
                hit.Normal = hitInfo.normal;
                hit.Distance = hitInfo.distance;
                return true;
            }

            return false;
        }

        public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0)
        {
            Vector3 closest = _collider.ClosestPoint(point);
            RKLog.Info($"ColliderSurface  ClosestSurfacePoint: {closest},{point}");
            return Raycast(new Ray(point, closest - point), out hit, maxDistance);
        }
    }
}
