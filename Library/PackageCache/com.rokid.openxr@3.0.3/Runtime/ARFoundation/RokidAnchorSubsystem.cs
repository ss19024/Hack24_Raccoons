using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features
{
    [Preserve]
    public sealed class RokidAnchorSubsystem : XRAnchorSubsystem
    {
        class RokidAnchorProvider : Provider
        {
            public override void Start()
            {
                Debug.Log("RK-ARFoundation: AnchorProvider Start");
                AnchorTracking_Start();
            }

            public override void Stop()
            {
                Debug.Log("RK-ARFoundation: AnchorProvider Stop");
                AnchorTracking_Stop();
            }

            public override void Destroy()
            {
                Debug.Log("RK-ARFoundation: AnchorProvider Destroy");
                AnchorTracking_Destroy();
            }

            public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                int addedCount, updatedCount, removedCount, elementSize;
                void* addedPtr, updatedPtr, removedPtr;
                var context = AnchorTracking_AcquireChanges(
                    out addedPtr, out addedCount,
                    out updatedPtr, out updatedCount,
                    out removedPtr, out removedCount,
                    out elementSize);

                try
                {
                    return new TrackableChanges<XRAnchor>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultAnchor, elementSize,
                        allocator);
                }
                finally
                {
                    AnchorTracking_ReleaseChanges(context);
                }
            }

            public override bool TryAddAnchor(
                Pose pose,
                out XRAnchor anchor)
            {
                bool result = AnchorTracking_TryAdd(pose, out anchor);
                //Debug.Log("RK-ARFoundation: AnchorProvider call TryAddAnchor result="+result+", pose="+pose+", anchor.pose"+anchor.pose+", trackableId="+anchor.trackableId+", trackingState="+anchor.trackingState+", sessionId="+anchor.sessionId);
                return result;
            }

            public override bool TryAttachAnchor(
                TrackableId attachedToId,
                Pose pose,
                out XRAnchor anchor)
            {
                return AnchorTracking_TryAttach(attachedToId, pose, out anchor);
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return AnchorTracking_TryRemove(anchorId);
            }


            [DllImport("rokid_openxr_api")]
            static extern void AnchorTracking_Start();

            [DllImport("rokid_openxr_api")]
            static extern void AnchorTracking_Stop();

            [DllImport("rokid_openxr_api")]
            static extern void AnchorTracking_Destroy();

            [DllImport("rokid_openxr_api")]
            static extern unsafe void* AnchorTracking_AcquireChanges(
                out void* addedPtr, out int addedLength,
                out void* updatedPtr, out int updatedLength,
                out void* removedPtr, out int removedLength,
                out int stride);

            [DllImport("rokid_openxr_api")]
            static extern unsafe void AnchorTracking_ReleaseChanges(void* changes);

            [DllImport("rokid_openxr_api")]
            static extern bool AnchorTracking_TryAdd(Pose pose, out XRAnchor anchor);

            [DllImport("rokid_openxr_api")]
            static extern bool AnchorTracking_TryAttach(TrackableId trackableToAffix, Pose pose, out XRAnchor anchor);

            [DllImport("rokid_openxr_api")]
            static extern bool AnchorTracking_TryRemove(TrackableId anchorId);
        }

        static internal string id { get; private set; }
        static RokidAnchorSubsystem() => id = "Rokid-OpenXR-Anchor-Subsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#if UNITY_6000_0_OR_NEWER
            XRAnchorSubsystemDescriptor.Register(new XRAnchorSubsystemDescriptor.Cinfo
#else
            XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
#endif
            {
                id = id,
                providerType = typeof(RokidAnchorProvider),
                subsystemTypeOverride = typeof(RokidAnchorSubsystem),
                supportsTrackableAttachments = true
            });
            Debug.Log("RK-ARFoundation: Register XRAnchorSubsystemDescriptor.Create done");
        }
    }
}