using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public interface IMovement
    {
        Pose Pose { get; }
        bool Stopped { get; }

        void MoveTo(Pose target);
        void UpdateTarget(Pose target);
        void StopAndSetPose(Pose pose);
        void Tick();
    }
}
