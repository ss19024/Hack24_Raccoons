using UnityEngine;
using System;
using System.Collections.Generic;
using Rokid.UXR.Utility;
using UnityEngine.Rendering;

namespace Rokid.UXR.Interaction
{
    [Flags]
    public enum GrabFlags
    {
        SnapOnAttach = 1 << 0, // The object should snap to the position of the specified attachment point on the hand.
        DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
        ReleaseFromOtherHand = 1 << 2, // This object will be detached from the other hand.
        ParentToHand = 1 << 3, // The object will be parented to the hand.
        VelocityMovement = 1 << 4, // The object will attempt to move to match the position and rotation of the hand.
        TurnOnKinematic = 1 << 5, // The object will not respond to external physics.
        TurnOffGravity = 1 << 6, // The object will not respond to external physics.
    };

    public struct GrabbedObject
    {
        public GameObject grabbedObject;
        public GrabInteractable interactable;
        public Rigidbody grabbedRigidbody;
        public CollisionDetectionMode collisionDetectionMode;
        public bool grabbedRigidbodyWasKinematic;
        public bool grabbedRigidbodyUsedGravity;
        public GameObject originalParent;
        public bool isParentedToHand;
        public GrabTypes grabbedWithType;
        public GrabFlags grabFlags;
        public Vector3 initialPositionalOffset;
        public Quaternion initialRotationalOffset;
        public Transform attachedOffsetTransform;
        public Transform handAttachmentPointTransform;
        public Vector3 easeSourcePosition;
        public Quaternion easeSourceRotation;
        public float grabTime;
        public bool HasAttachFlag(GrabFlags flag)
        {
            return (grabFlags & flag) == flag;
        }
    }

    public class Hand : MonoBehaviour
    {

        #region  Event
        public static event Action<HandType, GameObject> OnHandHoverBegin;
        public static event Action<HandType, GameObject> OnHandHoverUpdate;
        public static event Action<HandType> OnHandHoverEnd;
        public static event Action<HandType, GameObject> OnGrabbedToHand;
        public static event Action<HandType, GameObject> OnGrabbedUpdate;
        public static event Action<HandType> OnReleasedFromHand;

        #endregion


        public const GrabFlags defaultGrabFlags = GrabFlags.ParentToHand |
                                                                     GrabFlags.DetachOthers |
                                                                     GrabFlags.ReleaseFromOtherHand |
                                                                     GrabFlags.TurnOnKinematic |
                                                                     GrabFlags.SnapOnAttach;
        public HandType handType;
        public Hand otherHand;
        public bool useHoverSphere;
        public float hoverUpdateInterval = 0.1f;
        public LayerMask hoverLayerMask = -1;
        public Transform hoverSphereTransform;
        public float hoverSphereRadius = 0.05f;
        private int prevOverlappingColliders = 0;
        private const int ColliderArraySize = 32;
        private Collider[] overlappingColliders;
        public bool hoverLocked { get; private set; }
        public bool hovering { get; private set; }
        /// <summary>
        /// 抓取的对象
        /// </summary>
        /// <typeparam name="GrabbedObject"></typeparam>
        /// <returns></returns>
        private List<GrabbedObject> grabbedObjects = new List<GrabbedObject>();
        private TextMesh debugText;
        public Transform objectGrabbedPoint;
        public Transform objectSnapPoint;
        private float fallbackMaxDistanceNoItem = 10.0f;
        private float fallbackMaxDistanceWithItem = 0.5f;
        private float fallbackInteractorDistance = -1.0f;
        private bool spewDebugText = false;
        private bool showDebugInteractables = false;
        public bool showDebugText = false;

        /// <summary>
        /// 当前覆盖的物体
        /// </summary>

        private GrabInteractable _hoveringInteractable;

