using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Rokid.UXR.Interaction
{
    public class GrabInteractable : MonoBehaviour, IHandHoverBegin, IHandHoverEnd, IGrabbedToHand, IReleasedFromHand
    {
        // [Tooltip("Hide the whole hand on grabbed and show on release")]
        // public bool hideHandOnGrabbed = true;

        [Tooltip("Specify whether you want to snap to the hand's object attachment point, or just the raw hand")]
        public bool useHandObjectAttachmentPoint = true;
        public bool attachEaseIn = false;
        [HideInInspector]
        public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float snapAttachEaseInTime = 0.15f;
        public bool snapAttachEaseInCompleted = false;

        [Tooltip("Set whether or not you want this interactable to highlight when hovering over it")]
        public bool changeScaleOnHover = true;

        [Tooltip("Higher is better")]
        public int hoverPriority = 0;

        [System.NonSerialized]
        public Hand grabbedToHand;

        [System.NonSerialized]
        public List<Hand> hoveringHands = new List<Hand>();
        [Tooltip("hoverScale/normalScale")]
        public float rate = 1.2f;
        private Vector3 hoverScale;
        private Vector3 normalScale;

        public UnityEvent OnPickUp = new UnityEvent();
        public UnityEvent OnDropDown = new UnityEvent();
        public UnityEvent OnHeldUpdate = new UnityEvent();
        public UnityEvent OnHoverBegin = new UnityEvent();
        public UnityEvent OnHoverEnd = new UnityEvent();

        public Hand hoveringHand
        {
            get
            {
                if (hoveringHands.Count > 0)
                    return hoveringHands[0];
                return null;
            }
        }

        public bool isDestroying { get; protected set; }


        #region  UnityEvent
        protected virtual void Start()
        {
            if (changeScaleOnHover)
            {
                normalScale = transform.localScale;
                hoverScale = normalScale * rate;
            }
        }

        private void OnDestroy()
        {
            isDestroying = true;
            if (grabbedToHand != null)
            {
                grabbedToHand.ReleaseObject(this.gameObject, false);
            }
        }

        #endregion

        #region  HandEvent

        public void OnGrabbedToHand(Hand hand)
        {
            grabbedToHand = hand;
            SetScale(normalScale);
            OnPickUp?.Invoke();
        }

        public void OnReleasedFromHand(Hand hand)
        {
            grabbedToHand = null;
            OnDropDown.Invoke();
        }

        public void OnHandHoverBegin(Hand hand)
        {
            hoveringHands.Add(hand);
            SetScale(hoverScale);
            OnHoverBegin?.Invoke();
        }

        public void OnHandHoverEnd(Hand hand)
        {
            hoveringHands.Remove(hand);
            SetScale(normalScale);
            OnHoverEnd?.Invoke();
        }

        private void SetScale(Vector3 scale)
        {
            if (changeScaleOnHover)
            {
                transform.localScale = scale;
            }
        }

        #endregion
    }
}
