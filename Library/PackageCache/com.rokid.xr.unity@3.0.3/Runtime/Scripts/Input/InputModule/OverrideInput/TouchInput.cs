using Rokid.UXR.Module;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class TouchInput : BaseInput
    {
        public override bool mousePresent => true;

        public override bool touchSupported => true;

        public override int touchCount => RKTouchInput.Instance.GetInsideTouchCount();

        private bool mouseActive = false;

        public override bool GetMouseButtonDown(int button)
        {
            return TouchPadEventInput.Instance.GetRaySelector().PointerDown;
        }

        public override bool GetMouseButtonUp(int button)
        {
            return TouchPadEventInput.Instance.GetRaySelector().PointerUp;
        }

        public override bool GetMouseButton(int button)
        {
            return TouchPadEventInput.Instance.GetRaySelector().Selecting;
        }
    }
}
