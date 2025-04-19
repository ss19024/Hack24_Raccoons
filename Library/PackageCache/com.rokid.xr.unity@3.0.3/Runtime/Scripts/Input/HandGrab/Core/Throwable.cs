using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Rokid.UXR.Interaction
{
    public enum ReleaseStyle
    {
        NoChange,
        GetFromHand,
        ShortEstimation
    }
    [RequireComponent(typeof(GrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class Throwable : MonoBehaviour, IHandHoverBegin, IHandHoverUpdate, IHandHoverEnd, IGrabbedToHand, IReleasedFromHand, IGrabbedUpdate
    {
        [Tooltip("The flags used to attach this object to the hand.")]
        public GrabFlags grabbedFlags = GrabFlags.ParentToHand | GrabFlags.ReleaseFromOtherHand | GrabFlags.TurnOnKinematic;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held")]
        public Transform grabbedOffset;

        [Tooltip("How fast must this object be moving to attach due to a trigger hold instead of a trigger press? (-1 to disable)")]
        public float catchingSpeedThreshold = -1;
        public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;
        public float scaleReleaseVelocity = 1.1f;
        public float scaleReleaseAngularVelocity = 1.0f;

        [Tooltip("The release velocity magnitude representing the end of the scale release velocity curve. (-1 to disable)")]
        public float scaleReleaseVelocityThreshold = -1.0f;
        [Tooltip("Use this curve to ease into the scaled release velocity based on the magnitude of the measured release velocity. This allows greater differentiation between a drop, toss, and throw.")]
        public AnimationCurve scaleReleaseVelocityCurve = AnimationCurve.EaseInOut(0.0f, 0.1f, 1.0f, 1.0f);

        [Tooltip("When detaching the object, should it return to its original parent?")]
        public bool restoreOriginalParent = false;
        protected VelocityEstimator velocityEstimator;
        protected bool grabbed = false;
        protected float grabTime;
        protected Vector3 grabPosition;
        protected Quaternion grabRotation;
        protected Transform attachEaseInTransform;
        //刚体插值类型
        protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;
        protected new Rigidbody rigidbody;
        public UnityEvent OnPickUp = new UnityEvent();
        public UnityEvent OnDropDown = new UnityEvent();
        public UnityEvent OnHeldUpdate = new UnityEvent();

        [HideInInspector]
        public GrabInteractable interactable;

        private void Start()
        {
            velocityEstimator = GetComponent<VelocityEstimator>();
            interactable = GetComponent<GrabInteractable>();
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.maxAngularVelocity = 50.0f;
        }

        #region  HandEvent

        public void OnHandHoverBegin(Hand hand)
        {

        }

        public void OnHandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != GrabTypes.None)
            {
                hand.GrabObject(gameObject, startingGrabType, grabbedFlags, grabbedOffset);
            }
        }

        public void OnHandHoverEnd(Hand hand)
        {

        }

        public void OnGrabbedToHand(Hand hand)
        {
            hadInterpolation = this.rigidbody.interpolation;
            grabbed = true;
            hand.HoverLock(null);
            rigidbody.interpolation = RigidbodyInterpolation.None;
            if (velocityEstimator != null)
                velocityEstimator.BeginEstimatingVelocity();
            grabTime = Time.time;
            grabPosition = transform.position;
            grabRotation = transform.rotation;
            OnPickUp?.Invoke();
        }

        public void OnGrabbedUpdate(Hand hand)
        {
            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.ReleaseObject(gameObject, restoreOriginalParent);

                // Uncomment to detach ourselves late in the frame.
                // This is so that any vehicles the player is attached to
                // have a chance to finish updating themselves.
                // If we detach now, our position could be behind what it
                // will be at the end of the frame, and the object may appear
                // to teleport behind the hand when the player releases it.
                //StartCoroutine( LateDetach( hand ) );
            }
            OnHeldUpdate?.Invoke();
        }

        public void OnReleasedFromHand(Hand hand)
        {
            grabbed = false;
            hand.HoverUnlock(null);
            rigidbody.interpolation = hadInterpolation;
            //释放的实收设置刚体的速度和角速度
            Vector3 velocity;
            Vector3 angularVelocity;
            GetReleaseVelocities(hand, out velocity, out angularVelocity);
           
#if UNITY_6000_0_OR_NEWER
            rigidbody.linearVelocity = velocity;
#else
            rigidbody.velocity = velocity;
#endif
            rigidbody.angularVelocity = angularVelocity;
            OnDropDown?.Invoke();
        }

        #endregion

        public virtual void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
            switch (releaseVelocityStyle)
            {
                case ReleaseStyle.ShortEstimation:
                    if (velocityEstimator != null)
                    {
                        velocityEstimator.FinishEstimatingVelocity();
                        velocity = velocityEstimator.GetVelocityEstimate();
                        angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                    }
                    else
                    {
                        RKLog.Warning("[RokidVR Interaction System] Throwable: No Velocity Estimator component on object but release style set to short estimation. Please add one or change the release style.");
                        
#if UNITY_6000_0_OR_NEWER
                        velocity = rigidbody.linearVelocity;
#else
                        velocity = rigidbody.velocity;
#endif
                        angularVelocity = rigidbody.angularVelocity;
                    }
                    break;
                case ReleaseStyle.GetFromHand:
                    velocity = hand.GetTrackedObjectVelocity(releaseVelocityTimeOffset);
                    angularVelocity = hand.GetTrackedObjectAngularVelocity(releaseVelocityTimeOffset);
                    break;
                default:
                case ReleaseStyle.NoChange:
#if UNITY_6000_0_OR_NEWER
                    velocity = rigidbody.linearVelocity;
#else
                    velocity = rigidbody.velocity;
#endif
                    angularVelocity = rigidbody.angularVelocity;
                    break;
            }

            if (releaseVelocityStyle != ReleaseStyle.NoChange)
            {
                float scaleFactor = 1.0f;
                if (scaleReleaseVelocityThreshold > 0)
                {
                    scaleFactor = Mathf.Clamp01(scaleReleaseVelocityCurve.Evaluate(velocity.magnitude / scaleReleaseVelocityThreshold));
                }
                velocity *= (scaleFactor * scaleReleaseVelocity);
                angularVelocity *= scaleReleaseAngularVelocity;
            }
        }
    }
}

