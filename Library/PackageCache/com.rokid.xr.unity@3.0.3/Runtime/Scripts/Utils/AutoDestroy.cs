using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rokid.UXR.Utility
{
    public class AutoDestroy : MonoBehaviour
    {
        void Start()
        {
            Destroy(this.gameObject);
        }
    }
}
