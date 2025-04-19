using Rokid.UXR.Utility;
using UnityEngine;
namespace Rokid.UXR.Interaction
{
    public class PokePose : MonoBehaviour
    {
        [SerializeField, Tooltip("Hand Type")]
        private HandType hand;
        [SerializeField, Tooltip("Skeleton Index")]
        private SkeletonIndexFlag skeletonIndex = SkeletonIndexFlag.INDEX_FINGER_TIP;
        private bool trackedSuccess;

        private Vector3 offsetToCamera = new Vector3(0, 10000, 0);


#if UNITY_EDITOR
        public float maxDistanceInEditor = 10.0f;
        private float raycastDistance = -1.0f;
#endif

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
            Gesture gesture = GesEventInput.Instance.GetGesture(hand);
            if (gesture.handOrientation == HandOrientation.Palm)
            {
                //放在距离相机天涯海角的距离
                this.transform.position = MainCameraCache.mainCamera.transform.position + offsetToCamera;
            }
            else
            {
                Pose pose = GesEventInput.Instance.GetSkeletonPose(skeletonIndex, hand);
                this.transform.position = pose.position;
                this.transform.rotation = pose.rotation;
            }
        }
        else
        {
            this.transform.position = MainCameraCache.mainCamera.transform.position + offsetToCamera;
        }
#else
            UpdateInEditor();
#endif
        }


        protected virtual void UpdateInEditor()
        {
#if UNITY_EDITOR
            if (GesEventInput.Instance.GetInteractorType(hand) == InteractorType.Near)
            {
                Ray ray = MainCameraCache.mainCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 oldPosition = transform.position;
                transform.position = MainCameraCache.mainCamera.transform.forward * (-1000.0f);

                RaycastHit raycastHit;
                if (Physics.Raycast(ray, out raycastHit, maxDistanceInEditor))
                {
                    if (Input.GetMouseButton(0))
                    {
                        if (Input.GetKey(KeyCode.B))
                        {
                            //模拟近场的Pinch拖拽
                            transform.position = raycastHit.point + raycastHit.normal * 0.02f;
                        }
                        else
                        {
                            //模拟近场的Poke点击以及Poke拖拽逻辑
                            transform.position = raycastHit.point - raycastHit.normal * 0.05f;
                        }
                    }
                    else
                    {
                        transform.position = raycastHit.point + raycastHit.normal * 0.02f;
                    }
                    raycastDistance = Mathf.Min(maxDistanceInEditor, raycastHit.distance);
                }
                else if (raycastDistance > 0.0f)
                {
                    transform.position = ray.origin + Mathf.Min(maxDistanceInEditor, raycastDistance) * ray.direction;
                }
                else
                {
                    transform.position = oldPosition;
                }
            }
            else
            {
                //放在距离相机天涯海角的距离
                this.transform.position = MainCameraCache.mainCamera.transform.position + offsetToCamera;
            }
#endif
        }
    }

}
