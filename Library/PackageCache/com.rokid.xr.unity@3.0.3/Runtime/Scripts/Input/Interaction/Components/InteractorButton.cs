using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class InteractorButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public enum TouchState
        {
            Normal = 1,//正常状态
            Hover = 2,//手指到达开始按钮浮起的位置
            Touch = 4,//手指触碰到浮起的
            PressDown = 8,//按下,触碰到按钮地板
        }

        public TouchState touchState
        {
            get;
            private set;
        } = TouchState.Normal;

        /// <summary>
        /// 默认抬起的高度，米
        /// </summary>
        public float normalOffsetAlongNormal = 0.00f;
        /// <summary>
        /// 被hover时自动抬起的高度，米
        /// </summary>
        public float hoverOffsetAlongNormal = 0.006f;
        /// <summary>
        /// 开始抬起时手指高度，米
        /// </summary>
        public float beginHoverOffsetAlongNormal = 0.08f;

        /// <summary>
        /// 抬起的UI图片
        /// </summary>
        public Transform buttonUp;
        public UnityEvent<TouchState> onStateChanged;
        public UnityEvent<PointerEventData> onPointerDown = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> onPointerUp = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> onPointerClick = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> onPointerEnter = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> onPointerExit = new UnityEvent<PointerEventData>();
        public HashSet<PokeInteractor> _pokeInteractors = new HashSet<PokeInteractor>();
        private PokeInteractable _pokeInteractable;

        private HashSet<PokeInteractor> hoverPokeInteractors = new HashSet<PokeInteractor>();
        private bool hoverEnter = false;
        private bool pointerDown = false;

        private PokeInteractable pokeInteractable
        {
            get
            {
                if (_pokeInteractable == null)
                {
                    _pokeInteractable = transform.GetComponentInParent<PokeInteractable>();
                    if (_pokeInteractable == null)
                    {
                        enabled = false;
                    }
                }

                return _pokeInteractable;
            }
        }
        void Awake()
        {
            if (!buttonUp)
            {
                enabled = false;
            }
            buttonUp.position = transform.position - transform.forward * normalOffsetAlongNormal;
        }

        private void OnEnable()
        {
            hoverEnter = false;
            _pokeInteractors.Clear();
            hoverPokeInteractors.Clear();
            _pokeInteractors.UnionWith(pokeInteractable.Interactors);
            pokeInteractable.WhenInteractorAdded.Action += HandleInteractorAdded;
            pokeInteractable.WhenInteractorRemoved.Action += HandleInteractorRemoved;
        }

        private void OnDisable()
        {
            OnPointerExit(null);
            pokeInteractable.WhenInteractorAdded.Action -= HandleInteractorAdded;
            pokeInteractable.WhenInteractorRemoved.Action -= HandleInteractorRemoved;
            _pokeInteractors.Clear();
            hoverPokeInteractors.Clear();
        }

        private void HandleInteractorAdded(PokeInteractor pokeInteractor)
        {
            _pokeInteractors.Add(pokeInteractor);
        }
        private void HandleInteractorRemoved(PokeInteractor pokeInteractor)
        {
            pokeInteractor.InteractorButtonUpPosition = Vector3.zero;
            _pokeInteractors.Remove(pokeInteractor);
        }


        private void Update()
        {
            if (hoverEnter)
            {
                if (hoverPokeInteractors.Count > 0)
                {
                    float closestDistance = int.MaxValue;
                    foreach (var v in hoverPokeInteractors)
                    {
                        float dis = Vector3.Dot(transform.position - v.transform.position, transform.forward);
                        closestDistance = closestDistance < dis ? closestDistance : dis;
                    }

                    closestDistance = closestDistance < 0 ? 0 : closestDistance;

                    if (closestDistance > beginHoverOffsetAlongNormal)
                    {
                        buttonUp.position = Vector3.Lerp(buttonUp.position, transform.position - transform.forward * normalOffsetAlongNormal, 20 * Time.deltaTime);
                    }
                    else if (closestDistance > hoverOffsetAlongNormal)
                    {
                        buttonUp.position = Vector3.Lerp(buttonUp.position, transform.position - transform.forward * hoverOffsetAlongNormal, 30 * Time.deltaTime);
                    }
                    else
                    {
                        buttonUp.position = transform.position -
                                            transform.forward * closestDistance;
                    }

                    foreach (var item in hoverPokeInteractors)
                    {
                        item.InteractorButtonUpPosition = buttonUp.position;
                    }

                    ChangeState(closestDistance);
                }
            }
        }

        void ChangeState(float closestDistance)
        {
            TouchState curState;
            if (closestDistance > beginHoverOffsetAlongNormal)
            {
                curState = TouchState.Normal;
            }
            else if (closestDistance > hoverOffsetAlongNormal)
            {
                curState = TouchState.Hover;
            }
            else if (closestDistance > 0.001)
            {
                curState = TouchState.Touch;
            }
            else
            {
                curState = TouchState.PressDown;
            }

            if (curState != touchState)
            {
                touchState = curState;
                onStateChanged?.Invoke(curState);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // RKLog.Info($"====InteractorButton==== P {eventData?.currentInputModule?.GetType().Name} pointerDown");
            if (_pokeInteractors.Count == 0)
            {
                buttonUp.position = transform.position;
            }
            onPointerDown?.Invoke(eventData);
            pointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // RKLog.Info($"====InteractorButton==== P {eventData?.currentInputModule?.GetType().Name} pointerUp");
            if (_pokeInteractors.Count == 0)
            {
                buttonUp.position = transform.position -
                                    transform.forward * normalOffsetAlongNormal;
            }
            onPointerUp?.Invoke(eventData);
            pointerDown = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // RKLog.Info($"====InteractorButton==== E {eventData?.currentInputModule?.GetType().Name} pointerEnter");
            foreach (var v in _pokeInteractors.ToArray())
            {
                if (eventData != null && v.realId == eventData.pointerId)
                {
                    hoverPokeInteractors.Add(v);
                }
            }

            if (hoverEnter)
            {
                return;
            }
            hoverEnter = true;
            onPointerEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // RKLog.Info($"====InteractorButton==== E {eventData?.currentInputModule?.GetType().Name} pointerExit");
            foreach (var v in hoverPokeInteractors.ToArray())
            {

                if (eventData != null && v.realId == eventData.pointerId)
                {
                    hoverPokeInteractors.Remove(v);
                }
            }

            hoverEnter = hoverPokeInteractors.Count > 0;
            if (hoverEnter)
            {
                return;
            }

            buttonUp.position = transform.position -
                                transform.forward * normalOffsetAlongNormal;
            if (touchState != TouchState.Normal)
            {
                touchState = TouchState.Normal;
                onStateChanged?.Invoke(touchState);
            }

            foreach (var item in _pokeInteractors)
            {
                item.InteractorButtonUpPosition = Vector3.zero;
            }
            onPointerExit?.Invoke(eventData);
            pointerDown = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onPointerClick?.Invoke(eventData);
        }
    }
}
