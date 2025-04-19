using System.Collections.Generic;
using UnityEngine;

namespace Rokid.UXR.Interaction {
	public class ColliderGroup
	{
	    private Collider _boundsCollider;
	    private List<Collider> _colliders;
	
	    public Collider Bounds => _boundsCollider;
	    public List<Collider> Colliders => _colliders;
	
	    public ColliderGroup(List<Collider> colliders, Collider boundsCollider)
	    {
	        _colliders = colliders;
	        _boundsCollider = boundsCollider;
	    }
	}
}
