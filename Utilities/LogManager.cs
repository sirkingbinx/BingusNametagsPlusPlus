using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class LogManager
{
    public static string LogFolder => Path.Combine(Constants.BingusNametagsData, "logs");
    public static bool LoggingToUnity = true;

    public static StreamWriter LogWriter;

    public static void CreateLog()
    {
        var logFile = Path.Combine(LogFolder,
            $"bg++_{DateTime.Now.ToShortDateString().Replace("/", "-")}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "-")}.txt");

        Debug.Log($"[BG++] creating a log file at \"{logFile}\"");

        try
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            LogWriter = new StreamWriter(logFile);

            LoggingToUnity = false;
        }
        catch (Exception ex)
        {
            Debug.Log($"============================================\n[BG++] could not generate log file. ${ex.Message}\nAll BingusNametags++ related messages are forwarded into the Unity Log. (Ctrl + F - search for [BG++])\n============================================");
        }

        LogWriter.AutoFlush = true;

        LogDivider();
        LogLine("BingusNametags++ log file");
        LogLine($"Unity Version: {Application.unityVersion}");
        LogLine($"Gorilla Tag Version: {Application.version}");
        LogDivider();
    }

    public static void LogDivider() =>
        LogLine("============================================");

    public static void LogLine(string text) => Log($"{text}\n");

    public static void Log(string text)
    {
        if (LoggingToUnity)
            Debug.Log($"[BG++]: {text}");
        else
            LogWriter.Write(text);
    }

    public static void LogException(Exception ex)
    {
        LogDivider();
        LogLine($"Error occured at {ex.Source}:");
        LogLine($"  Msg: {ex.Message}");
        LogLine($"  Stack Trace: {ex.StackTrace}");
        LogDivider();
    }
}