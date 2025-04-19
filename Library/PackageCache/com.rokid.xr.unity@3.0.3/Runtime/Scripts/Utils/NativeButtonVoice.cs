using UnityEngine;
using Rokid.UXR.Interaction;
namespace Rokid.UXR.Utility
{
    public class NativeButtonVoice : MonoBehaviour
    {
        private InteractorButton button;
        private RKButton rkButton;
        private void Start()
        {
            //RKButton优先
            rkButton = GetComponent<RKButton>();
            if (rkButton != null)
            {
                rkButton?.onPointerClick.AddListener(data =>
                {
                    NativeAudioPlay.Instance.PlayAudioButtonClick();
                });
                rkButton?.onPointerDown.AddListener(data =>
                {
                    NativeAudioPlay.Instance.PlayAudioButtonDown();
                });
            }
            else
            {
                button = GetComponent<InteractorButton>();
                button?.onPointerClick.AddListener(data =>
                {
                    NativeAudioPlay.Instance.PlayAudioButtonClick();
                });
                button?.onPointerDown.AddListener(data =>
                {
                    NativeAudioPlay.Instance.PlayAudioButtonDown();
                });
            }
        }
    }
}

