

using UnityEngine;
using UnityEngine.Assertions;
namespace Rokid.UXR.Interaction
{
    public class BoxProximityField : MonoBehaviour, IProximityField
    {
        [SerializeField]
        private Transform _boxTransform;

        protected virtual void Start()
        {
            Assert.IsNotNull(_boxTransform);
        }

        // Closest point in box is computed by transforming the point to OBB space,
        // clamping to a 1-1-1 box, and transforming the point back to world space
        public Vector3 ComputeClosestPoint(Vector3 point)
        {
            Vector3 localPoint = _boxTransform.InverseTransformPoint(point);

            localPoint.x = Mathf.Clamp(localPoint.x, -0.5f, 0.5f);
            localPoint.y = Mathf.Clamp(localPoint.y, -0.5f, 0.5f);
            localPoint.z = Mathf.Clamp(localPoint.z, -0.5f, 0.5f);

            Vector3 worldPoint = _boxTransform.TransformPoint(localPoint);
            return worldPoint;
        }
    }
}
