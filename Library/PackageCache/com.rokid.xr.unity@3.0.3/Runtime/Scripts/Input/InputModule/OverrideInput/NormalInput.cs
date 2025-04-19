using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class NormalInput : BaseInput
    {
        public override bool mousePresent => true;

        public override bool touchSupported => true;

        public override int touchCount => 0;

        private bool mouseActive = false;

        public override bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(0);
        }

        public override bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(0);
        }

        public override bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(0);
        }
    }
}
