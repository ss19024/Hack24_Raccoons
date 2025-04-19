using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rokid.UXR.Utility
{
    public class GlobalFPS : MonoBehaviour
    {
        private float fpsUpdateTime = 1.0f;
        private float passedTime = 0.0f;

        private Canvas canvas;
        public Text textField;

        private void Start()
        {
            canvas = gameObject.GetComponent<Canvas>();
            if (MainCameraCache.mainCamera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = MainCameraCache.mainCamera;
                canvas.planeDistance = 3;
                if (textField != null)
                {
                    float textFieldWidth = textField.rectTransform.rect.width;
                    float textFieldHeight = textField.rectTransform.rect.height;
                    textField.rectTransform.anchoredPosition = new Vector2(Screen.width / 2.0f - 2.3f * textFieldWidth, Screen.height / 2.0f + 2.0f * textFieldHeight);
                }
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (canvas != null && MainCameraCache.mainCamera != null)
            {
                canvas.worldCamera = MainCameraCache.mainCamera;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void LateUpdate()
        {
            passedTime += Time.smoothDeltaTime;
            if (passedTime >= fpsUpdateTime && textField != null)
            {
                textField.text = "RenderFPS: " + Mathf.RoundToInt(1.0f / Time.smoothDeltaTime);
                passedTime = 0.0f;
            }
        }
    }
}
