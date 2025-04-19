using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class ThreeDofInput : BaseInput
    {
        public override bool mousePresent => true;

        public override bool touchSupported => true;

        public override int touchCount => 0;

        private bool mouseActive = false;

        public override bool GetMouseButtonDown(int button)
        {
            return ThreeDofEventInput.Instance.GetRaySelector().PointerDown;
        }

        public override bool GetMouseButtonUp(int button)
        {
            return ThreeDofEventInput.Instance.GetRaySelector().PointerUp;
        }

        public override bool GetMouseButton(int button)
        {
            return ThreeDofEventInput.Instance.GetRaySelector().Selecting;
        }
    }
}
