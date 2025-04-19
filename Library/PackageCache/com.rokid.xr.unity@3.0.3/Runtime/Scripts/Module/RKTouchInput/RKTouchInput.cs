using System;
using System.Collections.Generic;
using Rokid.UXR.Interaction;
using UnityEngine;

namespace Rokid.UXR.Module
{
    public enum FingerOperation
    {
        None,
        OneFinger,
        TwoFinger,
        ThreeFinger
    }

    public class RKTouchInput : MonoSingleton<RKTouchInput>
    {

        /// <summary>
        /// When use inside touch please register this event
        /// </summary>
        public static event Action OnUpdate;
        public static event Action<FingerOperation, FingerOperation> OnFingerOperationChanged;
        private FingerOperation fingerOperation = FingerOperation.None;
        public FingerOperation FingerOperation { get { return fingerOperation; } }
        private FingerOperation oldFingerOperation = FingerOperation.None;
        private float elapsedTime = 0;
        private int oldOperationFingerCount = 0;
        private struct InsideTouch
        {
            public Touch touch;
            public override string ToString()
            {
                return TouchToString(touch);
            }
        }

        private List<InsideTouch> insideTouchList = new List<InsideTouch>();
        private List<InsideTouch> clearTouchList = new List<InsideTouch>();

        internal void Init()
        {

        }

        private bool inputLock;

        public void Lock(bool isLock)
        {
            this.inputLock = isLock;
        }

        protected override void OnSingletonInit()
        {
            UnityPlayerAPI.Instance.SetSystemScreenOrientation(ScreenOrientation.Portrait);
            this.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        public bool TryGetSmoothInsideTouchDeltaPosition(int index, out Vector2 delta, bool useMovedTouchFirst = true)
        {
            delta = Vector2.zero;
            if (index >= GetInsideTouchCount())
            {
                return false;
            }
            if (useMovedTouchFirst)
            {
                switch (GetInsideTouchWithTargetPhase(index, TouchPhase.Moved).phase)
                {
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                        delta = GetInsideTouchWithTargetPhase(index, TouchPhase.Moved).deltaPosition;
                        break;
                }
            }
            else
            {
                switch (GetInsideTouch(index).phase)
                {
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                        delta = GetInsideTouch(index).deltaPosition;
                        break;
                }
            }
            //处理翻转
            switch (UnityPlayerAPI.Instance.GetUnityScreenOrientation())
            {
                case ScreenOrientation.LandscapeLeft:
                    delta = new Vector2(-delta.y, delta.x);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    delta = -delta;
                    break;
                case ScreenOrientation.LandscapeRight:
                    delta = new Vector2(delta.y, -delta.x);
                    break;
            }
            return true;
        }

        public bool TryGetSmoothInsideTouchDeltaPosition(int index, out Vector2 delta, ref Vector2 preDelta, ref int stationaryCount, ref int xInverseCount, ref int yInverseCount, int stationaryThreshold = 1, int inverseThreshold = 0, float inverseLengthDistance = 0.0f, bool useMovedTouchFirst = true)
        {
            delta = Vector2.zero;
            if (index >= GetInsideTouchCount())
            {
                return false;
            }
            Touch touch = useMovedTouchFirst ? GetInsideTouchWithTargetPhase(index, TouchPhase.Moved) : GetInsideTouch(index);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    stationaryCount = 0;
                    xInverseCount = 0;
                    yInverseCount = 0;
                    preDelta = Vector2.zero;
                    break;
                case TouchPhase.Stationary:
                    stationaryCount++;
                    if (stationaryCount > stationaryThreshold)
                    {
                        preDelta = touch.deltaPosition;
                    }
                    break;
                case TouchPhase.Moved:
                    stationaryCount = 0;
                    float xResult = preDelta.x * touch.deltaPosition.x;
                    if (xResult >= 0)
                    {
                        xInverseCount = 0;
                        preDelta.x = touch.deltaPosition.x;
                    }
                    else
                    {
                        xInverseCount++;
                        if (xInverseCount > inverseThreshold || Mathf.Abs(preDelta.x) > inverseLengthDistance)
                        {
                            // RKLog.KeyInfo($"====RKTouchInput==== Trigger X Inverse {xInverseCount},{inverseThreshold},{Mathf.Abs(preDelta.x)}{inverseLengthDistance}");
                            preDelta.x = touch.deltaPosition.x;
                        }
                    }

                    float yResult = preDelta.y * touch.deltaPosition.y;
                    if (yResult >= 0)
                    {
                        yInverseCount = 0;
                        preDelta.y = touch.deltaPosition.y;
                    }
                    else
                    {
                        yInverseCount++;
                        if (yInverseCount > inverseThreshold || Mathf.Abs(preDelta.y) > inverseLengthDistance)
                        {
                            // RKLog.KeyInfo($"====RKTouchInput==== Trigger Y Inverse {yInverseCount},{inverseThreshold},{Mathf.Abs(preDelta.y)},{inverseLengthDistance}");
                            preDelta.y = touch.deltaPosition.y;
                        }
                    }
                    break;
            }
            delta = preDelta;
            // RKLog.KeyInfo($"====RKTouchInput==== delta:{delta.ToString("0.00000")}, touchPhase:{touch.phase}");
            //处理翻转
            switch (UnityPlayerAPI.Instance.GetUnityScreenOrientation())
            {
                case ScreenOrientation.LandscapeLeft:
                    delta = new Vector2(-delta.y, delta.x);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    delta = -delta;
                    break;
                case ScreenOrientation.LandscapeRight:
                    delta = new Vector2(delta.y, -delta.x);
                    break;
            }
            return true;
        }

