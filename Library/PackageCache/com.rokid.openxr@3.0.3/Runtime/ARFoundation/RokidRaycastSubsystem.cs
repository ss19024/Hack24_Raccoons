using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features
{
    [Preserve]
    public sealed class RokidRaycastSubsystem : XRRaycastSubsystem
    {
        class RokidRaycastProvider : Provider
        {
            public override void Start()
            {
                Debug.Log("RK-ARFoundation: RaycastProvider Start");
            }

            public override void Stop()
            {
                Debug.Log("RK-ARFoundation: RaycastProvider Stop");
            }

            public override void Destroy()
            {
                Debug.Log("RK-ARFoundation: RaycastProvider Destroy");
            }
        }

        static internal string id { get; private set; }
        static RokidRaycastSubsystem() => id = "Rokid-OpenXR-Raycast-Subsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#if UNITY_6000_0_OR_NEWER
            XRRaycastSubsystemDescriptor.Register(new XRRaycastSubsystemDescriptor.Cinfo
#else
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
#endif
            {
                id = id,
                providerType = typeof(RokidRaycastProvider),
                subsystemTypeOverride = typeof(RokidRaycastSubsystem),
            });
            Debug.Log("RK-ARFoundation: Register XRRaycastSubsystemDescriptor.Create done");
        }
    }
}