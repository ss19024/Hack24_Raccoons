
namespace Rokid.UXR.Interaction
{

    using UnityEngine;

    public static partial class Collisions
    {
        /// <summary>
        /// Approximate capsule collision by doing sphere collisions down the capsule length
        /// </summary>
        /// <param name="p0">Capsule Start</param>
        /// <param name="p1">Capsule End</param>
        /// <param name="radius">Capsule Radius</param>
        /// <param name="collider">Collider to check against</param>
        /// <returns>Whether or not an approximate collision occured.</returns>
        public static bool IsCapsuleWithinColliderApprox(Vector3 p0, Vector3 p1, float radius, Collider collider)
        {
            int divisions = Mathf.CeilToInt((p1 - p0).magnitude / radius) * 2;

            if (divisions == 0)
            {
                return IsSphereWithinCollider(p0, radius, collider);
            }

            float tStep = 1f / divisions;
            for (int i = 0; i <= divisions; i++)
            {
                Vector3 point = Vector3.Lerp(p0, p1, tStep * i);
                if (IsSphereWithinCollider(point, radius, collider))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
