using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

public class ConsoleTree : TreeView
{
    public event System.Action<LogEntry> OnSelectionChanged;
    private readonly TreeViewItem _rootTreeItem = new TreeViewItem(0, -1, "root");
    private int _id = 3;
    protected override TreeViewItem BuildRoot()
    {
        return _rootTreeItem;
    }

    public ConsoleTree(TreeViewState state, bool showBgColor, float itemHeight) : base(state)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = showBgColor;
        rowHeight = itemHeight;

        ConsoleTreeItem test1 = new ConsoleTreeItem(1, 0, "child1");
        _rootTreeItem.AddChild(test1);

        ConsoleTreeItem test2 = new ConsoleTreeItem(2, 0, "child2");
        _rootTreeItem.AddChild(test2);
        ConsoleTreeItem test3 = new ConsoleTreeItem(3, 1, "child3");
        _rootTreeItem.AddChild(test3);
        Reload();
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
        {
            SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
        }
    }

    public void AddLogTreeItem(LogEntry entry)
    {
        _id++;
        ConsoleTreeItem item = new ConsoleTreeItem(_id, 0, entry);
        item.icon = entry.Icon;
        _rootTreeItem.AddChild(item);

        Reload();
        ScrollToLatest();
    }

    private void ScrollToLatest()
    {
        var scorllPos = state.scrollPos;
        scorllPos.y = Mathf.Max(0, GetRows().Count * rowHeight - treeViewRect.height / rowHeight);
        state.scrollPos = scorllPos;
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

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        if(selectedIds == null || selectedIds.Count <= 0) return;

        var assetItem = FindItem(selectedIds[0], rootItem) as ConsoleTreeItem;
        if (assetItem != null)
        {
            if (OnSelectionChanged != null)
            {
                if (selectedIds.Count > 1)
                    OnSelectionChanged(null);
                else
                    OnSelectionChanged(assetItem.LogEntry);
            }
        }
    }

    public void Clear()
    {
        rootItem.children.Clear();
        Reload();
    }
}
