using System;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public interface ICustomCursorVisual
    {
        public Transform CustomCursorVisual { get; set; }

        public Pose CustomTargetPose { get; set; }

        public void CustomCursorAlpha(float alpha);

        public event Action<HandType, bool> OnCustomFocusCursorActive;
    }
}

