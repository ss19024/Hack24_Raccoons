
#if ENABLE_INPUT_SYSTEM
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Rokid.UXR.Interaction
{
    public struct StationProDeviceState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('S', 'T', 'P', 'R');

        [InputControl(name = "selectButton", layout = "Button", bit = 0, displayName = "Select Button")]
        [InputControl(name = "xButton", layout = "Button", bit = 1, displayName = "X Button")]
        [InputControl(name = "oButton", layout = "Button", bit = 2, displayName = "O Button")]
        [InputControl(name = "upButton", layout = "Button", bit = 3, displayName = "Up Button")]
        [InputControl(name = "downButton", layout = "Button", bit = 4, displayName = "Down Button")]
        [InputControl(name = "leftButton", layout = "Button", bit = 5, displayName = "Left Button")]
        [InputControl(name = "rightButton", layout = "Button", bit = 6, displayName = "Right Button")]
        [InputControl(name = "menuButton", layout = "Button", bit = 7, displayName = "Menu Button")]
        public ushort buttons;
    }

#if UNITY_EDITOR
    [InitializeOnLoad] // Call static class constructor in editor.
#endif
    [InputControlLayout(stateType = typeof(StationProDeviceState))]
    public class StationProDevice : InputDevice, IInputUpdateCallbackReceiver
    {
        // [InitializeOnLoad] will ensure this gets called on every domain (re)load
        // in the editor.
#if UNITY_EDITOR
        static StationProDevice()
        {
            // Trigger our RegisterLayout code in the editor.
            Initialize();
        }
#endif


        // We can also expose a '.current' getter equivalent to 'Gamepad.current'.
        // Whenever our device receives input, MakeCurrent() is called. So we can
        // simply update a '.current' getter based on that.
        public static StationProDevice current { get; private set; }
        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        // When one of our custom devices is removed, we want to make sure that if
        // it is the '.current' device, we null out '.current'.
        protected override void OnRemoved()
        {
            base.OnRemoved();
            if (current == this)
                current = null;
        }
        public ButtonControl selectButton { get; protected set; }
        public ButtonControl xButton { get; protected set; }
        public ButtonControl oButton { get; protected set; }
        public ButtonControl upButton { get; protected set; }
        public ButtonControl downButton { get; protected set; }
        public ButtonControl leftButton { get; protected set; }
        public ButtonControl rightButton { get; protected set; }
        public ButtonControl menuButton { get; protected set; }


        // In the player, [RuntimeInitializeOnLoadMethod] will make sure our
        // initialization code gets called during startup.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputSystem.RegisterLayout<StationProDevice>(
                matches: new InputDeviceMatcher()
                    .WithInterface("StationPro"));
            var stationProDevice = InputSystem.devices.FirstOrDefault(x => x is StationProDevice);
            if (stationProDevice == null)
            {
                InputSystem.AddDevice(new InputDeviceDescription
                {
                    interfaceName = "StationPro",
                    product = "Rokid Product"
                });
            }
        }

        // FinishSetup is where our device setup is finalized. Here we can look up
        // the controls that have been created.
        protected override void FinishSetup()
        {
            base.FinishSetup();

            selectButton = GetChildControl<ButtonControl>("selectButton");
            xButton = GetChildControl<ButtonControl>("xButton");
            oButton = GetChildControl<ButtonControl>("oButton");
            upButton = GetChildControl<ButtonControl>("upButton");
            downButton = GetChildControl<ButtonControl>("downButton");
            leftButton = GetChildControl<ButtonControl>("leftButton");
            rightButton = GetChildControl<ButtonControl>("rightButton");
            menuButton = GetChildControl<ButtonControl>("menuButton");
        }

        public void OnUpdate()
        {
            var state = new StationProDeviceState();
            if (Input.GetKey(KeyCode.JoystickButton0))
                state.buttons |= 1 << 0;
            if (Input.GetKey(KeyCode.JoystickButton2))
                state.buttons |= 1 << 1;
            if (Input.GetKey(KeyCode.JoystickButton3))
                state.buttons |= 1 << 2;
            if (Input.GetKey(KeyCode.UpArrow))
                state.buttons |= 1 << 3;
            if (Input.GetKey(KeyCode.DownArrow))
                state.buttons |= 1 << 4;
            if (Input.GetKey(KeyCode.LeftArrow))
                state.buttons |= 1 << 5;
            if (Input.GetKey(KeyCode.RightArrow))
                state.buttons |= 1 << 6;
            if (Input.GetKey(KeyCode.Menu))
                state.buttons |= 1 << 7;
            InputSystem.QueueStateEvent(this, state);
        }
    }
}

#endif