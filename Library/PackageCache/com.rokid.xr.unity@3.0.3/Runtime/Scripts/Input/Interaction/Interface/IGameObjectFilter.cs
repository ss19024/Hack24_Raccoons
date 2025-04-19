using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public interface IGameObjectFilter
    {
        bool Filter(GameObject gameObject);
    }
}
