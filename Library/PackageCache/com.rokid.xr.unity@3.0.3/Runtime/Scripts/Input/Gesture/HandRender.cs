using System.Collections.Generic;
using UnityEngine;
using Rokid.UXR.Utility;
using UnityEngine.Rendering;


namespace Rokid.UXR.Interaction
{
    public class HandRender : AutoInjectBehaviour
    {
        /// <summary>
        /// Is handtype
        /// </summary>
        public HandType handType;

        /// <summary>
        /// Is draw handmesh 
        /// </summary>
        public bool drawMesh = false;
        /// <summary>
        /// Is draw skeleton
        /// </summary>
        public bool drawSkeleton = true;
        /// <summary>
        /// Is draw hand root axis
        /// </summary>
        public bool drawHandRootAxis = false;
        /// <summary>
        /// Is show debug text
        /// </summary>
        public bool showDebugText = false;

        /// <summary>
        /// Bone node templates are used to generate bone
        /// </summary>
        public GameObject skeletonNode;
        /// <summary>
        /// Coordinate axis
        /// </summary>s
        [SerializeField, Autowrited("HandRootAxis")]
        private Transform handRootAxis;
        /// <summary>
        /// 手的渲染
        /// </summary>
        [SerializeField, Autowrited("RKHandVisual")]
        private HandVisual handVisual;
        /// <summary>
        /// 调试模式的手
        /// </summary>
        [SerializeField, Autowrited("HandGestureInEditor")]
        private Transform handVisualInEditor;
        private TextMesh debugText;
        private Dictionary<SkeletonIndexFlag, GameObject> skeletonDict = new Dictionary<SkeletonIndexFlag, GameObject>();


        private void Start()
        {

            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            Init();
#if UNITY_EDITOR
            handVisualInEditor?.gameObject.SetActive(true);
#else 
            Destroy(handVisualInEditor?.gameObject);
#endif
        }

        private void OnEnable()
        {
            GesEventInput.OnRenderHand += OnRenderHand;
        }

        private void OnDisable()
        {
            GesEventInput.OnRenderHand -= OnRenderHand;
        }

        private void Init()
        {
            if (Utils.IsAndroidPlatform() && drawMesh == false)
            {
                Destroy(handVisual.GetComponent<InputModuleSwitchActive>());
                handVisual.gameObject.SetActive(false);
            }
#if  UNITY_6000_0_OR_NEWER
            if (GraphicsSettings.defaultRenderPipeline != null)
#else
            if (GraphicsSettings.renderPipelineAsset != null)
#endif
            {
                handVisual.UpdateHandMeshMaterials(new Material[]{
                    Resources.Load<Material>("Materials/URP/RokidHand"), Resources.Load<Material>("Materials/URP/HandMesh_Finger")
                });
            }
            if (showDebugText)
            {
                if (debugText == null)
                {
                    debugText = new GameObject("_debug_text").AddComponent<TextMesh>();
                    debugText.fontSize = 120;
                    debugText.characterSize = 0.001f;
                    debugText.transform.parent = transform;
                    debugText.color = Color.green;
                    debugText.transform.localScale = Vector3.one;
                    debugText.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }
                debugText.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
        }

        private void OnTrackedFailed(HandType handType)
        {
            if (handType == this.handType || handType == HandType.None)
            {
                if (drawHandRootAxis)
                    handRootAxis.gameObject.SetActive(false);
                if (showDebugText)
                    debugText.gameObject.SetActive(false);
            }
        }

        private void OnRenderHand(HandType handType, GestureBean bean)
        {
            if (handType == this.handType)
            {
                if (bean.skeletons != null && bean.skeletons.Length > 0)
                {
                    if (drawSkeleton)
                        DrawSkeleton(bean.skeletons, bean.skeletonsRot);
                    if (drawHandRootAxis)
                    {
                        handRootAxis.gameObject.SetActive(true);
                        handRootAxis.transform.SetPose(GesEventInput.Instance.GetHandPose(handType));
                    }
                }
            }
        }

        void DrawSkeleton(Vector3[] skeletons, Quaternion[] rotations)
        {
            if (skeletons.Length == 0 || drawSkeleton == false)
            {
                return;
            }
            //绘制骨骼点
            for (int i = 0; i < skeletons.Length; i++)
            {
                GameObject go = null;
                SkeletonIndexFlag indexFlag = (SkeletonIndexFlag)i;
                if (skeletonDict.ContainsKey(indexFlag))
                {
                    go = skeletonDict[indexFlag];
                }
                else
                {
                    go = GameObject.Instantiate(skeletonNode);
                    go.GetComponent<MeshRenderer>().enabled = true;
                    go.name = indexFlag.ToString();
                    go.transform.SetParent(transform);
                    skeletonDict.Add(indexFlag, go);
                }
                go.gameObject.SetActive(true);
                go.transform.position = skeletons[i];
                go.transform.rotation = rotations[i];
            }
        }

        private void Update()
        {
            if (showDebugText)
            {
                debugText.gameObject.SetActive(true);
                debugText.transform.SetPose(GesEventInput.Instance.GetHandPose(handType));

                if (handType == HandType.RightHand)
                {
                    debugText.transform.position += new Vector3(-0.05f, 0.1f, 0.0f);
                    debugText.alignment = TextAlignment.Right;
                    debugText.anchor = TextAnchor.UpperRight;
                }
                else
                {
                    debugText.transform.position += new Vector3(0.05f, 0.1f, 0.0f);
                    debugText.alignment = TextAlignment.Left;
                    debugText.anchor = TextAnchor.UpperLeft;
                }

                debugText.text = string.Format(
                    "WristPos: {0}\n" +
                    "GestureType: {1}\n" +
                    "HandType: {2}\n" +
                    "HandOrientation: {3}\n" +
                    "HandRotation: {4}\n",
                    GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.WRIST, handType).position.ToString("0.000"),
                    GesEventInput.Instance.GetGesture(handType).gesType.ToString(),
                    handType.ToString(),
                    GesEventInput.Instance.GetGesture(handType).handOrientation == 0 ? HandOrientation.Palm : HandOrientation.Back,
                    GesEventInput.Instance.GetHandPose(handType).rotation.eulerAngles.ToString("0.000"));
            }
            else
            {
                if (debugText != null)
                {
                    Destroy(debugText.gameObject);
                }
            }
        }
    }
}
