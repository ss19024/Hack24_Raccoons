using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rokid.UXR.Utility {
	[DisallowMultipleComponent]
	public class CanvasRegister : MonoBehaviour
	{
	    public static List<Canvas> canvasList = new List<Canvas>();
	    private Canvas m_Canvas;
	
	    private void Start()
	    {
	
	    }
	    private void OnEnable()
	    {
	        if (m_Canvas != null)
	            canvasList.Add(m_Canvas);
	    }
	
	    private void OnDisable()
	    {
	        if (m_Canvas != null)
	            canvasList.Remove(m_Canvas);
	    }
	    public void Awake()
	    {
	        m_Canvas = this.GetComponent<Canvas>();
	    }
	}
}
