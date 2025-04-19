using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features
{
    internal class RokidCorePromise<T> : Promise<T>
    {
        protected override void OnKeepWaiting()
        {
            if (s_LastFrameUpdated == Time.frameCount)
                return;
            s_LastFrameUpdated = Time.frameCount;
        }

        internal new void Resolve(T result)
        {
            base.Resolve(result);
        }

        static int s_LastFrameUpdated;
    }

    [Preserve]
    public sealed class RokidSessionSubsystem : XRSessionSubsystem
    {
        class RokidCoreSessionProvider : XRSessionSubsystem.Provider
        {
            public override void Start()
            {
                Debug.Log("RK-ARFoundation: SessionProvider Start");
            }

            public override void Stop()
            {
                Debug.Log("RK-ARFoundation: SessionProvider Stop");
            }

            public override void Destroy()
            {
                Debug.Log("RK-ARFoundation: SessionProvider Destroy");
            }

            static Promise<T> ExecuteAsync<T>(Action<IntPtr> apiMethod)
            {
                var promise = new RokidCorePromise<T>();
                GCHandle gch = GCHandle.Alloc(promise);
                apiMethod(GCHandle.ToIntPtr(gch));
                return promise;
            }


            static void ResolvePromise<T>(IntPtr context, T arg) where T : struct
            {
                GCHandle gch = GCHandle.FromIntPtr(context);
                var promise = (RokidCorePromise<T>)gch.Target;
                if (promise != null)
                    promise.Resolve(arg);
                gch.Free();
            }

            public override Promise<SessionAvailability> GetAvailabilityAsync()
            {
                return ExecuteAsync<SessionAvailability>((context) =>
                {
                    var sessionAvailability = SessionAvailability.Supported | SessionAvailability.Installed;
                    ResolvePromise(context, sessionAvailability);
                });
            }

            public override Promise<SessionInstallationStatus> InstallAsync()
            {
                return ExecuteAsync<SessionInstallationStatus>((context) =>
                {
                    var sessionInstallation = SessionInstallationStatus.Success;
                    ResolvePromise(context, sessionInstallation);
                });
            }

            public override TrackingState trackingState => TrackingState.Tracking;

            public override Feature currentTrackingMode => Feature.PositionAndRotation | Feature.PlaneTracking | Feature.ImageTracking;
        }

        static internal string id { get; private set; }
        static RokidSessionSubsystem() => id = "Rokid-OpenXR-CoreSession-Subsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#if UNITY_6000_0_OR_NEWER
            XRSessionSubsystemDescriptor.Register(new XRSessionSubsystemDescriptor.Cinfo
#else
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
#endif
            {
                id = id,
                providerType = typeof(RokidCoreSessionProvider),
                subsystemTypeOverride = typeof(RokidSessionSubsystem),
            });
            Debug.Log("RK-ARFoundation: Register XRSessionSubsystemDescriptor.Create done");
        }
    }
}