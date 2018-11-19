using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class AdvancedConsole : EditorWindow
{
    [MenuItem("Window/Open AdvancedConsole")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindowWithRect<AdvancedConsole>(ViewRect);
    }

    public static Texture2D Log;
    public static Texture2D Warn;
    public static Texture2D Error;

    private static readonly List<LogEntry> _entries = new List<LogEntry>();
    private TreeViewState _treeViewState = new TreeViewState();
    private ConsoleTree _consoleTree;

    private TreeViewState _stackTreeViewState = new TreeViewState();
    private StackTrackTree _stackTree;

    public static Rect ViewRect = new Rect(200, 300, 1100, 600);
    private Vector2 _ViewRect;
    private int _id;
    private Rect _splitterRect;
    private float verticalSplitterPercent;
    private float topTreeHeight;
    private float bottomTreeHeight;
    private bool m_ResizingVerticalSplitterLeft;

    public AdvancedConsole()
    {
        //Debug.LogError("AdvancedConsole construct:" + _entries.Count);
    }

    ~AdvancedConsole()
    {
        Debug.LogError("destroy AdvancedConsole:" + _entries.Count);
    }

    private void WriteFile()
    {
        string path = "D:\\Temppp.text";
        if(File.Exists(path))
            File.Delete(path);
        FileStream file = File.Create(path);

        if (_entries.Count > 0)
        {
            var bytes = Encoding.UTF8.GetBytes(_entries[0].Content);
            file.Write(bytes, 0, bytes.Length);
            file.Flush();
            file.Close();
        }
    }

    // ReSharper disable once UnusedMember.Local
    void OnEnable()
    {
        Application.logMessageReceivedThreaded += OnLogging;
        //Application.logMessageReceived += OnLogging;

        if (_consoleTree == null)
        {
            _consoleTree = new ConsoleTree(_treeViewState, true, 22f);
            _consoleTree.OnSelectionChanged += OnSelecLogChanged;
            _stackTree = new StackTrackTree(_stackTreeViewState, false, 18f);

            Log = EditorGUIUtility.Load("log.png") as Texture2D;
            Warn = EditorGUIUtility.Load("warn.png") as Texture2D;
            Error = EditorGUIUtility.Load("error.png") as Texture2D;
        }

        topTreeHeight = ViewRect.height * 0.5f;
        verticalSplitterPercent = topTreeHeight / ViewRect.height;
        _splitterRect = new Rect(0, ViewRect.height * verticalSplitterPercent, ViewRect.width, 3);
        bottomTreeHeight = ViewRect.height - topTreeHeight - _splitterRect.height;
    }

    void OnDisable()
    {
        Debug.LogError("OnDisable:" + _entries.Count);
        WriteFile();
    }

    // ReSharper disable once UnusedMember.Local
    void OnGUI()
    {
        ViewRect = position;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        HandleVerticalResize();

        _consoleTree.OnGUI(new Rect(0, 0, ViewRect.width, topTreeHeight));

        _stackTree.OnGUI(new Rect(0, topTreeHeight + _splitterRect.height, ViewRect.width, bottomTreeHeight));
    }


    private void HandleVerticalResize()
    {
        _splitterRect.y = (int)(ViewRect.height * verticalSplitterPercent);

        EditorGUIUtility.AddCursorRect(_splitterRect, MouseCursor.ResizeVertical);
        if (Event.current.type == EventType.MouseDown && _splitterRect.Contains(Event.current.mousePosition))
            m_ResizingVerticalSplitterLeft = true;

        if (m_ResizingVerticalSplitterLeft)
        {
            verticalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.y / ViewRect.height, 0.25f, 0.98f);
            _splitterRect.y = (int)(ViewRect.height * verticalSplitterPercent);
            topTreeHeight = _splitterRect.y;
            bottomTreeHeight = ViewRect.height - topTreeHeight - _splitterRect.height;
        }


        if (Event.current.type == EventType.MouseUp)
        {
            m_ResizingVerticalSplitterLeft = false;
        }
    }

    private void OnLogging(string condition, string stackTrace, LogType type)
    {
        LogEntry entry = new LogEntry
        {
            Output = condition,
            StackTrace = stackTrace,
            LogType = type,
        };
        _consoleTree.AddLogTreeItem(entry);
        _entries.Add(entry);
    }

    private void OnSelecLogChanged(LogEntry logEntry)
    {
        if (logEntry == null)
            _stackTree.ClearStackTrack();
        else
            _stackTree.SetStackTrack(logEntry);
    }

}
