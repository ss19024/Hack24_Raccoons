

using UnityEngine;
namespace Rokid.UXR.Interaction
{
    public abstract class PointerInteractor<TInteractor, TInteractable> : Interactor<TInteractor, TInteractable>
                                        where TInteractor : Interactor<TInteractor, TInteractable>
                                        where TInteractable : PointerInteractable<TInteractor, TInteractable>
    {
        public int realId => idIndex + Identifier;
        [SerializeField]
        public int idIndex = 0;
        /// <summary>
        /// 生成点的事件
        /// </summary>
        /// <param name="pointerEventType"></param>
        /// <param name="interactable"></param>
        protected void GeneratePointerEvent(PointerEventType pointerEventType, TInteractable interactable, int idIndex)
        {
            Pose pose = ComputePointerPose();

            if (interactable == null)
            {
                return;
            }

            if (interactable.PointableElement != null)
            {
                if (pointerEventType == PointerEventType.Hover)
                {
                    interactable.PointableElement.WhenPointerEventRaised +=
                        HandlePointerEventRaised;
                }
                else if (pointerEventType == PointerEventType.Unhover)
                {
                    interactable.PointableElement.WhenPointerEventRaised -=
                        HandlePointerEventRaised;
                }
            }

            interactable.PublishPointerEvent(
                new PointerEvent(Identifier + idIndex, pointerEventType, pose, Data));
        }

        /// <summary>
        /// 处理引发的事件指针
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void HandlePointerEventRaised(PointerEvent evt)
        {
            if (evt.Identifier == Identifier &&
                evt.Type == PointerEventType.Cancel &&
                Interactable != null)
            {
                TInteractable interactable = Interactable;
                interactable.RemoveInteractorByIdentifier(Identifier);
                interactable.PointableElement.WhenPointerEventRaised -=
                    HandlePointerEventRaised;
            }
        }

        protected override void InteractableSet(TInteractable interactable)
        {
            idIndex++;
            base.InteractableSet(interactable);
            GeneratePointerEvent(PointerEventType.Hover, interactable, idIndex);
        }

        protected override void InteractableUnset(TInteractable interactable)
        {
            GeneratePointerEvent(PointerEventType.Unhover, interactable, idIndex);
            base.InteractableUnset(interactable);
        }

        // 增加接口，长按button进入锁定状态后，需要cancel，以免退出锁定状态后触发click
        public void CancelWhenEnterLock()
        {
            if (_selectedInteractable == null)
            {
                return;
            }
            GeneratePointerEvent(PointerEventType.Cancel, _selectedInteractable, idIndex);
        }
        // cancel后重新获取hover
        public void ReHoverWhenLock()
        {
            if (_selectedInteractable == null)
            {
                return;
            }
            GeneratePointerEvent(PointerEventType.Hover, _selectedInteractable, idIndex);
        }

        protected override void InteractableSelected(TInteractable interactable)
        {
            base.InteractableSelected(interactable);
            GeneratePointerEvent(PointerEventType.Select, interactable, idIndex);
        }

        protected override void InteractableUnselected(TInteractable interactable)
        {
            GeneratePointerEvent(PointerEventType.Unselect, interactable, idIndex);
            base.InteractableUnselected(interactable);
        }

        protected override void DoPostprocess()
        {
            base.DoPostprocess();
            if (_interactable != null)
            {
                GeneratePointerEvent(PointerEventType.Move, _interactable, idIndex);
            }
        }

        protected abstract Pose ComputePointerPose();
    }
}

