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
        EditorWindow.GetWindow<AdvancedConsole>();
    }

    public static Texture2D Log;
    public static Texture2D Warn;
    public static Texture2D Error;

    private static readonly List<LogEntry> _entries = new List<LogEntry>();
    private TreeViewState _treeViewState = new TreeViewState();
    private ConsoleLogTree _consoleLogTree;

    private TreeViewState _stackTreeViewState = new TreeViewState();
    private StackTrackTree _stackTree;

    private SearchField _searchField;


    private const int pading = 2;
    private bool _dirty;
    private string _searchString;
    private int _id;
    private Rect _splitterRect;
    private float verticalSplitterPercent = 0.6f;
    private float topTreeHeight;
    private float bottomTreeHeight;
    private bool m_ResizingVerticalSplitterLeft;
    private Rect viewRect;
    private float searchBarHeight = 16f;
    private GUIStyle _btnStyle;

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

        if (_consoleLogTree == null)
        {
            Log = EditorGUIUtility.Load("log.png") as Texture2D;
            Warn = EditorGUIUtility.Load("warn.png") as Texture2D;
            Error = EditorGUIUtility.Load("error.png") as Texture2D;

            _consoleLogTree = new ConsoleLogTree(_treeViewState, true, 22f);
            _consoleLogTree.OnSelectionChanged += OnSelecLogChanged;
            _stackTree = new StackTrackTree(_stackTreeViewState, false, 18f);

            _searchField = new SearchField();

            _btnStyle = new GUIStyle();
            _btnStyle.alignment = TextAnchor.MiddleCenter;
            var pressState = new GUIStyleState();
            pressState.background = Log;
            _btnStyle.onActive = pressState;
        }

        viewRect = position;
        float startY = pading * 2 + searchBarHeight;
        topTreeHeight = viewRect.height * verticalSplitterPercent - startY;
        _splitterRect = new Rect(0, viewRect.height * verticalSplitterPercent, viewRect.width, 5);
        bottomTreeHeight = viewRect.height - topTreeHeight - _splitterRect.height;
    }

    void OnDisable()
    {
        Debug.LogError("OnDisable:" + _entries.Count);
        WriteFile();
    }

    // ReSharper disable once UnusedMember.Local
    void OnGUI()
    {
        viewRect = position;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        HandleVerticalResize();

        if (GUI.Button(new Rect(10, 2, 50, searchBarHeight), "Clear", EditorStyles.toolbarButton))
            ClearLog();

        GUILayout.BeginHorizontal();

        GUILayout.Space(5);

        //collapse = GUILayout.Toggle(collapse, new GUIContent("Collapse"), EditorStyles.toolbarButton, GUILayout.Width(50));
        //clearOnPlay = GUILayout.Toggle(clearOnPlay, new GUIContent("Clear On Play"), EditorStyles.toolbarButton, GUILayout.Width(70));
        //errorPause = GUILayout.Toggle(errorPause, new GUIContent("Error Pause"), EditorStyles.toolbarButton, GUILayout.Width(60));

        //GUILayout.FlexibleSpace();

        //showLog = GUILayout.Toggle(showLog, new GUIContent("L"), EditorStyles.toolbarButton, GUILayout.Width(30));
        //showWarnings = GUILayout.Toggle(showWarnings, new GUIContent("W"), EditorStyles.toolbarButton, GUILayout.Width(30));
        //showErrors = GUILayout.Toggle(showErrors, new GUIContent("E"), EditorStyles.toolbarButton, GUILayout.Width(30));

        GUILayout.EndHorizontal();

        _consoleLogTree.OnGUI(new Rect(0, pading * 2 + searchBarHeight, viewRect.width, topTreeHeight - pading - searchBarHeight));

        _stackTree.OnGUI(new Rect(0, topTreeHeight + _splitterRect.height, viewRect.width, bottomTreeHeight));

        float searchWidth = Mathf.Max(100, viewRect.width * 0.3f);
        _searchString =_searchField.OnGUI(
                new Rect(viewRect.width - searchWidth, pading, searchWidth - 5f, searchBarHeight),
                _searchString);
        _consoleLogTree.searchString = _searchString;

        if (m_ResizingVerticalSplitterLeft || _dirty)
        {
            _dirty = false;
            Repaint();
        }
    }

    private void HandleVerticalResize()
    {
        _splitterRect.y = (int)(viewRect.height * verticalSplitterPercent);

        EditorGUIUtility.AddCursorRect(_splitterRect, MouseCursor.ResizeVertical);
        if (Event.current.type == EventType.MouseDown && _splitterRect.Contains(Event.current.mousePosition))
            m_ResizingVerticalSplitterLeft = true;

        if (m_ResizingVerticalSplitterLeft)
        {
            verticalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.y / viewRect.height, 0.25f, 0.92f);
            _splitterRect.y = (int)(viewRect.height * verticalSplitterPercent);
        }

        topTreeHeight = _splitterRect.y;
        bottomTreeHeight = viewRect.height - topTreeHeight - _splitterRect.height;

        //check control size finish
        if (Event.current.type == EventType.MouseUp)
        {
            m_ResizingVerticalSplitterLeft = false;
        }
    }

    private void ClearLog()
    {
        _dirty = true;
        _entries.Clear();
        _consoleLogTree.Clear();
        _stackTree.ClearStackTrack();
    }

    private void OnLogging(string condition, string stackTrace, LogType type)
    {
        LogEntry entry = new LogEntry
        {
            Output = condition,
            StackTrace = stackTrace,
            LogType = type,
        };
        _consoleLogTree.AddLogTreeItem(entry);
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
