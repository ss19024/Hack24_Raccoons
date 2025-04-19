using UnityEngine;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Event Interface
    /// </summary>
    public interface IEventInput
    {
        /// <summary>
        /// Init event module
        /// </summary>
        /// <param name="parent">Initialize the parent of the interactor</param>
        public void Initialize(Transform parent);

        /// <summary>
        /// Release event module
        /// </summary>
        public void Release();

        /// <summary>
        /// Activate the module (only if the module has already been initialized to activate successfully)
        /// </summary>
        public void ActiveModule();

        /// <summary>
        /// Generated interactor
        /// </summary>
        /// <value></value>
        public Transform Interactor { get; set; }

        /// <summary>in
        /// Get current input module event system pixel drag threshold 
        /// </summary>
        /// <value></value>
        public int PixelDragThreshold { get; set; }

        /// <summary>
        /// Get Ray caster 
        /// </summary>
        /// <value></value>
        public BaseRayCaster GetRayCaster(HandType hand = HandType.None);

        /// <summary>
        /// Get ray selector
        /// </summary>
        /// <value></value>
        public ISelector GetRaySelector(HandType hand = HandType.None);

        /// <summary>
        /// Get ray pose
        /// </summary>
        /// <value></value>
        public IRayPose GetRayPose(HandType hand = HandType.None);

        /// <summary>
        /// The interactor is sleep
        /// </summary>
        /// <value></value>
        public void Sleep(bool sleep);

        /// <summary>
        /// The interactor is lock can not to auto active
        /// </summary>
        /// <value></value>
        public void Lock(bool isLock);

        /// <summary>
        /// The input module type 
        /// </summary>
        /// <value></value>
        public InputModuleType inputModuleType { get; }

    }
}

