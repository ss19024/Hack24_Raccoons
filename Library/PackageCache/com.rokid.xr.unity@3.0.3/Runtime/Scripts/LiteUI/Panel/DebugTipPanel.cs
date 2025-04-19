using Rokid.UXR.UI;
using UnityEngine.UI;
using UnityEngine;

namespace Rokid.UXR.UI
{
    [ExecuteInEditMode]
    public class DebugTipPanel : BasePanel, IDialog
    {
        [SerializeField]
        private Image icon;

        public enum DebugType
        {
            GESTURE,
            MOUSE,
            RAY
        }
        public void Init(DebugType type)
        {
            switch (type)
            {
                case DebugType.GESTURE:
                    icon.sprite = Resources.Load<Sprite>("Textures/Mock/Gesture");
                    break;
                case DebugType.MOUSE:
                    icon.sprite = Resources.Load<Sprite>("Textures/Mock/Mouse");
                    break;
                case DebugType.RAY:
                    icon.sprite = Resources.Load<Sprite>("Textures/Mock/Ray");
                    break;
            }
        }
    }
}
