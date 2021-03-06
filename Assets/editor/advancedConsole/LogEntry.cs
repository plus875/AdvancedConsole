﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class LogEntry
{
    [SerializeField]
    public int IntId;
    [SerializeField]
    public LogType LogType;
    [SerializeField]
    public string Output;
    [SerializeField]
    public string StackTrace;

    public string Content
    {
        get { return Output + "\n" + StackTrace; }
    }

    public Texture2D Icon
    {
        get
        {
            switch (LogType)
            {
                case LogType.Error:
                    return AdvancedConsole.Instance.Error;
                case LogType.Warning:
                    return AdvancedConsole.Instance.Warn;
                default:
                    return AdvancedConsole.Instance.Log;
            }
        }
    }

    private List<string> _stackList { get; set; }
    public List<string> GetStackTrack()
    {
        if (_stackList == null)
        {
            _stackList = new List<string>(5);
            _stackList.Add(Output);

            int lineIndex = 0;
            string sep = "\n";
            string text = string.Empty;
            while (true)
            {
                int n = StackTrace.IndexOf(sep, lineIndex, StringComparison.Ordinal);
                if (n == -1)
                    break;
                int len = n - lineIndex;
                text = StackTrace.Substring(lineIndex, len);
                _stackList.Add(text);
                lineIndex = n + 1;
                if (string.IsNullOrEmpty(text))
                    break;
            }
        }
        return _stackList;
    }
}