using System;
using Rokid.UXR;
using Rokid.UXR.Module;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;

public class MixThreeDof : MonoSingleton<MixThreeDof>
{
    private Quaternion phoneRotation = Quaternion.identity;
    private float[] data = new float[4];
    public Quaternion deltaOrientation { get; private set; }
    private Vector3 preForward = Vector3.zero;
    private Vector3 curForward = Vector3.zero;
    private RKCameraRig cameraRig;

    private bool recenter = false;

    void Start()
    {
        NativeInterface.NativeAPI.OpenPhoneTracker();
        cameraRig = MainCameraCache.mainCamera.GetComponent<RKCameraRig>();
    }

    protected override void OnDestroy()
    {
        NativeInterface.NativeAPI.ClosePhoneTracker();
    }

    private void Update()
    {
        if (Utils.IsAndroidPlatform())
        {
            if (cameraRig.updateType == RKCameraRig.UpdateType.Update || cameraRig.updateType == RKCameraRig.UpdateType.UpdateAndBeforeRender)
                GetData();
        }
    }


    protected virtual void FixedUpdate()
    {
        if (Utils.IsAndroidPlatform())
        {
            if (cameraRig.updateType == RKCameraRig.UpdateType.Update || cameraRig.updateType == RKCameraRig.UpdateType.UpdateAndBeforeRender)
                GetData();
        }
    }


    /// <inheritdoc />
    // For the same reason as DefaultExecutionOrder, a callback order is specified to
    // apply the pose to the Transform before default user scripts execute.
    [BeforeRenderOrder(-30000)]
    protected virtual void OnBeforeRender()
    {
        if (Utils.IsAndroidPlatform())
        {
            if (cameraRig.updateType == RKCameraRig.UpdateType.UpdateAndBeforeRender)
                GetData();
        }
    }


    /// <inheritdoc />
    protected virtual void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    /// <inheritdoc />
    protected virtual void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    private void GetData()
    {
        NativeInterface.NativeAPI.GetPhonePose(data);
#if USE_ROKID_OPENXR
        phoneRotation[0] = -data[0];
        phoneRotation[1] = -data[1];
        phoneRotation[2] = data[2];
        phoneRotation[3] = data[3];
#else
        phoneRotation[0] = data[0];
        phoneRotation[1] = data[1];
        phoneRotation[2] = -data[2];
        phoneRotation[3] = data[3];
#endif
        if (preForward == Vector3.zero || recenter)
        {
            recenter = false;
            preForward = Vector3.ProjectOnPlane(phoneRotation * Vector3.forward, Vector3.up);
        }
        curForward = Vector3.ProjectOnPlane(phoneRotation * Vector3.forward, Vector3.up);
        deltaOrientation = Quaternion.FromToRotation(preForward, curForward);
    }

    //重置空间
    public void Recenter()
    {
        this.recenter = true;
    }
}