        public Vector2 GetInsideTouchDeltaPosition(int index, bool useMovedTouchFirst = true)
        {
            if (index >= GetInsideTouchCount())
                return Vector2.zero;
            Vector2 delta = useMovedTouchFirst ? GetInsideTouchWithTargetPhase(index, TouchPhase.Moved).deltaPosition : GetInsideTouch(index).deltaPosition;
            switch (UnityPlayerAPI.Instance.GetUnityScreenOrientation())
            {
                case ScreenOrientation.Portrait:
                    return delta;
                case ScreenOrientation.LandscapeLeft:
                    return new Vector2(-delta.y, delta.x);
                case ScreenOrientation.PortraitUpsideDown:
                    return -delta;
                case ScreenOrientation.LandscapeRight:
                    return new Vector2(delta.y, -delta.x);
            }
            return delta;
        }

        public Vector2 GetInsideTouchDeltaPosition()
        {
            if (GetInsideTouchCount() == 0)
                return Vector2.zero;
            for (int i = 0; i < GetInsideTouchCount(); i++)
            {
                if (GetInsideTouch(i).phase == TouchPhase.Moved)
                {
                    Vector2 delta = GetInsideTouch(i).deltaPosition;
                    switch (UnityPlayerAPI.Instance.GetUnityScreenOrientation())
                    {
                        case ScreenOrientation.Portrait:
                            return delta;
                        case ScreenOrientation.LandscapeLeft:
                            return new Vector2(-delta.y, delta.x);
                        case ScreenOrientation.PortraitUpsideDown:
                            return -delta;
                        case ScreenOrientation.LandscapeRight:
                            return new Vector2(delta.y, -delta.x);
                    }
                    return GetInsideTouch(i).deltaPosition;
                }
            }
            return Vector2.zero;
        }

        public Vector2 GetInsideTouchPosition(int index)
        {
            if (index >= GetInsideTouchCount())
                return Vector2.zero;
            Vector2 position = GetInsideTouch(index).position;
            switch (UnityPlayerAPI.Instance.GetUnityScreenOrientation())
            {
                case ScreenOrientation.Portrait:
                    return position;
                case ScreenOrientation.LandscapeLeft:
                    return new Vector2(UnityPlayerAPI.Instance.PhoneScreenHeight - position.y, position.x);
                case ScreenOrientation.PortraitUpsideDown:
                    return new Vector2(UnityPlayerAPI.Instance.PhoneScreenWidth - position.x, UnityPlayerAPI.Instance.PhoneScreenHeight - position.y);
                case ScreenOrientation.LandscapeRight:
                    return new Vector2(position.x - UnityPlayerAPI.Instance.PhoneScreenWidth, position.y);
            }
            return Vector2.zero;
        }


        public int GetScreenWidth()
        {
            switch (UnityPlayerAPI.Instance.GetUnityScreenOrientation())
            {
                case ScreenOrientation.PortraitUpsideDown:
                case ScreenOrientation.Portrait:
                    return UnityPlayerAPI.Instance.PhoneScreenWidth;
                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                    return UnityPlayerAPI.Instance.PhoneScreenHeight;
            }
            return UnityPlayerAPI.Instance.PhoneScreenWidth;
        }

        public int GetScreenHeight()
        {
            switch (UnityPlayerAPI.Instance.GetUnityScreenOrientation())
            {
                case ScreenOrientation.PortraitUpsideDown:
                case ScreenOrientation.Portrait:
                    return UnityPlayerAPI.Instance.PhoneScreenHeight;
                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                    return UnityPlayerAPI.Instance.PhoneScreenWidth;
            }
            return UnityPlayerAPI.Instance.PhoneScreenWidth;
        }

