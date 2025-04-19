using UnityEngine;

namespace Rokid.UXR.Interaction
{

    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    public class BoxColliderSizeFitToCanvas : MonoBehaviour
    {
        [SerializeField]
        private float rectZ = 0.001f;
        [SerializeField]
        private BoxCollider boxCollider;
        [SerializeField]
        private BoxProximityField boxProximity;
        [SerializeField]
        private Canvas targetCanvas;
        private Vector3 preSize = Vector3.one;
        private Vector3 prePos = Vector3.zero;

        private void Start()
        {
            if (targetCanvas == null)
                targetCanvas = GetComponentInChildren<Canvas>();
            if (boxCollider == null)
                boxCollider = transform.GetComponent<BoxCollider>();
            if (boxProximity == null)
                boxProximity = transform.GetComponentInChildren<BoxProximityField>();
        }

        private void Update()
        {
            if (boxCollider != null && boxProximity != null && targetCanvas != null)
            {
                Vector3 size = targetCanvas.GetComponent<RectTransform>().rect.size * targetCanvas.transform.localScale;
                size[2] = rectZ;
                Vector3 position = targetCanvas.transform.position;
                if (preSize != size || prePos != position)
                {
                    preSize = size;
                    prePos = position;
                    boxCollider.size = size;
                    Transform tsf = boxProximity.transform;
                    boxProximity.transform.localScale = new Vector3(size.x, size.y, rectZ);
                    boxProximity.transform.position = targetCanvas.transform.position;
                    boxCollider.center = new Vector3(targetCanvas.transform.localPosition.x, targetCanvas.transform.localPosition.y, 0);
                }
            }
        }
    }
}
