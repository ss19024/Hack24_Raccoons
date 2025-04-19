using System.Collections.Generic;
using UnityEngine;

namespace Rokid.UXR
{
    /// <summary>
    /// Unity Android CallBridge
    /// </summary>
    public class CallBridge
    {
        private static AndroidJavaObject bridge = new AndroidJavaClass("com.rokid.unitycallbridge.UnityCallBridge");
        public static List<string> cmdlist = new List<string>();
        static Request.Callback joinCallback = new Request.Callback();

        public static AndroidJavaObject CallAndroid(Request request)
        {
            //RKLog.Error("name="+request.name);
            return bridge.CallStatic<AndroidJavaObject>("onUnityCall", CreateBaseRequest(request));
        }

        public static bool CovertBool(AndroidJavaObject obj)
        {
            return bridge.CallStatic<bool>("ConvertBoolean", obj);
        }

        public static int CovertInt(AndroidJavaObject obj)
        {
            return bridge.CallStatic<int>("ConvertInt", obj);
        }

        public static string CovertString(AndroidJavaObject obj)
        {
            return bridge.CallStatic<string>("ConvertString", obj);
        }

        public static float ConvertFloat(AndroidJavaObject obj)
        {
            return bridge.CallStatic<float>("ConvertFloat", obj);
        }

        public static double ConvertDouble(AndroidJavaObject obj)
        {
            return bridge.CallStatic<double>("ConvertDouble", obj);
        }

        public static Request.Callback CreateCallback(string name, string method)
        {
            return CreateCallback(name, method, null);
        }

        public static Request.Callback CreateCallback(string name, string method, string param)
        {
            joinCallback.name = name;
            joinCallback.method = method;
            joinCallback.param = param;
            return joinCallback;
        }

        private static string CreateBaseRequest(Request request)
        {
            return JsonUtility.ToJson(request);
        }

        public static void RegisterStaticService(string serviceClass)
        {
            AndroidJavaClass service = new AndroidJavaClass(serviceClass);
            bridge.CallStatic("registerStaticService", service);
        }
    }
}