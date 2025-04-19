using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public class RokidController : XRBaseController
    {
        public static InputDevice ControllerDevice;
        
        [Header("Key Interaction")]
        [SerializeField]
        bool IsShowRay = true;

        [SerializeField]
        XRRayInteractor rayInteractor;

        [SerializeField]
        bool ExitGameByClickXKey = true;
        
        [SerializeField]
        bool ResetPoseByDoubleClickOKey = true;
        private bool keyDownStatus;
        private int keyDownCount;
        private float lastTime;
        private float currentTime;
        public float timeElapse = 0.3f;
        
        [Header("Key Binding")]
        [SerializeField]
        InputActionProperty m_SelectKeyAction = new InputActionProperty(new InputAction("Select", type: InputActionType.Button));
        [SerializeField]
        InputActionProperty m_XKeyAction = new InputActionProperty(new InputAction("X", type: InputActionType.Button));
        [SerializeField]
        InputActionProperty m_OKeyAction = new InputActionProperty(new InputAction("O", type: InputActionType.Button));
        [SerializeField]
        InputActionProperty m_UpKeyAction = new InputActionProperty(new InputAction("Up", type: InputActionType.Button));
        [SerializeField]
        InputActionProperty m_DownKeyAction = new InputActionProperty(new InputAction("Down", type: InputActionType.Button));
        [SerializeField]
        InputActionProperty m_LeftKeyAction = new InputActionProperty(new InputAction("Left", type: InputActionType.Button));
        [SerializeField]
        InputActionProperty m_RightKeyAction = new InputActionProperty(new InputAction("Right", type: InputActionType.Button));
        [SerializeField]
        InputActionProperty m_MenuKeyAction = new InputActionProperty(new InputAction("Menu", type: InputActionType.Button));
        
        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);

            if (controllerState == null)
                return;

            if (!ControllerDevice.isValid)
            {
                ControllerDevice = RokidUtils.FindRokidDevice(InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller);
            }
            if (!ControllerDevice.isValid)
            {
                return;
            }

            if (rayInteractor != null)
            {
                rayInteractor.enabled = IsShowRay;
            }

            if (IsShowRay)
            {
                ControllerDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 ControllerPosition);
                ControllerDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion ControllerRotation);
                controllerState.position = ControllerPosition;
                controllerState.rotation = ControllerRotation;
                controllerState.isTracked = true;
                controllerState.inputTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
            }
            else
            {
                controllerState.isTracked = false;
                controllerState.inputTrackingState = InputTrackingState.None;
            }
        }

        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);
            if (controllerState == null)
                return;

            bool triggerButton = false;
            bool gripButton = false;
            bool menuButton = false;
            if (m_SelectKeyAction.action != null)
            {
                triggerButton = IsPressed(m_SelectKeyAction.action);
            }

            if (m_XKeyAction.action != null)
            {
                menuButton = IsPressed(m_XKeyAction.action);
            }

            if (m_OKeyAction.action != null)
            {
                gripButton = IsPressed(m_OKeyAction.action);
            }

            controllerState.ResetFrameDependentStates();
            controllerState.selectInteractionState.SetFrameState(triggerButton, triggerButton ? 1.0f : 0.0f);
            controllerState.activateInteractionState.SetFrameState(triggerButton, triggerButton ? 1.0f : 0.0f);
            controllerState.uiPressInteractionState.SetFrameState(triggerButton, triggerButton ? 1.0f : 0.0f);

            if (IsShowRay && ResetPoseByDoubleClickOKey)
            {
                if (gripButton)
                {
                    if (!keyDownStatus)
                    {
                        keyDownStatus = true;
                        if (keyDownCount == 0)
                        {
                            lastTime = Time.realtimeSinceStartup;
                        }

                        keyDownCount++;
                    }
                }

                if (!gripButton)
                {
                    keyDownStatus = false;
                }

                if (keyDownStatus)
                {
                    if (keyDownCount >= 2)
                    {
                        currentTime = Time.realtimeSinceStartup;
                        if (currentTime - lastTime < timeElapse)
                        {
                            lastTime = currentTime;
                            keyDownCount = 0;
                            RokidExtensionAPI.RokidOpenXR_API_RecenterPhonePose();
                        }
                        else
                        {
                            lastTime = Time.realtimeSinceStartup;
                            keyDownCount = 1;
                        }
                    }
                }
            }

            if (ExitGameByClickXKey)
            {
                if (menuButton)
                {
#if !UNITY_EDITOR
                    Debug.Log("Call Application Quit...Bye...");
                    Application.Quit();
#endif
                }
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            EnableAllDirectActions();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DisableAllDirectActions();
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        bool IsPressed(InputAction action)
        {
            if (action == null)
                return false;

#if INPUT_SYSTEM_1_1_OR_NEWER || INPUT_SYSTEM_1_1_PREVIEW // 1.1.0-preview.2 or newer, including pre-release
                return action.phase == InputActionPhase.Performed;
#else
            if (action.activeControl is ButtonControl buttonControl)
            {
                return buttonControl.isPressed;
            }

            return action.triggered || action.phase == InputActionPhase.Performed;
#endif
        }

        void EnableAllDirectActions()
        {
            m_SelectKeyAction.EnableDirectAction();
            m_XKeyAction.EnableDirectAction();
            m_OKeyAction.EnableDirectAction();
            m_UpKeyAction.EnableDirectAction();
            m_DownKeyAction.EnableDirectAction();
            m_LeftKeyAction.EnableDirectAction();
            m_RightKeyAction.EnableDirectAction();
            m_MenuKeyAction.EnableDirectAction();
        }

        void DisableAllDirectActions()
        {
            m_SelectKeyAction.DisableDirectAction();
            m_XKeyAction.DisableDirectAction();
            m_OKeyAction.DisableDirectAction();
            m_UpKeyAction.DisableDirectAction();
            m_DownKeyAction.DisableDirectAction();
            m_LeftKeyAction.DisableDirectAction();
            m_RightKeyAction.DisableDirectAction();
            m_MenuKeyAction.DisableDirectAction();
        }
    }
}