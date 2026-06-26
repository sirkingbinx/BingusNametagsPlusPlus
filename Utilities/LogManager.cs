using System;
using System.IO;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class LogManager
{
    public static string LogFile => Path.Combine(Constants.BingusNametagsData, "Latest.log");
    public static bool LoggingToUnity = true;

    public static StreamWriter? LogWriter;

    public static void CreateLog()
    {
        Debug.Log($"[BG++] creating a log file at \"{LogFile}\"");

        try
        {
            if (!Directory.Exists(Constants.BingusNametagsData))
                Directory.CreateDirectory(Constants.BingusNametagsData);

            LogWriter = new StreamWriter(LogFile);
            LogWriter.AutoFlush = true;

            LoggingToUnity = false;
        }
        catch (Exception ex)
        {
            Debug.Log($"============================================\n[BG++] could not generate log file. ${ex.Message}\nAll BingusNametags++ related messages are forwarded into the Unity Log. (Ctrl + F - search for [BG++])\n============================================");
        }

        LogDivider();
        LogLine($"BingusNametags++ {Constants.Version} (channel: {(Constants.Channel == ReleaseChannel.Stable ? "stable" : "beta")})");
        LogLine($"Unity Version: {Application.unityVersion}");
        LogLine($"Gorilla Tag Version: {Application.version}");
        LogDivider();
    }

    public static void LogDivider() =>
        LogLine("============================================");

    public static void LogLine(string text) => Log($"{text}");

    public static void Log(string text, string ending = "\n")
    {
        if (LoggingToUnity)
            Debug.Log($"[BG++]: {text}");
        else
            LogWriter?.Write($"{text}{ending}");
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