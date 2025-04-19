using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rokid.UXR
{
    /// <summary>
    /// Auto inject component
    /// </summary>
    public class AutoInjectComponent
    {
        /// <summary>
        /// Find method
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="filedInfos"></param>
        /// <param name="obj"></param>
        private static void Find(Transform transform, Dictionary<FieldInfo, string> filedInfos, object obj, Dictionary<FieldInfo, string> flag)
        {
            foreach (Transform child in transform)
            {
                foreach (KeyValuePair<FieldInfo, string> pair in filedInfos)
                {
                    var fieldName = pair.Key.ToString().Split(' ')[1].ToLower();
                    string objName = string.IsNullOrEmpty(pair.Value) ? fieldName : pair.Value;
                    if (objName.ToLower() == child.name.ToLower())
                    {
                        string[] names = pair.Key.FieldType.ToString().Split('.');
                        string typeName = names[names.Length - 1];
                        if (typeName.ToLower() == "gameobject")
                        {
                            pair.Key.SetValue(obj, child.gameObject);
                        }
                        else
                        {
                            pair.Key.SetValue(obj, child.GetComponent(typeName));
                        }
                        flag.Add(pair.Key, pair.Value);
                    }
                }
                foreach (var key in flag.Keys)
                {
                    filedInfos.Remove(key);
                }
                flag.Clear();
                if (child.childCount > 0 && filedInfos.Count > 0)
                {
                    Find(child, filedInfos, obj, flag);
                }
            }
        }

        /// <summary>
        /// 自动注入组件(通过Autowrited定义的对象名(如果对象名为空,则使用字段名)和场景的中的对象名称匹配选择) 
        /// 注意名称查找不区分大小写
        /// </summary>
        /// <param name="tsf">绑定组件的对象tsf</param>
        /// <param name="obj">组件实例</param>
        public static void AutoInject(Transform tsf, object obj)
        {
            var type = obj.GetType();
            List<FieldInfo> allFiledInfos = new List<FieldInfo>();
            Dictionary<FieldInfo, string> needInjectFieldInfos = new Dictionary<FieldInfo, string>();
            allFiledInfos.AddRange(type.GetFields().ToList()); //找到类中的所有的公共字段
            allFiledInfos.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).ToList()); //找到类中的所有的私有字段
            for (int i = 0; i < allFiledInfos.Count; i++)
            {
                //获取所有的特性
                object[] objects = allFiledInfos[i].GetCustomAttributes(true);
                try
                {
                    for (int j = 0; j < objects.Length; j++)
                    {
                        if (objects[j].GetType() == typeof(Autowrited))
                        {
                            Autowrited autowrited = (Autowrited)objects[j];
                            needInjectFieldInfos.Add(allFiledInfos[i], autowrited.targetObjName);
                            break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    RKLog.Info(e.ToString());
                }
            }
            Find(tsf, needInjectFieldInfos, obj, new Dictionary<FieldInfo, string>());
            foreach (var fileInfo in needInjectFieldInfos.Keys)
            {
                if (fileInfo.GetValue(obj) == null)
                {
                    RKLog.Info(string.Format("{0},{1},没有找到目标组件", obj, fileInfo.ToString().Split(' ')[1]));
                }
            }
        }
    }
}
