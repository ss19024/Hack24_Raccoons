
using System;
namespace Rokid.UXR.Interaction
{
    public interface IPointable
    {
        event Action<PointerEvent> WhenPointerEventRaised;
    }
}
