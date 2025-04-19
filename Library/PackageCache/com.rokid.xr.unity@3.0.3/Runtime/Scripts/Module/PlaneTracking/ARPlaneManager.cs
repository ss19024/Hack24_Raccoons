using System.Collections.Generic;
using UnityEngine;
using Rokid.UXR.Native;
using Unity.Collections;
using Rokid.UXR.Utility;
using UnityEngine.UI;
using System;

namespace Rokid.UXR.Module
{

    public enum PlaneType
    {
        Horizontal,
        Vertical
    }

    public enum PlaneDetectMode
    {
        Horizontal = 1, //Only detect horizontal plane
        Vertical = 2,//Only detect vertical plane
        HorizontalAndVertical = 3 // Detect horizontal and vertical plane
    }


    public class ARPlaneManager : MonoSingleton<ARPlaneManager>
    {
        private Dictionary<long, BoundedPlane> boundedData = new Dictionary<long, BoundedPlane>();
        private Dictionary<long, ARPlane> planeData = new Dictionary<long, ARPlane>();
        [SerializeField]
        private ARPlane planePrefab;
        [SerializeField]
        private PlaneDetectMode planeDetectMode = PlaneDetectMode.Horizontal;
        [SerializeField]
        private Text logText;

        private bool enablePlaneTracker;

        public static event Action<ARPlane> OnPlaneAdded;
        public static event Action<ARPlane> OnPlaneUpdated;
        public static event Action<ARPlane> OnPlaneRemoved;

        private void Start()
        {
            if (planePrefab != null)
            {
                planePrefab.gameObject.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            if (enablePlaneTracker)
                ClosePlaneTracker();
        }

        public void OpenPlaneTracker()
        {
            NativeInterface.NativeAPI.OpenPlaneTracker(planeDetectMode);
            enablePlaneTracker = true;
        }

        public void ClosePlaneTracker()
        {
            NativeInterface.NativeAPI.ClosePlaneTracker();
            enablePlaneTracker = false;
        }

        public void SetPlaneDetectMode(PlaneDetectMode planeDetectMode)
        {
            this.planeDetectMode = planeDetectMode;
            NativeInterface.NativeAPI.SetPlaneDetectMode(planeDetectMode);
        }

        public PlaneDetectMode GetPlaneDetectMode()
        {
            if (Utils.IsAndroidPlatform())
            {
                return NativeInterface.NativeAPI.GetPlaneDetectMode();
            }
            else
            {
                return this.planeDetectMode;
            }
        }

        private void Update()
        {
            if (enablePlaneTracker)
            {
                TrackableChanges<long> changes = NativeInterface.NativeAPI.GetChanges(Allocator.Temp);
                if (changes.added.Length > 0)
                {
                    if (logText != null)
                    {
                        string msg = $"====ARPlane====: Process Added Plane {changes.added.Length},{changes.updated[0]}";
                        RKLog.KeyInfo(msg);
                        logText.text = msg;
                    }
                    // 处理修改平面增加逻辑
                    for (int i = 0; i < changes.added.Length; i++)
                    {
                        long planeHandle = changes.added[i];
                        BoundedPlane boundedPlane = new BoundedPlane() { planeHandle = planeHandle };
                        if (NativeInterface.NativeAPI.TryGetBoundedPlane(planeHandle, ref boundedPlane))
                        {
                            ARPlane arPlane = Instantiate(planePrefab);
                            arPlane.Init(ref boundedPlane);
                            boundedData.Add(planeHandle, boundedPlane);
                            planeData.Add(planeHandle, arPlane);
                            OnPlaneAdded?.Invoke(arPlane);
                            if (logText != null)
                            {
                                string msg = $"====ARPlane====: Do Process Plane Added,{planeHandle}";
                                logText.text += msg;
                                RKLog.KeyInfo("====ARPlane====: Do Process Plane Added:" + boundedPlane.ToString());
                            }
                        }
                    }
                }
                if (changes.updated.Length > 0)
                {
                    if (logText != null)
                    {
                        string msg = $"====ARPlane====: Process Updated Plane {changes.updated.Length},{changes.updated[0]}";
                        RKLog.KeyInfo(msg);
                        logText.text = msg;
                    }
                    // 处理平面更新逻辑
                    for (int i = 0; i < changes.updated.Length; i++)
                    {
                        long planeHandle = changes.updated[i];
                        if (boundedData.TryGetValue(planeHandle, out BoundedPlane boundedPlane))
                        {
                            if (NativeInterface.NativeAPI.TryGetBoundedPlane(planeHandle, ref boundedPlane))
                            {
                                if (planeData.TryGetValue(planeHandle, out ARPlane arPlane))
                                {
                                    arPlane.UpdatePlane(ref boundedPlane);
                                    OnPlaneUpdated?.Invoke(arPlane);
                                    if (logText != null)
                                    {
                                        string msg = $"====ARPlane====: Do Process Plane Updated,{planeHandle}";
                                        logText.text += msg;
                                        RKLog.KeyInfo("====ARPlane====: Do Process Plane Updated:" + boundedPlane.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                if (changes.removed.Length > 0)
                {
                    if (logText != null)
                    {
                        string msg = $"====ARPlane====: Process Removed Plane {changes.removed.Length},{changes.updated[0]} ";
                        RKLog.KeyInfo(msg);
                        logText.text = msg;
                    }
                    // 处理平面移除逻辑
                    for (int i = 0; i < changes.removed.Length; i++)
                    {
                        long planeHandle = changes.removed[i];
                        if (boundedData.TryGetValue(planeHandle, out BoundedPlane boundedPlane))
                        {
                            if (planeData.TryGetValue(planeHandle, out ARPlane arPlane))
                            {
                                if (logText != null)
                                {
                                    string msg = $"====ARPlane====: Do Process Plane Removed,{planeHandle}";
                                    logText.text += msg;
                                    RKLog.KeyInfo("====ARPlane====: Do Process Plane Removed:" + boundedPlane.ToString());
                                }
                                arPlane.DestroyPlane(ref boundedPlane);
                                OnPlaneRemoved?.Invoke(arPlane);
                            }
                        }
                        boundedData.Remove(planeHandle);
                        planeData.Remove(planeHandle);
                    }
                }
                changes.Dispose();
            }
        }
    }
}
