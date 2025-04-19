using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Rokid.UXR.Interaction
{

	/// <summary>
	/// 不考虑拖拽阈值的按钮
	/// </summary>
	public class RKButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		/// <summary>
		/// 点击事件
		/// </summary>
		[SerializeField]
		public float clickTime = 1f;

		/// <summary>
		/// 长按时间
		/// </summary>
		[SerializeField]
		public float longPressTime = 2f;
		[SerializeField, Tooltip("pointerDown->pointerExit过程会触发ButtonClick")]
		public bool exitTriggerClick = false;
		private float elapsedTime;
		private bool pointerDown;
		private bool pointerUp;
		private bool pointerExit;
		public UnityEvent<PointerEventData> onPointerClick = new UnityEvent<PointerEventData>();
		public UnityEvent<PointerEventData> onPointerDown = new UnityEvent<PointerEventData>();
		public UnityEvent<PointerEventData> onLongClick = new UnityEvent<PointerEventData>();
		private PointerEventData eventData;
		private Vector3 pointerDownPos;
		private Vector3 pointerExitPos;
		private float result;


		public void OnPointerDown(PointerEventData eventData)
		{
			this.eventData = eventData;
			pointerDownPos = new Vector3(eventData.pointerCurrentRaycast.worldPosition.x,
			eventData.pointerCurrentRaycast.worldPosition.y, eventData.pointerCurrentRaycast.worldPosition.z);
			// RKLog.Info("====RKButton==== pointerDownPos:" + pointerDownPos);
			pointerDown = true;
			pointerUp = false;
			onPointerDown?.Invoke(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerUp = true;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			pointerExit = false;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			pointerExitPos = new Vector3(eventData.pointerCurrentRaycast.worldPosition.x,
			eventData.pointerCurrentRaycast.worldPosition.y, eventData.pointerCurrentRaycast.worldPosition.z);
			// RKLog.Debug("====RKButton==== pointerExitPos:" + pointerExitPos);
			Vector3 exitForward = pointerExitPos - pointerDownPos;
			result = Vector3.Dot(exitForward.normalized, transform.forward);
			pointerExit = true;
		}

		private void OnDisable()
		{
			result = 0;
			pointerDownPos = Vector3.zero;
			pointerExitPos = Vector3.zero;
			elapsedTime = 0;
		}

		private void Update()
		{
			if (pointerDown)
			{
				elapsedTime += Time.deltaTime;
			}
			if (pointerDown && pointerUp && pointerExit == false)
			{
				pointerDown = false;
				pointerUp = false;
				pointerExit = false;
				if (elapsedTime < clickTime)
				{
					onPointerClick?.Invoke(eventData);
				}
				if (clickTime > longPressTime)
				{
					onLongClick?.Invoke(eventData);
				}
				eventData = null;
				elapsedTime = 0;
			}
			if (pointerExit)
			{
				if (exitTriggerClick && pointerDown && elapsedTime < clickTime && result != 0)
				{
					RKLog.Debug("====RKButton==== dot.result:" + result);
					onPointerClick?.Invoke(eventData);
				}
				if (clickTime > longPressTime)
				{
					onLongClick?.Invoke(eventData);
				}
				pointerDown = false;
				pointerUp = false;
				eventData = null;
				elapsedTime = 0;
			}
		}
	}
}
