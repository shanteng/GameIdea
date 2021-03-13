
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


public class PrefabsTreeItem : TreeViewItem
{
    private string name;
    public bool defaultSelect;

    public string Name
    {
        get
        {
            return name;
        }

        set
        {
            name = value;
        }
    }

    public PrefabsTreeItem(int id, int depth, string name) : base(id, depth, name)
    {
        this.id = id;
        this.depth = depth;
        this.name = name;
        this.defaultSelect = false;
    }

    private PrefabsTreeItem() : base(0, -1)
    {

    }
}
