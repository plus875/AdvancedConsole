using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ConsoleTree : TreeView {
    private readonly TreeViewItem _rootTreeItem = new TreeViewItem(0, -1, "root");
    private int _id = 3;
    protected override TreeViewItem BuildRoot()
    {
        return _rootTreeItem;
    }

    //protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    //{
    //    return null;
    //}

    public ConsoleTree(TreeViewState state) : base(state)
    {
        TreeViewItem test1 = new TreeViewItem(1, 0, "child1");
        _rootTreeItem.AddChild(test1);
        TreeViewItem test2 = new TreeViewItem(2, 0, "child2");
        _rootTreeItem.AddChild(test2);
        TreeViewItem test3 = new TreeViewItem(3, 1, "child3");
        _rootTreeItem.AddChild(test3);
        Reload();
    }

    public ConsoleTree(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
    {
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
        {
            SetSelection(new int[1], TreeViewSelectionOptions.FireSelectionChanged);
        }
    }

    public void AddLogTreeItem(LogEntry entry)
    {
        _id++;
        TreeViewItem item = new TreeViewItem(_id, 0, entry.output);
        _rootTreeItem.AddChild(item);
    }

    protected override void DoubleClickedItem(int id)
    {
        var assetItem = FindItem(id, rootItem) as ConsoleTreeItem;
        if (assetItem != null)
        {
            Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.fullAssetName);
            EditorGUIUtility.PingObject(o);
            Selection.activeObject = o;
        }
    }
}
