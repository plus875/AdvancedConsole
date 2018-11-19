using System;
using System.Collections.Generic;
using UnityEngine;

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

    private List<string> _stackList;

    public List<string> GetStackTrack()
    {
        if (_stackList == null)
        {
            _stackList = new List<string>(10);
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