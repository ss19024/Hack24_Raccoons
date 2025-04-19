using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public static partial class Collisions
    {
        public static bool IsPointWithinCollider(Vector3 point, Collider collider)
        {
            if (!collider.bounds.Contains(point))
            {
                return false;
            }

            Vector3 closestPoint = collider.ClosestPoint(point);
            if (collider is MeshCollider)
            {
                return (closestPoint - point).sqrMagnitude < collider.contactOffset * collider.contactOffset;
            }
            return closestPoint.Equals(point);
        }
    }
}
