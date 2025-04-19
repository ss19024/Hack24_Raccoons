using UnityEngine;
using Rokid.UXR.UI;
using Rokid.UXR.Interaction;
namespace Rokid.UXR.Utility
{
    public class HandTipSample : MonoBehaviour
    {
        private void Start()
        {
            GesEventInput.OnHandLostInCameraSpace += OnHandLostInCameraSpace;
        }

        private void OnDestroy()
        {
            GesEventInput.OnHandLostInCameraSpace -= OnHandLostInCameraSpace;
        }

        private void OnHandLostInCameraSpace(HandType handType)
        {
            if (InputModuleManager.Instance.GetGesActive())
            {
                RKLog.Info("====HandTipSample====: OnHandLostInCameraSpace");
                //增加提示
                UIManager.Instance.CreatePanel<TipPanel>(true).Init("请将手放置在相机的可视范围内", TipLevel.Warning, 0.5f);
            }
        }
    }

}
