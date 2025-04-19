using System;
using System.Collections.Generic;
using System.Linq;
using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rokid.UXR.Components
{
    [RequireComponent(typeof(Image))]
    public class AdsorbArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBezierForAdsorb
    {
        public class PointerDataHandle
        {
            public int pointerId;
            public bool active;
            public bool dragging;
            public bool hovering;
            public Vector3 hoverPosition;
            public PointerEventData eventData;

            public override string ToString()
            {
                return $"pointerId:{pointerId},active:{active},dragging:{dragging},hovering:{hovering},hoverPosition:{hoverPosition}\r\n";
            }
        }
        protected Dictionary<int, PointerDataHandle> pointerDataHandles = new Dictionary<int, PointerDataHandle>();

        [SerializeField]
        private Text logText;

        public static event Action<PointerEventData> OnPointerChanged;

        protected virtual void Start()
        {

        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!pointerDataHandles.ContainsKey(eventData.pointerId))
            {
                pointerDataHandles.Add(eventData.pointerId, new PointerDataHandle()
                {
                    pointerId = eventData.pointerId,
                    hovering = true,
                    active = TryGetActivePointerId(out int pointerId) == false,
                    eventData = eventData
                });
            }
            else
            {
                pointerDataHandles[eventData.pointerId].hovering = true;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (pointerDataHandles.TryGetValue(eventData.pointerId, out PointerDataHandle pointerData))
            {
                if (pointerData.active)
                {
                    if (pointerData.dragging == false)
                    {
                        pointerDataHandles.Remove(pointerData.pointerId);
                        ChangeActivePointer();
                    }
                    else
                    {
                        pointerData.hovering = false;
                    }
                }
                else
                {
                    pointerDataHandles.Remove(pointerData.pointerId);
                }
            }
        }

        public virtual Vector3 GetBezierAdsorbPoint(int pointerId)
        {
            return transform.position;
        }

        public virtual Vector3 GetBezierAdsorbNormal(int pointerId)
        {
            return transform.forward;
        }

        public virtual bool IsEnableBezierCurve(int pointerId)
        {
            if (pointerDataHandles.TryGetValue(pointerId, out PointerDataHandle pointerData))
            {
                return pointerData.active;
            }
            return false;
        }

        public virtual bool ActiveAdsorb()
        {
            return pointerDataHandles.Count > 0;
        }


        protected bool TryGetActivePointerId(out int pointerId)
        {
            pointerId = 0;
            foreach (var pointer in pointerDataHandles)
            {
                if (pointer.Value.active)
                {
                    pointerId = pointer.Value.pointerId;
                    return true;
                }
            }
            return false;
        }

        protected void ChangeActivePointer()
        {
            if (pointerDataHandles.Count > 0)
            {
                pointerDataHandles.First().Value.active = true;
                OnPointerChanged?.Invoke(pointerDataHandles.First().Value.eventData);
            }
        }

        private void OnDisable()
        {
            pointerDataHandles.Clear();
        }

        private void Update()
        {
            if (logText != null)
            {
                logText.text = "";
                foreach (PointerDataHandle pointer in pointerDataHandles.Values)
                {
                    logText.text += pointer.ToString();
                }
            }
        }
    }
}

