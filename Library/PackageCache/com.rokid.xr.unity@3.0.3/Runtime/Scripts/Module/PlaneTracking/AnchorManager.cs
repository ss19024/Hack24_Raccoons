using Rokid.UXR.Interaction;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnchorManager : MonoBehaviour
{

    [SerializeField, Tooltip("Anchor point prefabricated body")]
    private GameObject anchorPrefab;
    [SerializeField, Tooltip("Follow plane or not")]
    private bool followPlane;

    private void Start()
    {
        RKPointerListener.OnPhysicalPointerDown += OnPointerDown;
        if (anchorPrefab != null)
        {
            anchorPrefab.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        RKPointerListener.OnPhysicalPointerDown -= OnPointerDown;
    }
    private void OnPointerDown(PointerEventData data)
    {
        ARPlane plane = data.pointerCurrentRaycast.gameObject.GetComponent<ARPlane>();
        if (data.pointerCurrentRaycast.gameObject != null && plane)
        {
            GameObject anchor = GameObject.Instantiate(anchorPrefab);
            anchor.gameObject.SetActive(true);
            if (followPlane)
            {
                anchor.transform.parent = plane.transform;
            }
            else
            {
                anchor.transform.parent = transform;
            }
            anchor.transform.SetPose(new Pose(data.pointerCurrentRaycast.worldPosition, data.pointerCurrentRaycast.gameObject.transform.rotation));
        }
    }
}
