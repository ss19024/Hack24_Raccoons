using Rokid.UXR.Utility;
using UnityEngine;
namespace Rokid.UXR.Interaction
{
    public class SnapPose : MonoBehaviour
    {
        [Tooltip("手的类型")]
        [SerializeField]
        private HandType hand;

        private bool trackedSuccess;

        private Vector3 offsetToCamera = new Vector3(0, 10000, 0);

        private void Start()
        {
            GesEventInput.OnTrackedSuccess += OnTrackedSuccess;
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            trackedSuccess = false;
        }

        private void OnTrackedSuccess(HandType hand)
        {
            if (this.hand == hand)
            {
                trackedSuccess = true;
            }
        }


        private void OnTrackedFailed(HandType handType)
        {
            if (this.hand == handType || handType == HandType.None)
            {
                trackedSuccess = false;
            }
        }

        private void OnDestroy()
        {
            GesEventInput.OnTrackedSuccess -= OnTrackedSuccess;
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
        }

        private void Update()
        {
#if !UNITY_EDITOR
            if (trackedSuccess)
            {
                Pose pose = PoseAdd(GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.THUMB_IP, hand), GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.INDEX_FINGER_TIP, hand));
                transform.SetPose(new Pose(pose.position, GesEventInput.Instance.GetHandPose(hand).rotation));
            }
            else
            {
                this.transform.position = MainCameraCache.mainCamera.transform.position + offsetToCamera;
            }
#endif
        }

        private Pose PoseAdd(Pose pose0, Pose pose1)
        {
            Pose pose = Pose.identity;
            pose.position = (pose0.position + pose1.position) / 2;
            pose.rotation = Quaternion.LookRotation((pose0.forward + pose1.forward) / 2);
            return pose;
        }
    }
}
