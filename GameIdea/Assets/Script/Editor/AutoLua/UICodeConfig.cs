using UnityEngine;

public class UICodeConfig : MonoBehaviour
{
    public static string Transform = "GameObject";
    /// <summary>
    /// 组件的列表
    /// </summary>
    public static string[] uiComponentName = new string[]
    {
        "Window",
        "Button",
         "ReturnButton",
         "Label",
         "ListView",
         "ScrollView",
          "ProgressBar",
           "DynamicImage",
            "Slider"
    };
    /// <summary>
    /// 组件的名字后缀
    /// </summary> 
    public static string[] uiVariableName = new string[] { "gameobj", "txt", "img", "rImg", "btn", "tgl", "slider", "scrlbar", "iptField", "scrollRect", "dropdown", "mask", "textMesh", "uiBtn" };


    public const string SystemTitle = "System( \"{0}System\", SystemScope.Hall) \n \n";
    public const string CustomControlTitle = "CustomControl( \"{0}\", \"Window\") \n \n";

    public const string startNamePanelDefine = "PanelDefine = \n{\n\t";
    public const string FileName = "File =\"{0}\",\n\t";
    public const string end = "\n}\n\n";

    public const string WindowStart = "\n\tWindows = \n\t{\n\t";
    public const string windowend = "\n\t}";

    public const string startNameControlDefine = "ControlDefine = \n{\n\t";

   
    public const string KuoHaoStart = "\t{\n\t";
    public const string KuoHaoEnd = "\t},\n\t";
    public const string NewLine = "\n\t";

    public const string luaVariable = "\t\tName = \"{0}\",\n\t";
    public const string uiType = "\t\tType = \"{0}\",\n\t";
    public const string uiAlias = "\t\tAlias = \"{0}\",\n\t";


    public const string HandleStart = "\t\tHandles = \n\t\t\t{\n";
    public const string HandleEnd = "\t\t\t},\n\t";

    public const string CacheSettingStart = "\t\tCacheSetting = \n\t\t\t{\n";
    public const string CacheSettingEnd = "\t\t\t}\n\t";
    public const string CacheUiType = "\t\t\t\tType = \"{0}\",\n";
    public const string CacheHandleStart = "\t\t\t\tHandles = \n\t\t\t\t{\n";
    public const string CacheHandleUser = "\t\t\t\t\t--Your Cache Handle Code\n";
    public const string CacheHandleEnd = "\t\t\t\t},\n";

    public const string OnClick = "On{0}Clicked";
    public const string OnBackClick = "OnBackButtonClick";

    public const string ClickHandle = "\t\t\t\t[\"OnClick\"] = \"{0}\",\n";
    public const string BackClickHandle = "\t\t\t\t[\"BackClick\"] = \"{0}\",\n";

    public const string OnListViewUpdated = "On{0}ListViewUpdated";
    public const string OnListItemUpdated = "On{0}ListItemUpdated";

    public const string ListViewUpdateHandle = "\t\t\t\t[\"ListViewUpdated\"] = \"{0}\",\n";
    public const string ListItemUpdateHandle = "\t\t\t\t[\"ListItemUpdated\"] = \"{0}\",\n";

    public const string FunctionName = "function {0}(self)\n\nend\n\n";
    // Lua代码配置
    public const string luaModule = "module(\"{0}\", package.seeall) \n\n;";

    public const string luaClassName = "\n\tfunction {0}:InitUI()";

    public const string luaAddEvent = "\n\tfunction {0}:AddEvent() \n";

    public const string luaClickEvent = "\n\tfunction {0}:On{1}Clicked()\n\n\n\tend\n";

    public const string luaValueChangedEvent = "\n\tfunction {0}:On{1}ValueChanged(arg)\n\n\n\tend\n";

   

    public const string luaEventAdd = "\n\t\tself.{0}.onClick:AddListener(function() self:On{1}Clicked(); end)";

    public const string luaEventChanged = "\n\t\tself.{0}.onValueChanged:AddListener(function(args) self:On{1}ValueChanged(args); end)";

    public const string luaEnd = "\n\tend\n";

    public const string luaAddEventMethod = "\n\n\t\tself:AddEvent();";
    
}

