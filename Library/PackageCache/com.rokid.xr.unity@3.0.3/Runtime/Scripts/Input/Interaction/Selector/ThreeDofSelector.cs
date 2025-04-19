using System;
using Rokid.UXR.Module;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class ThreeDofSelector : MonoBehaviour, ISelector
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

        void LateUpdate()
        {
            pointerDown = pointerUp = false;
            if (ThreeDofEventInput.Instance.GetThreeDofType() == ThreeDofType.Phone || ThreeDofEventInput.Instance.GetThreeDofType() == ThreeDofType.Station2)
            {
                if (RKTouchInput.Instance.GetInsideTouchCount() > 0)
                {
                    if (touchDown == false && RKTouchInput.Instance.GetInsideTouch(0).phase == TouchPhase.Began)
                    {
                        touchDown = true;
                        selecting = true;
                        pointerDown = true;
                        WhenSelected?.Invoke();
                    }

                    if (touchDown && RKTouchInput.Instance.GetInsideTouch(0).phase == TouchPhase.Ended || RKTouchInput.Instance.GetInsideTouch(0).phase == TouchPhase.Canceled)
                    {
                        touchDown = false;
                        selecting = false;
                        pointerUp = true;
                        WhenUnselected?.Invoke();
                    }
                }

                if (touchDown && Input.touchCount == 0)
                {
                    touchDown = false;
                    selecting = false;
                    pointerUp = true;
                    WhenUnselected?.Invoke();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    selecting = true;
                    pointerDown = true;
                    WhenSelected?.Invoke();
                }

                if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.JoystickButton0))
                {
                    selecting = false;
                    pointerUp = true;
                    WhenUnselected?.Invoke();
                }
            }
        }
    }
}
