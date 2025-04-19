using Rokid.UXR.Interaction;
using UnityEngine;
namespace Rokid.UXR.Utility
{
    [ExecuteAlways]
    public class FollowCamera : MonoBehaviour
    {
        public enum FollowType
        {
            RotationAndPosition, //Follows the position and rotation of the camera.
            PositionOnly, // Follows only the position of the camera
            RotationOnly // Follows only the rotation of the camera
        }

        [SerializeField, Tooltip("Follow Camera Pose Type")]
        private FollowType followType = FollowType.RotationAndPosition;
        [SerializeField, Tooltip("Deviation from Camera Position")]
        private Vector3 offsetPosition = new Vector3(0, 0, 0);

        [SerializeField, Tooltip("Deviation from Camera Rotation")]
        private Quaternion offsetRotation = Quaternion.identity;
        [SerializeField, Tooltip("Lock X-axis while following camera rotation")]
        private bool lockRotX = false;
        [SerializeField, Tooltip("Lock Y-axis while following camera rotation")]
        private bool lockRotY = false;
        [SerializeField, Tooltip("Lock Z-axis while following camera rotation")]
        private bool lockRotZ = false;
        [SerializeField, Tooltip("adjust camera center by fov")]
        private bool adjustCenterByFov = true;
        private Vector3 oriOffsetPosition = Vector3.zero;

        private bool isUpdate;

        private void Start()
        {
            if (isUpdate == false)
            {
                oriOffsetPosition = offsetPosition;
                AdjustCenterByCameraFov(adjustCenterByFov);
            }
        }
        private void LateUpdate()
        {
            switch (followType)
            {
                case FollowType.RotationAndPosition:
                    this.transform.position = MainCameraCache.mainCamera.transform.TransformPoint(offsetPosition);
                    Vector3 cameraEuler = (offsetRotation * MainCameraCache.mainCamera.transform.rotation).eulerAngles;
                    this.transform.rotation = Quaternion.Euler(lockRotX ? 0 : cameraEuler.x, lockRotY ? 0 : cameraEuler.y, lockRotZ ? 0 : cameraEuler.z);
                    //RKLog.KeyInfo($"====FollowCamera==== {gameObject.name} {offsetPosition},{this.transform.position}");
                    break;
                case FollowType.PositionOnly:
                    this.transform.position = MainCameraCache.mainCamera.transform.position + offsetPosition;
                    break;
                case FollowType.RotationOnly:
                    Vector3 cameraEuler1 = (offsetRotation * MainCameraCache.mainCamera.transform.rotation).eulerAngles;
                    this.transform.rotation = Quaternion.Euler(lockRotX ? 0 : cameraEuler1.x, lockRotY ? 0 : cameraEuler1.y, lockRotZ ? 0 : cameraEuler1.z);
                    break;
            }

        }

        public void AdjustCenterByCameraFov(bool adjustCenterByFov, bool useLeftEyeFov = true)
        {
            this.adjustCenterByFov = adjustCenterByFov;
            if (adjustCenterByFov)
            {
                Vector3 center = Utils.GetCameraCenter(oriOffsetPosition.z, useLeftEyeFov);
                offsetPosition = center + new Vector3(oriOffsetPosition.x, oriOffsetPosition.y, 0);
            }
            else
            {
                offsetPosition = oriOffsetPosition;
            }
        }

        public void UpdateOffsetPosition(Vector3 offsetPosition, bool adjustCenterByFov)
        {
            isUpdate = true;
            this.offsetPosition = this.oriOffsetPosition = offsetPosition;
            if (Utils.IsAndroidPlatform())
            {
                AdjustCenterByCameraFov(adjustCenterByFov);
            }
            else
            {
                this.offsetPosition = offsetPosition;
            }
            LateUpdate();
        }
    }
}

