using UnityEngine;

namespace Rokid.UXR.Utility
{
    [ExecuteAlways]
    public class UIOverlay : MonoBehaviour
    {
        /// <summary>
        ///  panel distance
        /// </summary>
        [SerializeField]
        private float panelDistance = 10;
        /// <summary>
        /// screen width
        /// </summary>
        private float width = 1920;
        /// <summary>
        /// screen heigh
        /// </summary>
        private float height = 1200;
        private Canvas canvas;
        private FollowCamera followCamera;
        private RectTransform rectTransform;
        private float prePanelDistance;

        private void Start()
        {
            SetFollowCamera();
            SetCanvasParam();
            SetRectParam();
        }

        private void SetFollowCamera()
        {
            followCamera = GetComponent<FollowCamera>();
            if (followCamera == null)
            {
                followCamera = gameObject.AddComponent<FollowCamera>();
            }
            followCamera.UpdateOffsetPosition(new Vector3(0, 0, panelDistance), true);
        }

        private void SetCanvasParam()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.WorldSpace;
        }

        private void SetRectParam()
        {
#if USE_ROKID_OPENXR
            width = 1920;
            height = 1200;
#else
            width = Utils.IsAndroidPlatform() ? Screen.width / 2 : Screen.width;
            height = Screen.height;
#endif
            rectTransform = canvas.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            rectTransform.sizeDelta = new Vector2(width, height);
            Vector3[] corners = Utils.GetCameraCorners(panelDistance);
            float uiWidth = corners[1].x - corners[0].x;
            float uiHeight = corners[0].y - corners[3].y;
            rectTransform.localScale = new Vector3(uiWidth / width, uiHeight / height, 1);
            rectTransform.localPosition = Vector3.zero;

            prePanelDistance = panelDistance;
        }

        private void Update()
        {
            if (prePanelDistance != panelDistance)
            {
                SetFollowCamera();
                SetRectParam();
            }
        }
    }
}

