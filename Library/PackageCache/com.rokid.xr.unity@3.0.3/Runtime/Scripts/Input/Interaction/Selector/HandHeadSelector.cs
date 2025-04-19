
using System;
using UnityEngine;
using Rokid.UXR.Utility;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Hand Selector
    /// </summary>
    public class HandHeadSelector : MonoBehaviour, ISelector, IHeadHandDriver
    {
        private bool press = false;
        public event Action WhenSelected;
        public event Action WhenUnselected;
        private bool selecting;
        public bool Selecting => selecting;
        private bool pointerDown;

        public bool PointerDown => pointerDown;
        private bool pointerUp;
        public bool PointerUp => pointerUp;

        public void OnHandPress(HandType hand)
        {
            if (press == false)
            {
                press = true;
                selecting = true;
                pointerDown = true;
                WhenSelected?.Invoke();
            }
        }

        public void OnHandRelease()
        {
            if (press)
            {
                press = false;
                selecting = false;
                pointerUp = true;
                WhenUnselected?.Invoke();
            }
        }

        public void OnChangeHoldHandType(HandType hand)
        {

        }

        void Update()
        {
            pointerDown = pointerUp = false;
            if (Utils.IsUnityEditor())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    selecting = true;
                    pointerDown = true;
                    WhenSelected?.Invoke();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    selecting = false;
                    pointerUp = true;
                    WhenUnselected?.Invoke();
                }
            }
        }

        public void OnBeforeChangeHoldHandType(HandType hand)
        {

        }
    }
}


