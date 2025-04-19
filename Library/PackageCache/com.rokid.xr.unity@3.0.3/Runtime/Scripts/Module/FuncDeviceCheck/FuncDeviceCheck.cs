using System.Collections.Generic;
using System.Linq;
using Rokid.UXR.Native;

namespace Rokid.UXR.Module
{
    /// <summary>
    /// 功能设备匹配工具类
    /// </summary>
    public class FuncDeviceCheck
    {
        public enum FuncEnum
        {
            HandTracking,
            Slam,
            CameraFunc
        }

        /// <summary>
        /// 功能检测
        /// </summary>
        /// <param name="glassDeviceModel"></param>
        /// <param name="funcName"></param>
        /// <returns></returns>
        public static bool Check(string glassDeviceModel, string funcName)
        {
            bool valid = false;
            List<DeviceFuncMatchInfo> deviceInfos = DeviceFuncMatchInfos.GetInfos();
            DeviceFuncMatchInfo info = deviceInfos.Where(item =>
            {
                return item.FuncName == funcName && ContainGlassDeviceModel(glassDeviceModel, item.GlassDeviceModels);
            }).FirstOrDefault();
            if (info != null)
            {
                RKLog.Info(string.Format("====FuncDeviceCheck==== {0}:功能支持,眼镜设备模型:{1}", funcName, glassDeviceModel));
                valid = true;
            }
            else
            {
                RKLog.Error(string.Format("====FuncDeviceCheck==== {0}:功能不支持,眼镜设备模型:{1}", funcName, glassDeviceModel));
            }
            return valid;
        }


        /// <summary>
        /// 是否存在模块
        /// </summary>
        /// <param name="glassDeviceModel"></param>
        /// <returns></returns>
        private static bool ContainGlassDeviceModel(string glassDeviceModel, string models)
        {
            string[] data = models.Split('|');
            for (int i = 0; i < data.Length; i++)
            {
                if (glassDeviceModel.Contains(data[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckHandTrackingFunc()
        {
            return Check(NativeInterface.NativeAPI.GetGlassName(), FuncEnum.HandTracking.ToString());
        }

        public static string GetOSVersion(string OSVersion)
        {
            int index = OSVersion.LastIndexOf('/');
            string osVersion = OSVersion.Substring(index + 1);
            osVersion = osVersion.Remove(osVersion.Length - 1);
            return osVersion;
        }

        public static bool CheckCameraFunc()
        {
            return Check(NativeInterface.NativeAPI.GetGlassName(), FuncEnum.CameraFunc.ToString());
        }

        public static bool CheckSlamFunc()
        {
            return Check(NativeInterface.NativeAPI.GetGlassName(), FuncEnum.Slam.ToString());
        }

        public static bool CheckFunc(FuncEnum func)
        {
            switch (func)
            {
                case FuncEnum.HandTracking:
                    return CheckHandTrackingFunc();
                case FuncEnum.CameraFunc:
                    return CheckCameraFunc();
                case FuncEnum.Slam:
                    return CheckSlamFunc();
                default:
                    return false;
            }
        }
    }

}
