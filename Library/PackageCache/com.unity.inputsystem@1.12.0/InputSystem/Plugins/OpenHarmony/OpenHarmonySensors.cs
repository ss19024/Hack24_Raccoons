#if UNITY_EDITOR || UNITY_OPENHARMONY || PACKAGE_DOCS_GENERATION
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.OpenHarmony.LowLevel;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Processors;
using UnityEngine.InputSystem.Controls;

namespace UnityEngine.InputSystem.OpenHarmony.LowLevel
{
    internal enum OpenHarmonySensorType
    {
        None = 0,
        Accelerometer = 1,
        Gyroscope = 2,
        AmbientLight = 5,
        MagneticField = 6,
        Barometer = 8,
        Hall = 10,
        Proximity = 12,
        Humidity = 13,
        Orientation = 256,
        Gravity = 257,
        LinearAccelerometer = 258,
        RotationVector = 259,
        GameRotationVector = 262,
        AmbientTemperature = 260, //Not release yet
        MagneticFieldUncalibrated = 261,
        GyroscopeUncalibrated = 263,
        SignificantMotion = 264,
        PedometerDetection = 265,
        Pedometer = 266,
        HeartRate = 278,
        WearDetection = 280,
        AccelerometerUncalibrated = 281
    }

    [Serializable]
    internal struct OpenHarmonySensorCapabilities
    {
        public OpenHarmonySensorType sensorType;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static OpenHarmonySensorCapabilities FromJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            return JsonUtility.FromJson<OpenHarmonySensorCapabilities>(json);
        }

