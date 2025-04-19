using System;
using System.Linq;
using System.Collections.Generic;
//using Newtonsoft.Json;
using System.Runtime.InteropServices;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
//using Google.XR.Cardboard;

namespace Rokid.UXR.Module
{
    public class RKSensorAPI : MonoSingleton<RKSensorAPI>
    {
        public class OSTInfo
        {
            public CameraIntrinsices camera;
        }

        public class CameraIntrinsices
        {
            public string model;
            public string shutter;
            public double fx;
            public double fy;
            public double cx;
            public double cy;
            public double k1;
            public double k2;
            public double k3;
            public double p1;
            public double p2;
            public float fl;
            public K k;
            public int width;
            public int height;
            public double cameraReadoutTime;
            public double cameraDelayTime;
        }

        public class K
        {
            public int rows;
            public int cols;
            public double[] data;
        }

        public struct YUVImage
        {
            /// <summary>
            /// YUV 图像数据
            /// </summary>
            public byte[] yuvBytes;
            /// <summary>
            /// 时间戳
            /// </summary>
            public long timeStamp;
            /// <summary>
            /// 是否成功获取数据
            /// </summary>
            public bool success;
        }

        public class IMUData
        {
            public long mGlassSensorTimeStamp;
            public float[] mGlassAccData;
            public float[] mGlassMagnetData;
            public float[] mGlassGyroData;

            public long mGlassRotationTimeStamp;
            public float[] mGlassGameRotationData;
            public float[] mGlassRotationData;
        }

        /// <summary>
        /// 图片数据cache
        /// </summary>
        private YUVImage imageData;
        private int width, height;
        private bool isInitCameraPreview, isInitImu;
        private bool getCameraData, getImuData;
        private bool onUsbDeviceInited;
        private double lastTimeStamp = 0;
        private IMUData imuData;


        private void InitCameraPreview()
        {
            NativeInterface.NativeAPI.StartCameraPreview();
            NativeInterface.NativeAPI.SetCameraPreviewDataType(2);
            width = NativeInterface.NativeAPI.GetPreviewWidth();
            height = NativeInterface.NativeAPI.GetPreviewHeight();
            NativeInterface.NativeAPI.OnCameraDataUpdate += OnCameraDataUpdate;
            imageData = new YUVImage()
            {
                yuvBytes = new byte[Convert.ToInt32(width * height * 1.5f)],
                timeStamp = 0
            };
            isInitCameraPreview = true;
        }

        private void ReleaseCameraPreview()
        {
            if (isInitCameraPreview)
            {
                NativeInterface.NativeAPI.StopCameraPreview();
                NativeInterface.NativeAPI.ClearCameraDataUpdate();
                isInitCameraPreview = false;
            }
        }

        private void InitIMU()
        {
            RKLog.Info("====RKSensorAPI==== InitIMU");
            NativeInterface.NativeAPI.RegisterGlassSensorEvent();
            NativeInterface.NativeAPI.RegisterRotationEvent();
            NativeInterface.NativeAPI.OnGlassIMURotationUpdate += OnGlassIMURotationUpdate;
            NativeInterface.NativeAPI.OnGlassIMUSensorUpdate += OnGlassIMUSensorUpdate;
            imuData = new IMUData()
            {
                mGlassSensorTimeStamp = 0,
                mGlassAccData = new float[3],
                mGlassMagnetData = new float[3],
                mGlassGyroData = new float[3],
                mGlassGameRotationData = new float[4],
                mGlassRotationData = new float[4]
            };
            isInitImu = true;
        }


        private void ReleaseIMU()
        {
            if (isInitImu)
            {
                NativeInterface.NativeAPI.UnregisterRotationEvent();
                NativeInterface.NativeAPI.UnregisterGlassSensorEvent();
                NativeInterface.NativeAPI.OnGlassIMURotationUpdate -= OnGlassIMURotationUpdate;
                NativeInterface.NativeAPI.OnGlassIMUSensorUpdate -= OnGlassIMUSensorUpdate;
                isInitImu = false;
            }
        }


        private void OnGlassIMUSensorUpdate(float[] acc, float[] gyr, float[] gnt, long timeStamp)
        {
            this.imuData.mGlassAccData = acc;
            this.imuData.mGlassGyroData = gyr;
            this.imuData.mGlassMagnetData = gnt;
            this.imuData.mGlassSensorTimeStamp = timeStamp;

            //计算IMU帧率,时间为纳秒
            CalIMUFPS(timeStamp);
        }


        private void CalIMUFPS(long timeStamp)
        {
            float delta;
            double currentTimeStamp = timeStamp;
            if (lastTimeStamp == 0)
            {
                delta = 1;
            }
            else
            {
                delta = (float)((currentTimeStamp - lastTimeStamp) / 1000000000);
                RKLog.Debug("====RKSensorAPI====: CalIMUFPS " + delta);
            }
            lastTimeStamp = currentTimeStamp;

            Loom.QueueOnMainThread(() =>
            {
                OnIMUUpdate?.Invoke(delta);
            });

            OnIMUDataCallBack?.Invoke(imuData);
        }

        private void OnGlassIMURotationUpdate(float[] gameRot, float[] rot, long timeStamp)
        {
            this.imuData.mGlassGameRotationData = gameRot;
            this.imuData.mGlassRotationData = rot;
            this.imuData.mGlassRotationTimeStamp = timeStamp;
        }


        private void OnCameraDataUpdate(int width, int height, byte[] data, long timestamp)
        {
            if (this.imageData.timeStamp != timestamp)
            {
                this.imageData.success = true;
                this.imageData.yuvBytes = data;
                this.imageData.timeStamp = timestamp;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (getCameraData)
                ReleaseCameraPreview();
            if (getImuData)
                ReleaseIMU();
        }

        private bool IsPreviewing()
        {
            return NativeInterface.NativeAPI.IsPreviewing();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ReleaseCameraPreview();
            }
        }


        private void Update()
        {
            if (getCameraData && IsPreviewing())
            {
                if (isInitCameraPreview == false)
                {
                    InitCameraPreview();
                }
            }

            if (onUsbDeviceInited == false && NativeInterface.NativeAPI.IsUSBConnect())
            {
                if (getImuData)
                {
                    if (isInitImu == false)
                        InitIMU();
                }
                onUsbDeviceInited = true;
                OnUsbDeviceInited?.Invoke();
            }
        }

        private double GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalMilliseconds;
        }

        #region API

        /// <summary>
        /// IMU数据回调,返回IMUData
        /// </summary>
        public static Action<IMUData> OnIMUDataCallBack;

        /// <summary>
        /// IMU刷新回调
        /// </summary>
        public static Action<float> OnIMUUpdate;

        /// <summary>
        /// USB初始化完成的回调
        /// </summary>
        public static Action OnUsbDeviceInited;

        /// <summary>
        /// 模块初始化
        /// </summary>
        public void Initialize(bool getCameraData, bool getImuData)
        {
            this.getCameraData = getCameraData;
            this.getImuData = getImuData;
        }

        /// <summary>
        /// 获取YUV数据
        /// </summary>
        /// <returns></returns>
        public YUVImage GetYUVImage()
        {
            return imageData;
        }

        /// <summary>
        /// 获取IMUData
        /// </summary>
        /// <returns></returns>
        public IMUData GetIMUData()
        {
            return imuData;
        }

        #endregion
    }
}

