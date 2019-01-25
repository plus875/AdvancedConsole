using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class StackTrackTree : TreeView
{
    private readonly TreeViewItem _rootTreeItem = new TreeViewItem(0, -1, "root");
    private int _id;
    protected override TreeViewItem BuildRoot()
    {
        return _rootTreeItem;
    }

    public StackTrackTree(TreeViewState state, bool showBgColor, float itemHeight) : base(state)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = showBgColor;
        rowHeight = itemHeight;

        StackTrackItem test1 = new StackTrackItem(1, 0, "");
        _rootTreeItem.AddChild(test1);
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
        var assetItem = FindItem(id, rootItem) as StackTrackItem;
        if (assetItem != null)
        {
            //Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.fullAssetName);
            //EditorGUIUtility.PingObject(o);
            //Selection.activeObject = o;
            assetItem.OnDoubleClick();
        }
    }


    public void ClearStackTrack()
    {
        rootItem.children.Clear();
        Reload();
    }

    public void SetStackTrack(LogEntry entry)
    {
        ClearStackTrack();
        var trackList = entry.GetStackTrack();
        for (var i = 0; i < trackList.Count; i++)
        {
            _id++;
            StackTrackItem item = new StackTrackItem(_id, 0, trackList[i]);
            rootItem.AddChild(item);
        }

        Reload();
    }
}
