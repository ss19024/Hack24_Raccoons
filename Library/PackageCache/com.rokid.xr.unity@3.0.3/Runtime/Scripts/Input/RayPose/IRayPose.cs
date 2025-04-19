using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public enum PoseUpdateType
    {
        Auto,
        FollowTargetPoint
    }
    public interface IRayPose
    {
        /// <summary>
        /// It is recommended to refresh this point in LateUpdate  this interface exclude gesture
        /// </summary>
        /// <param name="point"></param>
        public void UpdateTargetPoint(Vector3 point);
        /// <summary>
        /// Set pose update type this interface exclude gesture
        /// </summary>
        /// <param name="type"></param>
        public void SetPoseUpdateType(PoseUpdateType type);

        /// <summary>
        /// Get pose update type this interface exclude gesture
        /// </summary>
        /// <param name="type"></param>
        public PoseUpdateType GetPoseUpdateType();
        /// <summary>
        /// Get current ray pose
        /// </summary>
        /// <value></value>
        public Pose RayPose { get; }
    }
}

