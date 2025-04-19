using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rokid.UXR.Interaction
{

    /// <summary>
    /// Defines a near-poke interaction that is driven by a near-distance
    /// proximity computation and a raycast between the position
    /// recorded across two frames against a target surface.
    /// 定义由近距离接近计算和跨两帧记录的位置与目标表面之间的光线投射驱动的近戳交互。
    /// </summary>
    public class PokeInteractor : PointerInteractor<PokeInteractor, PokeInteractable>
    {
        [SerializeField]
        private HandType hand = HandType.None;
        [SerializeField]
        // 戳原点跟踪提供的变换
        [Tooltip("The poke origin tracks the provided transform.")]
        private Transform _pointTransform;

        [SerializeField, Optional]
        private Transform _surfaceHitPoint;

        [SerializeField]
        // (米，世界）定位在原点的球体的半径
        [Tooltip("(Meters, World) The radius of the sphere positioned at the origin.")]
        private float _radius = 0.005f;

        [SerializeField]
        // (米, 世界) 当 poke 原点超过此值时，将触发 poke unselect
        [Tooltip("(Meters, World) A poke unselect fires when the poke origin surpasses this " +
                 "distance above a surface.")]
        private float _touchReleaseThreshold = 0.002f;

        // [FormerlySerializedAs("_zThreshold")]
        [SerializeField]
        //（米，世界）到表面的距离低于该阈值
        [Tooltip("(Meters, World) The threshold below which distances to a surface " +
                 "are treated as equal for the purposes of ranking.")]
        private float _equalDistanceThreshold = 0.001f;
        private Vector3 ClosestPoint;
        public Vector3 TouchPoint;
        public Vector3 TouchNormal { get; private set; }
        [HideInInspector]
        public Vector3 InteractorButtonUpPosition;
        public float Radius => _radius;

        public Vector3 Origin { get; private set; }

        private Vector3 _previousPokeOrigin;

        /// <summary>
        /// 前一个候选者
        /// </summary>
        private PokeInteractable _previousCandidate = null;
        /// <summary>
        /// 点击可交互
        /// </summary>
        private PokeInteractable _hitInteractable = null;

        private Vector3 _previousSurfacePointLocal;
        private Vector3 _firstTouchPointLocal;
        private Vector3 _targetTouchPointLocal;
        private Vector3 _easeTouchPointLocal;

        private bool _isDragging;
        private ProgressCurve _dragEaseCurve;
        private Vector3 _dragCompareSurfacePointLocal;
        private float _maxDistanceFromFirstTouchPoint;

        private Dictionary<PokeInteractable, Matrix4x4> _previousSurfaceTransformMap;
        private float _previousProgress;



        protected override void Start()
        {
            base.Start();
            Assert.IsNotNull(_pointTransform);
            _dragEaseCurve = new ProgressCurve();
            _previousSurfaceTransformMap = new Dictionary<PokeInteractable, Matrix4x4>();
        }


        protected override void DoPreprocess()
        {
            base.DoPreprocess();
            _previousPokeOrigin = Origin;
            Origin = _pointTransform.position;
        }

        protected override void DoPostprocess()
        {
            base.DoPostprocess();
            var interactables = PokeInteractable.Registry.List(this);
            foreach (PokeInteractable interactable in interactables)
            {
                _previousSurfaceTransformMap[interactable] =
                    interactable.Surface.Transform.worldToLocalMatrix;
            }
        }

        protected override bool ComputeShouldSelect()
        {
            return _hitInteractable != null;
        }

        protected override bool ComputeShouldUnselect()
        {
            return _hitInteractable == null;
        }

        protected override void DoHoverUpdate()
        {
            if (_interactable != null)
            {
                TouchPoint = _interactable.ComputeClosestPoint(Origin);
                TouchNormal = _interactable.ClosestSurfaceNormal(TouchPoint);
            }
        }

        public HandType GetHandType()
        {
            return hand;
        }

        protected override PokeInteractable ComputeCandidate()
        {
            if (_hitInteractable != null)
            {
                return _hitInteractable;
            }

            // First, see if we trigger a press on any interactable
            PokeInteractable closestInteractable = ComputeSelectCandidate();
            if (closestInteractable != null)
            {
                // We have found an active hit target, so we return it
                _hitInteractable = closestInteractable;
                _previousCandidate = closestInteractable;
                return _hitInteractable;
            }

            // Otherwise we have no active interactable, so we do a proximity-only check for
            // closest hovered interactable (above the surface)
            // 计算获取最优的Hover可交互物体
            closestInteractable = ComputeBestHoverInteractable();
            _previousCandidate = closestInteractable;

            return closestInteractable;
        }

        private PokeInteractable ComputeSelectCandidate()
        {
            PokeInteractable closestInteractable = null;
            float closestDist = float.MaxValue;
            float minNormalProject = float.MaxValue;

            var interactables = PokeInteractable.Registry.List(this);

            // Check the surface first as a movement through this will
            // automatically put us in a "active" state. We expect the raycast
            // to happen only in one direction
            foreach (PokeInteractable interactable in interactables)
            {
                Matrix4x4 previousSurfaceMatrix =
                    _previousSurfaceTransformMap.ContainsKey(interactable)
                        ? _previousSurfaceTransformMap[interactable]
                        : interactable.Surface.Transform.worldToLocalMatrix;

                Vector3 localPokeOrigin = previousSurfaceMatrix.MultiplyPoint(_previousPokeOrigin);
                Vector3 adjustedPokeOrigin =
                    interactable.Surface.Transform.TransformPoint(localPokeOrigin);

                if (!PassesEnterHoverDistanceCheck(adjustedPokeOrigin, interactable))
                {
                    continue;
                }

                Vector3 moveDirection = Origin - adjustedPokeOrigin;
                float magnitude = moveDirection.magnitude;
                if (magnitude == 0f)
                {
                    return null;
                }

                moveDirection /= magnitude;
                Ray ray = new Ray(adjustedPokeOrigin, moveDirection);
                //TODO Poke 碰撞计算的关键逻辑
                UnityEngine.Debug.DrawLine(adjustedPokeOrigin, adjustedPokeOrigin + moveDirection * 10f, Color.green);

                Vector3 closestSurfaceNormal = interactable.ClosestSurfaceNormal(Origin);

                // First check that we are moving towards the surface by checking
                // the direction of our position delta with the forward direction of the surface normal.
                // This is to not allow presses from "behind" the surface.

                // Check if we are moving toward the surface
                if (Vector3.Dot(moveDirection, closestSurfaceNormal) < 0f)
                {
                    // Then do a raycast against the surface
                    bool hit = interactable.Surface.Raycast(ray, out SurfaceHit surfaceHit);
                    hit = hit && surfaceHit.Distance <= magnitude;

                    if (!hit)
                    {
                        // We may still be touching the surface within our radius
                        float distance = ComputeDistanceAbove(interactable, Origin);
                        if (distance <= 0)
                        {
                            Vector3 closestSurfacePointToOrigin = interactable.ClosestSurfacePoint(Origin);
                            hit = true;
                            surfaceHit = new SurfaceHit()
                            {
                                Point = closestSurfacePointToOrigin,
                                Normal = interactable.ClosestSurfaceNormal(Origin),
                                Distance = distance
                            };
                        }
                    }

                    if (hit)
                    {
                        if (_surfaceHitPoint)
                        {
                            _surfaceHitPoint.position = surfaceHit.Point;
                        }
                        // Check if our collision lies outside of the optional volume mask
                        if (interactable.VolumeMask != null &&
                            !Collisions.IsPointWithinCollider(surfaceHit.Point, interactable.VolumeMask))
                        {
                            continue;
                        }

                        float distanceFromEdge =
                            ComputeDistanceFrom(interactable, surfaceHit.Point);

                        // Check if our collision lies outside of the max distance in the proximityfield
                        if (distanceFromEdge > 0.01f)
                        {
                            continue;
                        }

                        // We collided against the surface and now we must rank this
                        // interactable versus others that also pass this test this frame.

                        // First we rank by normal distance traveled,
                        // and secondly by closer proximity
                        float normalProjection = Vector3.Dot(adjustedPokeOrigin - surfaceHit.Point, surfaceHit.Normal);
                        bool normalDistanceEqual = Mathf.Abs(normalProjection - minNormalProject) < _equalDistanceThreshold;
                        bool checkEdgeDistance = !normalDistanceEqual ||
                                                 interactable.TiebreakerScore ==
                                                 closestInteractable.TiebreakerScore;
                        // Check if the point is either closer along the normal or
                        // the normal delta with the best point so far is within the zThreshold and
                        // is closer to the surface intersection point
                        if ((!normalDistanceEqual && normalProjection < minNormalProject) ||
                            (normalDistanceEqual && interactable.TiebreakerScore > closestInteractable.TiebreakerScore) ||
                            (checkEdgeDistance && distanceFromEdge < closestDist))
                        {
                            minNormalProject = normalProjection;
                            closestDist = distanceFromEdge;
                            closestInteractable = interactable;
                        }
                    }
                }
            }

            if (closestInteractable != null)
            {
                ClosestPoint = closestInteractable.ComputeClosestPoint(Origin);
                TouchPoint = ClosestPoint;
                TouchNormal = closestInteractable.ClosestSurfaceNormal(TouchPoint);
            }
            else
            {
                TouchPoint = ClosestPoint = Vector3.zero;
            }
            return closestInteractable;
        }

        private bool PassesEnterHoverDistanceCheck(Vector3 position, PokeInteractable interactable)
        {
            if (interactable == _previousCandidate)
            {
                return true;
            }

            return ComputeDistanceAbove(interactable, position) > interactable.EnterHoverDistance;
        }

        private PokeInteractable ComputeBestHoverInteractable()
        {
            PokeInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            var interactables = PokeInteractable.Registry.List(this);

            int interactableCount = 0;

            // We check that we're above the surface first as we don't
            // care about hovers that originate below the surface
            foreach (PokeInteractable interactable in interactables)
            {
                interactableCount++;
                // Hover if between EnterHover and MaxDistance
                // Or if above EnterHover last frame and within MaxDistance this frame:
                // eg. if EnterHover and MaxDistance are the same, still want to hover in one frame
                if (!PassesEnterHoverDistanceCheck(Origin, interactable) &&
                    !PassesEnterHoverDistanceCheck(_previousPokeOrigin, interactable))
                {
                    continue;
                }

                Vector3 closestSurfacePoint = interactable.ClosestSurfacePoint(Origin);
                Vector3 closestSurfaceNormal = interactable.ClosestSurfaceNormal(Origin);

                Vector3 surfaceToPoint = Origin - closestSurfacePoint;
                float magnitude = surfaceToPoint.magnitude;
                if (magnitude != 0f)
                {
                    // Check if our position is above the surface
                    if (Vector3.Dot(surfaceToPoint, closestSurfaceNormal) > 0f)
                    {
                        // Check if our position lies outside of the optional volume mask
                        if (interactable.VolumeMask != null &&
                            !Collisions.IsPointWithinCollider(Origin, interactable.VolumeMask))
                        {
                            continue;
                        }

                        // We're above the surface so now we must rank this
                        // interactable versus others that also pass this test this frame
                        // but may be at a closer proximity.
                        float distanceFromSurfacePoint = ComputeDistanceFrom(interactable, Origin);
                        if (distanceFromSurfacePoint > interactable.MaxDistance)
                        {
                            continue;
                        }

                        Ray ray = new Ray(Origin, interactable.ClosestSurfacePoint(Origin) - Origin);
                        Debug.DrawLine(Origin, interactable.ClosestSurfacePoint(Origin), Color.yellow);
                        bool hit = interactable.ColliderSurface.Raycast(ray, out SurfaceHit testHit, 1000);
                        if (!hit)
                        {
                            // RKLog.Info("====Interaction==== filter use ray ");
                            continue;
                        }

                        if (distanceFromSurfacePoint < closestDistance ||
                            Mathf.Abs(distanceFromSurfacePoint - closestDistance) < _equalDistanceThreshold
                            && interactable.TiebreakerScore > closestInteractable.TiebreakerScore)
                        {
                            closestDistance = distanceFromSurfacePoint;
                            closestInteractable = interactable;
                        }
                    }
                }
            }

            // RKLog.Debug("====Interaction==== interactables.Count :" + interactableCount);

            if (closestInteractable != null)
            {
                ClosestPoint = closestInteractable.ComputeClosestPoint(Origin);
                TouchPoint = ClosestPoint;
                TouchNormal = closestInteractable.ClosestSurfaceNormal(TouchPoint);
            }

            return closestInteractable;
        }

        protected override void InteractableSelected(PokeInteractable interactable)
        {
            if (interactable != null)
            {
                _previousSurfacePointLocal =
                _firstTouchPointLocal =
                _easeTouchPointLocal =
                _targetTouchPointLocal =
                interactable.Surface.Transform.InverseTransformPoint(TouchPoint);

                Vector3 lateralComparePoint = interactable.ClosestSurfacePoint(Origin);
                _dragCompareSurfacePointLocal = interactable.Surface.Transform.InverseTransformPoint(lateralComparePoint);
                _dragEaseCurve.Copy(interactable.DragThresholding.DragEaseCurve);
                _isDragging = false;

                _maxDistanceFromFirstTouchPoint = 0;
            }

            base.InteractableSelected(interactable);
        }

        protected override void HandleDisabled()
        {
            _hitInteractable = null;
            base.HandleDisabled();
        }

        protected override Pose ComputePointerPose()
        {
            if (Interactable == null)
            {
                return Pose.identity;
            }

            return new Pose(
                TouchPoint,
                Quaternion.LookRotation(Interactable.ClosestSurfaceNormal(TouchPoint))
            );
        }

        // The distance above a surface along the closest normal.
        // Returns 0 for where the sphere touches the surface along the normal.
        private float ComputeDistanceAbove(PokeInteractable interactable, Vector3 point)
        {
            //TODO Key Point 计算Hover的距离
            Vector3 closestSurfacePoint = interactable.ClosestSurfacePoint(point);
            Vector3 closestSurfaceNormal = interactable.ClosestSurfaceNormal(point);
            Vector3 surfaceToPoint = point - closestSurfacePoint;
            return Vector3.Dot(surfaceToPoint, closestSurfaceNormal) - _radius;
        }

        // The distance below a surface along the closest normal. Always positive.
        private float ComputeDepth(PokeInteractable interactable, Vector3 point)
        {
            return Mathf.Max(0f, -ComputeDistanceAbove(interactable, point));
        }

        // The distance from the closest point as computed by the proximity field and surface.
        // Returns the distance to the point without taking into account the surface normal.
        private float ComputeDistanceFrom(PokeInteractable interactable, Vector3 point)
        {
            Vector3 closestSurfacePoint = interactable.ComputeClosestPoint(point);
            Vector3 surfaceToPoint = point - closestSurfacePoint;
            return surfaceToPoint.magnitude - _radius;
        }

        protected override void DoSelectUpdate()
        {
            PokeInteractable interactable = _selectedInteractable;
            if (interactable == null)
            {
                _hitInteractable = null;
                return;
            }

            // Unselect if the interactor is above the surface by at least _touchReleaseThreshold
            if (ComputeDistanceAbove(interactable, Origin) > _touchReleaseThreshold)
            {
                _hitInteractable = null;
                return;
            }

            Vector3 closestSurfacePointWorld = interactable.ClosestSurfacePoint(Origin);

            Vector3 positionOnSurfaceLocal =
                interactable.Surface.Transform.InverseTransformPoint(closestSurfacePointWorld);

            if (interactable.DragThresholding.Enabled)
            {
                float worldDepthDelta = Mathf.Abs(ComputeDepth(interactable, Origin) -
                                              ComputeDepth(interactable, _previousPokeOrigin));
                Vector3 positionDeltaLocal = positionOnSurfaceLocal - _previousSurfacePointLocal;
                Vector3 positionDeltaWorld =
                    interactable.Surface.Transform.TransformVector(positionDeltaLocal);

                bool isZMotion = worldDepthDelta > positionDeltaWorld.magnitude &&
                                 worldDepthDelta > interactable.DragThresholding.ZThreshold;

                if (isZMotion)
                {
                    _dragCompareSurfacePointLocal = positionOnSurfaceLocal;
                }

                if (!_isDragging)
                {
                    if (!isZMotion)
                    {
                        Vector3 surfaceDeltaLocal =
                            positionOnSurfaceLocal - _dragCompareSurfacePointLocal;
                        Vector3 surfaceDeltaWorld =
                            interactable.Surface.Transform.TransformVector(surfaceDeltaLocal);
                        if (surfaceDeltaWorld.magnitude >
                            interactable.DragThresholding.SurfaceThreshold)
                        {
                            _isDragging = true;
                            _dragEaseCurve.Start();
                            _previousProgress = 0;
                            _targetTouchPointLocal = positionOnSurfaceLocal;
                        }
                    }
                }
                else
                {
                    if (isZMotion)
                    {
                        _isDragging = false;
                    }
                    else
                    {
                        _targetTouchPointLocal = positionOnSurfaceLocal;
                    }
                }
            }
            else
            {
                _targetTouchPointLocal = positionOnSurfaceLocal;
            }

            Vector3 pinnedTouchPointLocal = _targetTouchPointLocal;
            if (SelectedInteractable.PositionPinning.Enabled)
            {
                Vector3 deltaFromCaptureLocal = pinnedTouchPointLocal - _firstTouchPointLocal;
                Vector3 deltaFromCaptureWorld =
                    interactable.Surface.Transform.TransformVector(deltaFromCaptureLocal);
                _maxDistanceFromFirstTouchPoint = Mathf.Max(deltaFromCaptureWorld.magnitude, _maxDistanceFromFirstTouchPoint);

                float deltaAsPercent = 1;
                if (SelectedInteractable.PositionPinning.MaxPinDistance != 0f)
                {
                    deltaAsPercent = Mathf.Clamp01(_maxDistanceFromFirstTouchPoint / SelectedInteractable.PositionPinning.MaxPinDistance);
                }

                pinnedTouchPointLocal = _firstTouchPointLocal + deltaFromCaptureLocal * deltaAsPercent;
            }

            float progress = _dragEaseCurve.Progress();
            if (progress != 1f)
            {
                float deltaProgress = progress - _previousProgress;

                Vector3 delta = pinnedTouchPointLocal - _easeTouchPointLocal;
                _easeTouchPointLocal += deltaProgress / (1f - _previousProgress) * delta;
                _previousProgress = progress;
            }
            else
            {
                _easeTouchPointLocal = pinnedTouchPointLocal;
            }

            TouchPoint =
                interactable.Surface.Transform.TransformPoint(_easeTouchPointLocal);
            TouchNormal = interactable.ClosestSurfaceNormal(TouchPoint);

            _previousSurfacePointLocal = positionOnSurfaceLocal;

            if (interactable.ReleaseDistance > 0.0f)
            {
                if (ComputeDistanceFrom(interactable, Origin) > interactable.ReleaseDistance)
                {
                    GeneratePointerEvent(PointerEventType.Cancel, interactable, idIndex);
                    _previousPokeOrigin = Origin;
                    _previousCandidate = null;
                    _hitInteractable = null;
                }
            }
        }
    }
}
