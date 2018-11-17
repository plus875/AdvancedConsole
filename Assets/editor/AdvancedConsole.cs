using System;
using System.Collections;
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
    private ConsoleTree _consoleTree;

    private Vector2 _position;
    private int _id;

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

    void OnEnable()
    {
        Application.logMessageReceivedThreaded += OnLogging;
        //Application.logMessageReceived += OnLogging;

        if (_consoleTree == null)
        {
            _consoleTree = new ConsoleTree(_treeViewState);

            Log = EditorGUIUtility.Load("log.png") as Texture2D;
            Warn = EditorGUIUtility.Load("warn.png") as Texture2D;
            Error = EditorGUIUtility.Load("error.png") as Texture2D;
        }
    }

    void OnDisable()
    {
        Debug.LogError("OnDisable:" + _entries.Count);
        WriteFile();
    }

    void OnGUI()
    {
        _position = EditorGUILayout.BeginScrollView(_position, GUILayout.Width(position.width), GUILayout.Height(300));
        //EditorGUILayout.LabelField(_logEntry.Content, GUILayout.Height(800));
        for (var i = 0; i < _entries.Count; i++)
        {
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(_entries[i].Content, GUILayout.Height(80), GUILayout.Width(position.width - 30));
            EditorGUILayout.Space();
            //EditorGUILayout.EndHorizontal();
            //EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Separator();

        EditorGUILayout.TextArea("1111111111111");

        _consoleTree.OnGUI(new Rect(0, 400, position.width, 100));
        _consoleTree.Reload();

        EditorGUILayout.LabelField("dddddddd\ndasfdasfds");
        EditorGUILayout.TextArea("2222222222222");
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
}

public class LogEntry
{
    public LogType LogType;
    public string Output;
    public string StackTrace;

    public string Content {
        get { return Output + "\n" + StackTrace; }
    }

    public Texture2D Icon
    {
        get
        {
            switch (LogType)
            {
                case LogType.Error:
                    return AdvancedConsole.Error;
                case LogType.Warning:
                    return AdvancedConsole.Warn;
                default:
                    return AdvancedConsole.Log;
            }
        }
       
    }
}