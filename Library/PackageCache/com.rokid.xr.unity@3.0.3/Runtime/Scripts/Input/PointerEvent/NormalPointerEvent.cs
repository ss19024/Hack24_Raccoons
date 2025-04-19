using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{

    #region  Custom Pointer Interface

    /// <summary>
    /// Triggered when the ray enters
    /// </summary>
    public interface IRayPointerEnter : IEventSystemHandler
    {
        void OnRayPointerEnter(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the ray exits
    /// </summary>
    public interface IRayPointerExit : IEventSystemHandler
    {
        void OnRayPointerExit(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when mouse ray starts
    /// </summary>
    public interface IRayBeginDrag : IEventSystemHandler
    {
        void OnRayBeginDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the ray drag ends
    /// </summary>
    public interface IRayEndDrag : IEventSystemHandler
    {
        void OnRayEndDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when ray dragging
    /// </summary>
    public interface IRayDrag : IEventSystemHandler
    {
        void OnRayDrag(Vector3 delta);
    }

    /// <summary>
    /// Triggered when ray dragging use targetPoint
    /// </summary>
    public interface IRayDragToTarget : IEventSystemHandler
    {
        void OnRayDragToTarget(Vector3 targetPoint);
    }
    /// <summary>
    /// Triggered when ray hovering
    /// </summary>
    public interface IRayPointerHover : IEventSystemHandler
    {
        void OnRayPointerHover(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when ray click
    /// </summary>
    public interface IRayPointerClick : IEventSystemHandler
    {
        void OnRayPointerClick(PointerEventData eventData);
    }
    /// <summary>
    /// Triggered when ray pointer down
    /// </summary>
    public interface IRayPointerDown : IEventSystemHandler
    {
        void OnRayPointerDown(PointerEventData eventData);
    }
    /// <summary>
    /// Triggered when ray pointer up
    /// </summary>
    public interface IRayPointerUp : IEventSystemHandler
    {
        void OnRayPointerUp(PointerEventData eventData);
    }

    #endregion

    #region BezierInterface

    public interface IBezierCurveDrag
    {
        /// <summary>
        /// Whether UI dragging with Pinch gesture is supported
        /// </summary>
        /// <returns></returns>
        bool IsEnablePinchBezierCurve();

        /// <summary>
        /// Whether to support dragging objects using Grip gestures
        /// </summary>
        /// <returns></returns>
        bool IsEnableGripBezierCurve();

        /// <summary>
        /// Whether it is currently in drag state
        /// </summary>
        /// <returns></returns>
        bool IsInBezierCurveDragging();

        /// <summary>
        /// Returns the world coordinates of the drag point
        /// </summary>
        /// <returns></returns>
        Vector3 GetBezierCurveEndPoint(int pointerId);

        /// <summary>
        /// Returns the world coordinates of the drag point
        /// </summary>
        /// <returns></returns>
        Vector3 GetBezierCurveEndNormal(int pointerId);
    }


    public class BezierPointerData
    {
        public int pointerId;
        public Vector3 hitLocalPos;
        public Vector3 hitLocalNormal;
    }


    public interface IBezierForAdsorb
    {
        bool IsEnableBezierCurve(int pointerId);

        bool ActiveAdsorb();

        Vector3 GetBezierAdsorbPoint(int pointerId);

        Vector3 GetBezierAdsorbNormal(int pointerId);
    }

    #endregion

    #region UI Utils Interface
    public interface IFloatingUI
    {

    }
    #endregion
}
