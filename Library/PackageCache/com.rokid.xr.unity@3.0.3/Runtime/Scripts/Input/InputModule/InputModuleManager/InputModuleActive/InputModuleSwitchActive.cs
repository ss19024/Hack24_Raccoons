using System;
using System.Threading;
using UnityEngine;
namespace Rokid.UXR.Interaction
{
    [Serializable]
    public class HandActiveDetail
    {
        [SerializeField, Tooltip("是否在手丢失的时候禁用")]
        public bool DisableOnHandLost = true;

        [SerializeField, Tooltip("左右手的激活类型")]
        public ActiveHandType activeHandType = ActiveHandType.LeftHand | ActiveHandType.RightHand;

        [SerializeField, Tooltip("远近场的激活类型")]
        public ActiveHandInteractorType activeHandInteractorType = ActiveHandInteractorType.Far | ActiveHandInteractorType.Near;

        [SerializeField, Tooltip("手心手背的激活类型")]
        public ActiveHandOrientationType activeHandOrientationType = ActiveHandOrientationType.Back | ActiveHandOrientationType.Palm;

        [SerializeField, Tooltip("默认手势交互|头手交互")]
        public ActiveHeadHandType activeHeadHandType = ActiveHeadHandType.NormalHand | ActiveHeadHandType.HeadHand;

        [SerializeField, Tooltip("手表模式的激活类型")]
        public ActiveWatchType activeWatchType = ActiveWatchType.DisableWatch | ActiveWatchType.EnableWatch;
        [SerializeField, Tooltip("手势射线激活类型")]
        public ActiveHandRayType activeHandRayType = ActiveHandRayType.LeftHandRay | ActiveHandRayType.RightHandRay;
    }

    /// <summary>
    /// This script implements the IInputModuleActive interface, which allows it to register its own activation status information to the InputModuleManager for centralized management and switching.
    /// </summary>
    public class InputModuleSwitchActive : MonoBehaviour, IInputModuleActive
    {
        [SerializeField, Optional, Tooltip("禁用/激活的Behaviour组件,如果为空则禁用/激活gameobject")]
        private MonoBehaviour behaviour;

        [SerializeField, Tooltip("交互模块的激活类型")]
        private ActiveModuleType activeModuleType;
        [SerializeField, Tooltip("手势的交互的激活细节")]
        private HandActiveDetail handActiveDetail;

        [SerializeField, Tooltip("是否在Start中自动注册状态机")]
        private bool autoRegisterOnStart = true;
        private bool registered = false;
        MonoBehaviour IInputModuleActive.Behaviour => this.behaviour;
        public GameObject Go => this.gameObject;

        ActiveModuleType IInputModuleActive.ActiveModuleType => this.activeModuleType;

        ActiveHandType IInputModuleActive.ActiveHandType => this.handActiveDetail.activeHandType;

        ActiveHandInteractorType IInputModuleActive.ActiveHandInteractorType => this.handActiveDetail.activeHandInteractorType;

        ActiveHandOrientationType IInputModuleActive.ActiveHandOrientationType => this.handActiveDetail.activeHandOrientationType;

        public ActiveWatchType ActiveWatchType => this.handActiveDetail.activeWatchType;

        public ActiveHeadHandType ActiveHeadHandType => this.handActiveDetail.activeHeadHandType;

        public bool DisableOnHandLost => this.handActiveDetail.DisableOnHandLost;

        public ActiveHandRayType ActiveHandRayType => this.handActiveDetail.activeHandRayType;

        private void Start()
        {
            if (autoRegisterOnStart)
                Reigster();
        }

        private void OnDestroy()
        {
            if (registered)
                UnRegister();
        }

        public void Reigster()
        {
            InputModuleManager.Instance.RegisterActive(this);
            registered = true;
        }

        public void UnRegister()
        {
            InputModuleManager.Instance.UnRegisterActive(this);
            registered = false;
        }
    }
}

