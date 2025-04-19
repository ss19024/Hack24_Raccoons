using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rokid.UXR.Interaction
{
    public abstract class PointerInteractable<TInteractor, TInteractable> : Interactable<TInteractor, TInteractable>,
            IPointable
            where TInteractor : Interactor<TInteractor, TInteractable>
            where TInteractable : PointerInteractable<TInteractor, TInteractable>
    {
        [SerializeField, Interface(typeof(IPointableElement)), Optional]
        private MonoBehaviour _pointableElement;

        public IPointableElement PointableElement { get; private set; }

        public event Action<PointerEvent> WhenPointerEventRaised = delegate { };


        public void PublishPointerEvent(PointerEvent evt)
        {
            WhenPointerEventRaised(evt);
        }

        protected override void Awake()
        {
            base.Awake();
            if (_pointableElement != null)
            {
                PointableElement = _pointableElement as IPointableElement;
            }
        }

        protected override void Start()
        {
            if (_pointableElement != null)
            {
                Assert.IsNotNull(PointableElement);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (PointableElement != null)
            {
                WhenPointerEventRaised += PointableElement.ProcessPointerEvent;
            }
        }

        protected override void OnDisable()
        {
            if (PointableElement != null)
            {
                WhenPointerEventRaised -= PointableElement.ProcessPointerEvent;
            }
            base.OnDisable();
        }
    }
}
