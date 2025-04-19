using UnityEngine;
using UnityEngine.Assertions;

namespace Rokid.UXR.Interaction {
	public class PointProximityField : MonoBehaviour, IProximityField
	{
	    [SerializeField]
	    private Transform _centerPoint;
	
	    protected virtual void Start()
	    {
	        Assert.IsNotNull(_centerPoint);
	    }
	
	    public Vector3 ComputeClosestPoint(Vector3 point)
	    {
	        return _centerPoint.position;
	    }
	}
}
