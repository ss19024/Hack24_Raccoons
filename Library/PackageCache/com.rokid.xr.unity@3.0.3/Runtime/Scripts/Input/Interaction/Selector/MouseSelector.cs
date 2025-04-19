using System;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class MouseSelector : MonoBehaviour, ISelector
    {
        public event Action WhenSelected;
        public event Action WhenUnselected;
        private bool selecting;
        public bool Selecting => selecting;
        private bool pointerDown;
        public bool PointerDown => pointerDown;
        private bool pointerUp;
        public bool PointerUp => pointerUp;

        void Update()
        {
            pointerDown = pointerUp = false;
            if (MouseEventInput.Instance.GetMouseButtonDown(0))
            {
                selecting = true;
                pointerDown = true;
                WhenSelected?.Invoke();
            }

            if (MouseEventInput.Instance.GetMouseButtonUp(0))
            {
                selecting = false;
                pointerUp = true;
                WhenUnselected?.Invoke();
            }
        }
    }
}
