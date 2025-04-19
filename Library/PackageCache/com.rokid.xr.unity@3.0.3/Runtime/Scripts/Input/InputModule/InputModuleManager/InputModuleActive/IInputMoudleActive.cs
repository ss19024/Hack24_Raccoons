using UnityEngine;
namespace Rokid.UXR.Interaction
{
    public interface IInputModuleActive
    {
        public MonoBehaviour Behaviour { get; }
        public ActiveModuleType ActiveModuleType { get; }
        public ActiveHandType ActiveHandType { get; }
        public ActiveHandInteractorType ActiveHandInteractorType { get; }
        public ActiveHandOrientationType ActiveHandOrientationType { get; }
        public ActiveWatchType ActiveWatchType { get; }
        public ActiveHeadHandType ActiveHeadHandType { get; }
        public ActiveHandRayType ActiveHandRayType { get; }
        public bool DisableOnHandLost { get; }
        public GameObject Go { get; }
    }
}