        private bool TouchInside(Touch touch)
        {
            if (InputModuleManager.Instance.GetThreeDofActive())
            {
                if (touch.position.y < UnityPlayerAPI.Instance.PhoneScreenHeight * 0.4f && Mathf.Abs(UnityPlayerAPI.Instance.PhoneScreenWidth * 0.5f - touch.position.x) > UnityPlayerAPI.Instance.PhoneScreenWidth * 0.35f)
                {
                    return false;
                }
            }
            return true;
        }

        public bool TouchInside(int index)
        {
            if (Input.touchCount == 0 || index >= Input.touchCount)
            {
                return false;
            }
            return TouchInside(Input.GetTouch(index));
        }

        public bool TouchInsideWithTargetPhase(int index, TouchPhase phase)
        {
            if (Input.touchCount == 0 || index >= Input.touchCount)
            {
                return false;
            }
            return TouchInside(Input.GetTouch(index)) && Input.GetTouch(index).phase == phase;
        }

        public int GetInsideTouchCount()
        {
            return insideTouchList.Count;
        }

        /// <summary>
        /// Get inside touch 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Touch GetInsideTouch(int index)
        {
            if (index >= insideTouchList.Count)
            {
                return default(Touch);
            }
            return insideTouchList[index].touch;
        }

        /// <summary>
        /// Gets index of inside touch points whose status is targetPhase
        /// , and returns the default touch point if no match is found
        /// </summary>
        /// <param name="index"></param>
        /// <param name="targetPhase"></param>
        /// <returns></returns>
        public Touch GetInsideTouchWithTargetPhase(int index, TouchPhase targetPhase)
        {
            if (index >= insideTouchList.Count)
            {
                return default(Touch);
            }
            int targetIndex = 0;
            for (int i = 0; i < GetInsideTouchCount(); i++)
            {
                Touch touch = insideTouchList[i].touch;
                if (touch.phase == targetPhase)
                {
                    if (targetIndex == index)
                        return touch;
                    targetIndex++;
                }
            }
            return insideTouchList[index].touch;
        }

        /// <summary>
        /// Gets index of inside touch points whose status is not targetStatus
        /// , and returns the default touch point if no match is found
        /// </summary>
        /// <param name="index"></param>
        /// <param name="targetPhase"></param>
        /// <returns></returns>
        public Touch GetInsideTouchExcludeTargetPhase(int index, TouchPhase targetPhase)
        {
            if (index >= insideTouchList.Count)
            {
                return default(Touch);
            }
            int targetIndex = 0;
            for (int i = 0; i < GetInsideTouchCount(); i++)
            {
                Touch touch = insideTouchList[i].touch;
                if (touch.phase != targetPhase)
                {
                    if (targetIndex == index)
                        return touch;
                    targetIndex++;
                }
            }
            return insideTouchList[index].touch;
        }


        private void RemoveInsideTouchByFingerId(int fingerId)
        {
            for (int i = 0; i < insideTouchList.Count; i++)
            {
                if (insideTouchList[i].touch.fingerId == fingerId)
                {
                    // RKLog.KeyInfo($"\r\n  <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ====RKTouchInput==== Removed Success {fingerId}>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>inside.Count:{insideTouchList.Count - 1} \r\n ");
                    insideTouchList.Remove(insideTouchList[i]);
                    break;
                }
            }
        }
        private static string TouchToString(Touch touch)
        {
            return $"\r\nTouch.FingerId:{touch.fingerId}\r\nTouch.Phase:{touch.phase}\r\nTouch.Position:{touch.position}\r\nTouch.DeltaPosition:{touch.deltaPosition}";
        }

        private void LogInsideTouch()
        {
            if (GetInsideTouchCount() > 0)
            {
                for (int i = 0; i < GetInsideTouchCount(); i++)
                {
                    RKLog.KeyInfo($"====RKTouchInput==== Inside <<{Input.touchCount},{i}>> {insideTouchList[i]} ");
                }
                RKLog.KeyInfo($"\r\n  -------------------------------------------- ====RKTouchInput==== -------------------------------------------- \r\n ");
            }
        }

