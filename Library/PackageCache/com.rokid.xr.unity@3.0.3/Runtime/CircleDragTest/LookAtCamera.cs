using Rokid.UXR.Utility;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        Vector3 forward = transform.position - MainCameraCache.mainCamera.transform.position;
        forward.y = 0;
        transform.rotation = Quaternion.LookRotation(forward);
    }
}
