using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Rokid Openxr Support",
        Desc = "Rokid Glass OpenXR Support",
        Company = "Rokid",
        DocumentationLink = "https://ar.rokid.com/sdk",
        OpenxrExtensionStrings = "",
        Version = "0.0.1",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        CustomRuntimeLoaderBuildTargets = new[] { BuildTarget.Android },
        FeatureId = featureId)]
#endif

    internal class RokidFeature : OpenXRFeature
    {
        public const string featureId = "com.rokid.openxr.feature";

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            Debug.Log("RK-Openxr: Rokid Openxr Support OnInstanceCreate");
            if (!base.OnInstanceCreate(xrInstance))
                return false;
            return RokidOpenXR_API_OnInstanceCreate(xrInstance);
        }

        protected override void OnSystemChange(ulong xrSystem)
        {
            Debug.Log("RK-Openxr: Rokid Openxr Support OnSystemChange");
            base.OnSystemChange(xrSystem);
            RokidOpenXR_API_OnSystemChange(xrSystem);
        }

        protected override void OnAppSpaceChange(ulong xrSpace)
        {
            Debug.Log("RK-Openxr: Rokid Openxr Support OnAppSpaceChange");
            base.OnAppSpaceChange(xrSpace);
            RokidOpenXR_API_OnAppSpaceChange(xrSpace);
        }

        protected override void OnSessionCreate(ulong xrSession)
        {
            Debug.Log("RK-Openxr: Rokid Openxr Support OnSessionCreate");
            base.OnSessionCreate(xrSession);
            RokidOpenXR_API_OnSessionCreate(xrSession);
        }

        [DllImport("rokid_openxr_api")]
        static extern bool RokidOpenXR_API_OnInstanceCreate(ulong xrInstance);

        [DllImport("rokid_openxr_api")]
        static extern void RokidOpenXR_API_OnSystemChange(ulong xrSystem);

        [DllImport("rokid_openxr_api")]
        static extern void RokidOpenXR_API_OnAppSpaceChange(ulong xrSpace);

        [DllImport("rokid_openxr_api")]
        static extern void RokidOpenXR_API_OnSessionCreate(ulong xrSession);
    }
}