using System.Net.Mime;

namespace Rokid.UXR.Utility
{
	using UnityEngine;
	using UnityEngine.UI;

	[RequireComponent(typeof(Text))]
	public class FPS : MonoBehaviour
	{
		private Text textField;
		private float fps = 60;

		void Awake()
		{
			textField = GetComponent<Text>();
		}

		void LateUpdate()
		{
			string text = "RenderFPS: ";
			float fps = 1.0f / Time.smoothDeltaTime;
			text += Mathf.RoundToInt(fps);
			textField.text = text;
			// RKLog.Debug("RenderFPS:" + text);
		}
	}
}
