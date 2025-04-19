using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Features
{
    [Flags]
    [Preserve]
    public enum RokidAimFlags : ulong
    {
        None = 0,
        Computed = 1 << 0,
        Valid = 1 << 1,
        IndexPinching = 1 << 2,
        MiddlePinching = 1 << 3,
        RingPinching = 1 << 4,
        LittlePinching = 1 << 5,
        SystemGesture = 1 << 6,
        DominantHand = 1 << 7,
        MenuPressed = 1 << 8,
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [Preserve, InputControlLayout(displayName = "Rokid Aim Hand", commonUsages = new[] { "LeftHand", "RightHand" })]
    public partial class RokidAimHand : TrackedDevice
    {
        public static RokidAimHand left { get; set; }

        public static RokidAimHand right { get; set; }

        public const float pressThreshold = 0.5f;

        [Preserve, InputControl(offset = 0)]
        public ButtonControl indexPressed { get; private set; }

        [Preserve, InputControl(offset = 1)]
        public ButtonControl middlePressed { get; private set; }

        [Preserve, InputControl(offset = 2)]
        public ButtonControl ringPressed { get; private set; }

        [Preserve, InputControl(offset = 3)]
        public ButtonControl littlePressed { get; private set; }

        [Preserve, InputControl]
        public IntegerControl aimFlags { get; private set; }

        [Preserve, InputControl]
        public AxisControl pinchStrengthIndex { get; private set; }

        [Preserve, InputControl]
        public AxisControl pinchStrengthMiddle { get; private set; }

        [Preserve, InputControl]
        public AxisControl pinchStrengthRing { get; private set; }

        [Preserve, InputControl]
        public AxisControl pinchStrengthLittle { get; private set; }
        
        private InputDevice LeftHandDevice;
        private InputDevice RightHandDevice;

        protected override void FinishSetup()
        {
            base.FinishSetup();

            indexPressed = GetChildControl<ButtonControl>(nameof(indexPressed));
            middlePressed = GetChildControl<ButtonControl>(nameof(middlePressed));
            ringPressed = GetChildControl<ButtonControl>(nameof(ringPressed));
            littlePressed = GetChildControl<ButtonControl>(nameof(littlePressed));
            aimFlags = GetChildControl<IntegerControl>(nameof(aimFlags));
            pinchStrengthIndex = GetChildControl<AxisControl>(nameof(pinchStrengthIndex));
            pinchStrengthMiddle = GetChildControl<AxisControl>(nameof(pinchStrengthMiddle));
            pinchStrengthRing = GetChildControl<AxisControl>(nameof(pinchStrengthRing));
            pinchStrengthLittle = GetChildControl<AxisControl>(nameof(pinchStrengthLittle));

            var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, InputSystem.CommonUsages.LeftHand);
                else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, InputSystem.CommonUsages.RightHand);
            }
        }

        public static RokidAimHand CreateHand(InputDeviceCharacteristics extraCharacteristics)
        {
            var desc = new InputDeviceDescription
            {
                product = k_RokidAimHandDeviceProductName,
                manufacturer = k_RokidAimHandDeviceManufacturerName,
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | extraCharacteristics,
                    inputFeatures = new List<XRFeatureDescriptor>
                    {
                        new XRFeatureDescriptor
                        {
                            name = "index_pressed",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "middle_pressed",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "ring_pressed",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "little_pressed",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_flags",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_pose_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_pose_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_strength_index",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_strength_middle",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_strength_ring",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_strength_little",
                            featureType = FeatureType.Axis1D
                        }
                    }
                }.ToJson()
            };
            return InputSystem.InputSystem.AddDevice(desc) as RokidAimHand;
        }

        public void UpdateHand(
            bool isHandRootTracked,
            RokidAimFlags aimFlags,
            Pose aimPose,
            float pinchIndex,
            float pinchMiddle,
            float pinchRing,
            float pinchLittle)
        {
            if (aimFlags != m_PreviousFlags)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(this.aimFlags, (int)aimFlags);
                m_PreviousFlags = aimFlags;
            }

            bool isIndexPressed = pinchIndex > pressThreshold;
            if (isIndexPressed != m_WasIndexPressed)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(indexPressed, isIndexPressed);
                m_WasIndexPressed = isIndexPressed;
            }

            bool isMiddlePressed = pinchMiddle > pressThreshold;
            if (isMiddlePressed != m_WasMiddlePressed)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(middlePressed, isMiddlePressed);
                m_WasMiddlePressed = isMiddlePressed;
            }

            bool isRingPressed = pinchRing > pressThreshold;
            if (isRingPressed != m_WasRingPressed)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(ringPressed, isRingPressed);
                m_WasRingPressed = isRingPressed;
            }

            bool isLittlePressed = pinchLittle > pressThreshold;
            if (isLittlePressed != m_WasLittlePressed)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(littlePressed, isLittlePressed);
                m_WasLittlePressed = isLittlePressed;
            }

            InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthIndex, pinchIndex);
            InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthMiddle, pinchMiddle);
            InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthRing, pinchRing);
            InputSystem.InputSystem.QueueDeltaStateEvent(pinchStrengthLittle, pinchLittle);

            if ((aimFlags & RokidAimFlags.Computed) == RokidAimFlags.None)
            {
                if (m_WasTracked)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, false);
                    InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                    m_WasTracked = false;
                }

                return;
            }

            if (isHandRootTracked)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(devicePosition, aimPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(deviceRotation, aimPose.rotation);

                if (!m_WasTracked)
                {
                    InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                    InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, true);
                }

                m_WasTracked = true;
            }
            else if (m_WasTracked)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, false);
                m_WasTracked = false;
            }
        }

        internal void UpdateHand(bool isLeft, bool isHandRootTracked)
        {
            if (isLeft && !LeftHandDevice.isValid)
            {
                LeftHandDevice = RokidUtils.FindRokidDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left);
            }
            if (!isLeft && !RightHandDevice.isValid)
            {
                RightHandDevice = RokidUtils.FindRokidDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right);
            }

            float pinchIndex = 0;
            float pinchMiddle = 0;
            float pinchRing = 0;
            float pinchLittle = 0;
            Vector3 aimPosePosition = new Vector3(0.0f, 0.0f, 0.0f);
            Quaternion aimPoseRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
            RokidAimFlags aimFlags = RokidAimFlags.None;
            if (isHandRootTracked)
            {
                aimFlags = RokidAimFlags.Valid | RokidAimFlags.Computed;
            }

            if (isLeft && isHandRootTracked && LeftHandDevice.isValid)
            {
                LeftHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 AimPosition);
                LeftHandDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion AimRotation);
                aimPosePosition = AimPosition;
                aimPoseRotation = AimRotation;
                LeftHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool IsTrigger);
                LeftHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool IsGrip);
                pinchIndex = (IsTrigger || IsGrip) ? 1 : 0;
            }

            if (!isLeft && isHandRootTracked && RightHandDevice.isValid)
            {
                RightHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 AimPosition);
                RightHandDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion AimRotation);
                aimPosePosition = AimPosition;
                aimPoseRotation = AimRotation;
                RightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool IsTrigger);
                RightHandDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool IsGrip);
                pinchIndex = (IsTrigger || IsGrip) ? 1 : 0;
            }

            // Debug.Log("RokidAim UpdateHand: isHandRootTracked = "+isHandRootTracked+", aimFlags="+aimFlags+", aimPosePosition="+aimPosePosition+", aimPoseRotation="+aimPoseRotation+", pinchIndex="+pinchIndex+", isLeft="+isLeft);

            UpdateHand(
                isHandRootTracked,
                aimFlags,
                new Pose(aimPosePosition, aimPoseRotation),
                pinchIndex,
                pinchMiddle,
                pinchRing,
                pinchLittle);
        }


#if UNITY_EDITOR
        static RokidAimHand() => RegisterLayout();
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterLayout()
        {
            InputSystem.InputSystem.RegisterLayout<RokidAimHand>(
                matches: new InputDeviceMatcher()
                    .WithProduct(k_RokidAimHandDeviceProductName)
                    .WithManufacturer(k_RokidAimHandDeviceManufacturerName));
        }

        const string k_RokidAimHandDeviceProductName = "Rokid Aim Hand Tracking";
        const string k_RokidAimHandDeviceManufacturerName = "OpenXR Rokid";

        RokidAimFlags m_PreviousFlags;
        bool m_WasTracked;
        bool m_WasIndexPressed;
        bool m_WasMiddlePressed;
        bool m_WasRingPressed;
        bool m_WasLittlePressed;
    }
}