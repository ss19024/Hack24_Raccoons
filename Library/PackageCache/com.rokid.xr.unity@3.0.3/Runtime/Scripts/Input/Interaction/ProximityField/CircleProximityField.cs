using UnityEngine;
using UnityEngine.Assertions;

namespace Rokid.UXR.Interaction {
	public class CircleProximityField : MonoBehaviour, IProximityField
	{
	    [SerializeField]
	    private Transform _transform;
	
	    [SerializeField]
	    private float _radius = 0.1f;
	
	    protected virtual void Start()
	    {
	        Assert.IsNotNull(_transform);
	    }
	
	    // Closest point to circle is computed by projecting point to the plane
	    // the circle is on and then clamping to the circle
	    public Vector3 ComputeClosestPoint(Vector3 point)
	    {
	        Vector3 vectorFromPlane = point - _transform.position;
	        Vector3 planeNormal = -1.0f * _transform.forward;
	        Vector3 projectedPoint = Vector3.ProjectOnPlane(vectorFromPlane, planeNormal);
	
	        float distanceFromCenterSqr = projectedPoint.sqrMagnitude;
	        float worldRadius = transform.lossyScale.x * _radius;
	        if (distanceFromCenterSqr > worldRadius * worldRadius)
	        {
	            projectedPoint = worldRadius * projectedPoint.normalized;
	        }
	        return projectedPoint + _transform.position;
	    }
	}
}
