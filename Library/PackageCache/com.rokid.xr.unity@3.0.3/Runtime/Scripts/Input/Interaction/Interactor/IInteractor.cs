
using System;

namespace Rokid.UXR.Interaction
{
    public struct InteractorStateChangeArgs
    {
        public InteractorState PreviousState { get; }
        public InteractorState NewState { get; }

        public InteractorStateChangeArgs(
            InteractorState previousState,
            InteractorState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// IInteractorView defines the view for an object that can interact with other objects.
    /// 定义可以与其他对象交互的对象的视图
    /// </summary>
    public interface IInteractorView
    {
        int Identifier { get; }
        public object Data { get; }

        bool HasCandidate { get; }
        object CandidateProperties { get; }

        bool HasInteractable { get; }
        bool HasSelectedInteractable { get; }

        InteractorState State { get; }
        event Action<InteractorStateChangeArgs> WhenStateChanged;
        event Action WhenPreprocessed;
        event Action WhenProcessed;
        event Action WhenPostprocessed;
    }

    public interface IUpdateDriver
    {
        bool IsRootDriver { get; set; }
        void Drive();
    }

    /// <summary>
    /// IInteractor defines an object that can interact with other objects
    /// and can handle selection events to change its state.
    /// 定义了一个可以与其他对象交互并可以处理选择事件以改变其状态的对象。
    /// </summary>
    public interface IInteractor : IInteractorView, IUpdateDriver
    {

        void Preprocess();
        void Process();
        void Postprocess();

        void ProcessCandidate();
        void Enable();
        void Disable();
        void Hover();
        void Unhover();
        void Select();
        void Unselect();

        bool ShouldHover { get; }
        bool ShouldUnhover { get; }
        bool ShouldSelect { get; }
        bool ShouldUnselect { get; }
    }
}
