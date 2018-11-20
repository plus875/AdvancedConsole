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

    private List<LogEntry> _entries = new List<LogEntry>();
    private List<LogEntry> _showingEntries = new List<LogEntry>();
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
    private bool _resizingVerticalSplitter;
    private Rect viewRect;
    private float searchBarHeight = 16f;

    private bool _showLog;
    private bool _showWarnings;
    private bool _showErrors;

    private int _logCount;
    private int _warnCount;
    private int _errorCount;

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

        if (_consoleLogTree == null)
        {
            Log = EditorGUIUtility.Load("log.png") as Texture2D;
            Warn = EditorGUIUtility.Load("warn.png") as Texture2D;
            Error = EditorGUIUtility.Load("error.png") as Texture2D;

            _consoleLogTree = new ConsoleLogTree(_treeViewState, true, 22f);
            _consoleLogTree.OnSelectionChanged += OnSelecLogChanged;
            _stackTree = new StackTrackTree(_stackTreeViewState, false, 18f);

            _searchField = new SearchField();
        }

        viewRect = position;
        float startY = pading * 2 + searchBarHeight;
        topTreeHeight = viewRect.height * verticalSplitterPercent - startY;
        _splitterRect = new Rect(0, viewRect.height * verticalSplitterPercent, viewRect.width, 5);
        bottomTreeHeight = viewRect.height - topTreeHeight - _splitterRect.height;

        RefreshShowingEntry();
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= OnLogging;

        //Debug.LogError("OnDisable:" + _entries.Count);
        //WriteFile();
    }

    // ReSharper disable once UnusedMember.Local
    void OnGUI()
    {
        viewRect = position;

        GUILayout.BeginVertical();
        GUILayout.Space(pading);
        GUILayout.EndVertical();

        HandleVerticalResize();

        if (GUI.Button(new Rect(10, pading, 50, searchBarHeight), "Clear", EditorStyles.toolbarButton))
            ClearLog();

        //toolbar
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        //collapse = GUILayout.Toggle(collapse, new GUIContent("Collapse"), EditorStyles.toolbarButton, GUILayout.Width(50));
        //clearOnPlay = GUILayout.Toggle(clearOnPlay, new GUIContent("Clear On Play"), EditorStyles.toolbarButton, GUILayout.Width(70));
        //errorPause = GUILayout.Toggle(errorPause, new GUIContent("Error Pause"), EditorStyles.toolbarButton, GUILayout.Width(60));
        GUILayout.FlexibleSpace();

        bool showLog = _showLog; bool showWarning = _showWarnings; bool showError = _showErrors;

        _showLog = GUILayout.Toggle(_showLog, new GUIContent(_logCount.ToString(), Log), EditorStyles.toolbarButton, GUILayout.MinWidth(30));
        _showWarnings = GUILayout.Toggle(_showWarnings, new GUIContent(_warnCount.ToString(), Warn), EditorStyles.toolbarButton, GUILayout.MinWidth(30));
        _showErrors = GUILayout.Toggle(_showErrors, new GUIContent(_errorCount.ToString(), Error), EditorStyles.toolbarButton, GUILayout.MinWidth(30));
        GUILayout.EndHorizontal();

        //log tree
        _consoleLogTree.OnGUI(new Rect(0, pading * 2 + searchBarHeight, viewRect.width, topTreeHeight - pading - searchBarHeight));

        //stackTrack tree
        _stackTree.OnGUI(new Rect(0, topTreeHeight + _splitterRect.height, viewRect.width, bottomTreeHeight));

        //searchBar
        float searchWidth = Mathf.Max(100, viewRect.width * 0.3f);
        _searchString =_searchField.OnGUI(
                new Rect(viewRect.width - searchWidth - CalcIconSize(), 1, searchWidth, searchBarHeight),
                _searchString);
        _consoleLogTree.searchString = _searchString;

        //check filter log
        if(showLog != _showLog || showWarning != _showWarnings || showError != _showErrors)
            RefreshShowingEntry();

        //check divide panel
        if (_resizingVerticalSplitter || _dirty)
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
            _resizingVerticalSplitter = true;

        if (_resizingVerticalSplitter)
        {
            verticalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.y / viewRect.height, 0.25f, 0.92f);
            _splitterRect.y = (int)(viewRect.height * verticalSplitterPercent);
        }

        topTreeHeight = _splitterRect.y;
        bottomTreeHeight = viewRect.height - topTreeHeight - _splitterRect.height;

        //check control size finish
        if (Event.current.type == EventType.MouseUp)
        {
            _resizingVerticalSplitter = false;
        }
    }

    private float CalcIconSize()
    {
        float baseSize = 30 * 3;
        const float charWidth = 6;
        baseSize += _logCount.ToString().Length * charWidth + _warnCount.ToString().Length * charWidth +
                    _errorCount.ToString().Length * charWidth;
        return baseSize;
    }

    private void ClearLog()
    {
        _dirty = true;
        _entries.Clear();
        RefreshShowingEntry();
        _stackTree.ClearStackTrack();
    }

    private void OnLogging(string condition, string stackTrace, LogType type)
    {
        LogEntry entry = CreateInstance<LogEntry>();
        entry.Output = condition;
        entry.StackTrace = stackTrace;
        entry.LogType = type;

        if(TryAddShowing(entry))
            _consoleLogTree.AddLogTreeItem(entry);
        _entries.Add(entry);
    }

    private bool TryAddShowing(LogEntry entry)
    {
        if (entry.LogType == LogType.Log)
        {
            _logCount++;
            if (_showLog)
            {
                _showingEntries.Add(entry);
                return true;
            }
        }

        if (entry.LogType == LogType.Warning)
        {
            _warnCount++;
            if (_showWarnings)
            {
                _showingEntries.Add(entry);
                return true;
            }
        }

        if (entry.LogType == LogType.Error)
        {
            _errorCount++;
            if (_showErrors)
            {
                _showingEntries.Add(entry);
                return true;
            }
        }
        
        return false;
    }

    private void RefreshShowingEntry()
    {
        _showingEntries.Clear();
        _logCount = 0;
        _warnCount = 0;
        _errorCount = 0;

        _consoleLogTree.Clear();
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            bool beShow = TryAddShowing(entry);
            if(beShow)
                _consoleLogTree.AddLogData(entry);
        }
        _consoleLogTree.EndAddAllLogData();
    }

    private void OnSelecLogChanged(LogEntry logEntry)
    {
        if (logEntry == null)
            _stackTree.ClearStackTrack();
        else
            _stackTree.SetStackTrack(logEntry);
    }
}
