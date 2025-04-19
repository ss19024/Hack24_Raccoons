
namespace Rokid.UXR.Interaction {
	
using UnityEngine;
	
	/// <summary>
	/// 计算接接近点的字段
	/// </summary>
	public interface IProximityField
	{
	    Vector3 ComputeClosestPoint(Vector3 point);
	}
}