        public GrabInteractable hoveringInteractable
        {
            get { return _hoveringInteractable; }
            set
            {
                if (_hoveringInteractable != value)
                {
                    if (_hoveringInteractable != null)
                    {
                        HandDebugLog("HoverEnd " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage(HandEventConst.OnHandHoverEnd, this, SendMessageOptions.DontRequireReceiver);
                    }
                    //Note: The _hoveringInteractable can change after sending the OnHandHoverEnd message so we need to check it again before broadcasting this message
                    if (_hoveringInteractable != null)
                    {
                        this.BroadcastMessage(HandEventConst.OnParentHoverEnd, _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has ended
                    }
                    _hoveringInteractable = value;
                    if (hoveringInteractable != null)
                    {
                        HandDebugLog("HoverBegin " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage(HandEventConst.OnHandHoverBegin, this, SendMessageOptions.DontRequireReceiver);

                        //Note: The _hoveringInteractable can change after sending the OnHandHoverBegin message so we need to check it again before broadcasting this message
                        if (_hoveringInteractable != null)
                        {
                            this.BroadcastMessage(HandEventConst.OnParentHoverBegin, _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has begun
                        }
                    }
                }
            }
        }

        public GameObject currentGrabbedObject
        {
            get
            {
                CleanUpGrabbedObjectStack();
                if (grabbedObjects.Count > 0)
                {
                    return grabbedObjects[grabbedObjects.Count - 1].grabbedObject;
                }
                return null;
            }
        }

        public GrabbedObject? currentGrabbedObjectInfo
        {
            get
            {
                CleanUpGrabbedObjectStack();
                if (grabbedObjects.Count > 0)
                {
                    return grabbedObjects[grabbedObjects.Count - 1];
                }
                return null;
            }
        }

        public void ForceHoverUnlock()
        {
            hoverLocked = false;
        }

        private void HandDebugLog(string msg)
        {
            if (spewDebugText)
            {
                RKLog.Info($"<b>[Rokid Interaction ]</b>] Hand {this.name}:{msg}");
            }
        }

        protected virtual void UpdateHovering()
        {
            if (hoverLocked) return;
            float closestDistance = float.MaxValue;
            GrabInteractable closestInteractable = null;

            if (useHoverSphere)
            {
                float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(hoverSphereTransform.transform.lossyScale.x);
                // RKLog.Info("scaleHoverRadius:" + scaledHoverRadius);
                CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractable, Color.green);
            }
            hoveringInteractable = closestInteractable;
        }

        private void CleanUpGrabbedObjectStack()
        {
            grabbedObjects.RemoveAll(l => l.grabbedObject == null);
        }

        public bool ObjectIsGrabbed(GameObject go)
        {
            for (int i = 0; i < grabbedObjects.Count; i++)
            {
                if (grabbedObjects[i].grabbedObject == go)
                    return true;
            }
            return false;
        }

        protected virtual void Start()
        {
            if (this.gameObject.layer == 0)
                RKLog.Warning("<b>[Rokid Interaction]</b> Hand is on default layer. This puts unnecessary strain on hover checks as it is always true for hand colliders (which are then ignored).");
            else
                hoverLayerMask &= ~(1 << this.gameObject.layer); //ignore self for hovering

            // allocate array for colliders
            overlappingColliders = new Collider[ColliderArraySize];

            if (showDebugText)
            {
                if (debugText == null)
                {
                    debugText = new GameObject("_debug_text").AddComponent<TextMesh>();
                    debugText.fontSize = 120;
                    debugText.characterSize = 0.001f;
                    debugText.transform.parent = transform;
                    debugText.color = Color.green;
                    debugText.transform.localScale = Vector3.one;
                    debugText.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }
                debugText.gameObject.SetActive(false);
            }
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            GesEventInput.OnTrackedSuccess += OnTrackSuccess;

            InteractorStateChange.OnHandDragStatusChanged += OnHandDragStatusChanged;
        }

        private void OnHandDragStatusChanged(HandType hand, bool dragging)
        {
            if (hand == this.handType)
            {
                hoverLocked = dragging;
            }
        }

        private void OnTrackSuccess(HandType handType)
        {
            if (handType == this.handType)
            {
                debugText?.gameObject.SetActive(true);
            }
        }

        private void OnTrackedFailed(HandType handType)
        {
            if (handType == this.handType || handType == HandType.None)
            {
                debugText?.gameObject.SetActive(false);

                while (grabbedObjects.Count > 0)
                {
                    ReleaseObject(grabbedObjects[0].grabbedObject);
                }
                OnHandHoverEnd?.Invoke(handType);
                OnReleasedFromHand?.Invoke(handType);
            }
        }

        private void OnDestroy()
        {
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
            GesEventInput.OnTrackedSuccess -= OnTrackSuccess;

            InteractorStateChange.OnHandDragStatusChanged -= OnHandDragStatusChanged;
        }

        private void OnEnable()
        {
            float hoverUpdateBegin = ((otherHand != null) && (otherHand.GetInstanceID() < GetInstanceID())) ? (0.5f * hoverUpdateInterval) : (0.0f);
            InvokeRepeating("UpdateHovering", hoverUpdateBegin, hoverUpdateInterval);
            InvokeRepeating("UpdateDebugText", hoverUpdateBegin, hoverUpdateInterval);
        }

        protected virtual void Update()
        {
            UpdateFallback();
            GameObject grabbedObject = currentGrabbedObject;
            if (grabbedObject != null)
            {
                grabbedObject.SendMessage(HandEventConst.OnGrabbedUpdate, this, SendMessageOptions.DontRequireReceiver);
                OnGrabbedUpdate?.Invoke(handType, grabbedObject);
            }

            if (hoveringInteractable != null)
            {
                if (hovering == false)
                {
                    OnHandHoverBegin?.Invoke(handType, hoveringInteractable.gameObject);
                    hovering = true;
                }
                OnHandHoverUpdate?.Invoke(handType, hoveringInteractable.gameObject);
                RKLog.KeyInfo($"====Hand==== Hovering :{hoveringInteractable.gameObject.name}");
                hoveringInteractable.SendMessage(HandEventConst.OnHandHoverUpdate, this, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if (hovering)
                {
                    OnHandHoverEnd?.Invoke(handType);
                    hovering = false;
                }
            }
        }

        private void UpdateDebugText()
        {
            if (showDebugText)
            {
                if (handType == HandType.RightHand)
                {
                    debugText.transform.localPosition = new Vector3(-0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Right;
                    debugText.anchor = TextAnchor.UpperRight;
                }
                else
                {
                    debugText.transform.localPosition = new Vector3(0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Left;
                    debugText.anchor = TextAnchor.UpperLeft;
                }

                debugText.text = string.Format(
                    "Hovering: {0}\n" +
                    "Hover Lock: {1}\n" +
                    "Grabbed: {2}\n" +
                    "Total Grabbed: {3}\n" +
                    "Type: {4}\n",
                    (hoveringInteractable ? hoveringInteractable.gameObject.name : "null"),
                    hoverLocked,
                    (currentGrabbedObject ? currentGrabbedObject.name : "null"),
                    grabbedObjects.Count,
                    handType.ToString());
            }
            else
            {
                if (debugText != null)
                {
                    Destroy(debugText.gameObject);
                }
            }
        }

        //-------------------------------------------------
        protected virtual void UpdateFallback()
        {
            if (IsUnityEditor() && GesEventInput.Instance.GetInteractorType(handType) == InteractorType.Near)
            {
                Ray ray = MainCameraCache.mainCamera.ScreenPointToRay(Input.mousePosition);

                if (grabbedObjects.Count > 0)
                {
                    // Holding down the mouse:
                    // move around a fixed distance from the camera
                    transform.position = ray.origin + fallbackInteractorDistance * ray.direction;
                }
                else
                {
                    // Not holding down the mouse:
                    // cast out a ray to see what we should mouse over

                    // Don't want to hit the hand and anything underneath it
                    // So move it back behind the camera when we do the raycast
                    Vector3 oldPosition = transform.position;
                    transform.position = MainCameraCache.mainCamera.transform.forward * (-1000.0f);

                    RaycastHit raycastHit;
                    if (Physics.Raycast(ray, out raycastHit, fallbackMaxDistanceNoItem))
                    {
                        transform.position = raycastHit.point;

                        // Remember this distance in case we click and drag the mouse
                        fallbackInteractorDistance = Mathf.Min(fallbackMaxDistanceNoItem, raycastHit.distance);
                    }
                    else if (fallbackInteractorDistance > 0.0f)
                    {
                        // Move it around at the distance we last had a hit
                        transform.position = ray.origin + Mathf.Min(fallbackMaxDistanceNoItem, fallbackInteractorDistance) * ray.direction;
                    }
                    else
                    {
                        // Didn't hit, just leave it where it was
                        transform.position = oldPosition;
                    }
                }
            }
        }

        public void Hide()
        {

        }

        public void Show()
        {

        }

        /// <summary>
        /// 释放物体
        /// </summary>
        /// <param name="objectToRelease">需要释放的对象</param>
        /// <param name="restoreOriginalParent">是否将对象设置回原来的父对象</param>
        public void ReleaseObject(GameObject objectToRelease, bool restoreOriginalParent = true)
        {
            int index = grabbedObjects.FindIndex(l => l.grabbedObject == objectToRelease);
            if (index != -1)
            {
                HandDebugLog("ReleaseObject " + objectToRelease);

                GameObject prevTopObject = currentGrabbedObject;

                // if (grabbedObjects[index].interactable != null)
                // {
                //     if (grabbedObjects[index].interactable.hideHandOnGrabbed)
                //         Show();
                // }

                Transform parentTransform = null;
                if (grabbedObjects[index].isParentedToHand)
                {
                    if (restoreOriginalParent && (grabbedObjects[index].originalParent != null))
                    {
                        parentTransform = grabbedObjects[index].originalParent.transform;
                    }

                    if (grabbedObjects[index].grabbedObject != null)
                    {
                        grabbedObjects[index].grabbedObject.transform.parent = parentTransform;
                    }
                }

                if (grabbedObjects[index].HasAttachFlag(GrabFlags.TurnOnKinematic))
                {
                    if (grabbedObjects[index].grabbedRigidbody != null)
                    {
                        grabbedObjects[index].grabbedRigidbody.isKinematic = grabbedObjects[index].grabbedRigidbodyWasKinematic;
                        grabbedObjects[index].grabbedRigidbody.collisionDetectionMode = grabbedObjects[index].collisionDetectionMode;
                    }
                }

                if (grabbedObjects[index].HasAttachFlag(GrabFlags.TurnOffGravity))
                {
                    if (grabbedObjects[index].grabbedObject != null)
                    {
                        if (grabbedObjects[index].grabbedRigidbody != null)
                            grabbedObjects[index].grabbedRigidbody.useGravity = grabbedObjects[index].grabbedRigidbodyUsedGravity;
                    }
                }

                if (grabbedObjects[index].grabbedObject != null)
                {
                    if (grabbedObjects[index].interactable == null || (grabbedObjects[index].interactable != null && grabbedObjects[index].interactable.isDestroying == false))
                        grabbedObjects[index].grabbedObject.SetActive(true);

                    grabbedObjects[index].grabbedObject.SendMessage(HandEventConst.OnReleaseFromHand, this, SendMessageOptions.DontRequireReceiver);
                    OnReleasedFromHand?.Invoke(handType);
                }

                grabbedObjects.RemoveAt(index);

                CleanUpGrabbedObjectStack();

                GameObject newTopObject = currentGrabbedObject;

                hoverLocked = false;

                //Give focus to the top most object on the stack if it changed
                if (newTopObject != null && newTopObject != prevTopObject)
                {
                    newTopObject.SetActive(true);
                    newTopObject.SendMessage(HandEventConst.OnHandFocusAcquired, this, SendMessageOptions.DontRequireReceiver);
                }
            }

            CleanUpGrabbedObjectStack();
        }

        protected virtual void FixedUpdate()
        {
            if (currentGrabbedObject != null)
            {
                GrabbedObject grabbedInfo = currentGrabbedObjectInfo.Value;
                if (grabbedInfo.grabbedObject != null)
                {
                    if (grabbedInfo.HasAttachFlag(GrabFlags.VelocityMovement))
                    {
                        if (grabbedInfo.interactable.attachEaseIn == false || grabbedInfo.interactable.snapAttachEaseInCompleted)
                            UpdateAttachedVelocity(grabbedInfo);

                        /*if (attachedInfo.interactable.handFollowTransformPosition)
                        {
                            skeleton.transform.position = TargetSkeletonPosition(attachedInfo);
                            skeleton.transform.rotation = attachedInfo.attachedObject.transform.rotation * attachedInfo.skeletonLockRotation;
                        }*/
                    }
                    else
                    {
                        if (grabbedInfo.HasAttachFlag(GrabFlags.ParentToHand))
                        {
                            grabbedInfo.grabbedObject.transform.position = TargetItemPosition(grabbedInfo);
                            grabbedInfo.grabbedObject.transform.rotation = TargetItemRotation(grabbedInfo);
                        }
                    }

                    if (grabbedInfo.interactable.attachEaseIn)
                    {
                        float t = RemapNumberClamped(Time.time, grabbedInfo.grabTime, grabbedInfo.grabTime + grabbedInfo.interactable.snapAttachEaseInTime, 0.0f, 1.0f);
                        if (t < 1.0f)
                        {
                            if (grabbedInfo.HasAttachFlag(GrabFlags.VelocityMovement))
                            {
                               
#if UNITY_6000_0_OR_NEWER
                                grabbedInfo.grabbedRigidbody.linearVelocity = Vector3.zero;
#else
                                grabbedInfo.grabbedRigidbody.velocity = Vector3.zero;
#endif
                                grabbedInfo.grabbedRigidbody.angularVelocity = Vector3.zero;
                            }
                            t = grabbedInfo.interactable.snapAttachEaseInCurve.Evaluate(t);
                            grabbedInfo.grabbedObject.transform.position = Vector3.Lerp(grabbedInfo.easeSourcePosition, TargetItemPosition(grabbedInfo), t);
                            grabbedInfo.grabbedObject.transform.rotation = Quaternion.Lerp(grabbedInfo.easeSourceRotation, TargetItemRotation(grabbedInfo), t);
                        }
                        else if (!grabbedInfo.interactable.snapAttachEaseInCompleted)
                        {
                            grabbedInfo.interactable.gameObject.SendMessage("OnThrowableAttachEaseInCompleted", this, SendMessageOptions.DontRequireReceiver);
                            grabbedInfo.interactable.snapAttachEaseInCompleted = true;
                        }
                    }
                }
            }
        }
        public static float RemapNumberClamped(float num, float low1, float high1, float low2, float high2)
        {
            return Mathf.Clamp(RemapNumber(num, low1, high1, low2, high2), Mathf.Min(low2, high2), Mathf.Max(low2, high2));
        }

        //-------------------------------------------------
        // Remap num from range 1 to range 2
        //-------------------------------------------------
        public static float RemapNumber(float num, float low1, float high1, float low2, float high2)
        {
            return low2 + (num - low1) * (high2 - low2) / (high1 - low1);
        }

        /// <summary>
        /// 抓取物体
        /// </summary>
        /// <param name="objectToGrabbed">被抓取的物体</param>
        /// <param name="grabbedWithType">抓取的类型</param>
        /// <param name="flags">抓取的flag</param>
        /// <param name="grabbedOffset">抓取的位置偏差</param>
        public void GrabObject(GameObject objectToGrabbed, GrabTypes grabbedWithType, GrabFlags flags = defaultGrabFlags, Transform grabbedOffset = null)
        {
            GrabbedObject grabObject = new GrabbedObject();
            grabObject.grabFlags = flags;
            grabObject.attachedOffsetTransform = grabbedOffset;
            grabObject.grabTime = Time.time;



            if (flags == 0)
            {
                flags = defaultGrabFlags;
            }

            //Make suer top object on stack in non-null
            CleanUpGrabbedObjectStack();

            if (ObjectIsGrabbed(objectToGrabbed))
                ReleaseObject(objectToGrabbed);

            if (grabObject.HasAttachFlag(GrabFlags.ReleaseFromOtherHand))
            {
                if (otherHand != null)
                    otherHand.ReleaseObject(objectToGrabbed);
            }

            if (grabObject.HasAttachFlag(GrabFlags.DetachOthers))
            {
                //Detach all the object from the stack 
                while (grabbedObjects.Count > 0)
                {
                    ReleaseObject(grabbedObjects[0].grabbedObject);
                }
            }

            if (currentGrabbedObject)
            {
                currentGrabbedObject.SendMessage(HandEventConst.OnHandFocusLost, this, SendMessageOptions.DontRequireReceiver);
            }

            grabObject.grabbedObject = objectToGrabbed;
            grabObject.interactable = objectToGrabbed.GetComponent<GrabInteractable>();
            grabObject.handAttachmentPointTransform = this.transform;

            if (grabObject.interactable != null)
            {
                if (grabObject.interactable.attachEaseIn)
                {
                    grabObject.easeSourcePosition = grabObject.grabbedObject.transform.position;
                    grabObject.easeSourceRotation = grabObject.grabbedObject.transform.rotation;
                    grabObject.interactable.snapAttachEaseInCompleted = false;
                }


                if (grabObject.interactable.useHandObjectAttachmentPoint)
                    grabObject.handAttachmentPointTransform = objectGrabbedPoint;

                // if (grabObject.interactable.hideHandOnGrabbed)
                //     Hide();
            }

            grabObject.originalParent = objectToGrabbed.transform.parent != null ? objectToGrabbed.transform.parent.gameObject : null;

            grabObject.grabbedRigidbody = objectToGrabbed.GetComponent<Rigidbody>();
            if (grabObject.grabbedRigidbody != null)
            {
                if (grabObject.interactable.grabbedToHand != null)//already attached to another hand
                {
                    // if it was attached to another hand ,get the flags from that hand
                    for (int attachedIndex = 0; attachedIndex < grabObject.interactable.grabbedToHand.grabbedObjects.Count; attachedIndex++)
                    {
                        GrabbedObject attachedObjectInList = grabObject.interactable.grabbedToHand.grabbedObjects[attachedIndex];
                        if (attachedObjectInList.interactable == grabObject.interactable)
                        {
                            grabObject.grabbedRigidbodyWasKinematic = attachedObjectInList.grabbedRigidbodyWasKinematic;
                            grabObject.grabbedRigidbodyUsedGravity = attachedObjectInList.grabbedRigidbodyUsedGravity;
                            grabObject.originalParent = attachedObjectInList.originalParent;
                        }
                    }
                }
                else
                {
                    grabObject.grabbedRigidbodyWasKinematic = grabObject.grabbedRigidbody.isKinematic;
                    grabObject.grabbedRigidbodyUsedGravity = grabObject.grabbedRigidbody.useGravity;
                }
            }

            grabObject.grabbedWithType = grabbedWithType;

            if (grabObject.HasAttachFlag(GrabFlags.ParentToHand))
            {
                //Parent the object to the hand
                objectToGrabbed.transform.parent = this.transform;
                grabObject.isParentedToHand = true;

                grabObject.initialPositionalOffset = grabObject.handAttachmentPointTransform.InverseTransformPoint(objectToGrabbed.transform.position);
                grabObject.initialRotationalOffset = Quaternion.Inverse(grabObject.handAttachmentPointTransform.rotation) * objectToGrabbed.transform.rotation;
            }
            else
            {
                grabObject.isParentedToHand = false;
            }

            if (grabObject.HasAttachFlag(GrabFlags.SnapOnAttach))
            {
                if (grabbedWithType == GrabTypes.Pinch)
                {
                    grabObject.handAttachmentPointTransform = objectSnapPoint;
                    grabObject.initialPositionalOffset = grabObject.handAttachmentPointTransform.InverseTransformPoint(objectToGrabbed.transform.position);
                    grabObject.initialRotationalOffset = Quaternion.Inverse(grabObject.handAttachmentPointTransform.rotation) * objectToGrabbed.transform.rotation;
                }
            }

            if (grabObject.HasAttachFlag(GrabFlags.TurnOnKinematic))
            {
                if (grabObject.grabbedRigidbody != null)
                {
                    grabObject.collisionDetectionMode = grabObject.grabbedRigidbody.collisionDetectionMode;
                    if (grabObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
                        grabObject.grabbedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                    grabObject.grabbedRigidbody.isKinematic = true;
                }
            }

            if (grabObject.HasAttachFlag(GrabFlags.TurnOffGravity))
            {
                if (grabObject.grabbedRigidbody != null)
                {
                    grabObject.grabbedRigidbody.useGravity = false;
                }
            }

            if (grabObject.interactable != null && grabObject.interactable.attachEaseIn)
            {
                grabObject.grabbedObject.transform.position = grabObject.easeSourcePosition;
                grabObject.grabbedObject.transform.rotation = grabObject.easeSourceRotation;
            }

            grabbedObjects.Add(grabObject);

            UpdateHovering();
            HandDebugLog("GrabbedToHand" + objectToGrabbed);
            objectToGrabbed.SendMessage(HandEventConst.OnGrabbedToHand, this, SendMessageOptions.DontRequireReceiver);
            OnGrabbedToHand?.Invoke(handType, objectToGrabbed);
        }

        /// <summary>
        /// 检查所有覆盖的物体
        /// </summary>
        /// <param name="hoverPosition"></param>
        /// <param name="hoverRadius"></param>
        /// <param name="closestDistance"></param>
        /// <param name="closestInteractable"></param>
        /// <param name="debugColor"></param>
        /// <returns></returns>
        protected virtual bool CheckHoveringForTransform(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref GrabInteractable closestInteractable, Color debugColor)
        {
            bool foundCloser = false;

            //null out old vals
            for (int i = 0; i < overlappingColliders.Length; i++)
            {
                overlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, hoverLayerMask.value);

            if (numColliding >= ColliderArraySize)
                RKLog.Warning("This hand is overlapping the max number of colliders");

            int iActualColliderCount = 0;

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
            {
                Collider collider = overlappingColliders[colliderIndex];

                if (collider == null)
                    continue;

                GrabInteractable contacting = collider.GetComponentInParent<GrabInteractable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Can't hover over the object if it's attached
                bool hoveringOverAttached = false;
                for (int attachedIndex = 0; attachedIndex < grabbedObjects.Count; attachedIndex++)
                {
                    if (grabbedObjects[attachedIndex].grabbedObject == contacting.gameObject)
                    {
                        hoveringOverAttached = true;
                        break;
                    }
                }

                if (hoveringOverAttached)
                    continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
                //float distance = Vector3.Distance(collider.bounds.center, hoverPosition);
                bool lowerPriority = false;
                if (closestInteractable != null)
                { // compare to closest interactable to check priority
                    lowerPriority = contacting.hoverPriority < closestInteractable.hoverPriority;
                }
                bool isCloser = (distance < closestDistance);
                if (isCloser && !lowerPriority)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                    foundCloser = true;
                }
                iActualColliderCount++;
            }

            if (showDebugInteractables && foundCloser)
            {
                Debug.DrawLine(hoverPosition, closestInteractable.transform.position, debugColor, .05f, false);
            }

            if (iActualColliderCount > 0 && iActualColliderCount != prevOverlappingColliders)
            {
                prevOverlappingColliders = iActualColliderCount;
            }
            return foundCloser;
        }

        //-------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        //
        // interactable - The Interactable to hover over indefinitely.
        //-------------------------------------------------
        public void HoverLock(GrabInteractable interactable)
        {
            HandDebugLog("HoverLock " + interactable);
            hoverLocked = true;
            hoveringInteractable = interactable;
        }

        //-------------------------------------------------
        // Stop hovering over this object indefinitely.
        //
        // interactable - The hover-locked Interactable to stop hovering over indefinitely.
        //-------------------------------------------------
        public void HoverUnlock(GrabInteractable interactable)
        {
            HandDebugLog("HoverUnlock " + interactable);
            if (hoveringInteractable == interactable)
            {
                hoverLocked = false;
            }
        }

        public bool IsGrabEnding(GameObject grabbedObject)
        {
            for (int i = 0; i < grabbedObjects.Count; i++)
            {
                if (grabbedObjects[i].grabbedObject == grabbedObject)
                {
                    return IsGrabRelease();
                }
            }
            return false;
        }

        public bool IsGrabRelease()
        {
            if (IsUnityEditor())
            {
                return !Input.GetMouseButton(0);
            }
            return GesEventInput.Instance.GetPinchDistance(handType) > 0.03f && GesEventInput.Instance.GetGestureType(handType) != GestureType.Grip;
        }

        public GrabTypes GetBestGrabbingType(GrabTypes preferred, bool forcePreference = false)
        {
            if (IsUnityEditor())
            {
                if (Input.GetMouseButton(0))
                    return preferred;
                else
                    return GrabTypes.None;
            }
            if (preferred == GrabTypes.Pinch)
            {
                if (GesEventInput.Instance.GetHandPress(handType, true))
                    return GrabTypes.Pinch;
                else if (forcePreference)
                    return GrabTypes.None;
            }
            if (preferred == GrabTypes.Grip)
            {
                if (GesEventInput.Instance.GetHandPress(handType, false))
                    return GrabTypes.Grip;
                else if (forcePreference)
                    return GrabTypes.None;
            }

            if (GesEventInput.Instance.GetHandPress(handType, true))
                return GrabTypes.Pinch;
            if (GesEventInput.Instance.GetHandPress(handType, false))
                return GrabTypes.Grip;

            return GrabTypes.None;
        }

        /// <summary>
        /// 获取开始抓取的类型
        /// </summary>
        /// <param name="explicitType"></param>
        /// <returns></returns>
        public GrabTypes GetGrabStarting(GrabTypes explicitType = GrabTypes.None)
        {
            if (explicitType != GrabTypes.None)
            {
                if (IsUnityEditor())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        return explicitType;
                    }
                    else
                    {
                        return GrabTypes.None;
                    }
                }
                else
                {
                    if (explicitType == GrabTypes.Pinch)
                        return GrabTypes.Pinch;
                    if (explicitType == GrabTypes.Grip)
                        return GrabTypes.Grip;
                }
            }
            else
            {
                if (IsUnityEditor())
                {
                    if (Input.GetMouseButtonDown(0))
                        return GrabTypes.Grip;
                    else
                        return GrabTypes.None;
                }
                else
                {
                    if (GesEventInput.Instance.GetHandDown(handType, false))
                        return GrabTypes.Grip;
                    if (GesEventInput.Instance.GetHandDown(handType, true))
                        return GrabTypes.Pinch;
                }
            }
            return GrabTypes.None;
        }

        /// <summary>
        /// 获取结束抓取的类型
        /// </summary>
        /// <param name="explicitType"></param>
        /// <returns></returns>
        public GrabTypes GetGrabEnding(GrabTypes explicitType = GrabTypes.None)
        {
            if (explicitType != GrabTypes.None)
            {
                if (IsUnityEditor())
                {
                    if (Input.GetMouseButtonUp(0))
                        return explicitType;
                    else
                        return GrabTypes.None;
                }
                else
                {
                    if (explicitType == GrabTypes.Pinch)
                        return GrabTypes.Pinch;
                    if (explicitType == GrabTypes.Grip)
                        return GrabTypes.Grip;
                }
            }
            else
            {
                if (IsUnityEditor())
                {
                    if (Input.GetMouseButtonUp(0))
                        return GrabTypes.Grip;
                    else
                        return GrabTypes.None;
                }
                else
                {
                    if (GesEventInput.Instance.GetHandUp(handType, false))
                        return GrabTypes.Grip;
                    if (GesEventInput.Instance.GetHandUp(handType, true))
                        return GrabTypes.Pinch;
                }
            }
            return GrabTypes.None;
        }

        private bool IsUnityEditor()
        {
            return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor;
        }


        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 1500f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;

        protected bool GetUpdatedAttachedVelocities(GrabbedObject grabbedObjectInfo, out Vector3 velocityTarget, out Vector3 angularTarget)
        {
            bool realNumbers = false;

            float velocityMagic = VelocityMagic;
            float angularVelocityMagic = AngularVelocityMagic;

            Vector3 targetItemPosition = TargetItemPosition(grabbedObjectInfo);
            Vector3 positionDelta = (targetItemPosition - grabbedObjectInfo.grabbedRigidbody.position);
            velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
            {
                if (IsUnityEditor())
                    velocityTarget /= 10; //hacky fix for fallback

                realNumbers = true;
            }
            else
                velocityTarget = Vector3.zero;

            Quaternion targetItemRotation = TargetItemRotation(grabbedObjectInfo);
            Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(grabbedObjectInfo.grabbedRigidbody.transform.rotation);

            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

                if (IsUnityEditor())
                    angularTarget /= 10; //hacky fix for fallback

                realNumbers &= true;
            }
            else
                angularTarget = Vector3.zero;

            // RKLog.Info($"====Hand====:  手的移动速度: {velocityTarget},手的角速度: {angularTarget}");
            return realNumbers;
        }

        protected void UpdateAttachedVelocity(GrabbedObject grabbedObjectInfo)
        {
            RKLog.Info("Update Attached Velocity");
            Vector3 velocityTarget, angularTarget;
            bool success = GetUpdatedAttachedVelocities(grabbedObjectInfo, out velocityTarget, out angularTarget);
            if (success)
            {
                float scale = Utils.GetLossyScale(currentGrabbedObjectInfo.Value.handAttachmentPointTransform);
                float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
                float maxVelocityChange = MaxVelocityChange * scale;

#if UNITY_6000_0_OR_NEWER
                grabbedObjectInfo.grabbedRigidbody.linearVelocity = Vector3.MoveTowards(grabbedObjectInfo.grabbedRigidbody.linearVelocity, velocityTarget, maxVelocityChange);
#else
                grabbedObjectInfo.grabbedRigidbody.velocity = Vector3.MoveTowards(grabbedObjectInfo.grabbedRigidbody.velocity, velocityTarget, maxVelocityChange);
#endif

               
                grabbedObjectInfo.grabbedRigidbody.angularVelocity = Vector3.MoveTowards(grabbedObjectInfo.grabbedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
            }
        }

        protected Quaternion TargetItemRotation(GrabbedObject grabbedObject)
        {
            return currentGrabbedObjectInfo.Value.handAttachmentPointTransform.rotation * grabbedObject.initialRotationalOffset;
        }

        protected Vector3 TargetItemPosition(GrabbedObject grabbedObject)
        {
            RKLog.Info("currentGrabbedObjectInfo.Value.handAttachmentPointTransform:" + currentGrabbedObjectInfo.Value.handAttachmentPointTransform);
            return currentGrabbedObjectInfo.Value.handAttachmentPointTransform.TransformPoint(grabbedObject.initialPositionalOffset);
        }

        public Vector3 GetTrackedObjectVelocity(float timeOffset = 0)
        {
            Vector3 velocityTarget, angularTarget;
            GetUpdatedAttachedVelocities(currentGrabbedObjectInfo.Value, out velocityTarget, out angularTarget);
            return velocityTarget;
        }

        public Vector3 GetTrackedObjectAngularVelocity(float timeOffset = 0)
        {
            Vector3 velocityTarget, angularTarget;
            GetUpdatedAttachedVelocities(currentGrabbedObjectInfo.Value, out velocityTarget, out angularTarget);
            return angularTarget;
        }
    }
}
