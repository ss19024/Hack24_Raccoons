using System;
using System.Collections.Generic;
using Rokid.UXR.Utility;
using UnityEngine;
using JsonUtils = Newtonsoft.Json.JsonConvert;

namespace Rokid.UXR.Interaction
{
    public struct TrackingHandData
    {
        public float[] verts_ndc;
        public float[] verts_cam;
        public float[] skeleton_ndc;
        public float[] quaternion;
        public float[] euler;
        public float[] rotation_axis;
        public int lr_hand;
        public int gesture_type;
        public int hand_orientation;
        public int is_pinch;
    };


    public struct TrackingFrameData
    {
        public TrackingHandData[] data;
        public int hand_num;
    };


    public class GestureMockInEditor : MonoBehaviour
    {
        [SerializeField]
        private TextAsset gestureDataCacheData;
        [SerializeField]
        private TextAsset gesFrameData;

        [SerializeField]
        private GesImplementation eventInput;
        private int gesFrame = 0;
        private List<TrackingFrameData> gestureFrame;

        [SerializeField]
        private List<GestureBean[]> gestureDataCache = new List<GestureBean[]>();

        private GestureBean[] gestureData = new GestureBean[2] { new GestureBean(), new GestureBean() };

        /// <summary>
        /// 模拟手势交互数据
        /// </summary>
        [SerializeField]
        private bool mockGesInteraction = true;

        /// <summary>
        /// 模拟手机数据
        /// </summary>
        [SerializeField]
        private bool mockPhoneData;

        private bool isRightHand = true;

        private bool mockGesInteractionRender;

        public List<TrackingFrameData> ReadTrackingFrameDataFromJSON(TextAsset gestureData)
        {
            return JsonUtils.DeserializeObject<List<TrackingFrameData>>(gestureData.ToString());
        }


#if UNITY_EDITOR
        private void Start()
        {
            if (Utils.IsAndroidPlatform())
            {
                mockGesInteraction = false;
                mockPhoneData = false;
            }
            if (mockPhoneData)
            {
                gestureDataCacheData = Resources.Load<TextAsset>("3DHandData/gesDataRecord");
                gestureDataCache = JsonUtils.DeserializeObject<List<GestureBean[]>>(gestureDataCacheData.ToString());
            }
            if (eventInput == null)
                eventInput = GetComponent<GesImplementation>();
            ChangeHand();
        }

        private void LateUpdate()
        {
            if (mockGesInteraction)
                MockGesInteraction();
            if (mockPhoneData)
                MockPhoneData();
        }

#endif

        private void ChangeHand()
        {
            isRightHand = !isRightHand;
            DataCache.Instance.Add("ThreeGesMockInEditor_ShowHandType",
            isRightHand ? HandType.RightHand : HandType.LeftHand, true);
        }

        /// <summary>
        /// 模拟手机数据
        /// </summary>
        private void MockPhoneData()
        {
            if (gesFrame >= gestureDataCache.Count - 1)
            {
                gesFrame = 0;
            }
            try
            {
                eventInput.ProcessData(gestureDataCache[gesFrame], 1);
            }
            catch (Exception e)
            {
                RKLog.Info(e.ToString());
            }
            // RKLog.Info("gesFrame:" + gesFrame);
            gesFrame++;
        }


        /// <summary>
        /// 模拟手势交互
        /// </summary>
        private void MockGesInteraction()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                ChangeHand();
            }
            gestureData[0].hand_type = isRightHand ? (int)HandType.RightHand : (int)HandType.LeftHand;
            gestureData[0].gesture_type = (int)GestureType.None;
            gestureData[0].hand_orientation = 1;
            GestureType gesType = GestureType.None;
            if (Input.GetKey(KeyCode.X))
            {
                //模拟Grip/Palm
                if (Input.GetMouseButtonDown(0))
                {
                    gesType = GestureType.Grip;
                }
                else if (Input.GetMouseButton(0))
                {
                    gesType = GestureType.Grip;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    gesType = GestureType.Palm;
                }
                else
                {
                    gesType = GestureType.Palm;
                }
            }
            else if (Input.GetKeyUp(KeyCode.X))
            {
                gesType = GestureType.Palm;
            }
            else
            {
                //模拟Pinch/UnPinch
                if (Input.GetMouseButtonDown(0))
                {
                    gesType = GestureType.Pinch;
                }
                else if (Input.GetMouseButton(0))
                {
                    gesType = GestureType.Pinch;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    gesType = GestureType.None;
                }
                else
                {
                    gesType = GestureType.None;
                }
            }
            if (Input.GetKey(KeyCode.Space))
            {
                eventInput.ProcessData(null, 0);
            }
            else
            {
                gestureData[0].gesture_type = (int)gesType;
                eventInput.ProcessData(gestureData, 1);
            }
        }
    }
}