namespace Rokid.UXR.Interaction
{
    public interface IHeadHandDriver
    {
        void OnBeforeChangeHoldHandType(HandType hand);
        void OnChangeHoldHandType(HandType hand);
        void OnHandPress(HandType hand);
        void OnHandRelease();
    }
}


