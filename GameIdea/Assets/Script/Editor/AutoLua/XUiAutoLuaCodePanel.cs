using System.Collections.Generic;
using System.Text;using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static Utils;
using System.IO;

public class XUiAutoLuaCodePanel : EditorWindow
{
    private List<GameObject> _uiCompleteList = new List<GameObject>();
    private RectTransform _selectedObj;
    private Vector2 _scrollTextPos;
    private TreeViewState _treeViewState;
    private PrefabsTreeViews _treeViews;
    private StringBuilder _allCodeBuilder = new StringBuilder();
    private List<string> _uiNameList;
    private List<string> _variableNameList;

    private void OnEnable()
    {
        this.init();
    }

    private void OnFocus()
    {
        this.init();
        //  this.DrawTitle();
    }

    private void OnDestroy()
    {
        Utils.SelectedDic = null;
        this._uiCompleteList = null;
        this._uiNameList = null;
        this._variableNameList = null;
    }

    private static readonly TextEditor CopyTool = new TextEditor();
    [MenuItem("GameObject/复制Hierarchy路径", priority = 20)]
    static void CopyPath()
    {
        Transform trans = Selection.activeTransform;
        if (null == trans) return;
        CopyTool.text = GetPath(trans);
        CopyTool.SelectAll();
        CopyTool.Copy();
        EditorUtility.DisplayDialog( $"CopyToClipBoard Success!", CopyTool.text,"OK");
    }
    public static string GetPath(Transform trans)
    {
        if (null == trans) return string.Empty;
        if (null == trans.parent) return trans.name;
        return GetPath(trans.parent) + "/" + trans.name;
    }

    private XUiAutoLuaCodePanel()
    {
        this.titleContent = new GUIContent("XUI可视化Lua代码生成器");
    }

    [MenuItem("GameObject/XUI可视化Lua代码生成器", false, priority = 20)]
    private static void showWindow()
    {
        EditorWindow wind=  EditorWindow.GetWindow(typeof(XUiAutoLuaCodePanel));
        wind.maximized = true;
    }

    public void init()
    {
        if (_treeViewState == null)
        {
            _treeViewState = new TreeViewState();
        }
    }

    private void OnGUI()
    {
        GUI.backgroundColor = Color.white;
        this.DrawTitle();
        if (this._selectedObj != null && dicNames.Count > 0)
        {
            this.DrawMainContent();
        }
    }

    private void DrawMainContent()
    {
        GUI.skin.scrollView.alignment = TextAnchor.UpperLeft;
        EditorGUILayout.Space();
        this.DrawUIEles();

        GUILayout.BeginHorizontal();
        GUILayout.Space(450);
        if (GUILayout.Button("保存脚本文件", GUILayout.Width(100), GUILayout.Height(20)))
        {
            this.CreateUIScript(".lua", "lua");
        }

        if (GUILayout.Button("复制代码", GUILayout.Width(100), GUILayout.Height(20)))
        {
            if (this._allCodeBuilder != null && this._allCodeBuilder.Length > 0)
            {
                TextEditor p = new TextEditor();
                p.text = this._allCodeBuilder.ToString();
                p.OnFocus();
                p.Copy();
            }
            EditorUtility.DisplayDialog("提示", "代码复制成功", "OK");
        }
        GUILayout.EndHorizontal();

        _scrollTextPos = EditorGUILayout.BeginScrollView(_scrollTextPos, GUILayout.Height(1000));
        this.DrawCodeView();
        EditorGUILayout.EndScrollView();
    }

    private void DrawCodeView()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(450);
        EditorGUILayout.TextArea(this.GenerateCode().ToString(), GUILayout.Width(800));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawUIEles()
    {
        if (this._selectedObj != null)
        {
            GameObject[] rects = Utils.GetAllUIChild(this._selectedObj.gameObject);
            if (_treeViews == null)
            {
                _treeViews = new PrefabsTreeViews(_treeViewState);
              
                this._treeViews.ParentTransform = this._selectedObj;
                this._treeViews.Reload();
                this.UpdateUIDict(rects);
            }

            this._treeViews.RefhreshSelectedState();
            this._treeViews.Repaint();
            this._treeViews.OnGUI(new Rect(0, 50, 400, 900));
           
        }
    }

    private void UpdateUIDict(GameObject[] uIBehaviours)
    {
        for (int i = 0; i < uIBehaviours.Length; i++)
        {
            GameObject tempBehaviour = uIBehaviours[i];
            if (!this._uiCompleteList.Contains(tempBehaviour))
            {
                this._uiCompleteList.Add(tempBehaviour);
            }
        }
    }

