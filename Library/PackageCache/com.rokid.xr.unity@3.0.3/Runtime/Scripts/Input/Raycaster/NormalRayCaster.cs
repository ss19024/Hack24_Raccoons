
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// 射线投射器
    /// </summary>
    public class NormalRayCaster : BaseRayCaster
    {
        protected override void Init()
        {
            base.Init();
            if (inputOverride == null)
            {
                inputOverride = GetComponent<NormalInput>();
                if (inputOverride == null)
                {
                    inputOverride = gameObject.AddComponent<NormalInput>();
                }
            }
        }
        protected override bool ProcessDrag(Ray ray)
        {
            // 计算拖拽点的目标位置
            var targetHitPoint = transform.position + ray.direction * oriHitPointDis;
            var delta = targetHitPoint - oriHitPoint;
            if (pointerEventData.pointerDrag != null)
            {
                RKExecuteEvents.Execute(pointerEventData.pointerDrag, delta,
                                          RKExecuteEvents.rayDragHandler);
                RKExecuteEvents.Execute(pointerEventData.pointerDrag, targetHitPoint,
                                  RKExecuteEvents.rayDragToTargetHandler);
                if (sendGlobalEvent)
                    RKPointerListener.OnPointerDrag?.Invoke(pointerEventData);
            }
            oriHitPoint = targetHitPoint;
            return true;
        }
    }
}
