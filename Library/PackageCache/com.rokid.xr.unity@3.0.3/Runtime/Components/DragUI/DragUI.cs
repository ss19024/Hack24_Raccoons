using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Components {

    public class DragUI : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {

        [SerializeField]
        public RectTransform canvas;//得到canvas的ugui坐标
        [SerializeField]
        public Transform frontParent;
        private RectTransform imgRect;
        private CanvasGroup canvasGroup;
        private Transform oldParent;
        private Transform nowParent;
        public int sibiling;
        private Vector2 offset;//临时记录点击点与UI的相对位置

        void Start()
        {
            //初始化组件
            imgRect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            sibiling = transform.GetSiblingIndex();
            oldParent = transform.parent;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // UnityEngine.RKLog.Info("On Drag");
            Vector2 mouseDrag = eventData.position; //当鼠标拖动时的屏幕坐标
            Vector2 uguiPos; //用来接收转换后的拖动坐标
            bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, mouseDrag, eventData.enterEventCamera, out uguiPos);
            if (isRect)
            {
                //设置图片的ugui坐标与鼠标的ugui坐标保持不变
                imgRect.anchoredPosition = offset + uguiPos;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //开始拖拽
            canvasGroup.blocksRaycasts = false;
            //这里必须先更换父物体否则,offset会计算出问题
            nowParent = transform.parent;
            transform.SetParent(frontParent);
            Vector2 mouseDown = eventData.position;
            Vector2 mouseUguiPos;
            bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, mouseDown, eventData.enterEventCamera, out mouseUguiPos);
            if (isRect)
            {
                offset = imgRect.anchoredPosition - mouseUguiPos;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            RKLog.Info("On End Drag");
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.name == "Grid")
                {
                    Transform targetParent = eventData.pointerCurrentRaycast.gameObject.transform;
                    if (targetParent.childCount > 0)
                    {
                        RKLog.Info("Change Position");
                        //交换位置
                        Transform target = targetParent.GetChild(0);
                        target.SetParent(oldParent);
                        target.localPosition = Vector3.zero;
                    }
                    transform.SetParent(targetParent);
                    transform.localPosition = Vector3.zero;
                }
                else
                {
                    //返回
                    transform.SetParent(oldParent);
                    transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                //返回
                transform.SetParent(oldParent);
                transform.localPosition = Vector3.zero;
            }
            canvasGroup.blocksRaycasts = true;
        }
	}
}
