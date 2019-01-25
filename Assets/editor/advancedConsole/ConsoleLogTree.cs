using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

public class ConsoleLogTree : TreeView
{
    public event System.Action<LogEntry> OnSelectionChanged;
    private List<TreeViewItem> _treeViewItems = new List<TreeViewItem>();
    private TreeViewItem _rootTreeItem;
    protected override TreeViewItem BuildRoot()
    {
        //Debug.LogWarning("build");
        _rootTreeItem = new TreeViewItem(0, -1, "root");
        _rootTreeItem.children = new List<TreeViewItem>(100);
        return _rootTreeItem;
    }

    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        _treeViewItems.Clear();

        bool inSearch = !string.IsNullOrEmpty(searchString);
        string matchStr = inSearch ? searchString.ToLower() : "";
        var showingList = AdvancedConsole.Instance._showingEntries;
        for (var i = 0; i < showingList.Count; i++)
        {
            var logEntry = showingList[i];

            bool visible = true;
            if (inSearch)
            {
                visible = logEntry.Output.ToLower().Contains(matchStr);
            }

            if (visible)
            {
                ConsoleLogTreeItem item = new ConsoleLogTreeItem(logEntry.IntId, 0, logEntry);
                item.icon = logEntry.Icon;
                _treeViewItems.Add(item);
            }
        }
        
        // We still need to setup the child parent information for the rows since this 
        // information is used by the TreeView internal logic (navigation, dragging etc)
        SetupParentsAndChildrenFromDepths(root, _treeViewItems);

        return _treeViewItems;
    }


    public ConsoleLogTree(TreeViewState state, bool showBgColor, float itemHeight) : base(state)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = showBgColor;
        rowHeight = itemHeight;
        
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

    protected override void DoubleClickedItem(int id)
    {
        var assetItem = FindItem(id, rootItem) as ConsoleLogTreeItem;
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

        var assetItem = FindItem(selectedIds[0], rootItem) as ConsoleLogTreeItem;
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

    public void AddLogTreeItem(LogEntry entry)
    {
        AddLogData(entry);
        EndAddAllLogData();
    }

    public void AddLogData(LogEntry entry)
    {
        ConsoleLogTreeItem item = new ConsoleLogTreeItem(entry.IntId, 0, entry);
        item.icon = entry.Icon;
        _rootTreeItem.AddChild(item);
    }

    public void EndAddAllLogData()
    {
        Reload();
        ScrollToLatest();
    }

    public void ScrollToLatest()
    {
        var scrollPos = state.scrollPos;
        scrollPos.y = Mathf.Max(0, GetRows().Count * rowHeight - treeViewRect.height / rowHeight);
        state.scrollPos = scrollPos;
    }
    
    public void Clear()
    {
        rootItem.children.Clear();
        Reload();
    }
}
