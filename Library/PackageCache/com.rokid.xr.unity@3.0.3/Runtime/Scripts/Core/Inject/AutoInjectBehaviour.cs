using UnityEngine;

namespace Rokid.UXR
{
    public class AutoInjectBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            AutoInjectComponent.AutoInject(transform, this);
        }
    }

}
