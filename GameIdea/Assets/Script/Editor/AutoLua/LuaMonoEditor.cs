#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class LuaWindowDefine
{
    public string Name;
    public string Type;
    public string Alias;
}

[CustomEditor(typeof(LuaMonoView), true)]
public class LuaMonoEditor : Editor
{
    protected SerializedProperty mLuaFileName;
    private List<LuaWindowDefine> _allDic = new List<LuaWindowDefine>();
    protected ReorderableList mGameobjectList;

    protected  void OnEnable()
    {
        LuaMonoView graphic = target as LuaMonoView;
        if (graphic != null)
            graphic.EditorLuaData = null;

        mGameobjectList = new ReorderableList(new List<string>(), typeof(string), false, true, false, false);
        mGameobjectList.showDefaultBackground = true;
        mGameobjectList.elementHeight = 20;
        mGameobjectList.drawHeaderCallback += new ReorderableList.HeaderCallbackDelegate(DrawLuaHeader);
        mGameobjectList.drawElementCallback += new ReorderableList.ElementCallbackDelegate(DrawLuaElement);
        mLuaFileName = serializedObject.FindProperty("mLuaFileName");

    }



    protected void DrawLuaHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, new GUIContent("LuaMono"));
    }


    protected void DrawLuaElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        LuaMonoView graphic = target as LuaMonoView;
        Transform tran = graphic.transform;
        rect.height = EditorGUIUtility.singleLineHeight;
        rect.x = 20;

        List<LuaWindowDefine> dataList = mGameobjectList.list as List<LuaWindowDefine>;
        LuaWindowDefine data = dataList[index];

        Transform item = Selection.activeGameObject.transform.Find(data.Name);
        GameObject obj = item != null ? item.gameObject : null;
        string title = Utils.combine(data.Alias, "(", data.Type, ")");
        EditorGUI.ObjectField(rect, title, obj, typeof(GameObject), true);
    }

    public override void OnInspectorGUI()
    {
        LuaMonoView graphic = target as LuaMonoView;
        EditorGUILayout.PropertyField(mLuaFileName, new GUIContent("Lua脚本名"));
        if (graphic.mLuaFileName.Equals("") == false)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("查看LuaMonoView"))
            {
                string path = System.Environment.CurrentDirectory + "/Assets/Script/Lua";
                List<string> fileList = new List<string>();
                var files = this.FindLuaFile(path, fileList);
                foreach (string name in files)
                {
                    int lastIndexOfPoint = name.LastIndexOf('.');
                    int lastIndexOfDash = name.LastIndexOf('\\') + 1;
                    int len = lastIndexOfPoint - lastIndexOfDash;
                    string luaName = name.Substring(lastIndexOfDash, len);
                    if (luaName.Equals(graphic.mLuaFileName))
                    {
                        this._allDic = this.ParseLuaFileToShowList(name);
                        if (this._allDic.Count > 0)
                            graphic.EditorLuaData = this._allDic;
                        break;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        serializedObject.ApplyModifiedProperties();
        //绘制关系图
        this.DrawLuaPanel();
    }//end func

    private void DrawLuaPanel()
    {
        if (this._allDic.Count > 0)
        {
            mGameobjectList.list = _allDic;
            mGameobjectList.DoLayoutList();
        }
    }

    public List<string> FindLuaFile(string path, List<string> FileList)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] fil = dir.GetFiles();
        DirectoryInfo[] dii = dir.GetDirectories();
        foreach (FileInfo f in fil)
        {
            long size = f.Length;
            FileList.Add(f.FullName);//添加文件路径到列表中
        }
        //获取子文件夹内的文件列表，递归遍历
        foreach (DirectoryInfo d in dii)
        {
            FindLuaFile(d.FullName, FileList);
        }
        return FileList;
    }//end fun

    private List<LuaWindowDefine> ParseLuaFileToShowList(string path)
    {
        //根据lua格式解析lua内的文件和GameObject对应关系
        List<LuaWindowDefine> list = new List<LuaWindowDefine>();
        StreamReader sr = new StreamReader(path);
        string lua = sr.ReadToEnd();
        sr.Close();
        sr = null;

        lua = lua.Trim().Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "");

        int startIndex = lua.IndexOf("ControlDefine");
        if (startIndex < 0)
            startIndex = lua.IndexOf("PanelDefine");

        if (startIndex < 0)
            startIndex = 0;

        lua = lua.Substring(startIndex, lua.Length - startIndex);
        //把内容缩减到第一个funcation
        int idxOfFunction = lua.IndexOf("function");
        if (idxOfFunction >= 0)
        {
            lua = lua.Substring(0, idxOfFunction);
        }

        startIndex = 0;
        int len = lua.Length;
        while (startIndex < len)
        {
            int StartNameIndex = lua.IndexOf("Name", startIndex);
            if (StartNameIndex < 0)
                break;

            int QuadIndex = lua.IndexOf(",", StartNameIndex);
            LuaWindowDefine kv = new LuaWindowDefine();
            int strLen = QuadIndex - StartNameIndex;

            startIndex = QuadIndex;
            string leftString = lua.Substring(StartNameIndex, strLen);
            string finalName = leftString.Replace("\"", "").Replace("Name", "").Replace("=", "").Trim();
            kv.Name = finalName;
            if (leftString.Contains("=") == false)
                continue;

            int leftLen = lua.Length - startIndex;
            if (leftLen > 10)
                leftLen = 10;
            string nextLua = lua.Substring(startIndex, leftLen);
            if (nextLua.Contains("Type") == false)
                continue;
            int TypeIndex = lua.IndexOf("Type", startIndex);
            if (TypeIndex < 0)
                continue;

            int QuadTypeIndex = lua.IndexOf(",", TypeIndex);
            strLen = QuadTypeIndex - TypeIndex;
            if (strLen <= 0)
                continue;
            leftString = lua.Substring(TypeIndex, strLen);
            string finalType = leftString.Replace("\"", "").Replace("Type", "").Replace("=", "").Trim();
            startIndex = QuadTypeIndex;
            kv.Type = finalType;
            if (leftString.Contains("=") == false)
                continue;

            leftLen = lua.Length - startIndex;
            if (leftLen > 10)
                leftLen = 10;
            nextLua = lua.Substring(startIndex, leftLen);
            if (nextLua.Contains("Alias") == false)
                continue;

            int AtlasIndex = lua.IndexOf("Alias", startIndex);
            if (AtlasIndex < 0)
                continue;

            int QuadAtlasIndex = lua.IndexOf(",", AtlasIndex);
            strLen = QuadAtlasIndex - AtlasIndex;
            if (strLen <= 0)
                continue;
            leftString = lua.Substring(AtlasIndex, strLen);
            string finalAtlas = leftString.Replace("\"", "").Replace("Alias", "").Replace("=", "").Trim();
            startIndex = QuadAtlasIndex;
            kv.Alias = finalAtlas;
            if (leftString.Contains("=") == false)
                continue;

            leftLen = lua.Length - startIndex;
            if (leftLen > 10)
                leftLen = 10;
            nextLua = lua.Substring(startIndex, leftLen);
            if (nextLua.Contains("Count"))
            {
                int CountIndex = lua.IndexOf("Count", startIndex);
                if (CountIndex >= 0)
                {
                    int QuadCountIndex = lua.IndexOf(",", CountIndex);
                    strLen = QuadCountIndex - CountIndex;
                    if (strLen > 0)
                    {
                        leftString = lua.Substring(CountIndex, strLen);
                        if (leftString.Contains("="))
                        {
                            startIndex = QuadCountIndex;
                            string finalCount = leftString.Replace("Count", "").Replace("=", "").Trim();
                            int itemCount = 0;
                            if (int.TryParse(finalCount, out itemCount))
                            {
                                for (int i = 0; i < itemCount; ++i)
                                {
                                    int index = i + 1;
                                    string itemName = Utils.combine(finalName, index);
                                    kv = new LuaWindowDefine();
                                    kv.Name = itemName;
                                    kv.Type = finalType;
                                    kv.Alias = finalAtlas;
                                    list.Add(kv);
                                }
                                continue;
                            }
                        }
                    }
                }
            }//end if   if (nextLua.Contains("Count"))
            list.Add(kv);
        }
        return list;
    }

}//end class
#endif