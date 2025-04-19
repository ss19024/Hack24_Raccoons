using UnityEngine;
using System.Collections.Generic;
using Rokid.UXR.UI;
using Rokid.UXR.Utility;
using System;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// 头手交互驱动器
    /// </summary>
    public class HeadHandDriver : MonoBehaviour
    {

        [SerializeField]
        private List<IHeadHandDriver> driverObjs = new List<IHeadHandDriver>();
        private HandType holdHandType = HandType.None;
        private bool isHovering = false;
        [SerializeField]
        public ActiveHandType activeHand = ActiveHandType.LeftHand | ActiveHandType.RightHand;

        public static event Action<HandType> OnHandInOperate;

        private void Start()
        {
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            FindComponents(transform);
        }

        private void OnDestroy()
        {
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
        }



        private void FindComponents(Transform tsf)
        {
            IHeadHandDriver[] handDrivers = tsf.GetComponents<IHeadHandDriver>();
            if (handDrivers != null && handDrivers.Length > 0)
                driverObjs.AddRange(handDrivers);
            if (tsf.childCount > 0)
            {
                foreach (Transform child in tsf)
                {
                    FindComponents(child);
                }
            }
        }

        private void OnTrackedFailed(HandType handType)
        {
            if (handType == holdHandType || handType == HandType.None)
            {
                holdHandType = HandType.None;
                ChangeHoldHandType();
            }
        }


        bool GetHandDown(HandType handType)
        {
            if (handType == HandType.LeftHand && (activeHand & ActiveHandType.LeftHand) == 0) return false;
            if (handType == HandType.RightHand && (activeHand & ActiveHandType.RightHand) == 0) return false;
            return GesEventInput.Instance.GetHandDown(handType);
        }
        bool GetHandPress(HandType handType)
        {
            if (handType == HandType.LeftHand && (activeHand & ActiveHandType.LeftHand) == 0) return false;
            if (handType == HandType.RightHand && (activeHand & ActiveHandType.RightHand) == 0) return false;
            return GesEventInput.Instance.GetHandPress(handType);
        }
        bool GetHandClick(HandType handType)
        {
            if (handType == HandType.LeftHand && (activeHand & ActiveHandType.LeftHand) == 0) return false;
            if (handType == HandType.RightHand && (activeHand & ActiveHandType.RightHand) == 0) return false;
            return GesEventInput.Instance.GetHandClick(handType);
        }
        bool GetHandUp(HandType handType)
        {
            if (handType == HandType.LeftHand && (activeHand & ActiveHandType.LeftHand) == 0) return false;
            if (handType == HandType.RightHand && (activeHand & ActiveHandType.RightHand) == 0) return false;
            return GesEventInput.Instance.GetHandUp(handType);
        }
        private void Update()
        {
            if (holdHandType == HandType.None)
            {
                if (GetHandDown(HandType.LeftHand))
                {
                    holdHandType = HandType.LeftHand;
                    ChangeHoldHandType();
                }
                else if (GetHandDown(HandType.RightHand))
                {
                    holdHandType = HandType.RightHand;
                    ChangeHoldHandType();
                }
                if (!GetHandPress(holdHandType))
                {
                    OnHandRelease();
                }
            }
            else
            {
                if (GetHandPress(holdHandType))
                {
                    OnHandPress();
                }
                if (holdHandType != HandType.RightHand && GetHandClick(HandType.RightHand))
                {
                    if (OnHandInOperate == null)
                    {
                        string msg = Utils.IsChineseLanguage() ? "你的左手已经在操作中" : "Your left hand is already in operate";
                        UIManager.Instance.CreatePanel<TipPanel>(true).Init(msg, TipLevel.Warning, 0.5f);
                    }
                    else
                    {
                        OnHandInOperate.Invoke(HandType.LeftHand);
                    }
                }
                if (holdHandType != HandType.LeftHand && GetHandClick(HandType.LeftHand))
                {
                    if (OnHandInOperate == null)
                    {
                        string msg = Utils.IsChineseLanguage() ? "你的右手已经在操作中" : "Your right hand is already in operate";
                        UIManager.Instance.CreatePanel<TipPanel>(true).Init(msg, TipLevel.Warning, 0.5f);
                    }
                    else
                    {
                        OnHandInOperate.Invoke(HandType.RightHand);
                    }
                }
            }
            if (GetHandUp(holdHandType))
            {
                RKLog.Info($"====HeadHandDriver==== GetHandUp: {holdHandType}");
                holdHandType = HandType.None;
                ChangeHoldHandType();
            }
        }

        private void ChangeHoldHandType()
        {
            for (int i = 0; i < driverObjs.Count; i++)
            {
                driverObjs[i].OnBeforeChangeHoldHandType(holdHandType);
            }
            for (int i = 0; i < driverObjs.Count; i++)
            {
                driverObjs[i].OnChangeHoldHandType(holdHandType);
            }
            RKLog.Info($"====HeadHandDriver==== ChangeHoldHandType: {holdHandType}");
        }

        private void OnHandPress()
        {
            for (int i = 0; i < driverObjs.Count; i++)
            {
                driverObjs[i].OnHandPress(holdHandType);
            }
        }

        private void OnHandRelease()
        {
            for (int i = 0; i < driverObjs.Count; i++)
            {
                driverObjs[i].OnHandRelease();
            }
        }
    }
}

