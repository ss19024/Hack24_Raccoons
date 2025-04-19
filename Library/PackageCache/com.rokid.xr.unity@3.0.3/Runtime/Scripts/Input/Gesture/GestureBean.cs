using UnityEngine;


namespace Rokid.UXR.Interaction
{
    [System.Serializable]
    public class GestureBean
    {
        /// <summary>
        /// Gesture type
        /// </summary>
        public int gesture_type;
        /// <summary>
        /// Hand type
        /// </summary>
        public int hand_type;
        /// <summary>
        /// Hand Orientation
        /// </summary>
        public int hand_orientation;
        /// <summary>
        /// Skeleton Pos
        /// </summary>
        public Vector3[] skeletons;
        /// <summary>
        /// Skeleton Rot
        /// </summary>
        public Quaternion[] skeletonsRot;
        /// <summary>
        /// Hand Root Rotation
        /// </summary>
        public Quaternion rotation;
        /// <summary>
        /// Hand root position
        /// </summary>
        public Vector3 position;


        /// <summary>
        /// Pinch distance
        /// </summary>
        public float pinchDistance;

        public string ThreeGesKeyInfo()
        {
            return $"\n handPose:{position},{rotation.eulerAngles}\n wristPose:{skeletons?[0]},{skeletonsRot?[0].eulerAngles} \n handType: {(HandType)hand_type} \n gestureType: {(GestureType)gesture_type} \n handOrientation:{hand_orientation} \n skeletons.Count:{skeletons?.Length}";
        }
    }
}
