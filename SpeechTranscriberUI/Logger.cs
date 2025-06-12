// Logger.cs
using System;
using System.IO;

public static class Logger
{
    private static readonly string logFile = "SpeechTranscriber.log";
    private static object lockObj = new object();
    public static void Log(string message)
    {
        try
        {
            lock (lockObj)
                File.AppendAllText(logFile, $"{DateTime.Now:u} {message}{Environment.NewLine}");
        }
        catch { /* Ignore logging errors */ }
    }
}
