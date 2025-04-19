using UnityEngine;
using UnityEngine.UI;
using System;
using Rokid.UXR.Native;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Utility
{
    public static partial class Utils
    {
        /// <summary>
        /// ClearChild
        /// </summary>
        /// <param name="transform"></param>
        public static void ClearTransformChild(Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Retrieve angular measurement describing how large a sphere or circle appears from a given point of view.
        /// Takes an angle (at given point of view) and a distance and returns the actual diameter of the object.
        /// </summary>
        public static float ScaleFromAngularSizeAndDistance(float angle, float distance)
        {
            float scale = 1.5f * distance * Mathf.Tan(angle * Mathf.Deg2Rad * 0.5f);
            return scale;
        }

        public static float GetYawAngleFromDirection(Vector3 direction)
        {
            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;//所给方向与世界坐标系的z轴夹角(yaw angle，偏航角)
            return yaw;
        }

        public static float GetPitchAngleFromDirection(Vector3 direction)
        {
            float pitch = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
            return pitch;
        }

        public static float GetRollAngleFromDirection(Vector3 direction)
        {
            float roll = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return roll;
        }

        public static void LogFuncTimeStamp(Action func, string tag)
        {
            double beginTime = GetTimeStamp();
            func?.Invoke();
            double endTime = GetTimeStamp();
            RKLog.KeyInfo($"{tag}:" + (endTime - beginTime));
        }

        public static double GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalMilliseconds;
        }

        public static long GetSystemStartupTimeStamp()
        {
            return (DateTime.Now.AddMilliseconds(-Environment.TickCount)).Ticks;
        }

        public static bool GetWorldPointInRectangle(RectTransform rect, Ray ray, out Vector3 worldPoint)
        {
            worldPoint = Vector2.zero;
            Plane plane = new Plane(rect.rotation * Vector3.back, rect.position);
            float enter = 0f;
            float num = Vector3.Dot(Vector3.Normalize(rect.position - ray.origin), plane.normal);
            if (num != 0f && !plane.Raycast(ray, out enter))
            {
                return false;
            }
            worldPoint = ray.GetPoint(enter);
            return true;
        }

        public static bool GetWorldPointInRectangle(Graphic rect, Ray ray, out Vector3 worldPoint)
        {
            worldPoint = Vector2.zero;
            Plane plane = new Plane(rect.transform.rotation * Vector3.back, rect.transform.position);
            float enter = 0f;
            float num = Vector3.Dot(Vector3.Normalize(rect.transform.position - ray.origin), plane.normal);
            if (num != 0f && !plane.Raycast(ray, out enter))
            {
                return false;
            }
            worldPoint = ray.GetPoint(enter);
            return true;
        }

        public static bool IsAndroidPlatform()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public static bool IsUnityEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public static bool InCameraView(Vector3 point, Camera camera)
        {
            var v = camera.WorldToViewportPoint(point);
            if (v.x > 0.8f || v.y > 0.8f || v.z < 0.1f)
            {
                return false;
            }
            return true;
        }
        private static bool useLeftEyeFov = true;
        private static Vector4 halfFovTan = Vector4.zero;
        private static bool fovChanged = false;
        public static Vector4 HalfFovTan
        {
            get
            {
                if (halfFovTan.x != 0 && fovChanged == false)
                    return halfFovTan;
                fovChanged = false;
                if (Utils.IsUnityEditor())
                {
                    Camera cam = MainCameraCache.mainCamera;
                    float aspectRatio = cam.aspect;
                    float verticalFOV = cam.fieldOfView;
                    float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(verticalFOV * Mathf.Deg2Rad / 2f) * aspectRatio) * Mathf.Rad2Deg;
                    halfFovTan.x = halfFovTan.y = Mathf.Tan(horizontalFOV / 2 * Mathf.Deg2Rad);
                    halfFovTan.z = halfFovTan.w = Mathf.Tan(verticalFOV / 2 * Mathf.Deg2Rad);
                }
                else
                {
                    float[] fov = new float[4];
                    NativeInterface.NativeAPI.GetUnityEyeFrustumHalf(!useLeftEyeFov, ref fov);
                    if (fov[0] > 0 && fov[1] > 0 && fov[2] > 0 && fov[3] > 0)
                    {
                        halfFovTan.x = Mathf.Tan(fov[0] * Mathf.Deg2Rad); // Left 
                        halfFovTan.y = Mathf.Tan(fov[1] * Mathf.Deg2Rad); // Right
                        halfFovTan.z = Mathf.Tan(fov[2] * Mathf.Deg2Rad); // Top 
                        halfFovTan.w = Mathf.Tan(fov[3] * Mathf.Deg2Rad); // Bottom
                    }
                }
                return halfFovTan;
            }
        }

        public static Vector3[] GetCameraCorners(float dis, bool useLeftEyeFov = true)
        {
            Vector3[] corners = new Vector3[4];
            Vector3 center = GetCameraCenter(dis);
            if (Utils.useLeftEyeFov != useLeftEyeFov)
            {
                fovChanged = true;
                Utils.useLeftEyeFov = useLeftEyeFov;
            }
            Vector2 distortionQuadSize = GetDistortionQuadSize();
            float width = (HalfFovTan.x + halfFovTan.y) * distortionQuadSize.x * dis;
            float height = (HalfFovTan.w + halfFovTan.z) * distortionQuadSize.y * dis;
            corners[0] = center + new Vector3(-width * 0.5f, height * 0.5f, 0);  // leftTop
            corners[1] = center + new Vector3(width * 0.5f, height * 0.5f, 0);   // rightTop
            corners[2] = center + new Vector3(-width * 0.5f, -height * 0.5f, 0); // leftBottom
            corners[3] = center + new Vector3(width * 0.5f, -height * 0.5f, 0);  // rightBottom
            return corners;
        }

        /// <summary>
        /// 获取相机中心点位置,该点是相对相机的局部坐标
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 GetCameraCenter(float distance, bool useLeftEyeFov = true)
        {
            if (Utils.useLeftEyeFov != useLeftEyeFov)
            {
                fovChanged = true;
                Utils.useLeftEyeFov = useLeftEyeFov;
            }
            if (IsAndroidPlatform())
            {
                float[] frustum = NativeInterface.NativeAPI.GetFrustum(useLeftEyeFov);
                MainCameraCache.mainCamera.projectionMatrix = Matrix4x4.Frustum(frustum[0], frustum[1], frustum[2], frustum[3], frustum[4], frustum[5]);
            }
            Vector2 distortCenter = GetDistortionQuadCenter(useLeftEyeFov);
            // RKLog.KeyInfo("distortCenter :" + distortCenter);
            Vector3 center = MainCameraCache.mainCamera.ViewportToWorldPoint(new Vector3(distortCenter.x, distortCenter.y, distance));
            //将世界坐标转换成相机的局部坐标
            return MainCameraCache.mainCamera.transform.InverseTransformPoint(center);
        }


        /// <summary>
        /// 获取相机中心点位置,该点是相对相机的局部坐标
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 GetCameraCenterSetCameraPositionAndRotation(float distance, Vector3 position, Quaternion rotation, bool useLeftEyeFov = true)
        {
            MainCameraCache.mainCamera.transform.localRotation = rotation;
            MainCameraCache.mainCamera.transform.localPosition = position;
            if (Utils.useLeftEyeFov != useLeftEyeFov)
            {
                fovChanged = true;
                Utils.useLeftEyeFov = useLeftEyeFov;
            }
            if (IsAndroidPlatform())
            {
                float[] frustum = NativeInterface.NativeAPI.GetFrustum(useLeftEyeFov);
                MainCameraCache.mainCamera.projectionMatrix = Matrix4x4.Frustum(frustum[0], frustum[1], frustum[2], frustum[3], frustum[4], frustum[5]);
            }
            Vector2 distortCenter = GetDistortionQuadCenter(useLeftEyeFov);
            // RKLog.KeyInfo("distortCenter :" + distortCenter);
            Vector3 center = MainCameraCache.mainCamera.ViewportToWorldPoint(new Vector3(distortCenter.x, distortCenter.y, distance));
            //将世界坐标转换成相机的局部坐标
            return MainCameraCache.mainCamera.transform.InverseTransformPoint(center);
        }


        public static Vector2 GetDistortionQuadCenter(bool useLeftEyeFov = true)
        {
            Vector2 center = Vector2.zero;
            if (IsAndroidPlatform())
            {
                float[] distortion_quad = NativeInterface.NativeAPI.GetDistortionQuad(useLeftEyeFov);
                for (int i = 0; i < distortion_quad.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        center.x += distortion_quad[i];
                    }
                    else
                    {
                        center.y += distortion_quad[i];
                    }
                }
                return center * 0.25f;
            }
            return Vector2.one * 0.5f;
        }

        public static Vector2 GetDistortionQuadSize(bool useLeftEyeFov = true)
        {
            if (IsAndroidPlatform())
            {
                float[] distortion_quad = NativeInterface.NativeAPI.GetDistortionQuad(useLeftEyeFov);
                return new Vector2(distortion_quad[6], distortion_quad[7]) - new Vector2(distortion_quad[0], distortion_quad[1]);
            }
            return Vector2.one;
        }
        public static bool IsChineseLanguage()
        {
            return Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.ChineseTraditional;
        }

        public static bool IsJapaneseLanguage()
        {
            return Application.systemLanguage == SystemLanguage.Japanese;
        }


        // 获得分辨率，当选择 Free Aspect 直接返回相机的像素宽和高
        public static Vector2 GetScreenPixelDimensions()
        {
            Vector2 dimensions = new Vector2(MainCameraCache.mainCamera.pixelWidth, MainCameraCache.mainCamera.pixelHeight);

#if UNITY_EDITOR
            // 获取编辑器 GameView 的分辨率
            float gameViewPixelWidth = 0, gameViewPixelHeight = 0;
            float gameViewAspect = 0;

            if (Editor__GetGameViewSize(out gameViewPixelWidth, out gameViewPixelHeight, out gameViewAspect))
            {
                if (gameViewPixelWidth != 0 && gameViewPixelHeight != 0)
                {
                    dimensions.x = gameViewPixelWidth;
                    dimensions.y = gameViewPixelHeight;
                }
            }
#endif
            return dimensions;
        }

        public static bool IsAnyCenterTouchDown()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began && Mathf.Abs(Input.GetTouch(i).position.y - 512) < 300)
                        return true;
                }
            }
            return false;
        }

        public static bool IsAnyCenterTouchPress()
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Mathf.Abs(Input.GetTouch(i).position.y - 512) < 300)
                        return true;
                }
            }
            return false;
        }

        public static bool IsPhone()
        {
            return !SystemInfo.deviceModel.Contains("station");
        }

        public static Vector3 GetCurrentUpDirection()
        {
            Vector3 targetUp = Vector3.up;

            if (Vector3.Angle(Vector3.up, MainCameraCache.mainCamera.transform.forward) <= 30)
            {
                targetUp = Vector3.ProjectOnPlane(MainCameraCache.mainCamera.transform.up, Vector3.up);
            }

            if (Vector3.Angle(Vector3.down, MainCameraCache.mainCamera.transform.forward) <= 30)
            {
                targetUp = Vector3.ProjectOnPlane(MainCameraCache.mainCamera.transform.up, Vector3.down);
            }

            return targetUp;
        }


        public static Pose SmoothToPose(Pose currentPose, Pose targetPose)
        {
            float deltaSqrDistance = Vector3.SqrMagnitude(currentPose.position - targetPose.position) * 600;
            float deltaAngle = Vector3.Angle(currentPose.forward, targetPose.rotation * Vector3.forward);
            float smoothSpeed = Mathf.Clamp(deltaSqrDistance * deltaAngle, 1f, 30);
            currentPose.position = Vector3.Lerp(currentPose.position, targetPose.position, Time.deltaTime * smoothSpeed);
            currentPose.rotation = Quaternion.Slerp(currentPose.rotation, targetPose.rotation, Time.deltaTime * smoothSpeed);
            return currentPose;
        }


