using UnityEngine;

namespace Rokid.UXR.Interaction {
	public static partial class Collisions
	{
	    public static Vector3 ClosestPointToColliders(Vector3 point, Collider[] colliders)
	    {
	        Vector3 closestPoint = point;
	        float closestDistance = float.MaxValue;
	        foreach (Collider collider in colliders)
	        {
	            if (Collisions.IsPointWithinCollider(point, collider))
	            {
	                return point;
	            }
	
	            Vector3 closest = collider.ClosestPoint(point);
	            float distance = (closest - point).magnitude;
	            if (distance < closestDistance)
	            {
	                closestDistance = distance;
	                closestPoint = closest;
	            }
	        }
	
	        return closestPoint;
	    }
	}
}
