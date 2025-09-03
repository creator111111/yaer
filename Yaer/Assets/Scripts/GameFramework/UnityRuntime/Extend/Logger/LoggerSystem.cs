using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameFramework.CoreExtend.Systems.Logger
{
    public class LoggerSystem: ILoggerSystem
    {
        private StreamWriter logWriter;
        
        public void Init()
        {
            // 初始化日志文件写入器
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var logFileName = $"log_{currentDate}.txt";
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "log")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "log"));

            logWriter = new StreamWriter(Path.Combine(Application.persistentDataPath + "/log/", logFileName), true);

            Application.logMessageReceived += (logString, stackTrace, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception)
                    LogError(logString + "\nStackTrace: " + stackTrace);
                else if (type == LogType.Warning)
                    LogWarning(logString + "\nStackTrace: " + stackTrace);
                else
                    Log(logString);
            };

            DeleteOldLogFiles();
        }

        public void Close()
        {
            // 关闭日志文件写入器
            if (logWriter != null) logWriter.Close();
        }

        private void Log(string message)
        {

            // 输出日志到日志文件
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now}] [info] {message}");
                logWriter.Flush();
            }
        }

        private void LogWarning(string message)
        {
            // 输出警告日志到控制台
            // Debug.LogWarning(message);

            // 输出警告日志到日志文件
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now}] [Waring] {message}");
                logWriter.Flush();
            }
        }

        private void LogError(string message)
        {
            // 输出错误日志到控制台
            // Debug.LogError(message);

            // 输出错误日志到日志文件
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now}] [Error] {message}");
                logWriter.Flush();
            }
        }

        private void DeleteOldLogFiles()
        {
            var directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/log");
            var logFiles = directoryInfo.GetFiles("*.txt");

            if (logFiles.Length > 10)
            {
                // 按创建时间排序，删除最旧的日志文件
                var sortedLogFiles = logFiles.OrderBy(f => f.CreationTime).ToList();
                for (var i = 0; i < sortedLogFiles.Count - 10; i++) sortedLogFiles[i].Delete();
            }
        }
    }
}