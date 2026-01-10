using System;
using System.IO;
using GorillaNetworking;
using UnityEngine;

namespace BingusNametagsPlusPlus.Utilities;

public static class LogManager
{
    public static string LogFolder => Path.Combine(Constants.BingusNametagsData, "logs");
    public static bool LoggingToUnity = false;

    public static StreamWriter LogWriter;

    public static void CreateLog()
    {
        var logFile = Path.Combine(LogFolder,
            $"bg++_{DateTime.Now.ToShortDateString().Replace("/", "-")}_{DateTime.Now.ToLongTimeString().Replace(" ", "-")}.txt");

        Debug.Log($"[BG++] creating a log file at \"{logFile}\"");

        try
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            LogWriter = File.CreateText(logFile);
        }
        catch (Exception ex)
        {
            Debug.Log($"[BG++] could not generate log file: ${ex.Message}");
            LoggingToUnity = true;
        }

        LogWriter.AutoFlush = true;

        LogLine();
        Log("BingusNametags++ log file");
        Log($"Unity Version: {Application.unityVersion}");
        Log($"Gorilla Tag Version: {GorillaComputer.instance.version}");
        LogLine();
    }

    public static void LogLine() =>
        Log("============================================");

    public static void Log(string text)
    {
        if (LoggingToUnity)
            Debug.Log($"[BG++]: {text}");
        else
            LogWriter.WriteLine(text);
    }

    public static void LogException(Exception ex)
    {
        LogLine();
        Log($"Error occured at {ex.Source}:");
        Log($"  Msg: {ex.Message}");
        Log($"  Stack Trace: {ex.StackTrace}");
        LogLine();
    }
}