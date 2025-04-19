using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor;

namespace UnityEngine.XR.OpenXR.Features
{
    [Preserve]
    public class RokidCameraSubsystem : XRCameraSubsystem
    {
        class RokidCameraProvider : Provider
        {
            public override void Start()
            {
                Debug.Log("RK-ARFoundation: CameraProvider Start");
            }

            public override void Stop()
            {
                Debug.Log("RK-ARFoundation: CameraProvider Stop");
            }

            public override void Destroy()
            {
                Debug.Log("RK-ARFoundation: CameraProvider Destroy");
            }
        }

        static internal string id { get; private set; }
        static RokidCameraSubsystem() => id = "Rokid-OpenXR-Camera-Subsystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#if UNITY_6000_0_OR_NEWER
            var cameraSubsystemCinfo = new Cinfo
#else
            var cameraSubsystemCinfo = new XRCameraSubsystemCinfo
#endif
            {
                id = id,
                providerType = typeof(RokidCameraProvider),
                subsystemTypeOverride = typeof(RokidCameraSubsystem),
                supportsAverageBrightness = false,
                supportsAverageColorTemperature = false,
                supportsColorCorrection = false,
                supportsDisplayMatrix = false,
                supportsProjectionMatrix = false,
                supportsTimestamp = false,
                supportsCameraConfigurations = false,
                supportsCameraImage = false,
                supportsAverageIntensityInLumens = false,
                supportsFocusModes = false,
                supportsFaceTrackingAmbientIntensityLightEstimation = false,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = false,
                supportsWorldTrackingHDRLightEstimation = false,
                supportsCameraGrain = false,
            };

#if UNITY_6000_0_OR_NEWER
            XRCameraSubsystemDescriptor.Register(cameraSubsystemCinfo);
#else
            if (!XRCameraSubsystem.Register(cameraSubsystemCinfo))
            {
                Debug.LogError($"RK-ARFoundation: Failed to register the XRCameraSubsystemCinfo subsystem.");
            }
#endif
            Debug.Log("RK-ARFoundation: Register XRCameraSubsystemCinfo done");
        }
    }
}