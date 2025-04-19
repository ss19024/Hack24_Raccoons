using UnityEngine;

namespace Rokid.UXR.Module
{
    public class RKGlobalFpsModule : MonoSingleton<RKGlobalFpsModule>
    {

#if !UNITY_EDITOR
        private const string SYS_PROP_OPM_FUNC_CONFIG = "persist.sys.rokid.opm.config";

        private const int FUNC_NONE = 0;
        private const int FUNC_SERVICE_START = 0x08000000;
        private const int FUNC_FPS = 0x00000001;
        
        private static int currentFuncConfig = 0;
#endif



        private GameObject fpsCanvas;

        private bool isActive = false;

        protected override void OnSingletonInit()
        {
            base.OnSingletonInit();
            this.gameObject.name = "RKGlobalFpsModule";
            DontDestroyOnLoad(transform);
            RKLog.Info("RKGlobalFpsModule OnSingletonInit init");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RKLog.Info("RKGlobalFpsModule OnDestroy");
        }

        public bool IsEnableAPM()
        {
            bool result = false;

#if !UNITY_EDITOR
            try
            {
                var systemProperties = new AndroidJavaClass("android.os.SystemProperties");
                string prop = systemProperties.CallStatic<string >("get", SYS_PROP_OPM_FUNC_CONFIG);
                
                if (!string.IsNullOrEmpty(prop)) {
                    currentFuncConfig = int.Parse(prop);
                }
                result = (currentFuncConfig & FUNC_SERVICE_START) == FUNC_SERVICE_START;
                RKLog.Info("RKGlobalFpsModule IsEnableAPM prop="+prop+", currentFuncConfig = "+ currentFuncConfig + ", result="+result);
            }
            catch (System.Exception e)
            {
                RKLog.Error("RKGlobalFpsModule IsEnableAPM error = "+ e.Message);
            }
#endif
            return result;
        }

        public void DestroyInstance()
        {
            RKLog.Info("RKGlobalFpsModule DestroyInstance");
            if (fpsCanvas != null)
            {
                Destroy(fpsCanvas);
            }
            Destroy(gameObject);
        }

        public void SetActive(bool active)
        {
            RKLog.Info("RKGlobalFpsModule SetActive active=" + active);
            if (active)
            {
                if (!isActive)
                {
                    fpsCanvas = Instantiate(Resources.Load<GameObject>("Prefabs/UI/UIComponents/RKFPSAnalyzer"));
                    fpsCanvas?.transform.SetParent(transform);
                    isActive = true;
                }
            }
            else
            {
                isActive = false;
                DestroyInstance();
            }
        }

    }
}
