using System.Net.Mime;

namespace Rokid.UXR.Utility {
	//
	// Licensed under the Apache License, Version 2.0 (the "License");
	// you may not use this file except in compliance with the License.
	// You may obtain a copy of the License at
	//
	//     http://www.apache.org/licenses/LICENSE-2.0
	//
	// Unless required by applicable law or agreed to in writing, software
	// distributed under the License is distributed on an "AS IS" BASIS,
	// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	// See the License for the specific language governing permissions and
	// limitations under the License.
	
using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Interaction;
	
	[RequireComponent(typeof(Text))]
	public class GesFPS : MonoBehaviour
	{
	    private Text textField;
	    private float fps = 60;
	
	
	    void Start()
	    {
	        textField = GetComponent<Text>();
	        GesEventInput.OnGesDataUpdate += OnGesDataUpdate;
	    }
	
	    private void OnDestroy()
	    {
	        GesEventInput.OnGesDataUpdate -= OnGesDataUpdate;
	    }
	
	    void OnGesDataUpdate(float delta)
	    {
	        string text = "GesFPS: ";
	        float interp = delta / (0.5f + delta);
	        float currentFPS = 1.0f / delta;
	        fps = Mathf.Lerp(fps, currentFPS, interp);
	        text += Mathf.RoundToInt(fps);
	        textField.text = text;
	        // RKLog.Info("GesFPS:" + text);
	    }
	}
}
