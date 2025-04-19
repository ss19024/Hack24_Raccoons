using System;
using UnityEngine;
using Rokid.UXR.Utility;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Hand Selector
    /// </summary>
    public class HandSelector : MonoBehaviour, ISelector
    {
        [SerializeField]
        private HandType handType;
        public event Action WhenSelected;
        public event Action WhenUnselected;
        private bool dragging = false;
        private bool selecting;
        public bool Selecting => selecting;
        private bool pointerDown;
        public bool PointerDown => pointerDown;
        private bool pointerUp;
        public bool PointerUp => pointerUp;

        private void Start()
        {
            InteractorStateChange.OnHandDragStatusChanged += OnHandDragStatusChanged;
        }

        private void OnDestroy()
        {
            InteractorStateChange.OnHandDragStatusChanged -= OnHandDragStatusChanged;
        }

        private void OnHandDragStatusChanged(HandType hand, bool dragging)
        {
            if (hand == handType)
            {
                if (dragging)
                    this.dragging = true;
            }
        }

        void Update()
        {
            pointerDown = pointerUp = false;
            if (dragging)
            {
                if (HandUtils.CanReleaseHandDrag(handType, 0.02f))
                {
                    dragging = false;
                    selecting = false;
                    pointerUp = true;
                    WhenUnselected?.Invoke();
                }
            }
            else
            {
                if (GesEventInput.Instance.GetHandDown(handType, true) || GesEventInput.Instance.GetHandDown(handType, false))
                {
                    selecting = true;
                    pointerDown = true;
                    WhenSelected?.Invoke();
                }

                if (GesEventInput.Instance.GetHandUp(handType, true) || GesEventInput.Instance.GetHandUp(handType, false))
                {
                    selecting = false;
                    pointerUp = true;
                    WhenUnselected?.Invoke();
                }
            }
        }
    }
}


