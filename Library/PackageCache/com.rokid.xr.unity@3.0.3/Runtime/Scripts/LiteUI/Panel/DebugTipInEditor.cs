using System.Transactions;
using Rokid.UXR.UI;
using UnityEngine;

namespace Rokid.UXR.UI
{
    [ExecuteInEditMode]
    public class DebugTipInEditor : BaseUI
    {
        [SerializeField]
        private DebugTipPanel.DebugType debugType;

        private DebugTipPanel.DebugType oriDebugType;

        private bool showTip = true;

        private DebugTipPanel tipPanel;

        protected override void Start()
        {
            base.Start();
            oriDebugType = debugType;
        }

        private void OnEnable()
        {
            showTip = true;
        }

        private void OnDisable()
        {
            showTip = false;
            if (tipPanel != null)
                DestroyImmediate(tipPanel.gameObject);
        }


        private void Update()
        {
            if (showTip)
            {
                showTip = false;
                tipPanel = CreatePanel<DebugTipPanel>(true);
                tipPanel.Init(debugType);
            }

            if (oriDebugType != debugType)
            {
                showTip = true;
                oriDebugType = debugType;
            }
        }
    }
}
