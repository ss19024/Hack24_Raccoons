using System;
using UnityEngine;

namespace Rokid.UXR
{

    /// <summary>
    /// 自动注入特性,name 为空则使用字段名匹配,需要配合AutoInjectComponent 或 基础AutoInjectBehaviour 使用
    /// Tip1: 字符串匹配不区分大小写
    /// Tip2: 字段为空的情况下才会激活自动注入的匹配
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class Autowrited : PropertyAttribute
    {
        public readonly string targetObjName;
        public Autowrited(string targetObjName)
        {
            this.targetObjName = targetObjName;
        }
        public Autowrited()
        {

        }
    }
}
