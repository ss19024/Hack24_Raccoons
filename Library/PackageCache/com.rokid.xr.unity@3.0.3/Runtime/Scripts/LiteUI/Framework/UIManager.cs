using UnityEngine;

namespace Rokid.UXR.UI
{
    public class UIManager : BaseUI
    {
        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    instance = go.AddComponent<UIManager>();
                }
                return instance;
            }
        }
    }
}

