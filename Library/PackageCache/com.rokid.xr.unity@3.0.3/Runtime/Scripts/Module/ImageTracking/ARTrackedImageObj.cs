using Rokid.UXR.Interaction;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Rokid.UXR.Module
{

    [DisallowMultipleComponent]
    public class ARTrackedImageObj : MonoBehaviour
    {
        [SerializeField, Tooltip("Tracking the image index, which corresponds to the recognized image index in the image database.")]
        public int trackedImageIndex = 0;
        [SerializeField, Tooltip("Does the model scale according to the image size")]
        public bool autoFitImageSize = true;
        [SerializeField, Tooltip("Whether to hide the model when the image trace is lost")]
        public bool disableWhenTraceLost = true;
        [SerializeField, Tooltip("Whether use smooth logic to update pose")]
        public bool useSmoothToPose = true;
        private bool isInitialize = false;
        private Vector3 oriSize;

        public UnityEvent<ARTrackedImageObj> OnARTrackedImageAdded = new UnityEvent<ARTrackedImageObj>();
        public UnityEvent<ARTrackedImageObj> OnARTrackedImageUpdated = new UnityEvent<ARTrackedImageObj>();
        public UnityEvent<ARTrackedImageObj> OnARTrackedImageRemoved = new UnityEvent<ARTrackedImageObj>();

        public ARTrackedImage trackedImage { get; private set; }
        private void Start()
        {
            Initialized();
        }

        private void Initialized()
        {
            if (!isInitialize)
            {
                isInitialize = true;
                ARTrackedImageManager.Instance.RegisterImageTrackedObj(this);
                oriSize = transform.localScale;
                this.gameObject.SetActive(false);
                if (trackedImage == null)
                {
                    trackedImage = new ARTrackedImage(trackedImageIndex);
                }
                RKLog.KeyInfo("====ARTrackedImageObj====: Initialized");
            }
        }

        public void Added(ARTrackedImage trackedImage)
        {
            if (trackedImageIndex == trackedImage.index)
            {
                Initialized();
                transform.SetPose(trackedImage.pose);
                if (autoFitImageSize)
                    transform.localScale = oriSize * trackedImage.sizeScale;
                this.gameObject.SetActive(true);
                this.trackedImage = trackedImage;
                OnARTrackedImageAdded.Invoke(this);
                RKLog.KeyInfo($"====ARTrackedImageObj====: Added {trackedImage.index}");
            }
        }
        public void Updated(ARTrackedImage trackedImage)
        {
            if (trackedImageIndex == trackedImage.index)
            {
                if (useSmoothToPose)
                {
                    transform.SetPose(Utils.SmoothToPose(transform.GetPose(), trackedImage.pose));
                }
                else
                {
                    transform.SetPose(trackedImage.pose);
                }

                if (autoFitImageSize)
                    transform.localScale = oriSize * trackedImage.sizeScale;
                this.trackedImage = trackedImage;
                OnARTrackedImageUpdated.Invoke(this);
                RKLog.KeyInfo($"====ARTrackedImageObj====: Updated {trackedImage.index}");
            }
        }
        public void Removed(int index)
        {
            if (trackedImageIndex == index)
            {
                if (disableWhenTraceLost)
                    this.gameObject.SetActive(false);
                OnARTrackedImageRemoved.Invoke(this);
                RKLog.KeyInfo($"====ARTrackedImageObj====: Removed {index}");
            }
            else
            {
                RKLog.KeyInfo($"====ARTrackedImageObj====: Removed Error {trackedImageIndex},{index}");
            }
        }

        private void OnDestroy()
        {
            ARTrackedImageManager.Instance.UnRegisterImageTrackedObj(trackedImageIndex);
        }
    }
}

