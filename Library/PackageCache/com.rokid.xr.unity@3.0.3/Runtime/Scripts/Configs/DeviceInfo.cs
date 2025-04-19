using System.Collections.Generic;
using UnityEngine;

namespace Rokid.UXR.Config
{
    public class DeviceInfos
    {
        private static List<DeviceInfo> info;
        public static List<DeviceInfo> GetInfos()
        {
            if (info == null)
            {
                info = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeviceInfo>>(Resources.Load<TextAsset>("Configs/DeviceInfo").ToString());
            }
            return info;
        }
    }

    [System.Serializable]
    public class DeviceInfo
    {
        /// 设备编号 
        public int DeviceId;
        /// 设备名称 
        public string DeviceName;
        /// 设备PID 
        public int PID;
        /// FOV 
        public float CameraFov;
        /// 是否有相机Camera 
        public int HaveCamera;
    }
}
