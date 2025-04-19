using UnityEngine;
using UnityEngine.Assertions;

namespace Rokid.UXR.Interaction
{
	public class CylinderProximityField : MonoBehaviour,
		IProximityField, ICurvedPlane
	{
		[SerializeField]
		private Cylinder _cylinder;

		[SerializeField]
		private float _rotation = 0f;

		[SerializeField, Range(0f, 360f)]
		private float _arcDegrees = 360;

		[SerializeField]
		private float _bottom = -1f;

		[SerializeField]
		private float _top = 1f;

		/// <summary>
		/// 提供一个ICurvePlane复写本地的属性
		/// </summary>
		[Tooltip("Providing an ICurvedPlane here will " +
			"override all other local properties")]
		[SerializeField, Optional, Interface(typeof(ICurvedPlane))]
		private MonoBehaviour _curvedPlane;

		private ICurvedPlane CurvedPlane;

		public Cylinder Cylinder => _cylinder;

		public float ArcDegrees
		{
			get => _arcDegrees;
			set => _arcDegrees = value;
		}
		public float Rotation
		{
			get => _rotation;
			set => _rotation = value;
		}
		public float Bottom
		{
			get => _bottom;
			set => _bottom = value;
		}
		public float Top
		{
			get => _top;
			set => _top = value;
		}

		protected virtual void Awake()
		{
			CurvedPlane = _curvedPlane != null ?
						  _curvedPlane as ICurvedPlane :
						  this;
		}

		protected virtual void Start()
		{
			Assert.IsNotNull(CurvedPlane);
			Assert.IsNotNull(CurvedPlane.Cylinder);
		}

		public Vector3 ComputeClosestPoint(Vector3 point)
		{
			return ComputeClosestPoint(CurvedPlane, point);
		}

		/// <summary>
		/// 最近点的计算
		/// </summary>
		/// <param name="curvedPlane"></param>
		/// <param name="point"></param>
		/// <returns></returns>
		private static Vector3 ComputeClosestPoint(ICurvedPlane curvedPlane, Vector3 point)
		{
			// RKLog.Debug($"CylinderProximityField ComputeClosestPoint");

			Vector3 localPoint = curvedPlane.Cylinder.transform.InverseTransformPoint(point);

			if (curvedPlane.Top > curvedPlane.Bottom)
			{
				localPoint.y = Mathf.Clamp(localPoint.y, curvedPlane.Bottom, curvedPlane.Top);
			}

			if (curvedPlane.ArcDegrees < 360)
			{
				float angle = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg % 360;
				float rotation = curvedPlane.Rotation % 360;

				if (angle > rotation + 180)
				{
					angle -= 360;
				}
				else if (angle < rotation - 180)
				{
					angle += 360;
				}

				angle = Mathf.Clamp(angle, rotation - curvedPlane.ArcDegrees / 2f,
										   rotation + curvedPlane.ArcDegrees / 2f);

				localPoint.x = Mathf.Sin(angle * Mathf.Deg2Rad) * curvedPlane.Cylinder.Radius;
				localPoint.z = Mathf.Cos(angle * Mathf.Deg2Rad) * curvedPlane.Cylinder.Radius;
			}
			else
			{
				Vector3 nearestPointOnCenterAxis = new Vector3(0f, localPoint.y, 0f);
				float distanceFromCenterAxis = Vector3.Distance(localPoint,
																nearestPointOnCenterAxis);
				localPoint = Vector3.MoveTowards(localPoint,
												 nearestPointOnCenterAxis,
												 distanceFromCenterAxis - curvedPlane.Cylinder.Radius);
			}

			return curvedPlane.Cylinder.transform.TransformPoint(localPoint);
		}
	}

}
