using System.Diagnostics;

namespace Rokid.UXR
{
    /// <summary>
    /// API library constants
    /// </summary>
    public static class ApiConstants
    {
#if USE_ROKID_OPENXR
        public const string ROKID_UXR_PLUGIN = "rokid_openxr_api";
#else
        public const string ROKID_UXR_PLUGIN = "GfxPluginRokidXRLoader";
#endif

    }
}


