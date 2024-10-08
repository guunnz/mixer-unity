using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // Include SceneManager

public class FileLogger : MonoBehaviour
{
    private string logFilePath;

    void Awake()
    {
        logFilePath = Path.Combine(Application.persistentDataPath, "game_logs.txt");

        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"{DateTime.Now}: [{type}] {logString}\n";
        if (type == LogType.Error || type == LogType.Exception)
        {
            logEntry += $"{stackTrace}\n";
            // Check if the log entry contains the specific 503 error message
            if (logString.Contains("HTTP/1.1 503"))
            {
                SceneManager.LoadScene(2); // Load scene number 2
            }
        }

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
