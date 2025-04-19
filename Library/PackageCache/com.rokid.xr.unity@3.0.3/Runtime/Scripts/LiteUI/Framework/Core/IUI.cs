using UnityEngine;
namespace Rokid.UXR.UI
{
    public interface IUI
    {
        Transform GetUIParent();

        string GetPanelPath();
        string GetItemPath();

        T CreatePanel<T>(bool dialog, string prefabName, bool findExitUI) where T : BasePanel;
        T CreatePanel<T>(Transform parent, bool dialog, string prefabName, bool findExitUI) where T : BasePanel;
        T CreateItem<T>(Transform parent, bool active) where T : BaseItem;

        void ExitPanel<T>() where T : BasePanel;
        void ExitDialog<T>() where T : IDialog;

        void ExitAll();
    }

}
