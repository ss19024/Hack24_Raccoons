using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaneRaycastSample : MonoBehaviour
{
    public GameObject spawnPrefab;

    private static List<ARRaycastHit> Hits;

    private ARRaycastManager mRaycastManager;

    private ARPlaneManager mPlaneManager;

    private GameObject spawnedObject = null;

    // 所有已检测到平面列表
    private List<ARPlane> mPlanes;

    // 显示平面标志
    private bool isShow = true;
    
    [SerializeField]
    private Toggle m_PlaneEnableToggle;
    [SerializeField]
    private Toggle m_PlaneDisplayToggle;
    [SerializeField]
    private Dropdown m_PlaneDetectionModeSelect;
    
    private InputActionProperty m_PositionAction = new InputActionProperty(new InputAction("Position", expectedControlType: "Vector3", binding: "<RokidController>/{DevicePosition}"));
    private InputActionProperty m_RotationAction = new InputActionProperty(new InputAction("Rotation", expectedControlType: "Quaternion", binding: "<RokidController>/{DeviceRotation}"));
    private InputActionProperty m_IsTrackedAction = new InputActionProperty(new InputAction("Is Tracked", type: InputActionType.Button, binding: "<RokidController>/isTracked") { wantsInitialStateCheck = true });
    private InputActionProperty m_SelectAction = new InputActionProperty(new InputAction("Select", type: InputActionType.Button, binding: "<RokidController>/Select"));
    
    void EnableAllDirectActions()
    {
        m_IsTrackedAction.EnableDirectAction();
        m_SelectAction.EnableDirectAction();
        m_PositionAction.EnableDirectAction();
        m_RotationAction.EnableDirectAction();
    }

    void DisableAllDirectActions()
    {
        m_IsTrackedAction.DisableDirectAction();
        m_SelectAction.DisableDirectAction();
        m_PositionAction.DisableDirectAction();
        m_RotationAction.DisableDirectAction();
    }

    void Start()
    {
        mPlanes = new List<ARPlane>();
        Hits = new List<ARRaycastHit>();
        mRaycastManager = GetComponent<ARRaycastManager>();
        mPlaneManager = GetComponent<ARPlaneManager>();
#if UNITY_6000_0_OR_NEWER
        mPlaneManager.trackablesChanged.AddListener(OnPlaneChanged);
#else
        mPlaneManager.planesChanged += OnPlaneChanged;
#endif
        

        if (m_PlaneEnableToggle)
        {
            m_PlaneEnableToggle.onValueChanged.AddListener(TogglePlaneDetection);
        }
        if (m_PlaneDisplayToggle)
        {
            m_PlaneDisplayToggle.onValueChanged.AddListener(TogglePlaneDisplay);
        }
        if (m_PlaneDetectionModeSelect)
        {
            m_PlaneDetectionModeSelect.onValueChanged.AddListener(SwitchPlaneDetectionMode);
        }
  
        EnableAllDirectActions();
    }

    private void OnDestroy()
    {
#if UNITY_6000_0_OR_NEWER
        mPlaneManager.trackablesChanged.RemoveListener(OnPlaneChanged);
#else
        mPlaneManager.planesChanged -= OnPlaneChanged;
#endif
        
        if (m_PlaneEnableToggle)
        {
            m_PlaneEnableToggle.onValueChanged.RemoveListener(TogglePlaneDetection);
        }
        if (m_PlaneDisplayToggle)
        {
            m_PlaneDisplayToggle.onValueChanged.RemoveListener(TogglePlaneDisplay);
        }
        if (m_PlaneDetectionModeSelect)
        {
            m_PlaneDetectionModeSelect.onValueChanged.RemoveListener(SwitchPlaneDetectionMode);
        }
        
        DisableAllDirectActions();
    }

    protected virtual bool IsPressed(InputAction action)
    {
        if (action == null)
            return false;

#if INPUT_SYSTEM_1_1_OR_NEWER || INPUT_SYSTEM_1_1_PREVIEW // 1.1.0-preview.2 or newer, including pre-release
                return action.phase == InputActionPhase.Performed;
#else
        if (action.activeControl is ButtonControl buttonControl)
            return buttonControl.isPressed;

        return action.triggered || action.phase == InputActionPhase.Performed;
#endif
    }

    /**
     * 平面变化事件处理方法
     */
#if UNITY_6000_0_OR_NEWER
    private void OnPlaneChanged(ARTrackablesChangedEventArgs<ARPlane> arg)
#else
    private void OnPlaneChanged(ARPlanesChangedEventArgs arg)
#endif
    {
        Debug.Log("RK-Openxr-plane: OnPlaneChanged Added Count = " + arg.added.Count);
        for (int i = 0; i < arg.added.Count; i++)
        {
            mPlanes.Add(arg.added[i]);
            arg.added[i].gameObject.SetActive(isShow);
        }
    }

    /**
     * 显示或隐藏素有已检测到的平面
     */
    void SetAllPlanesActive(bool value)
    {
        foreach (var plane in mPlaneManager.trackables)
        {
            plane.gameObject.SetActive(value);
        }
    }

    /**
     * 开关平面检测功能以及切换检测到平面显隐
     */
    void TogglePlaneDetection(bool enable)
    {
        Debug.Log("RK-Openxr-plane: TogglePlaneDetection enable = " + enable);
        mPlaneManager.enabled = enable;

        if (mPlaneManager.enabled)
        {
            SetAllPlanesActive(true);
        }
        else
        {
            SetAllPlanesActive(false);
        }
    }
    
    /**
     * 切换检测到平面显隐
     */
    void TogglePlaneDisplay(bool show)
    {
        Debug.Log("RK-Openxr-plane: TogglePlaneDisplay show = " + show+", isShow="+isShow+", mPlanes.Count="+mPlanes.Count);
        isShow = show;
        for (int i = mPlanes.Count - 1; i >= 0; i--)
        {
            if (mPlanes[i] == null || mPlanes[i].gameObject == null)
            {
                mPlanes.Remove(mPlanes[i]);
            }
            else
            {
                mPlanes[i].gameObject.SetActive(isShow);
            }
        }

    }
    
    /**
     * 隐藏不同模式下的平面
     */
    void TogglePlaneDisplayOnModeChange(PlaneDetectionMode mode)
    {
        if (!isShow)
        {
            return;
        }
        
        for (int i = mPlanes.Count - 1; i >= 0; i--)
        {
            if (mPlanes[i] == null || mPlanes[i].gameObject == null)
            {
                mPlanes.Remove(mPlanes[i]);
            }
            else
            {
                Debug.Log("RK-Openxr-plane: TogglePlaneDisplayOnModeChange mPlanes["+i+"].alignment="+mPlanes[i].alignment+", mode="+mode);
                if ((mode == PlaneDetectionMode.Horizontal && mPlanes[i].alignment == PlaneAlignment.Vertical) || 
                    (mode == PlaneDetectionMode.Vertical && mPlanes[i].alignment != PlaneAlignment.Vertical))
                {
                    mPlanes[i].gameObject.SetActive(false);
                    continue;
                }
                mPlanes[i].gameObject.SetActive(true);
            }
        }

    }
    
    
    /**
     * 设置检测模式
     */
    void SwitchPlaneDetectionMode(int mode)
    {
        Debug.Log("RK-Openxr-plane: SwitchPlaneDetectionMode mode = " + mode);
        if (mode == 0)
        {
            mPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
        }
        else if (mode == 1)
        {
            mPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        }
        else if (mode == 2)
        {
            mPlaneManager.requestedDetectionMode = PlaneDetectionMode.Vertical;
        }
        
        TogglePlaneDisplayOnModeChange(mPlaneManager.requestedDetectionMode);
    }
    
    
    void Update()
    {
        bool isSelect = IsPressed(m_SelectAction.action);
        bool isTracked = IsPressed(m_IsTrackedAction.action);
        
        if (isSelect && isTracked)
        {
            Vector3 position = m_PositionAction.action.ReadValue<Vector3>();
            Quaternion rotation = m_RotationAction.action.ReadValue<Quaternion>();
            
            Ray controllerRay = new Ray(position,  rotation * Vector3.forward);
        
            if (mRaycastManager.Raycast(controllerRay, Hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
            {
                var hitPos = Hits[0].pose;
                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(spawnPrefab, hitPos.position, hitPos.rotation);
                }
                else
                {
                    spawnedObject.transform.position = hitPos.position;
                }
            }
        }
    }
}
