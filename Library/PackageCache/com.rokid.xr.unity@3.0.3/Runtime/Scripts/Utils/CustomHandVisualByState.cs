using UnityEngine;
using Rokid.UXR.Interaction;
using System;
using System.Runtime.CompilerServices;

namespace Rokid.UXR.Utility
{
    public class CustomHandVisualByState : MonoBehaviour
    {
        [SerializeField]
        private ActiveHandType hand;
        [SerializeField]
        private GameObject handRay;

        [SerializeField]
        private GameObject handVisual;

        private bool handRayActive = false;
        private bool handVisualActive = false;
        private InteractorType interactorType = InteractorType.None;

        private void Start()
        {
            InputModuleManager.OnObjectActive += OnObjectActive;
            InteractorStateChange.OnInteractorTypeChange += OnInteractorTypeChange;
        }

        private void OnDestroy()
        {
            InputModuleManager.OnObjectActive -= OnObjectActive;
            InteractorStateChange.OnInteractorTypeChange -= OnInteractorTypeChange;
        }

        private void OnInteractorTypeChange(HandType hand, InteractorType interactorType)
        {
            if (this.hand == ActiveHandType.LeftHand && hand == HandType.LeftHand || this.hand == ActiveHandType.RightHand && hand == HandType.RightHand)
            {
                this.interactorType = interactorType;
            }
        }

        private void OnObjectActive(IInputModuleActive moduleActive, bool active)
        {
            if (moduleActive.ActiveHandType == this.hand)
            {
                if (moduleActive.Go == handRay)
                {
                    handRayActive = active;
                }
                else if (moduleActive.Go == handVisual)
                {
                    handVisualActive = active;
                }
            }
        }

        private void LateUpdate()
        {
            if (Utils.IsUnityEditor())
                return;
            if (handVisualActive && handRayActive && enabled)
            {
                handVisualActive = false;
                handRayActive = false;
                handVisual.gameObject.SetActive(false);
            }
            if (handVisualActive == false && interactorType == InteractorType.Near && enabled)
            {
                handVisualActive = true;
                handVisual.gameObject.SetActive(true);
            }
            // Debug.Log($"====CustomHandVisualByState===={hand} {handVisualActive}  handVisualActive:{handVisualActive},interactorType:{interactorType}");
        }
    }
}
