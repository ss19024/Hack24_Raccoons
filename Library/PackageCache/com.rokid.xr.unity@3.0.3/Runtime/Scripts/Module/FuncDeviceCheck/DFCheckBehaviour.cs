using UnityEngine;
using Rokid.UXR.Native;
using Rokid.UXR.UI;
using UnityEngine.SceneManagement;

namespace Rokid.UXR.Module
{
    /// <summary>
    /// Device diagnostic tool
    /// </summary>
    public class DFCheckBehaviour : MonoBehaviour
    {
        public float delayTime = 5;
        private bool checkValid;
        public FuncDeviceCheck.FuncEnum needCheckFunc;

        void Start()
        {
#if !UNITY_EDITOR
            if (!CheckDevice())
            {
                //add tip
                string tipInfo = string.Format("当前设备{0},不支持{1}功能,{2}秒后退出场景", NativeInterface.NativeAPI.GetGlassName(), needCheckFunc.ToString(), delayTime);
                UIManager.Instance.CreatePanel<TipPanel>(true).Init(tipInfo, TipLevel.Error, delayTime, () =>
                {
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        Quit();
                    }
                    else
                    {
                        SceneManager.LoadScene(0);
                    }
                });
            }
#endif
        }

        public void Quit()
        {
            Application.Quit();
        }

        public bool CheckDevice()
        {
            return FuncDeviceCheck.CheckFunc(needCheckFunc);
        }
    }

}
