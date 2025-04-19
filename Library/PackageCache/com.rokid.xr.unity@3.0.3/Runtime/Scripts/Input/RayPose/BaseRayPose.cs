using System;
using System.Collections.Generic;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Base Ray Pose
    /// </summary>
    public abstract class BaseRayPose : MonoBehaviour, IRayPose
    {
        [Serializable]
        public class EditorParams
        {
            [Tooltip("是否使用鼠标模拟射线Rotate")]
            public bool useMouseRotate = true;
            [Tooltip("射线是否跟随相机,只在编辑器生效")]
            public bool followCameraInEditor = true;
            [Tooltip("相机空间下的局部坐标,只有在followCamera为真的情况下生效")]
            public Vector3 localPositionInCameraSpace = new Vector3(0, -0.1f, 0);
            public float maxDistanceInEditor = 10.0f;
            [HideInInspector]
            public float raycastDistance = -1.0f;
            public BaseRayCaster rayCaster;
            public static readonly RaycastHit[] hits = new RaycastHit[4];
            public readonly List<RaycastResult> sortedRaycastResults = new List<RaycastResult>();
            public Transform rayOrigin;
        }

        [SerializeField, Tooltip("编辑器模拟参数")]
        private EditorParams editorParams;

        private Transform hostTransform;

        protected PoseUpdateType updateType = PoseUpdateType.Auto;

        protected static Dictionary<InputModuleType, Pose> InteractorPose = new Dictionary<InputModuleType, Pose>(){
            {InputModuleType.TouchPad,Pose.identity},{InputModuleType.Mouse,Pose.identity}
        };

        protected virtual void Start()
        {
            if (editorParams.rayCaster == null)
                editorParams.rayCaster = GetComponent<BaseRayCaster>();
            hostTransform = editorParams.rayOrigin == null ? transform : editorParams.rayOrigin;
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (editorParams.useMouseRotate && updateType == PoseUpdateType.Auto)
            {
                UpdateInEditor();
            }
#endif
        }

#if UNITY_EDITOR
        protected virtual void UpdateInEditor()
        {
            if (editorParams.followCameraInEditor)
            {
                hostTransform.position = MainCameraCache.mainCamera.transform.TransformPoint(editorParams.localPositionInCameraSpace);
            }

            if (editorParams.rayCaster != null)
            {
                Ray ray = MainCameraCache.mainCamera.ScreenPointToRay(Input.mousePosition);
                editorParams.rayCaster.Raycast(ray, Mathf.Infinity, editorParams.sortedRaycastResults);
                RaycastResult raycastResult = editorParams.rayCaster.FirstRaycastResult(editorParams.sortedRaycastResults);
                if (raycastResult.gameObject != null)
                {
                    Vector3 targetPos = raycastResult.worldPosition;
                    Vector3 toDirection = targetPos - hostTransform.position;
                    hostTransform.localRotation = Quaternion.FromToRotation(Vector3.forward, toDirection);
                    editorParams.raycastDistance = Mathf.Min(editorParams.maxDistanceInEditor, raycastResult.distance);
                }
                else if (editorParams.raycastDistance > 0.0f)
                {
                    Vector3 targetPos = ray.origin + Mathf.Min(editorParams.maxDistanceInEditor, editorParams.raycastDistance) * ray.direction;
                    Vector3 toDirection = targetPos - hostTransform.position;
                    hostTransform.localRotation = Quaternion.FromToRotation(Vector3.forward, toDirection);
                }
                else
                {
                    hostTransform.localRotation = Quaternion.identity;
                }
            }
        }
#endif

        #region LimitInViewField
        protected void LimitInViewField()
        {
            transform.position = MainCameraCache.mainCamera.transform.position;
            Vector3 pointInWorld = transform.forward + transform.position;
            Vector3 pointInCamera = MainCameraCache.mainCamera.transform.InverseTransformPoint(pointInWorld);
            pointInCamera.z = Mathf.Abs(pointInCamera.z);
            Vector3[] corners = Utils.GetCameraCorners(pointInCamera.z);
            if (pointInCamera.x < corners[0].x) //Left 
            {
                pointInCamera.x = corners[0].x;
            }
            else if (pointInCamera.x > corners[1].x) //Right
            {
                pointInCamera.x = corners[1].x;
            }
            if (pointInCamera.y > corners[0].y)  //Top
            {
                pointInCamera.y = corners[0].y;
            }
            else if (pointInCamera.y < corners[3].y) //Bottom
            {
                pointInCamera.y = corners[3].y;
            }
            pointInWorld = MainCameraCache.mainCamera.transform.TransformPoint(pointInCamera);
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, pointInWorld - transform.position);
        }

        #endregion

        #region  Interface
        public virtual void UpdateTargetPoint(Vector3 point)
        {

        }

        public void SetPoseUpdateType(PoseUpdateType type)
        {
            this.updateType = type;
        }

        public PoseUpdateType GetPoseUpdateType()
        {
            return updateType;
        }

        public Pose RayPose => transform.GetPose();
        #endregion
    }
}
