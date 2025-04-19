using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public class InteractorStateChange : MonoBehaviour
    {
        [SerializeField]
        private HandType hand;

        [SerializeField]
        private InteractorState prePokeState;
        [SerializeField]
        private RayInteractor rayInteractor;
        [SerializeField]
        private PokeInteractor pokeInteractor;
        private bool isFar = true;
        private bool dragging = false;
        private bool isGrabHovering = false;
        private bool isGrabbedToHand = false;
        private bool stateChanged = true;

        [Obsolete("Use OnInteractorTypeChange instead")]
        public static event UnityAction<HandType> OnPokeInteractorHover;
        [Obsolete("Use OnInteractorTypeChange instead")]
        public static event UnityAction<HandType> OnPokeInteractorUnHover;
        public static event UnityAction<HandType, InteractorType> OnInteractorTypeChange;
        public static event UnityAction<HandType, Pose> OnPokePoseUpdate;
        public static event UnityAction<HandType, Vector3> OnPokeSelectUpdate;
        public static event UnityAction<HandType> OnPokeUnSelectUpdate;
        public static event UnityAction<HandType, bool> OnHandDragStatusChanged;
        public static event UnityAction<HandType, PointerEventData, bool> OnHandDragStatusChangedWithData;


        private void Start()
        {
            prePokeState = pokeInteractor.State;
            RKPointerListener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd += OnPointerDragEnd;
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            Hand.OnHandHoverBegin += OnHandHoverBegin;
            Hand.OnHandHoverEnd += OnHandHoverEnd;
            Hand.OnGrabbedToHand += OnGrabbedToHand;
            Hand.OnReleasedFromHand += OnReleaseFromHand;
        }

        private void OnReleaseFromHand(HandType handType)
        {
            if (handType == hand)
            {
                isGrabbedToHand = false;
                stateChanged = true;
                // RKLog.KeyInfo("====InteractorStateChange====: OnReleaseToHand");
            }
        }

        private void OnGrabbedToHand(HandType handType, GameObject gameObject)
        {
            if (handType == hand)
            {
                isGrabbedToHand = true;
                stateChanged = true;
                // RKLog.KeyInfo("====InteractorStateChange====: OnGrabbedToHand");
            }
        }

        private void OnHandHoverBegin(HandType handType, GameObject gameObject)
        {
            if (handType == hand)
            {
                isGrabHovering = true;
                stateChanged = true;
                // RKLog.KeyInfo("====InteractorStateChange====: OnHoverBegin");
            }
        }

        private void OnHandHoverEnd(HandType handType)
        {
            if (handType == hand)
            {
                isGrabHovering = false;
                stateChanged = true;
                // RKLog.KeyInfo("====InteractorStateChange====: OnHoverEnd");
            }
        }

        private void OnTrackedFailed(HandType hand)
        {
            // RKLog.KeyInfo($"====InteractorStateChange====: OnTrackedFailed {hand} {dragging}");
            if (this.hand == hand || hand == HandType.None)
            {
                if (dragging)
                {
                    dragging = false;
                    stateChanged = true;
                    OnHandDragStatusChanged?.Invoke(this.hand, dragging);
                    OnHandDragStatusChangedWithData?.Invoke(this.hand, null, dragging);
                }
            }
        }

        private void OnPointerDragEnd(PointerEventData pointerData)
        {
            if (pointerData.pointerId == rayInteractor.realId)
            {
                dragging = false;
                stateChanged = true;
                OnHandDragStatusChanged?.Invoke(hand, dragging);
                OnHandDragStatusChangedWithData?.Invoke(this.hand, pointerData, dragging);
                // RKLog.KeyInfo($"====InteractorStateChange====: OnPointerDragEnd {hand},{dragging}");
            }
        }

        private void OnPointerDragBegin(PointerEventData pointerData)
        {
            if (pointerData.pointerId == rayInteractor.realId)
            {
                dragging = true;
                stateChanged = true;
                OnHandDragStatusChanged?.Invoke(hand, dragging);
                OnHandDragStatusChangedWithData?.Invoke(this.hand, pointerData, dragging);
                // RKLog.KeyInfo($"====InteractorStateChange====: OnPointerDragBegin {hand},{dragging}");
            }
        }

        private void OnDisable()
        {
            stateChanged = true;
            dragging = false;
            OnHandDragStatusChanged?.Invoke(hand, dragging);
            OnHandDragStatusChangedWithData?.Invoke(this.hand, null, dragging);
        }

        private void OnDestroy()
        {
            RKPointerListener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerListener.OnPointerDragEnd -= OnPointerDragEnd;
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
            Hand.OnHandHoverBegin -= OnHandHoverBegin;
            Hand.OnHandHoverEnd -= OnHandHoverEnd;
            Hand.OnGrabbedToHand -= OnGrabbedToHand;
            Hand.OnReleasedFromHand -= OnReleaseFromHand;
        }

        private void Update()
        {
            if (dragging && HandUtils.CanReleaseHandDrag(hand))
            {
                stateChanged = true;
                dragging = false;
                OnHandDragStatusChanged?.Invoke(hand, dragging);
                OnHandDragStatusChangedWithData?.Invoke(this.hand, null, dragging);
                // RKLog.KeyInfo($"====InteractorStateChange====: PointerRelease {hand},{dragging}");
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F))
            {
                isFar = !isFar;
                if (isFar)
                {
                    // RKLog.Debug($"====InteractorStateChange====:Set InteractorType.Far :{hand}");
                    GesEventInput.Instance.SetInteractorType(InteractorType.Far, hand);
                    OnPokeInteractorUnHover?.Invoke(hand);
                    OnInteractorTypeChange?.Invoke(hand, InteractorType.Far);
                }
                else
                {
                    // RKLog.Debug($"====InteractorStateChange====:Set InteractorType.Near :{hand}");
                    GesEventInput.Instance.SetInteractorType(InteractorType.Near, hand);
                    OnPokeInteractorHover?.Invoke(hand);
                    OnInteractorTypeChange?.Invoke(hand, InteractorType.Near);
                }
            }
#else
            if (prePokeState != pokeInteractor.State)
            {
                stateChanged = true;
                prePokeState = pokeInteractor.State;
            }
            if (stateChanged)
            {
                // RKLog.KeyInfo($"====InteractorStateChange====: stateChanged  handType: {hand}, isFar: {isFar},dragging: {dragging},isGrabHovering: {isGrabHovering} ,isGrabbedToHand: {isGrabbedToHand}");
                if (dragging == false)
                {
                    if (pokeInteractor.State == InteractorState.Hover || pokeInteractor.State == InteractorState.Select || isGrabHovering || isGrabbedToHand)
                    {
                        OnPokeInteractorHover?.Invoke(hand);
                        OnInteractorTypeChange?.Invoke(hand, InteractorType.Near);
                        GesEventInput.Instance.SetInteractorType(InteractorType.Near, hand);
                        isFar = false;
                    }

                    if ((pokeInteractor.State == InteractorState.Normal || pokeInteractor.State == InteractorState.Disabled) && isGrabHovering == false && isGrabbedToHand == false)
                    {
                        OnPokeInteractorUnHover?.Invoke(hand);
                        OnInteractorTypeChange?.Invoke(hand, InteractorType.Far);
                        GesEventInput.Instance.SetInteractorType(InteractorType.Far, hand);
                        isFar = true;
                    }
                }
                stateChanged = false;
            }

            if (pokeInteractor.State == InteractorState.Select)
            {
                OnPokeSelectUpdate?.Invoke(hand, pokeInteractor.TouchPoint);
            }
            else
            {
                OnPokeUnSelectUpdate?.Invoke(hand);
            }

            if (GesEventInput.Instance.GetInteractorType(hand) == InteractorType.Near)
            {
                OnPokePoseUpdate?.Invoke(hand, new Pose(pokeInteractor.transform.position, Quaternion.FromToRotation(Vector3.forward, -pokeInteractor.TouchNormal)));
            }
#endif
        }
    }
}

