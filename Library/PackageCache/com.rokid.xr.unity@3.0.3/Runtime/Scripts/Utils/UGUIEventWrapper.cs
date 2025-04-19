using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Utility {
	public class UGUIEventWrapper : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
	{
	    [SerializeField]
	    public UnityEvent WhenPointerDown;
	    [SerializeField]
	    public UnityEvent WhenPointerClick;
	
	    private void Start()
	    {
	
	    }
	    public void OnPointerDown(PointerEventData eventData)
	    {
	        WhenPointerDown?.Invoke();
	    }
	
	    public void OnPointerClick(PointerEventData eventData)
	    {
	        WhenPointerClick?.Invoke();
	    }
	}
}
