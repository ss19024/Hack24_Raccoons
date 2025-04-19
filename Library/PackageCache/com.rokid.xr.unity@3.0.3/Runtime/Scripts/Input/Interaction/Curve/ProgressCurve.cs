using System;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// ProgressCurve provides a helper for creating curves for easing.
    /// In some respects it works like an AnimationCurve except that ProgressCurve
    /// always takes in a normalized AnimationCurve and a second parameter
    /// defines the length of the animation.
    ///
    /// A few helper methods are provided to track progress through the animation.
    /// </summary>
    [Serializable]
    public class ProgressCurve
    {
        [SerializeField]
        private AnimationCurve _animationCurve;

        [SerializeField]
        private float _animationLength;

        private float _animationStartTime;

        public float AnimationLength => _animationLength;

        public ProgressCurve()
        {
            _animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            _animationLength = 1.0f;
        }

        public ProgressCurve(AnimationCurve animationCurve, float animationLength)
        {
            _animationCurve = animationCurve;
            _animationLength = animationLength;
        }

        public ProgressCurve(ProgressCurve other)
        {
            Copy(other);
        }

        public void Copy(ProgressCurve other)
        {
            _animationCurve = other._animationCurve;
            _animationLength = other._animationLength;
            _animationStartTime = other._animationStartTime;
        }

        public void Start()
        {
            _animationStartTime = Time.time;
        }

        public float Progress()
        {
            if (_animationLength <= 0f)
            {
                return _animationCurve.Evaluate(1.0f);
            }

            float normalizedTimeProgress = Mathf.Clamp01(ProgressTime() / _animationLength);
            return _animationCurve.Evaluate(normalizedTimeProgress);
        }

        public float ProgressIn(float time)
        {
            if (_animationLength <= 0f)
            {
                return _animationCurve.Evaluate(1.0f);
            }

            float normalizedTimeProgress = Mathf.Clamp01((ProgressTime() + time) / _animationLength);
            return _animationCurve.Evaluate(normalizedTimeProgress);
        }

        public float ProgressTime()
        {
            return Mathf.Clamp(Time.time - _animationStartTime, 0f, _animationLength);
        }

        public void End()
        {
            _animationStartTime = Time.time - _animationLength;
        }
    }
}
