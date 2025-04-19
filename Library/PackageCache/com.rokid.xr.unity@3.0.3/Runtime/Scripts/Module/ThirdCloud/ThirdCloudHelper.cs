using System;
using UnityEngine;
namespace Rokid.UXR.Module
{
    [Serializable]
    public class ThirdUserInfoData
    {
        public string accountId;
        public int disabled;
        public string email;
        public long gmtCreate;
        public bool isVoiceAuthed;
        public string mobile;
        public string regionCode;
        public string source;
        public int status;
        public int type;
        public string userId;
        public string userName;
        public bool voiceAuthed;
        public string password;
        public string headIcon;
        public string userType;
        public string idCard;
        public string createDateLong;
        public string agreement;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
    public class ThirdCloudHelper : MonoSingleton<ThirdCloudHelper>
    {
        private Action<ThirdUserInfoData> success;
        private Action<string> getTokenSuccess;
        private Action<string> error;


        protected override void Awake()
        {
            base.Awake();
            this.gameObject.name = "ThirdCloudHelper";
        }

        private void GetToken(string key, string security)
        {
            CallBridge.CallAndroid(Request.Build()
                .Name("ThirdCloud.getThreeCloudToken")
                .Param("key", key)
                .Param("security", security)
                .AndroidCallback(CallBridge.CreateCallback(this.gameObject.name, "OnGetTokenCallBack")));
        }


        private void GetUserInfo(string token)
        {
            CallBridge.CallAndroid(Request.Build()
                .Name("ThirdCloud.getUserInfo")
                .Param("token", token)
                .AndroidCallback(CallBridge.CreateCallback(this.gameObject.name, "OnGetUserInfoCallBack")));
        }

        private void OnGetTokenCallBack(string data)
        {
            if (!string.IsNullOrEmpty(data) && data.Contains("success"))
            {
                int index = data.IndexOf('-') + 1;
                string token = data.Substring(index, data.Length - index);
                // RKLog.Info("====ThirdCloudHelper==== GetToken Success :" + token);
                getTokenSuccess?.Invoke(token);
                GetUserInfo(token);
            }
            else
            {
                // RKLog.Info("====ThirdCloudHelper==== GetToken Faild :" + data);
                error?.Invoke(data);
            }
        }
        private void OnGetUserInfoCallBack(string data)
        {
            if (!string.IsNullOrEmpty(data) && data.Contains("success"))
            {
                int index = data.IndexOf('-') + 1;
                string userInfo = data.Substring(index, data.Length - index);
                // RKLog.Info("====ThirdCloudHelper==== GetUserInfo Success :" + userInfo);
                success?.Invoke(JsonUtility.FromJson<ThirdUserInfoData>(userInfo));
            }
            else
            {
                // RKLog.Info("====ThirdCloudHelper==== GetUserInfo Faild :" + data);
                error?.Invoke(data);
            }
        }

        #region  对外接口
        public void Init()
        {
            new AndroidJavaObject("com.rokid.uxr.thirdcloud.ThirdCloud");
            CallBridge.CallAndroid(Request.Build()
                .Name("ThirdCloud.init"));
        }
        public void GetToken(string key, string security, Action<string> success, Action<string> error)
        {
            this.getTokenSuccess = success;
            this.error = error;
            CallBridge.CallAndroid(Request.Build()
                .Name("ThirdCloud.getThreeCloudToken")
                .Param("key", key)
                .Param("security", security)
                .AndroidCallback(CallBridge.CreateCallback(this.gameObject.name, "OnGetTokenCallBack")));
        }

        public void GetUserInfo(string token, Action<ThirdUserInfoData> success, Action<string> error)
        {
            this.success = success;
            this.error = error;
            GetUserInfo(token);
        }

        /// <summary>
        /// 获取用户接口
        /// </summary>
        /// <param name="key">外部传入需要开发者申请</param>
        /// <param name="security">外部传入需要开发者申请</param>
        /// <param name="success"></param>
        /// <param name="error"></param>
        public void GetUserInfo(string key, string security, Action<ThirdUserInfoData> success, Action<string> error)
        {
            this.success = success;
            this.error = error;
            GetToken(key, security);
        }

        public void TestEnv(bool isTest)
        {
            CallBridge.CallAndroid(Request.Build()
                .Name("ThirdCloud.testEnv")
                .Param("isTest", isTest));
        }
        #endregion
    }


}
