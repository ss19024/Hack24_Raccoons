using Rokid.UXR.Native;

namespace Rokid.UXR.Utility
{
    public class NativeAudioPlay : MonoSingleton<NativeAudioPlay>
    {
        int buttonDown;
        int buttonClick;
        void Start()
        {
            NativeInterface.NativeAPI.makeSoundPool();
            buttonDown = NativeInterface.NativeAPI.loadSound("Android Native Audio/ButtonDown.wav");
            buttonClick = NativeInterface.NativeAPI.loadSound("Android Native Audio/ButtonUp.wav");
        }

        public void PlayAudioButtonDown()
        {
            if (buttonDown != 0)
                NativeInterface.NativeAPI.playSound(buttonDown);
        }

        public void PlayAudioButtonClick()
        {
            if (buttonClick != 0)
                NativeInterface.NativeAPI.playSound(buttonClick);
        }
    }
}
