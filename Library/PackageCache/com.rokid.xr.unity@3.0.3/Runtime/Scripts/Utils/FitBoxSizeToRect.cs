using UnityEngine;

namespace Rokid.UXR.Utility
{
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(RectTransform))]
    public class FitBoxSizeToRect : MonoBehaviour
    {
        private BoxCollider boxCollider;
        private RectTransform targetRect;

        private void Start()
        {
            boxCollider = transform.GetComponent<BoxCollider>();
            targetRect = transform.GetComponent<RectTransform>();
        }

        private void Update()
        {
            boxCollider.size = new Vector3(targetRect.rect.width, targetRect.rect.height, 0.2f);
        }
    }
}
