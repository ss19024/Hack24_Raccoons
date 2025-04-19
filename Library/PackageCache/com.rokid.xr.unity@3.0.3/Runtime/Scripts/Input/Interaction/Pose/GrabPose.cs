using Rokid.UXR.Utility;
using UnityEngine;
namespace Rokid.UXR.Interaction
{
    public class GrabPose : MonoBehaviour
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
                Pose pose = GesEventInput.Instance.GetHandPose(hand);
                transform.SetPose(pose);
            }
            else
            {
                this.transform.position = MainCameraCache.mainCamera.transform.position + offsetToCamera;
            }
#endif
        }
    }

}
