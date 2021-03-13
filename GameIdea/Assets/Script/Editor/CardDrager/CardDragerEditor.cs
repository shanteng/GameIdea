#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardDrager), true)]
public class CardDragerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CardDrager Director = target as CardDrager;
        base.OnInspectorGUI();
        if (GUILayout.Button("预览"))
        {
            Director.EditorPreview();
        }
        else if (GUILayout.Button("删除预览元素"))
        {
            Director.RemovePreview();
        }
        else if (Director._isEditorChange)
        {
            Director.RefreshEditor();
            Director._isEditorChange = false;
        }
    }
}
#endif