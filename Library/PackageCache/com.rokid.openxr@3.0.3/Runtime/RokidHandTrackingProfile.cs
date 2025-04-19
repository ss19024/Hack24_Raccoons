using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Input;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Rokid HandTracking Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Rokid",
        Desc = "Allows for mapping input to the HandTracking interaction profile.",
        OpenxrExtensionStrings = extensionString,
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class RokidHandTrackingProfile : OpenXRInteractionFeature
    {
        public const string featureId = "com.unity.openxr.feature.input.rokidhandtrackingprofile";

        [Preserve, InputControlLayout(displayName = "Rokid Hand Interaction (OpenXR)", isGenericTypeOfDevice = true, commonUsages = new[] { "LeftHand", "RightHand" })]
        public class RokiHandTracking : OpenXRDevice
        {
            [Preserve, InputControl(offset = 0, aliases = new[] { "TriggerButton" }, usage = "TriggerButton")]
            public ButtonControl Trigger { get; private set; }

            [Preserve, InputControl(offset = 1, aliases = new[] { "GripButton" }, usage = "GripButton")]
            public ButtonControl Grip { get; private set; }

            protected override void FinishSetup()
            {
                base.FinishSetup();
                Trigger = GetChildControl<ButtonControl>("TriggerButton");
                Grip = GetChildControl<ButtonControl>("GripButton");
            }
        }

        public const string profile = "/interaction_profiles/rokid/hand_interaction";

        public const string aim_pose = "/input/aim/pose";
        public const string pinch_button = "/input/pinch/click";
        public const string grip_button = "/input/squeeze/click";

        private const string kDeviceLocalizedName = "Rokid OpenXR HandTracking";

        public const string extensionString = "XR_EXT_hand_interaction";


        protected override bool OnInstanceCreate(ulong instance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled(extensionString))
                return false;

            return base.OnInstanceCreate(instance);
        }


        protected override void RegisterDeviceLayout()
        {
            InputSystem.InputSystem.RegisterLayout(typeof(RokiHandTracking),
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(kDeviceLocalizedName));
        }

        protected override void UnregisterDeviceLayout()
        {
            InputSystem.InputSystem.RemoveLayout(nameof(RokiHandTracking));
        }

        protected override InteractionProfileType GetInteractionProfileType()
        {
            return typeof(RokiHandTracking).IsSubclassOf(typeof(XRController))
                ? InteractionProfileType.XRController
                : InteractionProfileType.Device;
        }

        protected override string GetDeviceLayoutName()
        {
            return nameof(RokiHandTracking);
        }

        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "RokiHandTracking",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "Rokid",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking |
                                                                       InputDeviceCharacteristics.Left |
                                                                       InputDeviceCharacteristics.TrackedDevice),
                        userPath = UserPaths.leftHand
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking |
                                                                       InputDeviceCharacteristics.Right |
                                                                       InputDeviceCharacteristics.TrackedDevice),
                        userPath = UserPaths.rightHand
                    }
                },
                actions = new List<ActionConfig>()
                {
                    new ActionConfig()
                    {
                        name = "TriggerButton",
                        localizedName = "Trigger Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "TriggerButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = pinch_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "GripButton",
                        localizedName = "Grip Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "GripButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = grip_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = aim_pose,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                }
            };
            AddActionMap(actionMap);
        }
    }
}