        public override string ToString()
        {
            return $"type = {sensorType.ToString()}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct OpenHarmonySensorState : IInputStateTypeInfo
    {
        public static FourCC kFormat = new FourCC('O', 'H', 'S', 'S'); //OpenHarmonySensorState

        ////FIXME: Sensors to check if values matches old system
        // Accelerometer - Input.acceleration: OK
        // MagneticField - no alternative in old system
        // Gyroscope - Input.gyro.rotationRate: OK
        // Light - no alternative in old system
        // Pressure - no alternative in old system
        // Proximity - no alternative in old system
        // Gravity - Input.gyro.gravity: OK
        // LinearAcceleration - Input.gyro.userAcceleration: OK
        // RotationVector - Input.gyro.attitude: OK
        // RelativeHumidity - no alternative in old system
        // AmbientTemperature - no alternative in old system //Not release yet
        // GameRotationVector - no alternative in old system //Will release in api 13
        // StepCounter - no alternative in old system
        // HeartRate - no alternative in old system


        [InputControl(name = "acceleration", layout = "Vector3", processors = "OpenHarmonyCompensateDirection", variants = "Accelerometer")]
        [InputControl(name = "angularVelocity", layout = "Vector3", processors = "CompensateDirection", variants = "Gyroscope")]
        [InputControl(name = "lightLevel", layout = "Axis", variants = "AmbientLight")]
        [InputControl(name = "magneticField", layout = "Vector3", variants = "MagneticField")]
        [InputControl(name = "atmosphericPressure", layout = "Axis", variants = "Barometer")]
        [InputControl(name = "distance", layout = "Axis", variants = "Proximity")]
        //[InputControl(name = "humidity", layout = "Axis", variants = "Humidity")] //Not release yet
        [InputControl(name = "gravity", layout = "Vector3", processors = "OpenHarmonyCompensateDirection", variants = "Gravity")]
        [InputControl(name = "acceleration", layout = "Vector3", processors = "OpenHarmonyCompensateDirection", variants = "LinearAcceleration")]
        [InputControl(name = "attitude", layout = "Quaternion", processors = "OpenHarmonyCompensateRotation", variants = "RotationVector")]
        //[InputControl(name = "attitude", layout = "Quaternion", processors = "OpenHarmonyCompensateRotation", variants = "GameRotationVector")] //Override RotationVector. Uncomment if you need.
        [InputControl(name = "stepCounter", layout = "Axis", variants = "Pedometer")]
        public fixed float data[4];

        public OpenHarmonySensorState WithData(params float[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            for (var i = 0; i < data.Length && i < 4; i++)
                this.data[i] = data[i];

            // Fill the rest with zeroes
            for (var i = data.Length; i < 4; i++)
                this.data[i] = 0.0f;

            return this;
        }

        public FourCC format => kFormat;
    }

    [DesignTimeVisible(false)]
    internal class OpenHarmonyCompensateDirectionProcessor : CompensateDirectionProcessor
    {
        private const float kSensorStandardGravity = 9.80665f;

        private const float kAccelerationMultiplier = -1.0f / kSensorStandardGravity;

        public override Vector3 Process(Vector3 vector, InputControl control)
        {
            return base.Process(vector * kAccelerationMultiplier, control);
        }
    }

    [DesignTimeVisible(false)]
    internal class OpenHarmonyCompensateRotationProcessor : CompensateRotationProcessor
    {
        public override Quaternion Process(Quaternion value, InputControl control)
        {
            // "...The rotation vector represents the orientation of the device as a combination of an angle and an axis, in which the device has rotated through an angle theta around an axis <x, y, z>."
            // "...The three elements of the rotation vector are < x * sin(theta / 2), y* sin(theta / 2), z* sin(theta / 2)>, such that the magnitude of the rotation vector is equal to sin(theta / 2), and the direction of the rotation vector is equal to the direction of the axis of rotation."
            // "...The three elements of the rotation vector are equal to the last three components of a unit quaternion < cos(theta / 2), x* sin(theta/ 2), y* sin(theta / 2), z* sin(theta/ 2)>."
            //
            // In other words, axis + rotation is combined into Vector3, to recover the quaternion from it, we must compute 4th component as 1 - sqrt(x*x + y*y + z*z)
            var sinRho2 = value.x * value.x + value.y * value.y + value.z * value.z;
            value.w = (sinRho2 < 1.0f) ? Mathf.Sqrt(1.0f - sinRho2) : 0.0f;

            return base.Process(value, control);
        }
    }
}

namespace UnityEngine.InputSystem.OpenHarmony
{
    /// <summary>
    /// Accelerometer device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "Accelerometer", hideInUI = true)]
    public class OpenHarmonyAccelerometer : Accelerometer
    {
    }

    /// <summary>
    /// Gyroscope device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "Gyroscope", hideInUI = true)]
    public class OpenHarmonyGyroscope : Gyroscope
    {
    }

    /// <summary>
    /// Light sensor device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "AmbientLight", hideInUI = true)]
    public class OpenHarmonyAmbientLightSensor : LightSensor
    {
    }

    /// <summary>
    /// Magnetic field sensor device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "MagneticField", hideInUI = true)]
    public class OpenHarmonyMagneticFieldSensor : MagneticFieldSensor
    {
    }


    /// <summary>
    /// Pressure sensor device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "Barometer", hideInUI = true)]
    public class OpenHarmonyBarometerSensor : PressureSensor
    {
    }

    /// <summary>
    /// Proximity sensor type on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "Proximity", hideInUI = true)]
    public class OpenHarmonyProximity : ProximitySensor
    {
    }

    /// <summary>
    /// Gravity sensor device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "Gravity", hideInUI = true)]
    public class OpenHarmonyGravitySensor : GravitySensor
    {
    }

    /// <summary>
    /// Linear acceleration sensor device on OpenHarmony.
    /// </summary>

    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "LinearAcceleration", hideInUI = true)]
    public class OpenHarmonyLinearAccelerationSensor : LinearAccelerationSensor
    {
    }


    /// <summary>
    /// Rotation vector sensor device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "RotationVector", hideInUI = true)]
    public class OpenHarmonyRotationVector : AttitudeSensor
    {
    }

    /// <summary>
    /// Game rotation vector sensor device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "GameRotationVector", hideInUI = true)]
    public class OpenHarmonyGameRotationVector : AttitudeSensor
    {
    }
    
    /// <summary>
    /// Step counter sensor device on OpenHarmony.
    /// </summary>
    [InputControlLayout(stateType = typeof(OpenHarmonySensorState), variants = "Pedometer", hideInUI = true)]
    public class OpenHarmonyPedometer : StepCounter
    {
    }
}

#endif
