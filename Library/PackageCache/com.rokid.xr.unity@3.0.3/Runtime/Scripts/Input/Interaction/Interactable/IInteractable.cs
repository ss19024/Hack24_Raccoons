
namespace Rokid.UXR.Interaction
{
    using System;
    using System.Collections.Generic;

    public struct InteractableStateChangeArgs
    {
        public InteractableState PreviousState { get; }
        public InteractableState NewState { get; }

        public InteractableStateChangeArgs(
            InteractableState previousState,
            InteractableState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// An IInteractableView defines the view for an object that can be
    /// interacted with.
    /// </summary>
    public interface IInteractableView
    {
        object Data { get; }

        InteractableState State { get; }
        event Action<InteractableStateChangeArgs> WhenStateChanged;

        int MaxInteractors { get; }
        int MaxSelectingInteractors { get; }

        IEnumerable<IInteractorView> InteractorViews { get; }
        IEnumerable<IInteractorView> SelectingInteractorViews { get; }

        event Action<IInteractorView> WhenInteractorViewAdded;
        event Action<IInteractorView> WhenInteractorViewRemoved;
        event Action<IInteractorView> WhenSelectingInteractorViewAdded;
        event Action<IInteractorView> WhenSelectingInteractorViewRemoved;
    }

    /// <summary>
    /// An object that can be interacted with, an IInteractable can, in addition to
    /// an IInteractableView, be enabled or disabled.
    /// </summary>
    public interface IInteractable : IInteractableView
    {
        void Enable();
        void Disable();
        new int MaxInteractors { get; set; }
        new int MaxSelectingInteractors { get; set; }
        void RemoveInteractorByIdentifier(int id);
    }
}
