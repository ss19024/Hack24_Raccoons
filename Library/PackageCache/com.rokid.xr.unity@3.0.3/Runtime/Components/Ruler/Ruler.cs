using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace Rokid.UXR.Components {
	[ExecuteInEditMode]
	public class Ruler : MonoBehaviour
	{
	    /// <summary>
	    /// 缓存直尺的刻度
	    /// </summary>
	    /// <typeparam name="int"></typeparam>
	    /// <typeparam name="GameObject"></typeparam>
	    /// <returns></returns>
	    public Dictionary<int, GameObject> dialDict = new Dictionary<int, GameObject>();
	
	    /// <summary>
	    /// 直尺长度
	    /// </summary>
	    [Range(1, 15)]
	    public int length = 1;
	
	    /// <summary>
	    /// 直尺刻度显示
	    /// </summary>
	    public GameObject tempDial;
	
	    public Transform ruler;
	
	    private float oldLength;
	
	    private void Update()
	    {
	        if (oldLength != length)
	        {
	            oldLength = length;
	            //更新直尺长度
	            ShowDial(length);
	
	            ruler.transform.localScale = new Vector3(0.4f, length, 1);
	        }
	    }
	
	    /// <summary>
	    /// 显示直尺的刻度
	    /// </summary>
	    private void ShowDial(int length)
	    {
	        foreach (var item in dialDict.Values)
	        {
	            if (item == null || item.gameObject == null)
	                continue;
	            item.gameObject.SetActive(false);
	        }
	        for (int i = 1; i < length; i++)
	        {
	            if (dialDict.ContainsKey(i) && dialDict[i].gameObject != null)
	            {
	                dialDict[i].gameObject.SetActive(true);
	            }
	            else
	            {
	                //先查找是否已经实例化tip如果
	                Transform tsf = transform.Find(i + "m");
	                if (tsf != null)
	                {
	                    if (dialDict.ContainsKey(i))
	                    {
	                        dialDict[i] = tsf.gameObject;
	                    }
	                    else
	                    {
	                        dialDict.Add(i, tsf.gameObject);
	                    }
	                    return;
	                }
	                GameObject go = GameObject.Instantiate(tempDial, new Vector3(0, 0.001f, i), Quaternion.Euler(90, 0, 0));
	                go.SetActive(true);
	                go.transform.SetParent(this.transform);
	                go.transform.localPosition = new Vector3(0, 0.001f, i);
	                if (dialDict.ContainsKey(i))
	                {
	                    dialDict[i] = go;
	                }
	                else
	                {
	                    dialDict.Add(i, go);
	                }
	                go.GetComponentInChildren<Text>().text = i + "m";
	                go.name = i + "m";
	            }
	        }
	    }
	}
}
