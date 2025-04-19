using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public static partial class Collisions
    {
        public static bool IsSphereWithinCollider(Vector3 point, float radius, Collider collider)
        {
            Vector3 boundsPoint = collider.bounds.ClosestPoint(point);
            if (Vector3.SqrMagnitude(boundsPoint - point) > radius * radius)
            {
                return false;
            }

            Vector3 closestPoint = collider.ClosestPoint(point);
            return Vector3.SqrMagnitude(closestPoint - point) <= radius * radius;
        }
    }
}
