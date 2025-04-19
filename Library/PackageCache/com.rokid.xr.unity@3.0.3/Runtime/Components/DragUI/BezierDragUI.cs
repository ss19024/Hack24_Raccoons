using System.Collections.Generic;
using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Components
{
    public class BezierDragUI : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IBezierCurveDrag
    {
        [SerializeField]
        public RectTransform canvas;//得到canvas的ugui坐标
        private RectTransform imgRect;
        private Vector2 offset;//临时记录点击点与UI的相对位置

        private bool IsDragging;

        void Start()
        {
            //初始化组件
            imgRect = GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // UnityEngine.RKLog.Info("On Drag");
            Vector2 mouseDrag = eventData.position; //当鼠标拖动时的屏幕坐标
            Vector2 uguiPos; //用来接收转换后的拖动坐标
            bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, mouseDrag, eventData.enterEventCamera, out uguiPos);
            if (isRect && Mathf.Abs(uguiPos.x) < Screen.width && Mathf.Abs(uguiPos.y) < Screen.height)
            {
                //设置图片的ugui坐标与鼠标的ugui坐标保持不变
                if (!UseBezierCurve)
                {
                    imgRect.anchoredPosition = offset + uguiPos;
                }
                else
                {
                    anchoredPos = offset + uguiPos;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;

            AddBezierPointerData(eventData);

            //开始拖拽
            //这里必须先更换父物体否则,offset会计算出问题
            Vector2 mouseDown = eventData.position;
            Vector2 mouseUguiPos;
            bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, mouseDown, eventData.enterEventCamera, out mouseUguiPos);
            if (isRect && Mathf.Abs(mouseUguiPos.x) < Screen.width && Mathf.Abs(mouseUguiPos.y) < Screen.height)
            {
                offset = imgRect.anchoredPosition - mouseUguiPos;
            }
            else
            {
                offset = imgRect.anchoredPosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            RKLog.Info("On End Drag");
            IsDragging = false;
            RemoveBezierPointerData(eventData);
        }

        #region BezierCurve
        [SerializeField]
        private bool UseBezierCurve = true;
        [SerializeField]
        private float moveLerpTime = 0.05f;

        private Dictionary<int, BezierPointerData> bezierPointerDatas = new Dictionary<int, BezierPointerData>();

        private Vector2 anchoredPos;

        public GameObject targetObj => this.gameObject;

        private void AddBezierPointerData(PointerEventData eventData)
        {
            BezierPointerData bezierPointerData = new BezierPointerData();
            bezierPointerData.pointerId = eventData.pointerId;
            bezierPointerData.hitLocalNormal = transform.InverseTransformVector(eventData.pointerCurrentRaycast.worldNormal);
            bezierPointerData.hitLocalPos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            bezierPointerDatas.Add(bezierPointerData.pointerId, bezierPointerData);
        }

        private void RemoveBezierPointerData(PointerEventData eventData)
        {
            bezierPointerDatas.Remove(eventData.pointerId);
        }

        private BezierPointerData GetBezierPointerData(int pointerId)
        {
            if (bezierPointerDatas.TryGetValue(pointerId, out BezierPointerData bezierPointerData))
            {
                return bezierPointerData;
            }
            else
            {
                RKLog.KeyInfo($"====BezierDragUI====: Can not find pointerId {pointerId}");
                return null;
            }
        }

        public bool IsEnablePinchBezierCurve()
        {
            return UseBezierCurve;
        }

        public bool IsEnableGripBezierCurve()
        {
            return false;
        }

        public bool IsInBezierCurveDragging()
        {
            return IsDragging;
        }

        public Vector3 GetBezierCurveEndPoint(int pointerId)
        {
            BezierPointerData pointerData = GetBezierPointerData(pointerId);
            if (pointerData != null)
            {
                return transform.TransformPoint(pointerData.hitLocalPos);
            }
            else
            {
                return transform.TransformPoint(Vector3.zero);
            }
        }

        private void Update()
        {
            if (UseBezierCurve && IsDragging)
            {
                imgRect.anchoredPosition = Vector2.Lerp(imgRect.anchoredPosition, anchoredPos, 1f - Mathf.Pow(moveLerpTime, Time.deltaTime));
            }
        }

        public Vector3 GetBezierCurveEndNormal(int pointerId)
        {
            return transform.forward;
        }

        #endregion
    }
}