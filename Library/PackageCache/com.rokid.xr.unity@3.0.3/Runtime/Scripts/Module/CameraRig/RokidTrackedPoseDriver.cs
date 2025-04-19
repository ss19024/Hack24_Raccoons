using System;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace Rokid.UXR.Module
{
    public class RokidTrackedPoseDriver : TrackedPoseDriver
    {
        [SerializeField]
        bool m_RotationExcludeRoll = false;

        public bool rotationExcludeRoll { get { return m_RotationExcludeRoll; } set { m_RotationExcludeRoll = value; } }

        [SerializeField]
        bool m_MixThreeDof = false;

        public bool mixThreeDof { get { return m_MixThreeDof; } set { m_MixThreeDof = value; } }

        [SerializeField]
        bool m_ZeroDofEiv = false;

        public bool zeroDofEiv { get { return m_ZeroDofEiv; } set { m_ZeroDofEiv = value; } }


        /// <summary>
        /// Sets the transform that is being driven by the <see cref="TrackedPoseDriver"/>. will only correct set the rotation or position depending on the <see cref="PoseDataFlags"/>
        /// </summary>
        /// <param name="newPosition">The position to apply.</param>
        /// <param name="newRotation">The rotation to apply.</param>
        /// <param name="poseFlags">The flags indicating which of the position/rotation values are provided by the calling code.</param>
        protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation, PoseDataFlags poseFlags)
        {
            if ((trackingType == TrackingType.RotationAndPosition ||
                trackingType == TrackingType.RotationOnly) &&
                (poseFlags & PoseDataFlags.Rotation) > 0)
            {
                if (m_RotationExcludeRoll)
                {
                    Vector3 euler = newRotation.eulerAngles;
                    transform.localRotation = Quaternion.Euler(euler.x, euler.y, 0);
                }
                else
                {
                    transform.localRotation = newRotation;
                }
                if (zeroDofEiv)
                {
                    float deltaAngle = Mathf.Max(Vector3.Angle(newRotation * Vector3.forward, Vector3.forward), Vector3.Angle(newRotation * Vector3.right, Vector3.right));
                    float smoothSpeed = Mathf.Clamp(deltaAngle * Mathf.Clamp(deltaAngle * 0.05f, 0.12f, 0.25f), 0.1f, 5);
                    Native.NativeInterface.NativeAPI.Set3DofAlpha(0.035f * smoothSpeed);
                }
                else if (mixThreeDof)
                {
                    transform.localRotation = Quaternion.Inverse(MixThreeDof.Instance.deltaOrientation) * transform.localRotation;
                }
            }

            if ((trackingType == TrackingType.RotationAndPosition ||
                trackingType == TrackingType.PositionOnly) &&
                (poseFlags & PoseDataFlags.Position) > 0)
            {
#if USE_ROKID_OPENXR
                transform.localPosition = newPosition + new Vector3(0, -1.6f, 0);
#else
                transform.localPosition = newPosition;
#endif
            }
        }
    }
}