        private void LogInputTouch()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    RKLog.KeyInfo($"====RKTouchInput ==== All <<{Input.touchCount},{i}>> {TouchToString(Input.GetTouch(i))} ");
                }
            }
        }

        private int TouchCountFilter(int touchCount, float fdThreshold = 700)
        {
            if (touchCount > 1)
            {
                int filterCount = 1;
                //使用组合算法
                for (int i = 0; i < touchCount; i++)
                {
                    for (int j = i + 1; j < touchCount; j++)
                    {
                        float sqrDistance = Vector2.SqrMagnitude(GetInsideTouch(i).position - GetInsideTouch(j).position);
                        if (sqrDistance < fdThreshold * fdThreshold)
                        {
                            filterCount++;
                        }
                        if (filterCount == touchCount)
                        {
                            break;
                        }
                    }
                }
                // RKLog.KeyInfo($"====RKTouchInput====  filterCount: {filterCount}");
                return filterCount;
            }
            return touchCount;
        }

        private void Update()
        {
            if (clearTouchList.Count > 0)
            {
                for (int i = 0; i < clearTouchList.Count; i++)
                {
                    RemoveInsideTouchByFingerId(clearTouchList[i].touch.fingerId);
                }
                clearTouchList.Clear();
            }
            // touch follow
            if (!inputLock && Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    for (int j = 0; j < insideTouchList.Count; j++)
                    {
                        if (Input.GetTouch(i).phase != TouchPhase.Ended && Input.GetTouch(i).phase != TouchPhase.Canceled)
                        {
                            //update touch
                            InsideTouch insideTouch = insideTouchList[j];
                            if (insideTouch.touch.fingerId == Input.GetTouch(i).fingerId)
                            {
                                insideTouch.touch = Input.GetTouch(i);
                                insideTouchList[j] = insideTouch;
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Ended || Input.GetTouch(i).phase == TouchPhase.Canceled)
                    {
                        for (int j = 0; j < insideTouchList.Count; j++)
                        {
                            InsideTouch insideTouch = insideTouchList[j];
                            if (insideTouch.touch.fingerId == Input.GetTouch(i).fingerId)
                            {
                                insideTouch.touch = Input.GetTouch(i);
                                insideTouchList[j] = insideTouch;
                                //remove touch
                                clearTouchList.Add(insideTouch);
                            }
                        }
                    }
                }

                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (TouchInsideWithTargetPhase(i, TouchPhase.Began))
                    {
                        //add inside touch
                        InsideTouch newInsideTouch = new InsideTouch
                        {
                            touch = Input.GetTouch(i),
                        };
                        insideTouchList.Add(newInsideTouch);
                        // RKLog.KeyInfo($"\r\n  <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ====RKTouchInput==== Added {Input.GetTouch(i).fingerId}>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>inside.count: {insideTouchList.Count} \r\n ");
                    }
                }
            }
            else
            {
                if (clearTouchList.Count > 0)
                    clearTouchList.Clear();
                if (insideTouchList.Count > 0)
                    insideTouchList.Clear();
            }

            #region  FingerChangeStateMachine
            int touchCount = TouchCountFilter(GetInsideTouchCount());
            if (oldOperationFingerCount != touchCount)
            {
                oldOperationFingerCount = touchCount;
                elapsedTime = 0;
            }
            switch (fingerOperation)
            {
                case FingerOperation.None:
                    switch (touchCount)
                    {
                        case 1:
                            fingerOperation = FingerOperation.OneFinger;
                            break;
                        case 2:
                            fingerOperation = FingerOperation.TwoFinger;
                            break;
                    }
                    if (touchCount >= 3)
                    {
                        elapsedTime += Time.deltaTime;
                        if (elapsedTime > 0.05f)
                        {
                            fingerOperation = FingerOperation.ThreeFinger;
                        }
                    }
                    break;
                case FingerOperation.OneFinger:
                    switch (touchCount)
                    {
                        case 0:
                            fingerOperation = FingerOperation.None;
                            break;
                        case 2:
                            elapsedTime += Time.deltaTime;
                            if (elapsedTime > 0.01f)
                            {
                                fingerOperation = FingerOperation.TwoFinger;
                            }
                            break;
                    }
                    if (touchCount >= 3)
                    {
                        elapsedTime += Time.deltaTime;
                        if (elapsedTime > 0.05f)
                        {
                            fingerOperation = FingerOperation.ThreeFinger;
                        }
                    }
                    break;
                case FingerOperation.TwoFinger:
                    switch (touchCount)
                    {
                        case 0:
                            fingerOperation = FingerOperation.None;
                            break;
                        case 1:
                            elapsedTime += Time.deltaTime;
                            if (elapsedTime > 0.1f)
                            {
                                fingerOperation = FingerOperation.OneFinger;
                            }
                            break;
                    }
                    if (touchCount >= 3)
                    {
                        elapsedTime += Time.deltaTime;
                        if (elapsedTime > 0.05f)
                        {
                            fingerOperation = FingerOperation.ThreeFinger;
                        }
                    }
                    break;
                case FingerOperation.ThreeFinger:
                    switch (touchCount)
                    {
                        case 0:
                            fingerOperation = FingerOperation.None;
                            break;
                    }
                    break;
            }
            if (oldFingerOperation != fingerOperation)
            {
                RKLog.KeyInfo($"====RKTouchInput==== FingerOperation Changed :{oldFingerOperation},{fingerOperation}");
                OnFingerOperationChanged?.Invoke(oldFingerOperation, fingerOperation);
                oldFingerOperation = fingerOperation;
            }
            #endregion
            OnUpdate?.Invoke();
        }
    }

}
