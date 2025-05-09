---
uid: xri-samples-legacy-xr-input-readers
---
# Legacy XR Input Readers

This sample contains assets that can be used with behaviors in the XR Interaction Toolkit that read input. Refer to [Input Readers](xref:xri-input-readers#legacy-xr-input-readers) for information about using these assets.

These ScriptableObjects are mainly intended to assist with migration from the older device-based controller input that can be used with the new input readers introduced with version 3.0.0. It is recommended that you use the input actions included in the [Starter Assets](samples-starter-assets.md) sample with input readers instead of these assets to take advantage of the benefits that the Input System package provides. For example, it separates the logical inputs from the physical inputs or bind multiple cross-platform controller inputs to a single semantic action. Some features of the XR Interaction Toolkit package, such as the XR Device Simulator, are only supported when using input actions.

|**Asset**| **Description**|
|---|---|
|**`Haptics\`**|Asset folder which contains a `Left_Controller` and `Right_Controller` ScriptableObject which can be used as an Object Reference value of the **Haptic Output** property on the Haptic Impulse Player component. This is an alternative to using an Input Action with a haptic control binding or an [Any] binding that identifies an input device.|
|**`Presets\`**|Asset folder which contains [presets](https://docs.unity3d.com/Manual/Presets.html) for ScriptableObjects to set the [`InputDevice`](https://docs.unity3d.com/ScriptReference/XR.InputDevice.html) Characteristics which identifies either the left or right motion controller. These can be used to streamline their configuration if one of the controls you want to read from is not included in this sample.|
|**`Left_*` and `Right_*`**|ScriptableObjects which can be used as an Object Reference value of input properties. These identify the `InputDevice` using Characteristics for the left or right motion controller and the Usage that identifies an input control to read with [`InputDevice.TryGetFeatureValue`](https://docs.unity3d.com/ScriptReference/XR.InputDevice.TryGetFeatureValue.html). The different assets correspond with different usage strings from [`CommonUsages`](https://docs.unity3d.com/ScriptReference/XR.CommonUsages.html), such as the primary face button.|

<a name="import"></a>
## Import the Legacy XR Input Readers sample

To import these sample assets in your project:

1. Open the **Package Manager** window (menu: **Window > Package Manager**).
2. Select the **XR Interaction Toolkit** from the list of packages in the project. (If you haven't added the package to the project yet, you must do so before proceeding.)
3. Select the **Samples** tab.
4. Click the **Import** button next to the **Legacy XR Input Readers** sample.

The asset files are installed into the default location for package samples, in the `Assets\Samples\XR Interaction Toolkit\[version]\Legacy XR Input Readers` folder. You can move these assets to a different location.