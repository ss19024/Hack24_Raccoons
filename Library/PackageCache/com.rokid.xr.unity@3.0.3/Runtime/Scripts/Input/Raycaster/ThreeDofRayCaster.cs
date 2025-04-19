using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// 射线投射器
    /// </summary>
    public class ThreeDofRayCaster : BaseRayCaster
    {
        [SerializeField]
        public float maxDragDistance = 10;
        [SerializeField]
        public float minDragDistance = 0.5f;
        [SerializeField]
        private float deltaZScale = 1.0f;
        private float curHitPointDis;
        private Vector3 curHitPoint;
        private Vector3 targetHitPoint;
        private Vector3 oriRayPos;
        private Vector3 dragOffset;

        protected override void Init()
        {
            base.Init();
            if (inputOverride == null)
            {
                inputOverride = GetComponent<ThreeDofInput>();
                if (inputOverride == null)
                {
                    inputOverride = gameObject.AddComponent<ThreeDofInput>();
                }
            }
        }

        protected override void OnBeginDrag()
        {
            base.OnBeginDrag();
            CalDeltaZ();
            targetHitPoint = rayOrigin.position + ray.direction * curHitPointDis;
            dragOffset = pointerEventData.pointerDrag.transform.position - targetHitPoint;
        }

        protected override bool ProcessDrag(Ray ray)
        {
            // 计算拖拽点的目标位置
            CalDeltaZ();
            targetHitPoint = rayOrigin.position + ray.direction * curHitPointDis;
            var delta = (targetHitPoint - oriHitPoint);
            if (pointerEventData.pointerDrag != null)
            {
                RKExecuteEvents.Execute(pointerEventData.pointerDrag, delta,
                                          RKExecuteEvents.rayDragHandler);
                RKExecuteEvents.Execute(pointerEventData.pointerDrag, targetHitPoint + dragOffset,
                                  RKExecuteEvents.rayDragToTargetHandler);
                if (sendGlobalEvent)
                    RKPointerListener.OnPointerDrag?.Invoke(pointerEventData);
            }
            curHitPoint = oriHitPoint = targetHitPoint;
            return true;
        }

        protected override void OnFirstSelect()
        {
            oriHitPoint = result.worldPosition;
            oriRayPos = ray.origin;
        }

        protected override void StatusRefresh()
        {
            oriRayPos = ray.origin;
        }

        private void CalDeltaZ()
        {
            if (Utils.IsAndroidPlatform())
            {
                Vector3 deltaZ = ray.direction * ThreeDofEventInput.Instance.ForwardSpeed() * deltaZScale;
                Vector3 deltaRayPos = ray.origin - oriRayPos;
                oriRayPos = ray.origin;
                curHitPoint = oriHitPoint + deltaRayPos;
                curHitPointDis = Mathf.Clamp(Vector3.Distance(curHitPoint + deltaZ, ray.origin), minDragDistance, maxDragDistance);
            }
            else
            {
                Vector3 deltaZ = ray.direction * Input.mouseScrollDelta.y * 0.5f;
                Vector3 deltaRayPos = ray.origin - oriRayPos;
                oriRayPos = ray.origin;
                curHitPoint = oriHitPoint + deltaRayPos;
                curHitPointDis = Mathf.Clamp(Vector3.Distance(curHitPoint + deltaZ, ray.origin), minDragDistance, maxDragDistance);
            }
        }
    }
}
