using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public struct PointerEvent : IEvent
    {
        public int Identifier { get; }
        public PointerEventType Type { get; }
        public Pose Pose { get; }
        public object Data { get; }

        public PointerEvent(int identifier, PointerEventType type, Pose pose, object data = null)
        {
            Identifier = identifier;
            Type = type;
            Pose = pose;
            Data = data;
        }
    }
}
