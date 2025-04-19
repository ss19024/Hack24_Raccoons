using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.OpenXR.Features
{
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Rokid ARFoundation Support",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Rokid",
        Desc = "Support the ARFoundation Feature",
        DocumentationLink = "",
        Version = "0.0.1",
        OpenxrExtensionStrings = "",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId)]
#endif
    public class RokidARFoundationFeature : OpenXRFeature
    {
        public const string featureId = "com.unity.openxr.feature.RokidARFoundation";

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            if (!base.OnInstanceCreate(xrInstance))
                return false;
            return RokidOpenXR_ARFoundation_OnInstanceCreate(xrInstance);
        }

        protected override void OnSystemChange(ulong xrSystem)
        {
            base.OnSystemChange(xrSystem);
            RokidOpenXR_ARFoundation_OnSystemChange(xrSystem);
        }

        protected override void OnAppSpaceChange(ulong xrSpace)
        {
            base.OnAppSpaceChange(xrSpace);
            RokidOpenXR_ARFoundation_OnAppSpaceChange(xrSpace);
        }

        protected override void OnSessionCreate(ulong xrSession)
        {
            base.OnSessionCreate(xrSession);
            Debug.Log("RK-ARFoundation-Feature: OnSessionCreate");

            // 1. create the XRSessionSubsystem
            var sessionDescriptors = new List<XRSessionSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(sessionDescriptors);
            if (sessionDescriptors.Count < 1)
            {
                Debug.Log("RK-ARFoundation-Feature: Feature Can't find the XRSessionSubsystemDescriptor");
                return;
            }

            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(sessionDescriptors, RokidSessionSubsystem.id);
            

            // 2. create the XRPlaneSubsystem
            var planeDescriptors = new List<XRPlaneSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(planeDescriptors);
            if (planeDescriptors.Count < 1)
            {
                Debug.Log("RK-ARFoundation-Feature: Can't find the XRPlaneSubsystemDescriptor");
                return;
            }

            CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(planeDescriptors, RokidPlaneSubsystem.id);

            // 3. create the XRAnchorSubsystem
            var anchorDescriptors = new List<XRAnchorSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(anchorDescriptors);
            if (anchorDescriptors.Count < 1)
            {
                Debug.Log("RK-ARFoundation-Feature: Can't find the XRAnchorSubsystemDescriptor");
                return;
            }

            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(anchorDescriptors, RokidAnchorSubsystem.id);

            // 4. create the XRRaycastSubsystem
            var raycastDescriptors = new List<XRRaycastSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(raycastDescriptors);
            if (raycastDescriptors.Count < 1)
            {
                Debug.Log("RK-ARFoundation-Feature: Can't find the XRRaycastSubsystemDescriptor");
                return;
            }

            CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(raycastDescriptors, RokidRaycastSubsystem.id);
            

            // 5. create the XRCameraSubsystem
            var cameraDescriptors = new List<XRCameraSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(cameraDescriptors);
            if (cameraDescriptors.Count < 1)
            {
                Debug.Log("RK-ARFoundation-Feature: Can't find the XRCameraSubsystemDescriptor");
                return;
            }

            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(cameraDescriptors, RokidCameraSubsystem.id);

            // 6. create the XRImageTrackingSubsystem
            var imageTrackingDescriptors = new List<XRImageTrackingSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(imageTrackingDescriptors);
            if (imageTrackingDescriptors.Count < 1)
            {
                Debug.Log("RK-ARFoundation-Feature: Can't find the XRImageTrackingSubsystemDescriptor");
                return;
            }

            CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(imageTrackingDescriptors, RokidImageTrackingSubsystem.id);

            RokidOpenXR_ARFoundation_OnSessionCreate(xrSession);
        }

        protected override void OnSubsystemStart()
        {
            Debug.Log("RK-ARFoundation-Feature: OnSubsystemStart");
        }

        protected override void OnSubsystemStop()
        {
            Debug.Log("RK-ARFoundation-Feature: OnSubsystemStop");
        }

        protected override void OnSubsystemDestroy()
        {
            Debug.Log("RK-ARFoundation-Feature: OnSubsystemDestroy");
        }

        [DllImport("rokid_openxr_api")]
        static extern bool RokidOpenXR_ARFoundation_OnInstanceCreate(ulong xrInstance);

        [DllImport("rokid_openxr_api")]
        static extern void RokidOpenXR_ARFoundation_OnSystemChange(ulong xrSystem);

        [DllImport("rokid_openxr_api")]
        static extern void RokidOpenXR_ARFoundation_OnAppSpaceChange(ulong xrSpace);

        [DllImport("rokid_openxr_api")]
        static extern void RokidOpenXR_ARFoundation_OnSessionCreate(ulong xrSession);

#if UNITY_EDITOR
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup targetGroup)
        {
#if UNITY_OPENXR_PACKAGE_1_2 && !UNITY_OPENXR_PACKAGE_1_6
            Debug.Log("RK-ARFoundation:  GetValidationChecks");
            results.Add(new ValidationRule(this)
            {
                message = "ARFoundation does not work on a Rokid device at runtime without a version of the OpenXR package at version 1.6.0 or newer.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    if (null == settings)
                        return false;

                    var questFeature = settings.GetFeature<OculusQuestFeature>();
                    return questFeature == null || !questFeature.enabled;
                },
                error = true
            });
#endif // UNITY_OPENXR_PACKAGE_1_6
        }
#endif // UNITY_EDITOR
    }
}