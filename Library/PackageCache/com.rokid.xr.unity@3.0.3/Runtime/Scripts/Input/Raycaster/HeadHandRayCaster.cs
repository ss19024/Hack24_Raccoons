using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class HeadHandRayCaster : BaseRayCaster, IHeadHandDriver
    {
        [SerializeField]
        private HandType hand;
        private GestureInput gesInput;

        protected override void Start()
        {
            base.Start();
            if (base.rayInteractor == null)
                base.rayInteractor = GetComponent<RayInteractor>();
        }

        protected override void Init()
        {
            base.Init();
            if (inputOverride == null)
            {
                inputOverride = GetComponent<GestureInput>();
                if (inputOverride == null)
                {
                    inputOverride = gameObject.AddComponent<GestureInput>();
                }
            }
            gesInput = GetComponent<GestureInput>();
            gesInput.SetHandType(hand);
        }


        protected override bool CanDrag(Vector3 delta)
        {
            return false;
        }


        protected override bool ProcessDrag(Ray ray)
        {
            RKLog.Info("====HeadHandRayCaster====: ProcessDrag");
            // 计算拖拽点的目标位置
            var targetHitPoint = ray.origin + ray.direction * oriHitPointDis;
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

        public void OnChangeHoldHandType(HandType hand)
        {
            this.hand = hand;
            gesInput.SetHandType(hand);
            TriggerPointerDown();
            RKLog.Info("====HeadHandRayCaster====: ChangeHoldHandType");
        }

        protected override bool TriggerPointerDown()
        {
            RKLog.Info("====HeadHandRayCaster====: TriggerPointerDown");
            pressTime = 0;
            Raycast(ray, Mathf.Infinity, sortedRaycastResults);
            result = FirstRaycastResult();
            UpdatePointerEventData();
            if (result.gameObject != null)
            {
                selectedObj = result.gameObject;
                OnFirstSelect();
            }
            else
            {
                ProcessNothingDownEvent(pointerEventData);
            }
            return true;
        }

        public void OnHandPress(HandType hand)
        {

        }

        public void OnHandRelease()
        {

        }

        public void OnBeforeChangeHoldHandType(HandType hand)
        {

        }
    }
}
