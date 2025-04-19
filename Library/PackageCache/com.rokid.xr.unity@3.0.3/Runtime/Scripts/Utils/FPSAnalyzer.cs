using System;
using Rokid.UXR.Interaction;
using Rokid.UXR.Native;
using UnityEngine;
using UnityEngine.UI;

namespace Rokid.UXR.Utility
{
    public class FPSAnalyzer : MonoBehaviour
    {
        /// <summary>
        /// 最大统计时长,单位秒
        /// </summary>
        [SerializeField]
        private int maxStatisticsTime = 60;
        /// <summary>
        /// 低帧率的阈值
        /// </summary>
        [SerializeField]
        private int lowFPSThreshold = 70;
        /// <summary>
        /// 平均帧率
        /// </summary>
        private int averageFPS;
        /// <summary>
        /// 总的帧率
        /// </summary>
        private int totalFPS;
        /// <summary>
        /// 低帧率个数
        /// </summary>
        private int lowFPSCount;
        /// <summary>
        /// 低帧率占比
        /// </summary>
        private float lowFPSPercent;
        /// <summary>
        /// 总的帧数
        /// </summary>
        private int totalFPSCount;
        /// <summary>
        /// 统计时长
        /// </summary>
        private float elapsedTime;

        private int FPS;
        [SerializeField]
        private Text logText;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            averageFPS = 0;
            lowFPSCount = 0;
            lowFPSPercent = 0;
            totalFPSCount = 0;
            elapsedTime = 0;
            totalFPS = 0;
            string value = NativeInterface.NativeAPI.GetPersistValue("unity_fps_analysis_time");
            if (!string.IsNullOrEmpty(value))
            {
                int time = Convert.ToInt32(value);
                if (time > 0)
                {
                    maxStatisticsTime = time;
                }
            }
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < maxStatisticsTime)
            {
                FPS = Mathf.RoundToInt(1 / Time.smoothDeltaTime);
                if (FPS >= 0)
                    totalFPS += FPS;
                totalFPSCount++;
                averageFPS = Mathf.RoundToInt((float)totalFPS / totalFPSCount);
                if (FPS < lowFPSThreshold)
                {
                    lowFPSCount++;
                    lowFPSPercent = (float)lowFPSCount / totalFPSCount;
                }

                if (logText != null)
                {
                    logText.text = $"FPS: {FPS}\r\nAverageFPS:{averageFPS}\r\nLowFPSThreshold:{lowFPSThreshold}\r\nLowFPSCount:{lowFPSCount}\r\nTotalFPSCount:{totalFPSCount}\r\nLowFPSPercent:{Mathf.RoundToInt(lowFPSPercent * 100)}%\r\nStatisticsTime:{(int)elapsedTime}\r\nDouble Click To Reset";
                }
            }

            if (IsDoubleClick())
            {
                Init();
            }
        }


        #region IsDoubleClick
        float doubleClickTime = 0.7f;
        float clickTime = 0;
        int clickCount = 0;
        //Only for station pro
        private bool IsDoubleClick()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton3) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                clickCount++;
            }
            if (clickCount == 1)
            {
                clickTime += Time.deltaTime;
            }
            if (clickTime < doubleClickTime)
            {
                if (clickCount == 2)
                {
                    clickTime = 0;
                    clickCount = 0;
                    return true;
                }
            }
            else
            {
                clickCount = 0;
                clickTime = 0;
            }
            return false;
        }
        #endregion
    }

}
