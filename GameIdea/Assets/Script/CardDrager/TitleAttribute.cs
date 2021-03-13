
using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
#endif

#if UNITY_EDITOR
/// <summary>
/// 标题属性
/// <para>ZhangYu 2018-06-21</para>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
#endif
public class TitleAttribute : PropertyAttribute
{

    /// <summary> 标题名称 </summary>
    public string title;
    /// <summary> 标题颜色 </summary>
    public string htmlColor;

    /// <summary> 在属性上方添加一个标题 </summary>
    /// <param name="title">标题名称</param>
    /// <param name="htmlColor">标题颜色</param>
    public TitleAttribute(string title, string htmlColor = "#FFFFFF")
    {
        this.title = title;
        this.htmlColor = htmlColor;
    }

}