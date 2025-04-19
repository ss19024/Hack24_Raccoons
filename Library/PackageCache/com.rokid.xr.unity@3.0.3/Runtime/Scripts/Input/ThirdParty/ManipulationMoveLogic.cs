using System;
using UnityEngine;
using Rokid.UXR.Utility;
using Rokid.UXR.Exentesions;

namespace Rokid.UXR.Interaction.ThirdParty
{
    /// <summary>
    /// Use MRTK Phyisc 
    /// Implements a move logic that will move an object based on the initial position of 
    /// the grab point relative to the pointer and relative to the object, and subsequent
    /// changes to the pointer and the object's rotation
    /// 
    /// Usage:
    /// When a manipulation starts, call Setup.
    /// Call Update any time to update the move logic and get a new rotation for the object.
    /// </summary>
    public class ManipulationMoveLogic
    {
        private float pointerRefDistance;

        private bool pointerPosIndependentOfHead = true;

        private Vector3 pointerLocalGrabPoint;
        private Vector3 objectLocalGrabPoint;
        private Vector3 grabToObject;

        private Vector3 objectLocalAttachPoint;
        private Vector3 attachToObject;

        /// <summary>
        /// Setup function
        /// </summary>
        public void Setup(Pose pointerCentroidPose, Vector3 grabCentroid, Pose objectPose, Vector3 objectScale)
        {
            pointerRefDistance = GetDistanceToBody(pointerCentroidPose);

            pointerPosIndependentOfHead = pointerRefDistance != 0;

            Quaternion worldToPointerRotation = Quaternion.Inverse(pointerCentroidPose.rotation);
            pointerLocalGrabPoint = worldToPointerRotation * (grabCentroid - pointerCentroidPose.position);

            attachToObject = objectPose.position - pointerCentroidPose.position;
            objectLocalAttachPoint = Quaternion.Inverse(objectPose.rotation) * (pointerCentroidPose.position - objectPose.position);
            objectLocalAttachPoint = objectLocalAttachPoint.Div(objectScale);

            grabToObject = objectPose.position - grabCentroid;
            objectLocalGrabPoint = Quaternion.Inverse(objectPose.rotation) * (grabCentroid - objectPose.position);
            objectLocalGrabPoint = objectLocalGrabPoint.Div(objectScale);
        }

        /// <summary>
        /// Update the position based on input.
        /// </summary>
        /// <returns>A Vector3 describing the desired position</returns>
        [Obsolete("This update function is out of date and does not properly support Near Manipulation. Use UpdateTransform instead")]
        public Vector3 Update(Pose pointerCentroidPose, Quaternion objectRotation, Vector3 objectScale, bool usePointerRotation)
        {
            return FarManipulationUpdate(pointerCentroidPose, objectRotation, objectScale, usePointerRotation);
        }

        /// <summary>
        /// Update the position based on input.
        /// </summary>
        /// <returns>A Vector3 describing the desired position</returns>
        public Vector3 UpdateTransform(Pose pointerCentroidPose, Transform currentTarget, bool isPointerAnchor, bool isNearManipulation)
        {
            if (isNearManipulation)
            {
                return NearManipulationUpdate(pointerCentroidPose, currentTarget);
            }
            else
            {
                return FarManipulationUpdate(pointerCentroidPose, currentTarget.rotation, currentTarget.localScale, isPointerAnchor);
            }
        }

        /// <summary>
        /// Updates the position during near manipulation
        /// </summary>
        /// <returns>A Vector3 describing the desired position during near manipulation</returns>
        private Vector3 NearManipulationUpdate(Pose pointerCentroidPose, Transform currentTarget)
        {
            Vector3 scaledLocalAttach = Vector3.Scale(objectLocalAttachPoint, currentTarget.localScale);
            Vector3 worldAttachPoint = currentTarget.rotation * scaledLocalAttach + currentTarget.position;
            return currentTarget.position + (pointerCentroidPose.position - worldAttachPoint);
        }

        /// <summary>
        /// Updates the position during far manipulation
        /// </summary>
        /// <returns>A Vector3 describing the desired position during far manipulation</returns>
        private Vector3 FarManipulationUpdate(Pose pointerCentroidPose, Quaternion objectRotation, Vector3 objectScale, bool isPointerAnchor)
        {
            float distanceRatio = 1.0f;

            if (pointerPosIndependentOfHead)
            {
                // Compute how far away the object should be based on the ratio of the current to original hand distance
                float currentHandDistance = GetDistanceToBody(pointerCentroidPose);
                distanceRatio = currentHandDistance / pointerRefDistance;
            }

            if (isPointerAnchor)
            {
                Vector3 scaledGrabToObject = Vector3.Scale(objectLocalGrabPoint, objectScale);
                Vector3 adjustedPointerToGrab = (pointerLocalGrabPoint * distanceRatio);
                adjustedPointerToGrab = pointerCentroidPose.rotation * adjustedPointerToGrab;

                return adjustedPointerToGrab - objectRotation * scaledGrabToObject + pointerCentroidPose.position;
            }
            else
            {
                return pointerCentroidPose.position + grabToObject + (pointerCentroidPose.rotation * pointerLocalGrabPoint) * distanceRatio;
            }
        }

        private float GetDistanceToBody(Pose pointerCentroidPose)
        {
            // The body is treated as a ray, parallel to the y-axis, where the start is head position.
            // This means that moving your hand down such that is the same distance from the body will
            // not cause the manipulated object to move further away from your hand. However, when you
            // move your hand upward, away from your head, the manipulated object will be pushed away.
            if (pointerCentroidPose.position.y > MainCameraCache.mainCamera.transform.position.y)
            {
                return Vector3.Distance(pointerCentroidPose.position, MainCameraCache.mainCamera.transform.position);
            }
            else
            {
                Vector2 headPosXZ = new Vector2(MainCameraCache.mainCamera.transform.position.x, MainCameraCache.mainCamera.transform.position.z);
                Vector2 pointerPosXZ = new Vector2(pointerCentroidPose.position.x, pointerCentroidPose.position.z);
                return Vector2.Distance(pointerPosXZ, headPosXZ);
            }
        }
    }
}
