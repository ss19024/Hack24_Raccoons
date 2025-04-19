using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rokid.UXR.Module
{
    public class OfflineVoiceModule : MonoSingleton<OfflineVoiceModule>
    {
        private static string RK_VOICE_SERVICE = "RKVoiceCommand.";

        private bool m_Init = false;

        protected override void OnSingletonInit()
        {
            base.OnSingletonInit();
            this.gameObject.name = "OfflineVoiceModule";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialized()
        {
            if (m_Init) return;
            m_Init = true;
        }

        /// <summary>
        /// add instruct 
        /// </summary>
        /// <param name="language">0 - zh, 1 - en</param>
        /// <param name="name">voice name</param>
        /// <param name="pinyin"></param>
        /// <param name="gameobj"></param>
        /// <param name="unitycallbackfunc"></param>
        /// <param name="tmp"></param>

        public void AddInstruct(LANGUAGE language, string name, string pinyin, string callbackObj, string callbackMethod)
        {
            Request re;
            if (language == LANGUAGE.CHINESE || language == LANGUAGE.ENGLISH)
            {
                Debug.Log("-uxr- OfflineVoiceModule AddInstruct.");
                re = Request.Build()
                .Name(RK_VOICE_SERVICE + "addCommand")
                .Param("zhKey", name)
                .Param("zhPinyin", pinyin)

                .AndroidCallback(CallBridge.CreateCallback(callbackObj, callbackMethod));
            }
            else
            {
                return;
            }
            CallBridge.CallAndroid(re);
        }


        public void ClearAllInstruct()
        {
            CallBridge.CallAndroid(Request.Build()
                .Name(RK_VOICE_SERVICE + "clearCommand"));
        }

        public void Commit()
        {
            CallBridge.CallAndroid(Request.Build()
                .Name(RK_VOICE_SERVICE + "addCommandFinish"));
        }

        public void ChangeVoiceCommandLanguage(LANGUAGE language)
        {
            int la = (int)language;
            CallBridge.CallAndroid(Request.Build()
                .Name(RK_VOICE_SERVICE + "changeVoiceLanguage").Param("language", la));
        }



    }

    public enum LANGUAGE
    {
        CHINESE = 0,//default
        ENGLISH = 1
    }
}
