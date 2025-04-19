using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_2021_1_OR_NEWER
using UnityEngine.Pool;
#else
using Rokid.UXR.Utility;
#endif
using static UnityEngine.EventSystems.ExecuteEvents;

namespace Rokid.UXR.Interaction
{
    public static class RKExecuteEvents
    {
        private static readonly EventFunction<IRayPointerDown> _RayPointerDown = Execute;
        private static void Execute(IRayPointerDown handler, BaseEventData eventData)
        {
            handler.OnRayPointerDown((PointerEventData)eventData);
        }
        private static readonly EventFunction<IRayPointerUp> _RayPointerUp = Execute;
        private static void Execute(IRayPointerUp handler, BaseEventData eventData)
        {
            handler.OnRayPointerUp((PointerEventData)eventData);
        }
        public delegate void RKEventFunction<T1>(T1 handler, Vector3 delta);
        private static readonly EventFunction<IRayPointerClick> _RayPointerClick = Execute;
        private static void Execute(IRayPointerClick handler, BaseEventData eventData)
        {
            handler.OnRayPointerClick((PointerEventData)eventData);
        }
        private static readonly EventFunction<IRayBeginDrag> _RayBeginDrag = Execute;
        private static void Execute(IRayBeginDrag handler, BaseEventData eventData)
        {
            handler.OnRayBeginDrag((PointerEventData)eventData);
        }
        private static readonly RKEventFunction<IRayDrag> _RayDrag = Execute;
        private static void Execute(IRayDrag handler, Vector3 delta)
        {
            handler.OnRayDrag(delta);
        }
        private static readonly RKEventFunction<IRayDragToTarget> _RayDragToTarget = Execute;
        private static void Execute(IRayDragToTarget handler, Vector3 targetPoint)
        {
            handler.OnRayDragToTarget(targetPoint);
        }
        private static readonly EventFunction<IRayEndDrag> _RayEndDrag = Execute;
        private static void Execute(IRayEndDrag handler, BaseEventData eventData)
        {
            handler.OnRayEndDrag((PointerEventData)eventData);
        }
        private static readonly EventFunction<IRayPointerEnter> _RayPointerEnter = Execute;
        private static void Execute(IRayPointerEnter handler, BaseEventData eventData)
        {
            handler.OnRayPointerEnter((PointerEventData)eventData);
        }
        private static readonly EventFunction<IRayPointerHover> _RayPointerHover = Execute;
        private static void Execute(IRayPointerHover handler, BaseEventData eventData)
        {
            handler.OnRayPointerHover((PointerEventData)eventData);
        }
        private static readonly EventFunction<IRayPointerExit> _RayPointerExit = Execute;
        private static void Execute(IRayPointerExit handler, BaseEventData eventData)
        {
            handler.OnRayPointerExit((PointerEventData)eventData);
        }

        public static EventFunction<IRayPointerDown> rayPointerDownHandler
        {
            get { return _RayPointerDown; }
        }

        public static EventFunction<IRayPointerUp> rayPointerUpHandler
        {
            get { return _RayPointerUp; }
        }

        public static EventFunction<IRayPointerClick> rayPointerClickHandler
        {
            get { return _RayPointerClick; }
        }

        public static EventFunction<IRayBeginDrag> rayBeginDragHandler
        {
            get { return _RayBeginDrag; }
        }
        public static EventFunction<IRayEndDrag> rayEndDragHandler
        {
            get { return _RayEndDrag; }
        }
        public static RKEventFunction<IRayDrag> rayDragHandler
        {
            get { return _RayDrag; }
        }
        public static RKEventFunction<IRayDragToTarget> rayDragToTargetHandler
        {
            get { return _RayDragToTarget; }
        }
        public static EventFunction<IRayPointerEnter> rayPointerEnterHandler
        {
            get { return _RayPointerEnter; }
        }
        public static EventFunction<IRayPointerHover> rayPointerHoverHandler
        {
            get { return _RayPointerHover; }
        }
        public static EventFunction<IRayPointerExit> rayPointerExitHandler
        {
            get { return _RayPointerExit; }
        }

        private static bool ShouldSendToComponent<T>(Component component) where T : IEventSystemHandler
        {
            var valid = component is T;
            if (!valid)
                return false;

            var behaviour = component as Behaviour;
            if (behaviour != null)
                return behaviour.isActiveAndEnabled;
            return true;
        }

        /// <summary>
        /// Get the specified object's event event.
        /// </summary>
        private static void GetEventList<T>(GameObject go, IList<IEventSystemHandler> results) where T : IEventSystemHandler
        {
            // Debug.LogWarning("GetEventList<" + typeof(T).Name + ">");
            if (results == null)
                throw new ArgumentException("Results array is null", "results");

            if (go == null || !go.activeInHierarchy)
                return;

            var components = ListPool<Component>.Get();
            go.GetComponents(components);

            var componentsCount = components.Count;
            for (var i = 0; i < componentsCount; i++)
            {
                if (!ShouldSendToComponent<T>(components[i]))
                    continue;

                // Debug.Log(string.Format("{2} found! On {0}.{1}", go, s_GetComponentsScratch[i].GetType(), typeof(T)));
                results.Add(components[i] as IEventSystemHandler);
            }
            ListPool<Component>.Release(components);
            // Debug.LogWarning("end GetEventList<" + typeof(T).Name + ">");
        }

        public static bool Execute<T>(GameObject target, Vector3 data, RKEventFunction<T> functor) where T : IEventSystemHandler
        {
            var internalHandlers = ListPool<IEventSystemHandler>.Get();
            GetEventList<T>(target, internalHandlers);
            //  if (s_InternalHandlers.Count > 0)
            //      Debug.Log("Executing " + typeof (T) + " on " + target);

            var internalHandlersCount = internalHandlers.Count;
            for (var i = 0; i < internalHandlersCount; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                    continue;
                }

                try
                {
                    functor(arg, data);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            var handlerCount = internalHandlers.Count;
            ListPool<IEventSystemHandler>.Release(internalHandlers);
            return handlerCount > 0;
        }

        public static bool Execute<T>(GameObject target, BaseEventData eventData, EventFunction<T> functor) where T : IEventSystemHandler
        {
            var internalHandlers = ListPool<IEventSystemHandler>.Get();
            GetEventList<T>(target, internalHandlers);
            //  if (s_InternalHandlers.Count > 0)
            //      Debug.Log("Executing " + typeof (T) + " on " + target);

            var internalHandlersCount = internalHandlers.Count;
            for (var i = 0; i < internalHandlersCount; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                    continue;
                }

                try
                {
                    functor(arg, eventData);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var handlerCount = internalHandlers.Count;
            ListPool<IEventSystemHandler>.Release(internalHandlers);
            return handlerCount > 0;
        }
    }
}