#if UNITY_EDITOR
        static bool Editor__getGameViewSizeError = false;
        public static bool Editor__gameViewReflectionError = false;

        // 尝试获取 GameView 的分辨率
        // 当正确获取到 GameView 的分辨率时，返回 true
        public static bool Editor__GetGameViewSize(out float width, out float height, out float aspect)
        {
            try
            {
                Editor__gameViewReflectionError = false;

                System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                System.Reflection.MethodInfo GetMainGameView = gameViewType.GetMethod("GetMainGameView", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                object mainGameViewInst = GetMainGameView.Invoke(null, null);
                if (mainGameViewInst == null)
                {
                    width = height = aspect = 0;
                    return false;
                }
                System.Reflection.FieldInfo s_viewModeResolutions = gameViewType.GetField("s_viewModeResolutions", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (s_viewModeResolutions == null)
                {
                    System.Reflection.PropertyInfo currentGameViewSize = gameViewType.GetProperty("currentGameViewSize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    object gameViewSize = currentGameViewSize.GetValue(mainGameViewInst, null);
                    System.Type gameViewSizeType = gameViewSize.GetType();
                    int gvWidth = (int)gameViewSizeType.GetProperty("width").GetValue(gameViewSize, null);
                    int gvHeight = (int)gameViewSizeType.GetProperty("height").GetValue(gameViewSize, null);
                    int gvSizeType = (int)gameViewSizeType.GetProperty("sizeType").GetValue(gameViewSize, null);
                    if (gvWidth == 0 || gvHeight == 0)
                    {
                        width = height = aspect = 0;
                        return false;
                    }
                    else if (gvSizeType == 0)
                    {
                        width = height = 0;
                        aspect = (float)gvWidth / (float)gvHeight;
                        return true;
                    }
                    else
                    {
                        width = gvWidth; height = gvHeight;
                        aspect = (float)gvWidth / (float)gvHeight;
                        return true;
                    }
                }
                else
                {
                    Vector2[] viewModeResolutions = (Vector2[])s_viewModeResolutions.GetValue(null);
                    float[] viewModeAspects = (float[])gameViewType.GetField("s_viewModeAspects", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).GetValue(null);
                    string[] viewModeStrings = (string[])gameViewType.GetField("s_viewModeAspectStrings", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).GetValue(null);
                    if (mainGameViewInst != null
                        && viewModeStrings != null
                        && viewModeResolutions != null && viewModeAspects != null)
                    {
                        int aspectRatio = (int)gameViewType.GetField("m_AspectRatio", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).GetValue(mainGameViewInst);
                        string thisViewModeString = viewModeStrings[aspectRatio];
                        if (thisViewModeString.Contains("Standalone"))
                        {
                            width = UnityEditor.PlayerSettings.defaultScreenWidth; height = UnityEditor.PlayerSettings.defaultScreenHeight;
                            aspect = width / height;
                        }
                        else if (thisViewModeString.Contains("Web"))
                        {
                            width = UnityEditor.PlayerSettings.defaultWebScreenWidth; height = UnityEditor.PlayerSettings.defaultWebScreenHeight;
                            aspect = width / height;
                        }
                        else
                        {
                            width = viewModeResolutions[aspectRatio].x; height = viewModeResolutions[aspectRatio].y;
                            aspect = viewModeAspects[aspectRatio];
                            // this is an error state
                            if (width == 0 && height == 0 && aspect == 0)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                if (Editor__getGameViewSizeError == false)
                {
                    Debug.LogError("GameCamera.GetGameViewSize - has a Unity update broken this?\nThis is not a fatal error !\n" + e.ToString());
                    Editor__getGameViewSizeError = true;
                }
                Editor__gameViewReflectionError = true;
            }
            width = height = aspect = 0;
            return false;
        }
#endif
    }
}
