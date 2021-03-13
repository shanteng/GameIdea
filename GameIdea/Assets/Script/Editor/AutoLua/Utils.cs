
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class Utils : MonoBehaviour
{
    /// <summary>
    /// game object 的选择状态字典
    /// </summary>
    private static Dictionary<int, bool> selectedDic;
    /// <summary>
    /// game object 的选择状态字典
    /// </summary>
    public static Dictionary<int, bool> SelectedDic
    {
        get
        {
            if (selectedDic == null)
            {
                selectedDic = new Dictionary<int, bool>();
            }
            return selectedDic;
        }

        set
        {
            selectedDic = value;
        }
    }

    public static UICompleteEnum SelectUIComponentOrderType(GameObject obj)
    {
        if (obj.GetComponent<EasyListView>() != null)
        {
            return UICompleteEnum.ListView;
        }

        UIBehaviour[] tmp = obj.GetComponents<UIBehaviour>();
        UICompleteEnum MaxOrder = UICompleteEnum.Window;
        int lenght = tmp.Length;
        //找出优先级比较高的UI
        for (int j = 0; j < tmp.Length; j++)
        {
            UICompleteEnum curOrder = Utils.GetUIComponent(tmp[j]);
            if (curOrder > MaxOrder)
            {
                MaxOrder = curOrder;
            }
        }
        return MaxOrder;
    }

    

   

    public enum UICompleteEnum
    {
        Window,
        Button,
        ReturnButton,
        Label,
        ListView,
        ScrollView,
        ProgressBar,
        DynamicImage,
        Slider,
    }

    public static UICompleteEnum GetUIComponent(UIBehaviour rect)
    {
        if(rect == null)
             return UICompleteEnum.Window;
        string typeName = rect.GetType().ToString();
        
        if (typeName.Equals("xUIControlMonoPanel"))
        {
            return UICompleteEnum.Window;
        }
        else if (typeName.Equals("xUIControlMonoButton"))
        {
               return UICompleteEnum.Button;
        }
        else if (typeName.Equals("xUIControlMonoLabel"))
        {
            return UICompleteEnum.Label;
        }
        else if (typeName.Equals("EasyListView"))
        {
            return UICompleteEnum.ListView;
        }
        else if (typeName.Equals("xUIControlMonoScrollView"))
        {
            return UICompleteEnum.ScrollView;
        }
        else if (typeName.Equals("xUIControlMonoProgressBar"))
        {
            return UICompleteEnum.ProgressBar;
        }
        else if (typeName.Equals("xUIControlMonoDynamicImage"))
        {
            return UICompleteEnum.DynamicImage;
        }
        else if (typeName.Equals("xUIControlMonoSlider"))
        {
            return UICompleteEnum.Slider;
        }

        return UICompleteEnum.Window;

    }

    /// <summary>
    /// 获取一个game object的路径
    /// </summary>
    /// <param name="rootGo"></param>
    /// <returns></returns>
    public static Dictionary<Transform, string> GetChildrenPaths(GameObject rootGo)
    {
        Dictionary<Transform, string> pathDic = new Dictionary<Transform, string>();
        string path = string.Empty;
        Transform[] tfArray = rootGo.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < tfArray.Length; i++)
        {
            Transform node = tfArray[i];

            string str = node.name;
            while (node.parent != null && node.gameObject != rootGo && node.parent.gameObject != rootGo)
            {
                str = string.Format("{0}/{1}", node.parent.name, str);
                node = node.parent;
            }
            path += string.Format("{0}\n", str);

            if (!pathDic.ContainsKey(tfArray[i]))
            {
                pathDic.Add(tfArray[i], str);
            }
        }
        return pathDic;
    }
    /// <summary>
    /// 获取所有的子物体
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static GameObject[] GetAllUIChild(GameObject go)
    {
        List<GameObject> goList = new List<GameObject>();
        Transform[] tfArray = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < tfArray.Length; i++)
        {
            Transform node = tfArray[i];
            if (!goList.Contains(node.gameObject))
            {
                goList.Add(node.gameObject);
            }
        }
        return goList.ToArray();
    }

    public static string UIName(GameObject ui)
    {
        return ui.name;
        //string name = ui.name;
        //name = name.Replace(" ", "_");
        //name = name.Replace("(", "_");
        //name = name.Replace(")", "_");
        //return name;
    }
    /// <summary>
    /// 生成game object的名字列表
    /// </summary>
    /// <param name="uiList"></param>
    /// <returns></returns>
    public static List<string> UINameList(List<UIBehaviour> uiList)
    {
        List<string> nameList = new List<string>();
        int index = 0;
        for (int i = 0; i < uiList.Count; i++)
        {
            string name = UIName(uiList[i].gameObject);
            if (nameList.Contains(name))
            {
                ++index;
                name = name + index;
                nameList.Add(name);
            }
            else
            {
                nameList.Add(name);
            }
        }
        return nameList;
    }

    private static object m_builderLock = new object();
    private static StringBuilder m_builder = new StringBuilder();

    public static string combine(params object[] texts)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            var len = texts.Length;
            for (int i = 0; i < len; i++)
            {
                m_builder.Append(texts[i]);
            }
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, params object[] texts)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            var len = texts.Length;
            for (int i = 0; i < len; i++)
            {
                m_builder.Append(texts[i]);
            }
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, int intParam)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            m_builder.Append(intParam.ToString());
            return m_builder.ToString(0, m_builder.Length);
        }
    }

    public static string combine(string main, float floatParam)
    {
        lock (m_builderLock)
        {
            m_builder.Remove(0, m_builder.Length);
            m_builder.Append(main);
            m_builder.Append(floatParam.ToString());
            return m_builder.ToString(0, m_builder.Length);
        }
    }
}
