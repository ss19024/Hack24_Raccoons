using UnityEngine;
using Rokid.UXR.Utility;
using Rokid.UXR.Interaction.ThirdParty;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class GesRayCaster : BaseRayCaster
    {
        [SerializeField]
        private HandType hand;
        private GestureInput gesInput;
        private ManipulationMoveLogic moveLogic;

        protected override void Start()
        {
            base.Start();
            if (base.rayInteractor == null)
                base.rayInteractor = GetComponent<RayInteractor>();
            moveLogic = new ManipulationMoveLogic();
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

        protected override void OnBeginDrag()
        {
            base.OnBeginDrag();
            moveLogic.Setup(transform.GetPose(), oriHitPoint, pointerEventData.pointerDrag.transform.GetPose(), pointerEventData.pointerDrag.transform.localScale);
        }

        protected override bool ProcessDrag(Ray ray)
        {
            var targetPosition = moveLogic.UpdateTransform(transform.GetPose(), selectedObj.transform, true, false);
            var delta = targetPosition - oriHitPoint;
            if (pointerEventData.pointerDrag != null)
            {
                RKExecuteEvents.Execute(pointerEventData.pointerDrag, delta,
                                          RKExecuteEvents.rayDragHandler);
                RKExecuteEvents.Execute(pointerEventData.pointerDrag, targetPosition,
                                  RKExecuteEvents.rayDragToTargetHandler);
                if (sendGlobalEvent)
                    RKPointerListener.OnPointerDrag?.Invoke(pointerEventData);
            }
            oriHitPoint = targetPosition;
            return true;
        }
    }
}
