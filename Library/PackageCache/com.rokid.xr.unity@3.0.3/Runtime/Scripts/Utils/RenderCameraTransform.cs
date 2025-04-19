
using Rokid.UXR.Module;
using UnityEngine;

namespace Rokid.UXR.Utility
{
    public class RenderCameraTransform : MonoSingleton<RenderCameraTransform>
    {
        public Vector3 position
        {
            get
            {
                if (headTrackingType == RKCameraRig.HeadTrackingType.ZeroDofEiv)
                {
                    return Vector3.zero;
                }
                else
                {
                    return MainCameraCache.mainCamera.transform.position;
                }
            }
        }

        public Quaternion rotation
        {
            get
            {
                if (headTrackingType == RKCameraRig.HeadTrackingType.ZeroDofEiv)
                {
                    return Quaternion.FromToRotation(Vector3.forward, zeroDofLocalCenter - position);
                }
                else
                {
                    return Quaternion.FromToRotation(Vector3.forward, localCenter) * MainCameraCache.mainCamera.transform.rotation;
                }
            }
        }
        public Vector3 forward
        {
            get
            {
                return rotation * Vector3.forward;
            }
        }

        public Vector3 up
        {
            get
            {
                return rotation * Vector3.up;
            }
        }
        public Vector3 right
        {
            get
            {
                return rotation * Vector3.right;
            }
        }

        private RKCameraRig cameraRig;
        private RKCameraRig.HeadTrackingType headTrackingType
        {
            get
            {
                if (cameraRig == null)
                {
                    cameraRig = MainCameraCache.mainCamera.transform.GetComponent<RKCameraRig>();
                }
                return cameraRig.headTrackingType;
            }
        }

        public new Transform transform
        {
            get
            {
                base.transform.position = position;
                base.transform.rotation = rotation;
                return base.transform;
            }
        }

        private Vector3 m_ZeroDofLocalCenter = Vector3.zero;
        private Vector3 zeroDofLocalCenter
        {
            get
            {
                if (m_ZeroDofLocalCenter == Vector3.zero)
                {
                    m_ZeroDofLocalCenter = Utils.GetCameraCenterSetCameraPositionAndRotation(1, Vector3.zero, Quaternion.identity);
                }
                return m_ZeroDofLocalCenter;
            }
        }

        private Vector3 m_LocalCenter;
        private Vector3 localCenter
        {
            get
            {
                if (m_LocalCenter == Vector3.zero)
                {
                    m_LocalCenter = Utils.GetCameraCenter(1);
                }
                return m_LocalCenter;
            }
        }

        void LateUpdate()
        {
            base.transform.position = position;
            base.transform.rotation = rotation;
        }

        protected override void Awake()
        {
            base.Awake();
            this.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
    }

}

