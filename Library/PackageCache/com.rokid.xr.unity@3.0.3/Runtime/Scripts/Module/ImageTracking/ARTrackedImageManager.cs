using UnityEngine;
using UnityEngine.UI;
using System;
using Rokid.UXR.Native;
using System.Collections.Generic;
using Rokid.UXR.Utility;
using System.IO;
using UnityEngine.Assertions;
using Newtonsoft.Json;
using System.Linq;


namespace Rokid.UXR.Module
{

    internal enum MarkerDBOperationStatus
    {
        DB_SUCCESS = 0, // 操作成功。
        DB_ERROR_IMAGE_EMPTY = -1, // 输入出图片是空
        DB_ERROR_DUPLICATE_ID = -2, // 重复ID
        DB_ERROR_NAME_INVALID = -3, // 名字非法
        DB_ERROR_DB_FULL = -4, // 数据库达到最大值限制
        DB_ERROR_ID_NOT_EXIST = -5, // ID不存在
        DB_ERROR_VERSION_NOT_SUPPORT = -6, //版本不支持
        DB_ERROR_INVALID_DATA = -7, //数据异常，无法解析
        DB_ERROR_FATAL = -8, //其他错误

        IMAGE_DATA_INVALID = -100
    };

    public enum MarkerDBPath
    {
        // The markerDB path created by marker creator app
        PersistentData,
        // This path is recommended for pre-generated markerDB
        StreamingAssets
    }

    public class ARTrackedImageManager : MonoSingleton<ARTrackedImageManager>
    {

        [SerializeField, Tooltip("Default generated image tracking prefab")]
        private GameObject defaultImageTrackedPrefab;

        [SerializeField]
        private MarkerDBPath markerDBPath = MarkerDBPath.StreamingAssets;
        [SerializeField]
        private bool onEnableOpenImageTracker = true;
        [SerializeField]
        private Text logText;
        private bool enableImageTracker;
        public static event Action<ARTrackedImage> OnTrackedImageAdded;
        public static event Action<ARTrackedImage> OnTrackedImageUpdated;
        public static event Action<ARTrackedImage> OnTrackedImageRemoved;

        RokidMarkerArray added = new RokidMarkerArray();
        RokidMarkerArray updated = new RokidMarkerArray();
        RokidMarkerArray removed = new RokidMarkerArray();

        ARTrackedImage[] addedTrackedImages = new ARTrackedImage[0];
        ARTrackedImage[] updatedTrackedImages = new ARTrackedImage[0];
        ARTrackedImage[] removedTrackedImages = new ARTrackedImage[0];
        private SortedDictionary<int, ARDBImage> arDBImageDict = new SortedDictionary<int, ARDBImage>();
        private Dictionary<int, ARTrackedImageObj> trackedImageObjDict = new Dictionary<int, ARTrackedImageObj>();

        private bool openOrCreateImageTrackerDB = false;
        private bool createCheckDatabase = false;

        private string _dbCorePath;

        public string dbCorePath
        {
            get
            {
                if (_dbCorePath == null)
                {
                    _dbCorePath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER, ARTrackedImageConstant.DB_CORE);
                }
                return _dbCorePath;
            }
        }

