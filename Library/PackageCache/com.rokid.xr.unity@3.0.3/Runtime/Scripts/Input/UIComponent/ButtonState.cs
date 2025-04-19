using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
	public class ButtonState : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private Sprite pressed;
		[SerializeField]
		private Sprite highlight;
		[SerializeField]
		private Sprite normal;
		[SerializeField]
		private Image targetGraphic;
		[SerializeField]
		private Image hotArea;
		[SerializeField]
		private bool hovering;
		[SerializeField]
		private bool pointerDown;

		/// <summary>
		/// 默认 graphicScale
		/// </summary>
		private Vector3 oriGraphicScale;

		private Vector3 oriHotAreaScale;

		/// <summary>
		/// hover状态 graphicScale
		/// </summary>
		public float hoverScale = 1.0f;

		/// <summary>
		/// hover状态 hotArea
		/// </summary>
		public float hotAreaHoverScale = 1.0f;

		public float downScale = 1.0f;

		public void OnPointerDown(PointerEventData eventData)
		{
			pointerDown = true;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerDown = false;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hovering = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hovering = false;
			pointerDown = false;
		}

		private void Start()
		{
			oriGraphicScale = targetGraphic.transform.localScale;
			if (hotArea != null)
				oriHotAreaScale = hotArea.transform.localScale;
		}

		private void OnEnable()
		{
			OnPointerExit(null);
		}

		private void OnDisable()
		{
			OnPointerExit(null);
		}

		private void Update()
		{
			if (hovering && !pointerDown)
			{
				targetGraphic.sprite = highlight;
				targetGraphic.transform.localScale = oriGraphicScale * hoverScale;
				if (hotArea != null)
					hotArea.transform.localScale = oriHotAreaScale * hotAreaHoverScale;
			}
			else if (pointerDown)
			{
				targetGraphic.sprite = pressed;
				targetGraphic.transform.localScale = oriGraphicScale * downScale;
				if (hotArea != null)
					hotArea.transform.localScale = oriHotAreaScale * hotAreaHoverScale;
			}
			else
			{
				targetGraphic.sprite = normal;
				targetGraphic.transform.localScale = oriGraphicScale;
				if (hotArea != null)
					hotArea.transform.localScale = oriHotAreaScale;
			}
		}
	}
}
