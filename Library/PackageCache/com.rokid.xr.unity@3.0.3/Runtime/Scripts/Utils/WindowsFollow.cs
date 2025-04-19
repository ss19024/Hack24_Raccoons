using UnityEngine;
using Rokid.UXR;
using UnityEngine.Video;

namespace Rokid.UXR.Utility
{
	[ExecuteAlways]
	public class WindowsFollow : MonoBehaviour
	{
		[Header("Window Settings")]
		[SerializeField, Tooltip("What part of the view port to anchor the window to.")]
		private TextAnchor windowAnchor = TextAnchor.LowerCenter;

		public TextAnchor WindowAnchor
		{
			get { return windowAnchor; }
			set { windowAnchor = value; }
		}
		[SerializeField, Tooltip("The offset from the view port center applied based on the window anchor selection.")]
		private Vector2 windowOffset = new Vector2(0.1f, 0.1f);

		public Vector2 WindowOffset
		{
			get { return windowOffset; }
			set { windowOffset = value; }
		}

		[SerializeField, Range(0.5f, 5.0f), Tooltip("Use to scale the window size up or down, can simulate a zooming effect.")]
		private float windowScale = 1.0f;

		public float WindowScale
		{
			get { return windowScale; }
			set { windowScale = Mathf.Clamp(value, 0.5f, 5.0f); }
		}

		[SerializeField, Range(0.0f, 100.0f), Tooltip("How quickly to interpolate the window towards its target position and rotation.")]
		private float windowFollowSpeed = 5.0f;

		[SerializeField]
		private float windowDistance = 10;

		public float WindowFollowSpeed
		{
			get { return windowFollowSpeed; }
			set { windowFollowSpeed = Mathf.Abs(value); }
		}

		private Quaternion windowHorizontalRotation;
		private Quaternion windowHorizontalRotationInverse;
		private Quaternion windowVerticalRotation;
		private Quaternion windowVerticalRotationInverse;

		private static readonly Vector2 defaultWindowRotation = Vector2.zero;
		private static readonly Vector3 defaultWindowScale = Vector3.one;

		private bool fixedWindowRoll;

		public bool FixedWindowRoll
		{
			get { return fixedWindowRoll; }
			set { fixedWindowRoll = value; }
		}

		private Transform window;

		private Vector3 cameraSpaceCenter;

		private Vector3 worldSpaceCenter
		{
			get
			{
				return MainCameraCache.mainCamera.transform.rotation * cameraSpaceCenter + MainCameraCache.mainCamera.transform.position;
			}
		}

		private void Start()
		{
			if (Utils.IsAndroidPlatform())
			{
				cameraSpaceCenter = Utils.GetCameraCenter(1);
			}
			windowHorizontalRotation = Quaternion.AngleAxis(defaultWindowRotation.y, Vector3.right);
			windowHorizontalRotationInverse = Quaternion.Inverse(windowHorizontalRotation);
			windowVerticalRotation = Quaternion.AngleAxis(defaultWindowRotation.x, Vector3.up);
			windowVerticalRotationInverse = Quaternion.Inverse(windowVerticalRotation);
			window = transform;
		}

		private void LateUpdate()
		{
			Transform cameraTransform = MainCameraCache.mainCamera.transform;
			float t = Time.deltaTime * windowFollowSpeed;
			window.position = Vector3.Lerp(window.position, CalculateWindowPosition(cameraTransform), t);
			window.rotation = Quaternion.Slerp(window.rotation, CalculateWindowRotation(cameraTransform), t);
			window.localScale = defaultWindowScale * windowScale;
		}

		private Vector3 CalculateWindowPosition(Transform cameraTransform)
		{
			Vector3 position;
			if (Utils.IsAndroidPlatform())
			{
				position = cameraTransform.position + Vector3.Normalize(worldSpaceCenter - cameraTransform.position) * windowDistance;
			}
			else
			{
				position = cameraTransform.position + (cameraTransform.forward * windowDistance);
			}
			Vector3 horizontalOffset = cameraTransform.right * windowOffset.x;
			Vector3 verticalOffset = cameraTransform.up * windowOffset.y;
			switch (windowAnchor)
			{
				case TextAnchor.UpperLeft: position += verticalOffset - horizontalOffset; break;
				case TextAnchor.UpperCenter: position += verticalOffset; break;
				case TextAnchor.UpperRight: position += verticalOffset + horizontalOffset; break;
				case TextAnchor.MiddleLeft: position -= horizontalOffset; break;
				case TextAnchor.MiddleRight: position += horizontalOffset; break;
				case TextAnchor.LowerLeft: position -= verticalOffset + horizontalOffset; break;
				case TextAnchor.LowerCenter: position -= verticalOffset; break;
				case TextAnchor.LowerRight: position -= verticalOffset - horizontalOffset; break;
			}

			return position;
		}

		private Quaternion CalculateWindowRotation(Transform cameraTransform)
		{
			Quaternion rotation = cameraTransform.rotation;
			if (Utils.IsAndroidPlatform())
			{
				rotation *= Quaternion.FromToRotation(cameraTransform.forward, worldSpaceCenter - cameraTransform.position);
				if (fixedWindowRoll)
					rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
			}
			switch (windowAnchor)
			{
				case TextAnchor.UpperLeft: rotation *= windowHorizontalRotationInverse * windowVerticalRotationInverse; break;
				case TextAnchor.UpperCenter: rotation *= windowHorizontalRotationInverse; break;
				case TextAnchor.UpperRight: rotation *= windowHorizontalRotationInverse * windowVerticalRotation; break;
				case TextAnchor.MiddleLeft: rotation *= windowVerticalRotationInverse; break;
				case TextAnchor.MiddleRight: rotation *= windowVerticalRotation; break;
				case TextAnchor.LowerLeft: rotation *= windowHorizontalRotation * windowVerticalRotationInverse; break;
				case TextAnchor.LowerCenter: rotation *= windowHorizontalRotation; break;
				case TextAnchor.LowerRight: rotation *= windowHorizontalRotation * windowVerticalRotation; break;
			}
			return rotation;
		}
	}
}
