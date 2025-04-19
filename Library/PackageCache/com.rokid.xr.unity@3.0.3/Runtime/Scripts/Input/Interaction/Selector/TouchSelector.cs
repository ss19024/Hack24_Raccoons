using System;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class TouchSelector : MonoBehaviour, ISelector
    {
        public event Action WhenSelected;
        public event Action WhenUnselected;
        private bool selecting;
        public bool Selecting => selecting;
        private bool pointerDown;
        public bool PointerDown => pointerDown;
        private bool pointerUp;
        public bool PointerUp => pointerUp;
        private bool touchDown = false;

        void Update()
        {
            pointerDown = pointerUp = false;
            if (!TouchPadEventInput.Instance.TouchMove() && !TouchPadEventInput.Instance.LongPress() && TouchPadEventInput.Instance.PointerUp())
            {
                WhenSelected?.Invoke();
                selecting = true;
                pointerDown = true;
                Loom.QueueOnMainThread(() => { selecting = false; pointerUp = true; WhenUnselected?.Invoke(); }, 0.1f);
            }

            if (touchDown && TouchPadEventInput.Instance.PointerUp())
            {
                touchDown = false;
                selecting = false;
                pointerUp = true;
                WhenUnselected?.Invoke();
                TouchPadEventInput.Instance.SetLongPress(false);
            }

            if (touchDown == false && TouchPadEventInput.Instance.LongPress())
            {
                touchDown = true;
                pointerDown = true;
                selecting = true;
                WhenSelected?.Invoke();
            }
        }
    }
}
