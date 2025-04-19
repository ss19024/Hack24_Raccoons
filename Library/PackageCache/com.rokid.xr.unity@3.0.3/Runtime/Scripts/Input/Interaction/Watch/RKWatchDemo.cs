using UnityEngine.UI;
using UnityEngine;
namespace Rokid.UXR.Interaction
{
    public class RKWatchDemo : MonoBehaviour
    {
        [SerializeField]
        private HandType hand;
        [SerializeField]
        private Text logText;
        private Color[] watchColors = new Color[] { Color.red, Color.green, Color.blue };
        private int colorIndex = 0;
        private Vector3 oriScale;
        private Material watchMat;
        private bool active;
        private bool stateChange;

        void Awake()
        {
            RKHandWatch.OnActiveWatch += OnActiveWatch;
            RKHandWatch.OnWatchPoseUpdate += OnWatchPoseUpdate;
        }

        private void Start()
        {
            this.gameObject.SetActive(false);
            watchMat = GetComponent<MeshRenderer>()?.material;
            oriScale = transform.localScale;
        }

        private void OnDestroy()
        {
            RKHandWatch.OnActiveWatch -= OnActiveWatch;
            RKHandWatch.OnWatchPoseUpdate -= OnWatchPoseUpdate;
        }

        private void OnDisable()
        {
            colorIndex = 0;
            watchMat?.SetColor("_Color", watchColors[colorIndex]);
        }

        private void OnWatchPoseUpdate(HandType hand, Pose pose)
        {
            if (hand == this.hand)
                transform.SetPose(pose);
        }

        private void OnActiveWatch(HandType hand, bool active)
        {
            if (hand == this.hand)
            {
                if (this.active != active && active == true)
                {
                    stateChange = true;
                }
                this.active = active;
                this.gameObject.SetActive(active);
            }
        }

        private Pose GetSkeletonPose(SkeletonIndexFlag index, HandType hand)
        {
            return GesEventInput.Instance.GetSkeletonPose(index, hand);
        }

        private void Update()
        {
            if (active == true && stateChange == true)
            {
                stateChange = false;
                return;
            }
            if (GesEventInput.Instance.GetHandDown(hand, false))
            {
                colorIndex++;
                if (colorIndex == watchColors.Length)
                {
                    colorIndex = 0;
                }
                // 处理切换逻辑
                watchMat.SetColor("_Color", watchColors[colorIndex]);
            }
        }
    }
}