using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// This component makes it possible to connect Interactables in the
    /// inspector to Unity Events that are broadcast on state changes.
    /// </summary>
    public class InteractableUnityEventWrapper : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IInteractableView))]
        private MonoBehaviour _interactableView;
        private IInteractableView InteractableView;

        [SerializeField]
        private UnityEvent _whenHover;
        [SerializeField]
        private UnityEvent _whenUnhover;
        [SerializeField]
        private UnityEvent _whenSelect;
        [SerializeField]
        private UnityEvent _whenUnselect;
        [SerializeField]
        private UnityEvent _whenInteractorViewAdded;
        [SerializeField]
        private UnityEvent _whenInteractorViewRemoved;
        [SerializeField]
        private UnityEvent _whenSelectingInteractorViewAdded;
        [SerializeField]
        private UnityEvent _whenSelectingInteractorViewRemoved;

        #region Properties

        public UnityEvent WhenHover => _whenHover;
        public UnityEvent WhenUnhover => _whenUnhover;
        public UnityEvent WhenSelect => _whenSelect;
        public UnityEvent WhenUnselect => _whenUnselect;
        public UnityEvent WhenInteractorViewAdded => _whenInteractorViewAdded;
        public UnityEvent WhenInteractorViewRemoved => _whenInteractorViewRemoved;
        public UnityEvent WhenSelectingInteractorViewAdded => _whenSelectingInteractorViewAdded;
        public UnityEvent WhenSelectingInteractorViewRemoved => _whenSelectingInteractorViewRemoved;

        #endregion

        protected bool _started = false;

        protected virtual void Awake()
        {
            InteractableView = _interactableView as IInteractableView;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            Assert.IsNotNull(InteractableView);
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                InteractableView.WhenStateChanged += HandleStateChanged;
                InteractableView.WhenInteractorViewAdded += HandleInteractorViewAdded;
                InteractableView.WhenInteractorViewRemoved += HandleInteractorViewRemoved;
                InteractableView.WhenSelectingInteractorViewAdded += HandleSelectingInteractorViewAdded;
                InteractableView.WhenSelectingInteractorViewRemoved += HandleSelectingInteractorViewRemoved;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                InteractableView.WhenStateChanged -= HandleStateChanged;
                InteractableView.WhenStateChanged -= HandleStateChanged;
                InteractableView.WhenInteractorViewAdded -= HandleInteractorViewAdded;
                InteractableView.WhenInteractorViewRemoved -= HandleInteractorViewRemoved;
                InteractableView.WhenSelectingInteractorViewAdded -= HandleSelectingInteractorViewAdded;
                InteractableView.WhenSelectingInteractorViewRemoved -= HandleSelectingInteractorViewRemoved;
            }
        }

        private void HandleStateChanged(InteractableStateChangeArgs args)
        {
            switch (args.NewState)
            {
                case InteractableState.Normal:
                    if (args.PreviousState == InteractableState.Hover)
                    {
                        _whenUnhover.Invoke();
                    }

                    break;
                case InteractableState.Hover:
                    if (args.PreviousState == InteractableState.Normal)
                    {
                        _whenHover.Invoke();
                    }
                    else if (args.PreviousState == InteractableState.Select)
                    {
                        _whenUnselect.Invoke();
                    }
                    break;
                case InteractableState.Select:
                    if (args.PreviousState == InteractableState.Hover)
                    {
                        if (_whenSelect != null)
                            _whenSelect.Invoke();
                    }
                    break;
            }
        }

        private void HandleInteractorViewAdded(IInteractorView interactorView)
        {
            WhenInteractorViewAdded.Invoke();
        }

        private void HandleInteractorViewRemoved(IInteractorView interactorView)
        {
            WhenInteractorViewRemoved.Invoke();
        }

        private void HandleSelectingInteractorViewAdded(IInteractorView interactorView)
        {
            WhenSelectingInteractorViewAdded.Invoke();
        }

        private void HandleSelectingInteractorViewRemoved(IInteractorView interactorView)
        {
            WhenSelectingInteractorViewRemoved.Invoke();
        }
    }
}