    private void DrawTitle()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 15;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        //不再重新赋值是为了防止误点，所以，如果想要选择别的game object只能重新打开
        if (_selectedObj == null && Selection.activeGameObject != null)
        {
            _selectedObj = this.GetSelectionObj();
        }

        if (_selectedObj != null)
        {
            GUILayout.Label("Selected Name:" + _selectedObj.name);
        }
        else
        {
            GUILayout.Label("please selected UI  in hierarchy");
        }

        if (GUILayout.Button("切换根节点", GUILayout.Width(100), GUILayout.Height(20)))
        {
            _treeViews = null;
            Utils.SelectedDic.Clear();
            this._uiCompleteList.Clear();
            _selectedObj = this.GetSelectionObj();
        }

    }

    private void CreateUIScript(string type1, string type2)
    {
        if (this._allCodeBuilder == null || this._allCodeBuilder.Length <= 0)
        {
            return;
        }

        string path = "";
        string fileName = this._selectedObj.gameObject.name;
        // bool isSystem = this._selectedObj.name.Contains("_Panel");
        //这里可以根据panel的命名规则存储到项目的不同目录中
        fileName = Utils.combine(this._selectedObj.name, "Control");
        path = System.Environment.CurrentDirectory + "/Assets/Script/Lua";

        path = EditorUtility.SaveFilePanel("Create Script", path, fileName + type1, type2);
        if (string.IsNullOrEmpty(path)) return;

        int index = path.LastIndexOf('/');
        File.WriteAllText(path, this._allCodeBuilder.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();

        Debug.Log("脚本生成成功,生成路径为:" + path);
        EditorPrefs.SetString("create_script_folder", path);
    }

    private StringBuilder GenerateCode()
    {
        if (this._allCodeBuilder == null)
            this._allCodeBuilder = new StringBuilder();
        else
            this._allCodeBuilder.Clear();

        this.InitVariableName();
        StringBuilder luaSb = this.WriteLuaCode();
        this._allCodeBuilder.Append(luaSb);
        return this._allCodeBuilder;
    }

    List<GameObject> listObj;
    private void InitVariableName()
    {
        listObj = this.BuildSelectUIList();
        if (this._variableNameList == null)
            this._variableNameList = new List<string>();
        else
            this._variableNameList.Clear();

        for (int i = 0; i < listObj.Count; i++)
        {
            string name = Utils.UIName(listObj[i]);
            this._variableNameList.Add(name);
        }
    }

    private StringBuilder WriteLuaCode()
    {
        StringBuilder pathBuilder = new StringBuilder();
        bool isSystem = this._selectedObj.name.Contains("_Panel");
        if (isSystem)
        {
            //System title 
            string systemName = this._selectedObj.name.Replace("UI_", "").Replace("_Panel", "");
            pathBuilder.AppendFormat(UICodeConfig.SystemTitle, systemName);
            pathBuilder.Append(UICodeConfig.startNamePanelDefine);
            pathBuilder.AppendFormat(UICodeConfig.FileName, this._selectedObj.name);
        }
        else
        {
            //CustomControl title
            string controlName = Utils.combine(this._selectedObj.name, "Control");
            pathBuilder.AppendFormat(UICodeConfig.CustomControlTitle, controlName);
            pathBuilder.Append(UICodeConfig.startNameControlDefine);
        }
        pathBuilder.Append(UICodeConfig.WindowStart);


        //控件查找
        List<string> functions = new List<string>();
        functions.Add("OnInit");
        if (isSystem)
        {
            functions.Add("OnShow");
            functions.Add("OnHide");
            functions.Add("OnRelease");
        }

       
        for (int i = 0; i < listObj.Count; i++)
        {
            string name = this._variableNameList[i];
            int curUIType = (int)Utils.SelectUIComponentOrderType(listObj[i]);
            //开始括号
            pathBuilder.Append(UICodeConfig.KuoHaoStart);
            pathBuilder.AppendFormat(UICodeConfig.luaVariable, dicNames[listObj[i].transform]);
           
            string UITypeName = UICodeConfig.uiComponentName[curUIType];
            pathBuilder.AppendFormat(UICodeConfig.uiType, UITypeName);
            pathBuilder.AppendFormat(UICodeConfig.uiAlias, listObj[i].name);

            //根据类型判断是否有Handle
            List<string> funs =   this.JudgeTypeHandles(listObj[i], pathBuilder, curUIType);
            functions.AddRange(funs);
            //结束括号
            pathBuilder.Append(UICodeConfig.KuoHaoEnd);
            pathBuilder.Append(UICodeConfig.NewLine);
        }


        pathBuilder.Append(UICodeConfig.windowend);
        pathBuilder.Append(UICodeConfig.end);

        //通用function方法
        foreach (string funName in functions)
        {
            this.BuildFucntion(pathBuilder, funName);
        }

        return pathBuilder;
    }

    private void BuildFucntion(StringBuilder pathBuilder,string funName)
    {
        pathBuilder.AppendFormat(UICodeConfig.FunctionName, funName);
    }

    private List<string> JudgeTypeHandles(GameObject uiBe, StringBuilder pathBuilder, int curUIType)
    {
        //返回需要Handle的所有functionName
        List<string> funcNames = new List<string>();
        bool needHandle = curUIType == (int)UICompleteEnum.Button
            || curUIType == (int)UICompleteEnum.ReturnButton
            || curUIType == (int)UICompleteEnum.ListView;

        //handle处理
        if (needHandle)
        {
            string funcName = "";
            pathBuilder.Append(UICodeConfig.HandleStart);
            if (curUIType == (int)UICompleteEnum.Button)
            {
                funcName = string.Format(UICodeConfig.OnClick, uiBe.name);
                pathBuilder.AppendFormat(UICodeConfig.ClickHandle, funcName);
                funcNames.Add(funcName);
            }
            else if (curUIType == (int)UICompleteEnum.ReturnButton)
            {
                funcName = UICodeConfig.OnBackClick;
                pathBuilder.AppendFormat(UICodeConfig.BackClickHandle, funcName);
                funcNames.Add(funcName);
            }
            else if (curUIType == (int)UICompleteEnum.ListView)
            {
                funcName = string.Format(UICodeConfig.OnListViewUpdated, uiBe.name);
                pathBuilder.AppendFormat(UICodeConfig.ListViewUpdateHandle, funcName);
                funcNames.Add(funcName);

                funcName = string.Format(UICodeConfig.OnListItemUpdated, uiBe.name);
                pathBuilder.AppendFormat(UICodeConfig.ListItemUpdateHandle, funcName);
                funcNames.Add(funcName);
            }
            pathBuilder.Append(UICodeConfig.HandleEnd);
        }//end needHandle


        //cache处理
        bool needCacheSetting = curUIType == (int)UICompleteEnum.ListView;
        if (needCacheSetting)
        {
            pathBuilder.Append(UICodeConfig.CacheSettingStart);
            EasyListView list = uiBe.GetComponent<EasyListView>();
            pathBuilder.AppendFormat(UICodeConfig.CacheUiType, list.DefaultItem.name);

            pathBuilder.Append(UICodeConfig.CacheHandleStart);
            pathBuilder.Append(UICodeConfig.CacheHandleUser);
            pathBuilder.Append(UICodeConfig.CacheHandleEnd);

            pathBuilder.Append(UICodeConfig.CacheSettingEnd);
        }//end needCacheSetting

        return funcNames;
    }//end function

    private List<GameObject> BuildSelectUIList()
    {
        List<GameObject> uiList = new List<GameObject>();
        if (this._uiNameList == null)
        {
            this._uiNameList = new List<string>();
        }
        this._uiNameList.Clear();
        if (this._uiCompleteList.Count == Utils.SelectedDic.Count)
        {
            for (int i = 0; i < Utils.SelectedDic.Count; i++)
            {
                if (Utils.SelectedDic[i] && this._uiCompleteList[i] != null)
                {
                    uiList.Add(this._uiCompleteList[i]);
                    this._uiNameList.Add(this._uiCompleteList[i].name);
                }
            }
        }
        else
        {
            this._allCodeBuilder = null;
            Debug.LogError("data error  selectedList count and selectedDic count is different, please reopen");
        }
        return uiList;
    }

    Dictionary<Transform, string> dicNames = new Dictionary<Transform, string>();
    private RectTransform GetSelectionObj()
    {
        GameObject go = Selection.activeGameObject;
        RectTransform objTransorm = null;
        if (go.GetComponent<RectTransform>() != null)
        {
            objTransorm = go.GetComponent<RectTransform>();
            dicNames = Utils.GetChildrenPaths(objTransorm.gameObject);
            return objTransorm;
        }
        return null;
    }
}