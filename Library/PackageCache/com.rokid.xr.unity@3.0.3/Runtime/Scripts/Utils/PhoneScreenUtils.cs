
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Rokid.UXR.Utility
{
    [ExecuteAlways]
    public class PhoneScreenUtils : MonoBehaviour
    {
        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private Camera phoneCamera;
        [SerializeField]
        private Text infoText;
        /// <summary>
        /// 在Unity编辑器上是否激活
        /// </summary>
        [SerializeField]
        private bool activeWhenUnityEditor;

        private void OnEnable()
        {
            // AutoInjectComponent.AutoInject(transform, this);
#if !UNITY_EDITOR
	        canvas.gameObject.SetActive(true);
	        phoneCamera.gameObject.SetActive(true);
	        phoneCamera.clearFlags=CameraClearFlags.SolidColor;
#endif
#if UNITY_2021_3_OR_NEWER
            if (infoText != null && SceneManager.GetActiveScene().name.Contains("RK"))
            {
                infoText.text = "Rokid_" + SceneManager.GetActiveScene().name.Split("RK")[1];
            }
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR
            canvas.gameObject.SetActive(activeWhenUnityEditor);
            phoneCamera.gameObject.SetActive(activeWhenUnityEditor);
#endif
        }
    }
}