        private string _dbDataPath;
        public string dbDataPath
        {
            get
            {
                if (_dbDataPath == null)
                {
                    _dbDataPath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER, ARTrackedImageConstant.DB_DATA);
                }
                return _dbDataPath;
            }
        }


        protected override void Awake()
        {
            base.Awake();
            Loom.Initialize();
            if (onEnableOpenImageTracker)
                OpenImageTracker();
            defaultImageTrackedPrefab?.gameObject.SetActive(false);
            Assert.IsNotNull(dbCorePath);
        }

        /// <summary>
        /// Open Image tracker
        /// </summary>
        public void OpenImageTracker()
        {
            RecoverOrClearTempFile(() =>
            {
                StartCoroutine(TrackedImageUtils.ReadAsset(markerDBPath, (data, path) =>
                {
                    if (NativeInterface.NativeAPI.TryOpenImageTracker(data, data.Length))
                    {
                        enableImageTracker = true;
                        if (logText != null)
                        {
                            string msg = "====ARTrackedImageManager====: Success Open Marker " + path;
                            logText.text = msg;
                            RKLog.KeyInfo(msg);
                        }
                    }
                    else
                    {
                        if (logText != null)
                        {
                            string msg = $"====ARTrackedImageManager====: Glass Not Match Or Algorithm Failed Open Marker {path}";
                            logText.text = msg;
                            RKLog.KeyInfo(msg);
                        }
                    }
                }, error =>
                {
                    if (logText != null)
                    {
                        string msg = $"====ARTrackedImageManager====: Failed Open Marker {error}";
                        logText.text = msg;
                        RKLog.KeyInfo(msg);
                    }
                }));
            }, msg =>
            {
                RKLog.Error(msg);
            });
        }

        /// <summary>
        /// Async Reboot Image tracker
        /// </summary>
        public void AsyncRebootImageTracker(Action success)
        {
            AsyncCloseImageTracker(() =>
            {
                OpenImageTracker();
                Loom.QueueOnMainThread(() =>
                {
                    success?.Invoke();
                });
            });
        }

        /// <summary>
        /// Reboot Image tracker
        /// </summary>
        public void RebootImageTracker()
        {
            CloseImageTracker();
            OpenImageTracker();
        }

        private void Update()
        {
            if (enableImageTracker)
            {
                NativeInterface.NativeAPI.TrackedImage_AcquireChanges(ref added, ref updated, ref removed);
                // Process Added Tracked Image
                if (TrackedImageUtils.TryGetARTrackedImages(added, ref addedTrackedImages))
                {
                    for (int i = 0; i < addedTrackedImages.Length; i++)
                    {
                        if (logText != null)
                        {
                            string msg = $"====ARTrackedImageManager====  OnTrackedImageAdded: {addedTrackedImages[i].ToString()}";
                            logText.text = msg;
                            RKLog.KeyInfo(msg);
                        }
                        OnTrackedImageAdded?.Invoke(addedTrackedImages[i]);
                        if (trackedImageObjDict.TryGetValue(addedTrackedImages[i].index, out ARTrackedImageObj imageObj))
                        {
                            imageObj.Added(addedTrackedImages[i]);
                        }
                        else
                        {
                            if (defaultImageTrackedPrefab != null)
                            {
                                GameObject go = Instantiate(defaultImageTrackedPrefab);
                                ARTrackedImageObj newImageObj = go.GetComponent<ARTrackedImageObj>();
                                if (newImageObj == null)
                                {
                                    newImageObj = go.AddComponent<ARTrackedImageObj>();
                                }
                                newImageObj.trackedImageIndex = addedTrackedImages[i].index;
                                newImageObj.Added(addedTrackedImages[i]);
                            }
                        }
                    }
                }
                // Process Updated Tracked Image
                if (TrackedImageUtils.TryGetARTrackedImages(updated, ref updatedTrackedImages))
                {
                    for (int i = 0; i < updatedTrackedImages.Length; i++)
                    {
                        if (logText != null)
                        {
                            string msg = $"====ARTrackedImageManager==== OnTrackedImageUpdated: {updatedTrackedImages[i].ToString()}";
                            logText.text = msg;
                            RKLog.KeyInfo(msg);
                        }
                        OnTrackedImageUpdated?.Invoke(updatedTrackedImages[i]);
                        if (trackedImageObjDict.TryGetValue(updatedTrackedImages[i].index, out ARTrackedImageObj imageObj))
                        {
                            imageObj.Updated(updatedTrackedImages[i]);
                        }
                    }
                }
                // Process Removed Tracked Image
                if (TrackedImageUtils.TryGetARTrackedImages(removed, ref removedTrackedImages))
                {
                    for (int i = 0; i < removedTrackedImages.Length; i++)
                    {
                        if (logText != null)
                        {
                            string msg = $"====ARTrackedImageManager==== OnTrackedImageRemoved: {removedTrackedImages[i]}";
                            logText.text = msg;
                            RKLog.KeyInfo(msg);
                        }
                        OnTrackedImageRemoved?.Invoke(removedTrackedImages[i]);
                        if (trackedImageObjDict.TryGetValue(removedTrackedImages[i].index, out ARTrackedImageObj imageObj))
                        {
                            imageObj.Removed(removedTrackedImages[i].index);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Register ARTrackedImageObj
        /// </summary>
        /// <param name="imageObj"></param>
        public void RegisterImageTrackedObj(ARTrackedImageObj imageObj)
        {
            if (trackedImageObjDict.ContainsKey(imageObj.trackedImageIndex))
            {
                RKLog.Error($"Image index duplication index:{imageObj.trackedImageIndex} ");
            }
            else
            {
                trackedImageObjDict.Add(imageObj.trackedImageIndex, imageObj);
            }
        }

        /// <summary>
        /// UnRegister ARTrackedImageObj
        /// </summary>
        public void UnRegisterImageTrackedObj(int imageIndex)
        {
            trackedImageObjDict.Remove(imageIndex);
        }

        protected override void OnDestroy()
        {
            if (enableImageTracker)
                CloseImageTracker();
        }

        /// <summary>
        /// Close image tracker
        /// </summary>
        public void CloseImageTracker()
        {
            NativeInterface.NativeAPI.CloseImageTracker();
            enableImageTracker = false;
        }

        /// <summary>
        /// Async Close image tracker
        /// </summary>
        public void AsyncCloseImageTracker(Action success)
        {
            Loom.RunAsync(CloseImageTracker, success);
        }


        /// <summary>
        /// Open or create image database
        /// </summary>
        public void OpenOrCreateTrackedImageDB(Action success, Action<string> failed)
        {
            if (createCheckDatabase)
            {
                createCheckDatabase = false;
                NativeInterface.NativeAPI.QualityDestroy();
            }
            AndroidJavaObject activity = null;
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
            if (activity != null)
            {
                NativeInterface.NativeAPI.QualityCreate(activity.GetRawObject());
                createCheckDatabase = true;
            }
            RecoverOrClearTempFile(() =>
            {
                openOrCreateImageTrackerDB = true;
                if (File.Exists(dbDataPath))
                {
                    Loom.RunAsync(() =>
                    {
                        string data = File.ReadAllText(dbDataPath);
                        ImageCheckResult quality = ImageCheckResult.GOOD;
                        ARDBImage[] arDBImages = JsonConvert.DeserializeObject<ARDBImage[]>(data);
                        foreach (ARDBImage item in arDBImages)
                        {
                            if (arDBImageDict.ContainsKey(item.index))
                            {
                                arDBImageDict[item.index] = item;
                            }
                            else
                            {
                                ARDBImage dbImage = new ARDBImage(item);
                                quality = dbImage.CheckImageQuality(dbImage.imagePath);
                                RKLog.KeyInfo($"====ARTrackedImageManager====: CheckImageQuality: {dbImage.imagePath},{quality}");
                                if (quality != ImageCheckResult.GOOD)
                                {
                                    failed?.Invoke($"====ARTrackedImageManager====: CheckImageQuality Failed: {dbImage.imagePath},{quality}");
                                    break;
                                }
                                else
                                {
                                    arDBImageDict.Add(dbImage.index, dbImage);
                                }
                            }
                        }
                        if (quality == ImageCheckResult.GOOD)
                        {
                            Loom.QueueOnMainThread(() =>
                            {
                                RKLog.KeyInfo($"====ARTrackedImageManager====:CheckImageQuality Success");
                                success?.Invoke();
                            });
                        }
                    });
                }
                else
                {
                    success?.Invoke();
                }
            }, msg =>
            {
                failed?.Invoke(msg);
            });
        }

        /// <summary>
        /// Close image database
        /// </summary>
        public void CloseTrackedImageDB()
        {
            NativeInterface.NativeAPI.QualityDestroy();
            createCheckDatabase = false;
            arDBImageDict.Clear();
        }

        /// <summary>
        /// Add image data to database
        /// </summary>
        /// <param name="arDBImage">The data obj in image database</param>
        public void AddDBImage(ARDBImage arDBImage)
        {
            if (openOrCreateImageTrackerDB)
            {
                RKLog.KeyInfo($"====ARTrackedImageManager====:AddDBImage: {arDBImage.ToString()}");
                if (!arDBImageDict.ContainsKey(arDBImage.index))
                {
                    arDBImageDict.Add(arDBImage.index, arDBImage);
                }
                else
                {
                    RKLog.Error($"====ARTrackedImageManager==== The AddDBImage index has contain : " + arDBImage.ToString());
                }
            }
        }
        /// <summary>
        /// Update image data to database
        /// </summary>
        /// <param name="arDBImage">The data obj in image database</param>
        public void UpdateDBImage(ARDBImage arDBImage)
        {
            if (openOrCreateImageTrackerDB)
            {
                RKLog.KeyInfo($"====ARTrackedImageManager====:UpdateDBImage: {arDBImage.ToString()}");
                if (arDBImageDict.ContainsKey(arDBImage.index))
                {
                    if (arDBImage.IsValid())
                    {
                        arDBImageDict[arDBImage.index] = arDBImage;
                    }
                    else
                    {
                        RKLog.Error("====ARTrackedImageManager==== The UpdateDBImage data is not valid : " + arDBImage.ToString());
                    }
                }
                else
                {
                    AddDBImage(arDBImage);
                }
            }
        }


        /// <summary>
        /// Delete image data by index
        /// </summary>
        /// <param name="arDBImage">The data obj in image database</param>
        public void DeleteDBImageByIndex(int index)
        {
            if (openOrCreateImageTrackerDB)
            {
                RKLog.KeyInfo($"====ARTrackedImageManager====:DeleteDBImageByIndex: {index}");
                if (arDBImageDict.ContainsKey(index))
                {
                    if (arDBImageDict[index].IsValid())
                        arDBImageDict[index].RemoveImageQualityCheck(arDBImageDict[index].imagePath);
                    arDBImageDict.Remove(index);
                }
                else
                {
                    RKLog.Error($"====ARTrackedImageManager==== DeleteDBImageByIndex index is not contain " + index);
                }
            }
        }

        /// <summary>
        /// Get all image data from database
        /// </summary>
        /// <param name="success">success callback</param>
        /// <param name="failed">failed callback</param>
        public List<ARDBImage> GetDBImages()
        {
            if (openOrCreateImageTrackerDB)
            {
                List<ARDBImage> data = new List<ARDBImage>();
                foreach (var item in arDBImageDict.Values)
                {
                    data.Add(item);
                }
                return data;
            }
            return null;
        }

        public bool ContainsImage(string imagePath)
        {
            foreach (var item in arDBImageDict.Values)
            {
                if (item.imagePath == imagePath)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetLastIndex()
        {
            if (arDBImageDict != null && arDBImageDict.Count > 0)
                return arDBImageDict.Last().Value.index + 1;
            return 1;
        }

        private enum DatabaseOperationStatus
        {
            None,
            Opened,
            FinishGenerateNewDBData,
            Closed
        }

        private void SetDBStatus(DatabaseOperationStatus dbStatus)
        {
            Loom.QueueOnMainThread(() =>
            {
                PlayerPrefs.SetInt("RokidImage_DatabaseOperationStatus", (int)dbStatus);
            });
        }

        /// <summary>
        /// Save image database
        /// </summary>
        public void SaveTrackedImageDB(Action success, Action<string> failed)
        {
            if (openOrCreateImageTrackerDB)
            {
                var dbFolderPath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER);
                var tempDBFolderPath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER_TEMP);
                var tempDBCorePath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER_TEMP, ARTrackedImageConstant.DB_CORE);
                var tempDbDataPath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER_TEMP, ARTrackedImageConstant.DB_DATA);
                Loom.RunAsync(() =>
                {
                    SetDBStatus(DatabaseOperationStatus.Opened);
                    //0.创建临时文件
                    if (Directory.Exists(tempDBFolderPath))
                        Directory.Delete(tempDBFolderPath, true);
                    Directory.CreateDirectory(tempDBFolderPath);
                    //1.生成核心数据库
                    int result = NativeInterface.NativeAPI.CreateImageDB();
                    if (result != 0)
                    {
                        SetDBStatus(DatabaseOperationStatus.None);
                        string error = $"====ARTrackedImageManager==== CreateImageDB Error:{(MarkerDBOperationStatus)result}";
                        RKLog.Error(error);
                        return error;
                    }
                    Utils.LogFuncTimeStamp(() =>
                    {
                        foreach (var item in arDBImageDict.Values)
                        {
                            if (!item.IsValid())
                            {
                                result = (int)MarkerDBOperationStatus.IMAGE_DATA_INVALID;
                                break;
                            }
                            result = NativeInterface.NativeAPI.AddDBImage(item);
                            if (result != 0)
                            {
                                break;
                            }
                        }
                    }, "TimeTest AddDBImage");
                    if (result != 0)
                    {
                        SetDBStatus(DatabaseOperationStatus.None);
                        string error = $"====ARTrackedImageManager==== AddDBImage Error:{(MarkerDBOperationStatus)result}";
                        RKLog.Error(error);
                        NativeInterface.NativeAPI.DestroyImageDB();
                        return error;
                    }
                    if (!File.Exists(tempDBCorePath))
                    {
                        File.Create(tempDBCorePath);
                    }
                    Utils.LogFuncTimeStamp(() =>
                    {
                        result = NativeInterface.NativeAPI.SaveImageDB(tempDBCorePath);
                    }, "TimeTest SaveImageDB");
                    if (result != 0)
                    {
                        SetDBStatus(DatabaseOperationStatus.None);
                        string error = $"====ARTrackedImageManager==== SaveImageDB Error:{(MarkerDBOperationStatus)result}";
                        RKLog.Error(error);
                        return error;
                    }
                    try
                    {
                        Utils.LogFuncTimeStamp(() =>
                        {
                            //2.写入Json
                            if (!File.Exists(tempDbDataPath))
                                File.Create(tempDbDataPath).Close();
                            File.WriteAllText(tempDbDataPath, JsonConvert.SerializeObject(arDBImageDict.Values, Formatting.Indented));
                            //3.写入图片
                            foreach (var image in arDBImageDict.Values)
                            {
                                if (File.Exists(image.imagePath) && !File.Exists(image.imageTempPath))
                                {
                                    File.Copy(image.imagePath, image.imageTempPath);
                                }
                            }
                            SetDBStatus(DatabaseOperationStatus.FinishGenerateNewDBData);
                            //4.移动文件
                            if (Directory.Exists(dbFolderPath))
                            {
                                Directory.Delete(dbFolderPath, true);
                            }
                            Directory.Move(tempDBFolderPath, dbFolderPath);
                            SetDBStatus(DatabaseOperationStatus.Closed);
                        }, "TimeTest Copy File");
                    }
                    catch (System.Exception e)
                    {
                        SetDBStatus(DatabaseOperationStatus.Opened);
                        string error = e.ToString();
                        RKLog.Error(error);
                        return error;
                    }
                    return null;
                }, success, failed);
            }
        }

        /// <summary>
        /// 数据库操作失败后重启,恢复操作
        /// </summary>
        /// <param name="success"></param>
        private void RecoverOrClearTempFile(Action success, Action<string> failed)
        {
            DatabaseOperationStatus dbOperationStatus = (DatabaseOperationStatus)PlayerPrefs.GetInt("RokidImage_DatabaseOperationStatus", (int)DatabaseOperationStatus.None);
            RKLog.KeyInfo("====RecoverOrClearTempFile====:" + dbOperationStatus);
            var dbFolderPath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER);
            var tempDBFolderPath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER_TEMP);
            bool hasError = false;
            try
            {
                switch (dbOperationStatus)
                {
                    case DatabaseOperationStatus.FinishGenerateNewDBData:
                        if (Directory.Exists(dbFolderPath))
                        {
                            Directory.Delete(dbFolderPath, true);
                        }
                        Directory.Move(tempDBFolderPath, dbFolderPath);
                        Loom.QueueOnMainThread(() =>
                        {
                            SetDBStatus(DatabaseOperationStatus.Closed);
                        });
                        break;
                    default:
                        if (Directory.Exists(tempDBFolderPath))
                        {
                            Directory.Delete(tempDBFolderPath, true);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                hasError = true;
                failed?.Invoke(e.ToString());
            }
            if (!hasError)
                success?.Invoke();
        }
    }
}
