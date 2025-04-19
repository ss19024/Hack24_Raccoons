
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Module Interactor, users bind the corresponding interactors to the corresponding modules, and handle updates, destruction, and other operations of the corresponding module interactors.
    /// </summary>
    public class ModuleInteractor : MonoBehaviour
    {
        public InputModuleType moduleType;

        private void Start()
        {

        }

        private void Awake()
        {
            switch (moduleType)
            {
                case InputModuleType.ThreeDof:
                    UpdateInteractor(ThreeDofEventInput.Instance, transform);
                    ThreeDofEventInput.OnReleaseThreeDofModule += DestroySelf;
                    break;
                case InputModuleType.Gesture:
                    UpdateInteractor(GesEventInput.Instance, transform);
                    GesEventInput.OnReleaseGesModule += DestroySelf;
                    break;
                case InputModuleType.Mouse:
                    UpdateInteractor(MouseEventInput.Instance, transform);
                    MouseEventInput.OnReleaseMouseModule += DestroySelf;
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (moduleType)
            {
                case InputModuleType.ThreeDof:
                    ThreeDofEventInput.OnReleaseThreeDofModule -= DestroySelf;
                    break;
                case InputModuleType.Gesture:
                    GesEventInput.OnReleaseGesModule -= DestroySelf;
                    break;
                case InputModuleType.Mouse:
                    MouseEventInput.OnReleaseMouseModule -= DestroySelf;
                    break;
            }
        }

        private void UpdateInteractor(IEventInput eventInput, Transform curInteractor)
        {
            if (eventInput.Interactor != null)
            {
                DestroyImmediate(eventInput.Interactor.gameObject);
            }
            eventInput.Interactor = this.transform;
        }

        private void DestroySelf()
        {
            Destroy(this.gameObject);
        }
    }

}

