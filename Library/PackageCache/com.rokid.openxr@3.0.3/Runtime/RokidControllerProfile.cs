using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Rokid Controller Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = "Rokid",
        Desc = "Allows for mapping input to the Generic Controller interaction profile.",
        OpenxrExtensionStrings = "",
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class RokidControllerProfile : OpenXRInteractionFeature
    {
        public const string featureId = "com.unity.openxr.feature.input.rokidcontrollerprofile";

        [Preserve, InputControlLayout(displayName = "Rokid Controller (OpenXR)", isGenericTypeOfDevice = true)]
        public class RokidController : OpenXRDevice
        {
            [Preserve, InputControl(offset = 0, aliases = new[] { "Select", "TriggerButton" }, usage = "TriggerButton")]
            public ButtonControl Select { get; private set; }

            [Preserve, InputControl(offset = 1, aliases = new[] { "X", "PrimaryButton" }, usage = "PrimaryButton")]
            public ButtonControl X { get; private set; }

            [Preserve, InputControl(offset = 2, aliases = new[] { "O", "GripButton" }, usage = "GripButton")]
            public ButtonControl O { get; private set; }

            [Preserve, InputControl(offset = 3, aliases = new[] { "Up" })]
            public ButtonControl Up { get; private set; }

            [Preserve, InputControl(offset = 4, aliases = new[] { "Down" })]
            public ButtonControl Down { get; private set; }

            [Preserve, InputControl(offset = 5, aliases = new[] { "Left" })]
            public ButtonControl Left { get; private set; }

            [Preserve, InputControl(offset = 6, aliases = new[] { "Right" })]
            public ButtonControl Right { get; private set; }

            [Preserve, InputControl(offset = 7, aliases = new[] { "Menu", "MenuButton" }, usage = "MenuButton")]
            public ButtonControl Menu { get; private set; }

            [Preserve, InputControl(offset = 8, aliases = new[] { "Primary2DAxisTouch" }, usage = "Primary2DAxisTouch")]
            public ButtonControl Primary2DAxisTouch { get; private set; }

            [Preserve, InputControl(aliases = new[] { "Primary2DAxis", "Joystick" }, usage = "Primary2DAxis")]
            public Vector2Control Primary2DAxis { get; private set; }

            protected override void FinishSetup()
            {
                base.FinishSetup();
                Select = GetChildControl<ButtonControl>("TriggerButton");
                X = GetChildControl<ButtonControl>("PrimaryButton");
                O = GetChildControl<ButtonControl>("GripButton");
                Up = GetChildControl<ButtonControl>("UpButton");
                Down = GetChildControl<ButtonControl>("DownButton");
                Left = GetChildControl<ButtonControl>("LeftButton");
                Right = GetChildControl<ButtonControl>("RightButton");
                Menu = GetChildControl<ButtonControl>("MenuButton");
                Primary2DAxisTouch = GetChildControl<ButtonControl>("Primary2DAxisTouch");
                Primary2DAxis = GetChildControl<Vector2Control>("Primary2DAxis");
            }
        }

        public const string profile = "/interaction_profiles/rokid/gamepad_controller";

        public const string select_button = "/input/select/click";
        public const string x_button = "/input/x/click";
        public const string o_button = "/input/o/click";
        public const string up_button = "/input/up/click";
        public const string down_button = "/input/down/click";
        public const string left_button = "/input/left/click";
        public const string right_button = "/input/right/click";
        public const string menu_button = "/input/menu/click";
        public const string aim_pose = "/input/aim/pose";
        public const string thumbstick_value = "/input/thumbstick/value";
        public const string thumbstick_touch = "/input/thumbstick/touch";

        private const string kDeviceLocalizedName = "Rokid OpenXR Controller";

        protected override void RegisterDeviceLayout()
        {
            InputSystem.InputSystem.RegisterLayout(typeof(RokidController),
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(kDeviceLocalizedName));
        }

        protected override void UnregisterDeviceLayout()
        {
            InputSystem.InputSystem.RemoveLayout(nameof(RokidController));
        }

        protected override string GetDeviceLayoutName()
        {
            return nameof(RokidController);
        }

        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "RokidController",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "Rokid",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HeldInHand |
                                                                       InputDeviceCharacteristics.Controller |
                                                                       InputDeviceCharacteristics.TrackedDevice),
                        userPath = UserPaths.gamepad
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
                                interactionPath = select_button,
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
                                interactionPath = o_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "PrimaryButton",
                        localizedName = "Primary Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "PrimaryButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = x_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "UpButton",
                        localizedName = "Controller Up Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "UpButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = up_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "DownButton",
                        localizedName = "Controller Down Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "DownButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = down_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "LeftButton",
                        localizedName = "Controller Left Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "LeftButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = left_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "RightButton",
                        localizedName = "Controller Right Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "RightButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = right_button,
                                interactionProfileName = profile
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig()
                    {
                        name = "MenuButton",
                        localizedName = "Menu Button",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "MenuButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = menu_button,
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
                    new ActionConfig
                    {
                        name = "Primary2DAxis",
                        localizedName = "Primary2DAxis",
                        type = ActionType.Axis2D,
                        usages = new List<string>()
                        {
                            "Primary2DAxis", "Joystick"
                        },
                        bindings = new List<ActionBinding>
                        {
                            new ActionBinding
                            {
                                interactionPath = thumbstick_value,
                                interactionProfileName = profile,
                            }
                        },
                        isAdditive = true
                    },
                    new ActionConfig
                    {
                        name = "Primary2DAxisTouch",
                        localizedName = "Primary2DAxisTouch",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "Primary2DAxisTouch"
                        },
                        bindings = new List<ActionBinding>
                        {
                            new ActionBinding
                            {
                                interactionPath = thumbstick_touch,
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