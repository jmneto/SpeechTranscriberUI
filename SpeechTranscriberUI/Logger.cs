// Logger.cs
using System;
using System.IO;

public static class Logger
{
    private static readonly string logFile = "SpeechTranscriber.log";
    public static void Log(string message)
    {
        try
        {
            File.AppendAllText(logFile, $"{DateTime.Now:u} {message}{Environment.NewLine}");
        }
        catch { /* Ignore logging errors */ }
    }
}
