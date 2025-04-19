using System.Collections.Generic;

namespace UnityEngine.XR.OpenXR.Features
{
    public static class RokidUtils
    {
        public static InputDevice FindRokidDevice(InputDeviceCharacteristics inputDeviceCharacteristics)
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(inputDeviceCharacteristics, devices);
            foreach (InputDevice device in devices)
            {
                if (device.name.Contains("Rokid") && device.isValid)
                {
                    return device;
                }
            }

            return new InputDevice();
        }
    }
}