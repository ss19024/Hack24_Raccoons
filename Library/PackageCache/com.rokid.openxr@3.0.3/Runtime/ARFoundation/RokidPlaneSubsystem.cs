using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using Unity.Jobs;

namespace UnityEngine.XR.OpenXR.Features
{
    [Preserve]
    public sealed class RokidPlaneSubsystem : XRPlaneSubsystem
    {
        class RokidPlaneProvider : Provider
        {
            private bool isStartTracking = false;

            public override void Start()
            {
                Debug.Log("RK-ARFoundation: PlaneProvider Start Tracking");
            }

            public override void Stop()
            {
                Debug.Log("RK-ARFoundation: PlaneProvider Stop Tracking, isStartTracking=" + isStartTracking);
                if (isStartTracking)
                {
                    PlaneTracking_StopTracking();
                    isStartTracking = false;
                }
            }

            public override void Destroy()
            {
                Debug.Log("RK-ARFoundation: PlaneProvider Destroy");
                PlaneTracking_Destroy();
            }

            public override void GetBoundary(
                TrackableId trackableId,
                Allocator allocator,
                ref NativeArray<Vector2> boundary)
            {
                unsafe
                {
                    int numPoints;
                    var plane = PlaneTracking_AcquireBoundary(trackableId, out numPoints);

                    CreateOrResizeNativeArrayIfNecessary(numPoints, allocator, ref boundary);

                    if (PlaneTracking_TryCopyBoundary(plane, boundary.GetUnsafePtr()))
                    {
                        Debug.Log("RK-ARFoundation: PlaneProvider GetBoundary boundary.Length=" + boundary.Length);
                        // Flip handedness and winding order
                        var flipHandednessHandle = new FlipBoundaryHandednessJob
                        {
                            vertices = boundary
                        }.Schedule(numPoints, 1);

                        new FlipBoundaryWindingJob
                        {
                            vertices = boundary
                        }.Schedule(flipHandednessHandle).Complete();
                    }
                    else
                    {
                        boundary.Dispose();
                    }
                }
            }

            struct FlipBoundaryWindingJob : IJob
            {
                public NativeArray<Vector2> vertices;

                public void Execute()
                {
                    var half = vertices.Length / 2;
                    for (int i = 0; i < half; ++i)
                    {
                        var j = vertices.Length - 1 - i;
                        var temp = vertices[j];
                        vertices[j] = vertices[i];
                        vertices[i] = temp;
                    }
                }
            }

            struct FlipBoundaryHandednessJob : IJobParallelFor
            {
                public NativeArray<Vector2> vertices;

                public void Execute(int index)
                {
                    vertices[index] = new Vector2(
                        vertices[index].x,
                        -vertices[index].y);
                }
            }

            public override TrackableChanges<BoundedPlane> GetChanges(
                BoundedPlane defaultPlane,
                Allocator allocator)
            {
                unsafe
                {
                    int addedLength, updatedLength, removedLength, elementSize;
                    void* addedPtr, updatedPtr, removedPtr;
                    var context = PlaneTracking_AcquireChanges(
                        out addedPtr, out addedLength,
                        out updatedPtr, out updatedLength,
                        out removedPtr, out removedLength,
                        out elementSize);

                    try
                    {
                        return new TrackableChanges<BoundedPlane>(
                            addedPtr, addedLength,
                            updatedPtr, updatedLength,
                            removedPtr, removedLength,
                            defaultPlane, elementSize,
                            allocator);
                    }
                    finally
                    {
                        PlaneTracking_ReleaseChanges(context);
                    }
                }
            }

            public override PlaneDetectionMode requestedPlaneDetectionMode
            {
                get
                {
                    int mode = PlaneTracking_GetRequestedPlaneDetectionMode();
                    Debug.Log("RK-ARFoundation: PlaneProvider getRequestedPlaneDetectionMode mode=" + mode);
                    if (mode == 3)
                    {
                        return (PlaneDetectionMode)(-1);
                    }

                    return (PlaneDetectionMode)mode;
                }
                set
                {
                    int mode;
                    if (value < 0)
                    {
                        mode = 3;
                    }
                    else
                    {
                        mode = (int)value;
                    }

                    Debug.Log("RK-ARFoundation: PlaneProvider setRequestedPlaneDetectionMode value=" + value +
                              ", mode=" + mode + ", isStartTracking=" + isStartTracking);
                    if (!isStartTracking)
                    {
                        PlaneTracking_StartTracking(mode);
                        isStartTracking = true;
                    }

                    PlaneTracking_SetRequestedPlaneDetectionMode(mode);
                }
            }

            public override PlaneDetectionMode currentPlaneDetectionMode
            {
                get
                {
                    int mode = PlaneTracking_GetCurrentPlaneDetectionMode();
                    Debug.Log("RK-ARFoundation: PlaneProvider currentPlaneDetectionMode mode=" + mode);
                    if (mode == 3)
                    {
                        return (PlaneDetectionMode)(-1);
                    }

                    return (PlaneDetectionMode)mode;
                }
            }


            [DllImport("rokid_openxr_api")]
            static extern void PlaneTracking_StartTracking(int mode);

            [DllImport("rokid_openxr_api")]
            static extern void PlaneTracking_StopTracking();

            [DllImport("rokid_openxr_api")]
            static extern unsafe void* PlaneTracking_AcquireChanges(
                out void* addedPtr, out int addedLength,
                out void* updatedPtr, out int updatedLength,
                out void* removedPtr, out int removedLength,
                out int elementSize);

            [DllImport("rokid_openxr_api")]
            static extern unsafe void* PlaneTracking_AcquireBoundary(TrackableId trackableId, out int numPoints);

            [DllImport("rokid_openxr_api")]
            static extern unsafe bool PlaneTracking_TryCopyBoundary(void* plane, void* boundaryOut);

            [DllImport("rokid_openxr_api")]
            static extern unsafe void PlaneTracking_ReleaseChanges(void* changes);

            [DllImport("rokid_openxr_api")]
            static extern int PlaneTracking_GetRequestedPlaneDetectionMode();

            [DllImport("rokid_openxr_api")]
            static extern void PlaneTracking_SetRequestedPlaneDetectionMode(int mode);

            [DllImport("rokid_openxr_api")]
            static extern int PlaneTracking_GetCurrentPlaneDetectionMode();

            [DllImport("rokid_openxr_api")]
            static extern void PlaneTracking_Destroy();
        }

        static internal string id { get; private set; }
        static RokidPlaneSubsystem() => id = "Rokid-OpenXR-PlaneTracking-Subsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#if UNITY_6000_0_OR_NEWER
            XRPlaneSubsystemDescriptor.Register(new XRPlaneSubsystemDescriptor.Cinfo
#else
            XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
#endif
            {
                id = id,
                providerType = typeof(RokidPlaneProvider),
                subsystemTypeOverride = typeof(RokidPlaneSubsystem),
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
                supportsArbitraryPlaneDetection = false,
                supportsBoundaryVertices = true
            });
            Debug.Log("RK-ARFoundation: Register XRPlaneSubsystemDescriptor.Create done");
        }
    }
}