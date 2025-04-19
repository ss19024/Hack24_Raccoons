using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.OpenXR.Features
{
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Rokid Hand Tracking Aim",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Rokid",
        Desc = "Allows for mapping input to the aim pose extension data. Will create an InputDevice for each hand if this and HandTracking are enabled.",
        DocumentationLink = "",
        Version = "0.0.1",
        OpenxrExtensionStrings = "",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Feature,
        FeatureId = featureId)]
#endif
    public class RokidHandTrackingAim : OpenXRFeature
    {
        public const string featureId = "com.unity.openxr.feature.input.RokidHandTrackingAim";

        protected override void OnSubsystemStart()
        {
            Debug.Log("RokidOpenxr: RokidHandTrackingAim OnSubsystemStart()");
            CreateHands();
            var subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
            if (subsystem != null)
                subsystem.updatedHands += OnUpdatedHands;
        }

        protected override void OnSubsystemStop()
        {
            Debug.Log("RokidOpenxr: RokidHandTrackingAim OnSubsystemStop()");
            var subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
            if (subsystem != null)
                subsystem.updatedHands -= OnUpdatedHands;
            DestroyHands();
        }

        void CreateHands()
        {
            Debug.Log("RokidOpenxr: RokidHandTrackingAim CreateHands()");
            if (RokidAimHand.left == null)
                RokidAimHand.left = RokidAimHand.CreateHand(InputDeviceCharacteristics.Left);

            if (RokidAimHand.right == null)
                RokidAimHand.right = RokidAimHand.CreateHand(InputDeviceCharacteristics.Right);
        }

        void DestroyHands()
        {
            Debug.Log("RokidOpenxr: RokidHandTrackingAim DestroyHands()");
            if (RokidAimHand.left != null)
            {
                InputSystem.InputSystem.RemoveDevice(RokidAimHand.left);
                RokidAimHand.left = null;
            }

            if (RokidAimHand.right != null)
            {
                InputSystem.InputSystem.RemoveDevice(RokidAimHand.right);
                RokidAimHand.right = null;
            }
        }

        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateType updateType)
        {
            bool isLeftHandRootTracked = false;
            bool isRightHandRootTracked = false;

            if ((successFlags & (XRHandSubsystem.UpdateSuccessFlags.All)) != 0)
            {
                isLeftHandRootTracked = true;
                isRightHandRootTracked = true;
            }
            else if ((successFlags & (XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints)) != 0)
            {
                isLeftHandRootTracked = true;
            }
            else if ((successFlags & (XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose |  XRHandSubsystem.UpdateSuccessFlags.RightHandJoints)) != 0)
            {
                isRightHandRootTracked = true;
            }

            RokidAimHand.left.UpdateHand(true, isLeftHandRootTracked);
            RokidAimHand.right.UpdateHand(false, isRightHandRootTracked);
        }
    }
}