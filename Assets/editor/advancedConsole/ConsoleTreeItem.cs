using UnityEditor.IMGUI.Controls;

public class ConsoleTreeItem : TreeViewItem
{
    public string fullAssetName;

    public ConsoleTreeItem(int id, int depth, string displayName) : base(id, depth, displayName)
    {
    }
}
