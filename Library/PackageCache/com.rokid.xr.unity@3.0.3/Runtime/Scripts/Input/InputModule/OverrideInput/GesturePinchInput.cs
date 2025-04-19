using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class GesturePinchInput : BaseInput
    {
        public override bool mousePresent => true;

        public override Vector2 mousePosition => GetGesPosition();

        public override bool touchSupported => true;

        public override int touchCount => 0;

        private bool mouseActive = false;

        public HandType hand = HandType.None;

        public void SetHandType(HandType hand)
        {
            this.hand = hand;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override bool GetMouseButtonDown(int button)
        {
            return GesEventInput.Instance.GetHandDown(hand, true);
        }

        public override bool GetMouseButtonUp(int button)
        {
            return GesEventInput.Instance.GetHandUp(hand, true);
        }

        public override bool GetMouseButton(int button)
        {
            return GesEventInput.Instance.GetHandPress(hand, true);
        }

        public Vector2 GetGesPosition()
        {
            Vector2 gesPos = Vector2.zero;
            Gesture gesture = GesEventInput.Instance.GetGesture(hand);
            if (gesture != null)
            {
                gesPos = gesture.position;
            }
            return gesPos;
        }
    }

}
