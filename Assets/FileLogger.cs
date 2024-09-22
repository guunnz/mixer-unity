using System;
using System.IO;
using UnityEngine;

public class FileLogger : MonoBehaviour
{
    private string logFilePath;

    void Awake()
    {
        // Set the log file path to the same folder as the game executable
        logFilePath = Path.Combine(Application.persistentDataPath, "game_logs.txt");

        // Clear the existing log file on game start
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // Register the callback to handle log messages
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        // Unregister the callback when the object is destroyed
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Format the log message
        string logEntry = $"{DateTime.Now}: [{type}] {logString}\n";
        if (type == LogType.Error || type == LogType.Exception)
        {
            logEntry += $"{stackTrace}\n";
        }

        // Write the log message to the file
        try
        {
            File.AppendAllText(logFilePath, logEntry);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }
}
