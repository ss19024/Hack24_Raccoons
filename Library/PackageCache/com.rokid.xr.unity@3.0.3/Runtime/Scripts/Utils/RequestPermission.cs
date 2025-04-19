using System;
using UnityEngine;
using UnityEngine.Android;

namespace Rokid.UXR.Utility {
	public class RequestPermission : MonoBehaviour
	{
	    private void Awake()
	    {
	        if (!Permission.HasUserAuthorizedPermission("android.permission.CAMERA"))
	            Permission.RequestUserPermission("android.permission.CAMERA");
	    }
	}
}
