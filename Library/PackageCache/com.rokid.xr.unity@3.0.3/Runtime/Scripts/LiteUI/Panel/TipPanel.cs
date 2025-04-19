using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rokid.UXR.UI
{
    public enum TipLevel
    {
        Normal,
        Warning,
        Error
    }

    public class TipPanel : BasePanel, IDialog
    {
        [SerializeField, Autowrited]
        private Image normal;
        [SerializeField, Autowrited]
        private Image warning;
        [SerializeField, Autowrited]
        private Image error;
        [SerializeField, Autowrited]
        private Text tipText;
        private float elapsedTime = 0;
        private float showTime;
        private Action finishCallBack;

        public void Init(string msg, TipLevel level, float showTime)
        {
            Init(msg, level, showTime, null);
        }

        public void Init(string msg, TipLevel level, float showTime, Action finishCallBack)
        {
            switch (level)
            {
                case TipLevel.Normal:
                    normal.gameObject.SetActive(true);
                    warning.gameObject.SetActive(false);
                    error.gameObject.SetActive(false);
                    break;
                case TipLevel.Warning:
                    normal.gameObject.SetActive(false);
                    warning.gameObject.SetActive(true);
                    error.gameObject.SetActive(false);
                    break;
                case TipLevel.Error:
                    normal.gameObject.SetActive(false);
                    warning.gameObject.SetActive(false);
                    error.gameObject.SetActive(true);
                    break;
            }
            tipText.text = msg;
            this.showTime = showTime;
            elapsedTime = 0;
            this.finishCallBack = finishCallBack;
        }


        private void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > showTime)
            {
                finishCallBack?.Invoke();
                Destroy(this.gameObject);
            }
        }
    }
}
