using System.Text.RegularExpressions;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

public sealed class ConsoleLogTreeItem : TreeViewItem
{
    public int intId;

    public string fullAssetName;

    [SerializeField]
    public LogEntry LogEntry;

    //public ConsoleLogTreeItem(int id, int depth, string displayName) : base(id, depth, displayName)
    //{
    //    intId = id;
    //}

    public ConsoleLogTreeItem(int id, int depth, LogEntry logEntry) : base(id, depth)
    {
        intId = id;
        LogEntry = logEntry;
        displayName = logEntry.Output;
    }
}

public sealed class StackTrackItem : TreeViewItem
{
    private static readonly Regex logRegex = new Regex(@" \(at (.+)\:(\d+)\)");

    public static string prefix = Application.dataPath.Replace("Assets", "");
    public string fileName;
    public int lineNo = -1;

    public StackTrackItem(int id, int depth, string displayName) : base(id, depth, displayName)
    {
    }

    public void OnDoubleClick()
    {
        if (lineNo < 0)
        {
            Match match = logRegex.Match(displayName);
            if (match.Success)
            {
                fileName = match.Groups[1].Value;
                lineNo = int.Parse(match.Groups[2].Value);
            }
        }

        if (lineNo >= 0)
        {
            InternalEditorUtility.OpenFileAtLineExternal(prefix + fileName, lineNo);
        }
    }
}