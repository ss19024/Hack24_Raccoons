using UnityEngine;
using Rokid.UXR.Config;
using Rokid.UXR.Native;

namespace Rokid.UXR.SubModule
{
    [DisallowMultipleComponent]
    public class MRCLoader : MonoBehaviour
    {
        public static bool isMRCDriverCreated;
        private bool isMRCInited;

        void Update()
        {
            if (isMRCDriverCreated)
            {
                return;
            }

            if (!isMRCInited && UXRSDKConfig.Instance.MRCActive)
            {
#if !UNITY_EDITOR
                if (NativeInterface.NativeAPI.GetHeadTrackingStatus() == HeadTrackingStatus.Tracking)
#endif
                {
                    isMRCInited = true;

                    //场景中是否已经有MRCDriver
                    GameObject driver = GameObject.Find("MRCDriver");
                    if (driver == null)
                    {
                        //加载MRC启动器Prefab，如果加载不到，说明未安装RokidMRC这个Package
                        GameObject mrcDriver = Resources.Load<GameObject>("MRCDriver");
                        if (mrcDriver == null)
                        {
                            RKLog.KeyInfo("Install Rokid MRC Package Through NPM");
                            Destroy(this.gameObject);
                            return;
                        }
                        //生成MRC启动器
                        driver = Instantiate(mrcDriver);
                        Destroy(this.gameObject);
                        isMRCDriverCreated = true;
                    }

                    //设置相机cullingMask
                    Camera renderCam = driver.transform.Find("RenderCamera").GetComponent<Camera>();
                    if (renderCam == null)
                    {
                        RKLog.Error("Rokid MRC Package Is Invalid");
                        return;
                    }

                    renderCam.cullingMask = UXRSDKConfig.Instance.MRCCameraRenderLayer;
                    renderCam.renderingPath = UXRSDKConfig.Instance.MRCCameraRenderingPath;
                }

            }
        }
    }
}
