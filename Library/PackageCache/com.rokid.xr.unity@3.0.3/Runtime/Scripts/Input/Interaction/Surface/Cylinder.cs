using UnityEngine;

namespace Rokid.UXR.Interaction {
	public class Cylinder : MonoBehaviour
	{
	    [SerializeField]
	    private float _radius = 1f;
	
	    public float Radius
	    {
	        get => _radius;
	        set => _radius = value;
	    }
	    
	    [SerializeField]
	    private float _CylinderCanvasScaleWidth = 1f;

	    public float CylinderCanvasScaleWidth
	    {
		    get => _CylinderCanvasScaleWidth;
		    set
		    {
			    _CylinderCanvasScaleWidth = value;
			    if (_CylinderCanvasScaleWidth < 1.0f)
			    {
				    _CylinderCanvasScaleWidth = 1.0f;
			    }
		    }
	    }

	    [SerializeField]
	    private float _CylinderCanvasScaleHeight = 1f;
	    
	    public float CylinderCanvasScaleHeight
	    {
		    get => _CylinderCanvasScaleHeight;
		    set
		    {
			    _CylinderCanvasScaleHeight = value;
			    if (_CylinderCanvasScaleHeight < 1.0f)
			    {
				    _CylinderCanvasScaleHeight = 1.0f;
			    }
		    }
	    }
	}
}
