
namespace Rokid.UXR.Interaction
{
    public class HandEventConst
    {
        public const string OnHandHoverBegin = "OnHandHoverBegin";
        public const string OnHandHoverUpdate = "OnHandHoverUpdate";
        public const string OnHandHoverEnd = "OnHandHoverEnd";
        public const string OnGrabbedToHand = "OnGrabbedToHand";
        public const string OnGrabbedUpdate = "OnGrabbedUpdate";
        public const string OnReleaseFromHand = "OnReleasedFromHand";
        public const string OnParentHoverEnd = "OnParentHoverEnd";
        public const string OnParentHoverBegin = "OnParentHoverBegin";
        public const string OnHandFocusAcquired = "OnHandFocusAcquired";
        public const string OnHandFocusLost = "OnHandFocusLost";
    }
    public interface IHandHoverBegin
    {
        void OnHandHoverBegin(Hand hand);
    }

    public interface IHandHoverUpdate
    {
        void OnHandHoverUpdate(Hand hand);
    }

    public interface IHandHoverEnd
    {
        void OnHandHoverEnd(Hand hand);
    }

    public interface IParentHoverBeing
    {
        void OnParentHandHoverBegin();
    }

    public interface IParentHoverEnd
    {
        void OnParentHoverEnd();
    }

    public interface IGrabbedToHand
    {
        void OnGrabbedToHand(Hand hand);
    }

    public interface IGrabbedUpdate
    {
        void OnGrabbedUpdate(Hand hand);
    }

    public interface IReleasedFromHand
    {
        void OnReleasedFromHand(Hand hand);
    }
}

