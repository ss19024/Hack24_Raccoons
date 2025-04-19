using System;
using System.Collections.Generic;
using UnityEngine;


namespace Rokid.UXR.Module
{
    /// <summary>
    /// Manage the initialization of the Android module
    /// </summary>
    public class ModuleManager : MonoSingleton<ModuleManager>
    {

        public class ModuleInfo
        {
            public string moduleName;
            public AndroidJavaObject module;
            public bool init;

        }
        /// <summary>
        /// This module can be initialized directly.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="ModuleInfo"></typeparam>
        /// <returns></returns>
        [SerializeField]
        private Dictionary<string, ModuleInfo> modules = new Dictionary<string, ModuleInfo>();

        /// <summary>
        /// This module needs to be initialized after the Slam initialization is completed.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="ModuleInfo"></typeparam>
        /// <returns></returns>
        [SerializeField]
        private Dictionary<string, ModuleInfo> afterSlamInitModules = new Dictionary<string, ModuleInfo>();


        /// <summary>
        /// Is Slam initialization completed?
        /// </summary>
        private bool slamInit = false;
        private Action OnSlamInit;
        private Action OnAfterSlamInit;

        public void Initialize()
        {
            //TODO nothing
        }

        protected override void OnSingletonInit()
        {
            OnSlamInit += onSlamInit;
            DontDestroyOnLoad(this.gameObject);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnSlamInit -= onSlamInit;
            //清空事件...
            OnSlamInit = null;
        }

        private void onSlamInit()
        {
            foreach (var module in afterSlamInitModules.Values)
            {
                if (!module.init)
                {
                    RKLog.Info("====ModuleManager====  new moduleInfo: " + module.moduleName);
                    module.module = new AndroidJavaObject(module.moduleName);
                    module.init = true;
                }
            }
        }

        void Update()
        {
            if (slamInit == false)
            {
                //TODO Slam Init...
                slamInit = true;
                RKLog.Info("====ModuleManager==== Slam Initd !!!");
                OnSlamInit?.Invoke();
                OnAfterSlamInit?.Invoke();
            }
        }

        /// <summary>
        /// Retrieve the initialized module.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public AndroidJavaObject GetModule(string moduleName)
        {
            ModuleInfo moduleInfo;
            if (modules.TryGetValue(moduleName, out moduleInfo))
            {
                if (moduleInfo.init)
                {
                    RKLog.Info($"====ModuleManager==== GetModule In modules {moduleName} ");
                    return moduleInfo.module;
                }
            }
            if (afterSlamInitModules.TryGetValue(moduleName, out moduleInfo))
            {
                if (moduleInfo.init)
                {
                    RKLog.Info($"====ModuleManager==== GetModule In afterSlamInitModuels {moduleName} ");
                    return moduleInfo.module;
                }
            }
            RKLog.Info($"====ModuleManager==== 模块未注册请注册模块 {moduleName} ");
            return null;
        }

        public AndroidJavaObject RegistModule(string moduleName)
        {
            if (!modules.ContainsKey(moduleName))
            {
                modules.Add(moduleName, new ModuleInfo()
                {
                    moduleName = moduleName,
                    module = new AndroidJavaObject(moduleName),
                    init = true
                });
            }
            return modules[moduleName].module;
        }

        /// <summary>
        /// Register the module.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="afterSlamInit"></param>
        public void RegistModule(string moduleName, bool afterSlamInit = false, Action registCallBack = null)
        {
            if (afterSlamInit)
            {
                if (!afterSlamInitModules.ContainsKey(moduleName) && !modules.ContainsKey(moduleName))
                {
                    if (slamInit)
                    {
                        RKLog.Info("====ModuleManager==== SlamInit 注册模块");
                        afterSlamInitModules.Add(moduleName, new ModuleInfo()
                        {
                            moduleName = moduleName,
                            module = new AndroidJavaObject(moduleName),
                            init = true
                        });
                        registCallBack?.Invoke();
                    }
                    else
                    {
                        RKLog.Info($"====ModuleManager==== 添加到afterSlamInitModules集合中 {moduleName}");
                        afterSlamInitModules.Add(moduleName, new ModuleInfo()
                        {
                            moduleName = moduleName,
                            init = false
                        });
                        OnAfterSlamInit += registCallBack;
                    }
                }
                else
                {
                    RKLog.Info($"====ModuleManager==== afterSlamInit 该模块已经注册  {moduleName}");
                    ModuleInfo info = null;
                    if (afterSlamInitModules.TryGetValue(moduleName, out info))
                    {
                        if (info.init)
                        {
                            registCallBack?.Invoke();
                        }
                        else
                        {
                            OnAfterSlamInit += registCallBack;
                        }
                    }
                }
            }
            else
            {
                if (!modules.ContainsKey(moduleName) && !afterSlamInitModules.ContainsKey(moduleName))
                {
                    modules.Add(moduleName, new ModuleInfo()
                    {
                        moduleName = moduleName,
                        module = new AndroidJavaObject(moduleName),
                        init = true
                    });
                }
                else
                {
                    RKLog.Info($"====ModuleManager==== 该模块已经注册  {moduleName}");
                }
                registCallBack?.Invoke();
            }
        }
    }
}

