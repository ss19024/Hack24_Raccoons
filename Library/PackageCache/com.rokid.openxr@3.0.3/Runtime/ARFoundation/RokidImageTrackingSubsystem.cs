using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features
{
    [Preserve]
    public sealed class RokidImageTrackingSubsystem : XRImageTrackingSubsystem
    {
        class RokidImageTrackingProvider : Provider
        {
            public override void Start()
            {
                Debug.Log("RK-ARFoundation-Marker: ImageTrackingProvider Start");
            }

            public override void Stop()
            {
                Debug.Log("RK-ARFoundation-Marker: ImageTrackingProvider Stop");
            }

            public override RuntimeReferenceImageLibrary imageLibrary
            {
                set
                {
                    Debug.Log("RK-ARFoundation-Marker: ImageTrackingProvider set imageLibrary value = " + value);
                    if (value == null)
                    {
                        // ImageTracking_SetDatabase(IntPtr.Zero);
                    }

                    if (value is RokidImageDatabase database)
                    {
                        // ImageTracking_SetDatabase((IntPtr)database);
                    }
                    else
                    {
                        throw new ArgumentException($"The {value.GetType().Name} is not a valid Rokid image library.");
                    }
                }
            }

            public unsafe override RuntimeReferenceImageLibrary CreateRuntimeLibrary(
                XRReferenceImageLibrary serializedLibrary)
            {
                if (serializedLibrary != null)
                {
                    Debug.Log("RK-ARFoundation-Marker: CreateRuntimeLibrary serializedLibrary.count=" +
                              serializedLibrary.count + ", serializedLibrary.guid=" + serializedLibrary.guid +
                              ", XRTrackedImage=" + sizeof(XRTrackedImage));
                }
                else
                {
                    Debug.Log("RK-ARFoundation-Marker: CreateRuntimeLibrary serializedLibrary is Null");
                }

                return new RokidImageDatabase(serializedLibrary);
            }

            public unsafe override TrackableChanges<XRTrackedImage> GetChanges(
                XRTrackedImage defaultTrackedImage,
                Allocator allocator)
            {
                void* addedPtr, updatedPtr, removedPtr;
                int addedLength, updatedLength, removedLength, stride;

                var context = ImageTracking_AcquireChanges(
                    out addedPtr, out addedLength,
                    out updatedPtr, out updatedLength,
                    out removedPtr, out removedLength,
                    out stride);

                try
                {
                    return new TrackableChanges<XRTrackedImage>(
                        addedPtr, addedLength,
                        updatedPtr, updatedLength,
                        removedPtr, removedLength,
                        defaultTrackedImage, stride,
                        allocator);
                }
                finally
                {
                    ImageTracking_ReleaseChanges(context);
                }
            }

            public override void Destroy()
            {
                Debug.Log("RK-ARFoundation-Marker: ImageTrackingProvider Destroy");
                ImageTracking_Destroy();
            }

            public override int requestedMaxNumberOfMovingImages
            {
                get => m_RequestedMaxNumberOfMovingImages;
                set => m_RequestedMaxNumberOfMovingImages = value;
            }

            int m_RequestedMaxNumberOfMovingImages;

            public override int currentMaxNumberOfMovingImages => Mathf.Max(m_RequestedMaxNumberOfMovingImages, ImageTracking_GetNumberOfTrackedImages());

            [DllImport("rokid_openxr_api")]
            static extern unsafe void* ImageTracking_AcquireChanges(
                out void* addedPtr, out int addedLength,
                out void* updatedPtr, out int updatedLength,
                out void* removedPtr, out int removedLength,
                out int stride);

            [DllImport("rokid_openxr_api")]
            static extern unsafe void ImageTracking_ReleaseChanges(void* changes);

            [DllImport("rokid_openxr_api")]
            static extern int ImageTracking_GetNumberOfTrackedImages();

            [DllImport("rokid_openxr_api")]
            static extern void ImageTracking_Destroy();
        }

        static internal string id { get; private set; }
        static RokidImageTrackingSubsystem() => id = "Rokid-OpenXR-ImageTracking-Subsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#if UNITY_6000_0_OR_NEWER
            XRImageTrackingSubsystemDescriptor.Register(new XRImageTrackingSubsystemDescriptor.Cinfo
#else
            XRImageTrackingSubsystemDescriptor.Create(new XRImageTrackingSubsystemDescriptor.Cinfo
#endif
            {
                id = id,
                providerType = typeof(RokidImageTrackingSubsystem.RokidImageTrackingProvider),
                subsystemTypeOverride = typeof(RokidImageTrackingSubsystem),
                supportsMovingImages = true,
                supportsMutableLibrary = true,
                supportsImageValidation = false,
            });
            Debug.Log("RK-ARFoundation: Register XRImageTrackingSubsystemDescriptor.Create done");
        }
    }
}