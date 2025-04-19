using System;
using UnityEngine;

namespace Rokid.UXR.Module
{
	public class OnLoadCompleteListener : AndroidJavaProxy
	{
		public Action<int> Callback;


		public OnLoadCompleteListener(Action<int> callback) : base("android.media.SoundPool$OnLoadCompleteListener")
		{
			Callback = callback;
		}


		void onLoadComplete(AndroidJavaObject soundPool, int sampleId, int status)
		{
			Callback(sampleId);
		}
	}
}
