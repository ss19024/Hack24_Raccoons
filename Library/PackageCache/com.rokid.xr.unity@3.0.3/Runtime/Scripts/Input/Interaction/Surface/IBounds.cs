
namespace Rokid.UXR.Interaction {
	
using UnityEngine;
	
	public interface IBounds
	{
	    /// <summary>
	    /// The world space axis-aligned bounding box (AABB)
	    /// </summary>
	    Bounds Bounds { get; }
	}
}
