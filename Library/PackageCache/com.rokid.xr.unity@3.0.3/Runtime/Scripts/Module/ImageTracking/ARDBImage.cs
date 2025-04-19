using System;
using System.IO;
using Newtonsoft.Json;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Module
{

    internal enum ImageCheckResult
    {
        GOOD = 0,
        ADD_INVALID_SIZE = -1,
        ADD_INVALID_ID = -2,
        ADD_MEM_EXT = -3,
        ADD_UNKNOWN_EXCEPT = -5,
        ERROR_MODEL = -6,
        ADD_NO_ENOUGH_KEYS = -7,
        ADD_REPETITION = -8,
        INVALID_IMAGE_SIZE = -9,
        PARAM_ERROR = -10,
        REBOOT = -11,
        IMAGE_EMPTY = -101,
        INVALID_FILE = -102,
        INVALID_DATA = -103,
        EMPTY_POINTER = -104
    }

    public enum OperationType
    {
        None,
        Add,
        Delete,
        Update
    }

    public class ARDBImage
    {
        public int index;
        public string guid;
        public string imageName;
        public string imageExtension;
        public bool specifySize;
        public float physicalWidth;
        public float physicalHeight;

        [JsonIgnore]
        private OperationType _operationType = OperationType.None;
        [JsonIgnore]
        public OperationType operationType
        {
            get
            {
                return _operationType;
            }
            set
            {
                _operationType = value;
            }
        }

        [JsonIgnore]
        public Texture2D texture;

        [JsonIgnore]
        public string imagePath;
        [JsonIgnore]
        public string imageTempPath;
        [JsonIgnore]
        private bool addedToQualityData;

        public ARDBImage()
        {

        }

        public ARDBImage(int index, OperationType operationType = OperationType.None)
        {
            this.index = index;
            this.operationType = operationType;
            this.imageName = "";
        }

        public ARDBImage(ARDBImage image, OperationType operationType = OperationType.None)
        {
            this.index = image.index;
            this.guid = image.guid;
            this.imageName = image.imageName;
            this.imageExtension = image.imageExtension;
            this.physicalWidth = image.physicalWidth;
            this.physicalHeight = image.physicalHeight;
            this.specifySize = image.specifySize;
            this.operationType = operationType;
            this.imagePath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER, guid) + imageExtension;
            this.imageTempPath = Path.Combine(Application.persistentDataPath, ARTrackedImageConstant.DB_FOLDER_TEMP, guid) + imageExtension;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(imageName) && (texture != null || File.Exists(imagePath));
        }

        internal ImageCheckResult CheckImageQuality(string imagePath)
        {
            if (ARTrackedImageManager.Instance.ContainsImage(imagePath))
            {
                return ImageCheckResult.ADD_REPETITION;
            }
            ImageCheckResult result = (ImageCheckResult)NativeInterface.NativeAPI.CheckImageQuality(imagePath);
            if (result == ImageCheckResult.GOOD)
            {
                addedToQualityData = true;
            }
            else
            {
                RemoveImageQualityCheck(imagePath);
            }
            return result;
        }

        internal void RemoveImageQualityCheck(string imagePath)
        {
            addedToQualityData = false;
            NativeInterface.NativeAPI.RemoveImageQualityCheck(imagePath);
        }

        public void CheckImageQuality(string imagePath, Action success, Action<string> failed)
        {
            Loom.RunAsync(() =>
            {
                if (addedToQualityData)
                    RemoveImageQualityCheck(this.imagePath);
                ImageCheckResult quality = CheckImageQuality(imagePath);
                if (quality == ImageCheckResult.GOOD)
                {
                    Loom.QueueOnMainThread(() =>
                    {
                        success?.Invoke();
                    });
                }
                else
                {
                    Loom.QueueOnMainThread(() =>
                    {
                        failed?.Invoke($"Image Quality check failed :{quality},{imagePath}");
                    });
                }
            });
        }

        public override string ToString()
        {
            return $"index:{index},guid:{guid},imageName:{imageName},imageExtension:{imageExtension},operation:{operationType},imagePath:{imagePath},specifySize:{specifySize},imageTempPath:{imageTempPath}";
        }
    }
}

