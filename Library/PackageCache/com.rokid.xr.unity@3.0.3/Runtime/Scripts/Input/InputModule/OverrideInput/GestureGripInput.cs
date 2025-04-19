using UnityEngine;
using UnityEngine.EventSystems;
using Rokid.UXR;

namespace Rokid.UXR.Interaction
{
    public class GestureGripInput : BaseInput
    {
        public override bool mousePresent => true;

        public override Vector2 mousePosition => GetGesPosition();

        public override bool touchSupported => false;

        public override int touchCount => 0;

        [SerializeField]
        private HandType hand;

        public void SetHandType(HandType hand)
        {
            this.hand = hand;
        }

        public override bool GetMouseButtonDown(int button)
        {
            return GesEventInput.Instance.GetHandDown(hand, false);
        }

        public override bool GetMouseButtonUp(int button)
        {
            return GesEventInput.Instance.GetHandUp(hand, false);
        }

        public override bool GetMouseButton(int button)
        {
            return GesEventInput.Instance.GetHandPress(hand, false);
        }

        public Vector3 GetGesPosition()
        {
            Vector3 gesPos = Vector2.zero;
            Gesture gesture = GesEventInput.Instance.GetGesture(hand);
            return gesPos;
        }
    }

}
