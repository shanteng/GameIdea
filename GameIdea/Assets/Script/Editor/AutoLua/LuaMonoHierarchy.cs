#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class LuaMonoHierarchy
{
    static LuaMonoHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowOnGUI;
    }

    static void HierarchyWindowOnGUI(int instanceId, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
        if (go == null)
            return;

        Rect toggleRect = new Rect(selectionRect);
        toggleRect.x += toggleRect.width - 20;
        toggleRect.width = 18;
        EditorGUI.BeginChangeCheck();
       // bool visible = GUI.Toggle(toggleRect, go.activeInHierarchy && go.activeSelf, string.Empty);
        if (EditorGUI.EndChangeCheck())
        {
     
        }

        if (Selection.activeObject != null && Selection.activeObject is GameObject)
        {
            GameObject selectObj = Selection.activeObject != null ? (GameObject)Selection.activeObject : null;
            if (selectObj)
            {
                LuaMonoView graphic = selectObj.GetComponent<LuaMonoView>();
                if (graphic != null && graphic.EditorLuaData != null)
                {
                    List<LuaWindowDefine> allDynamicWins = (List<LuaWindowDefine>)graphic.EditorLuaData;
                    foreach (LuaWindowDefine win in allDynamicWins)
                    {
                        Transform obj = selectObj.transform.Find(win.Name);
                        if (obj != null && obj.gameObject.Equals(go))
                        {
                            toggleRect.y += 6;
                            toggleRect.x = 20;
                            toggleRect.width = 5;
                            toggleRect.height = 5;
                            GUI.color = Color.green;
                            GUI.DrawTexture(toggleRect, EditorGUIUtility.whiteTexture);
                            GUI.color = Color.white;
                            break;
                        }
                    }//end for
                }//end if
            }//end if
        }
        //end tiny
    }

   

}//end class
#endif