// PromptRFA/Logger.cs
using System;
using System.IO;

namespace PromptRFA
{
    public static class Logger
    {
        // ログの保存先 (環境に合わせて変更してください)
        private const string LOG_PATH = @"C:\Users\81805\StudioProjects\prompt_rfa\debug_log.txt";

        // 初期化（開始ログ）
        public static void Initialize()
        {
            try 
            { 
                File.WriteAllText(LOG_PATH, "=== PromptRFA Log Started ===\n"); 
            } 
            catch { /* 無視 */ }
        }

        // 追記
        public static void Write(string message)
        {
            try
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                File.AppendAllText(LOG_PATH, $"[{time}] {message}\n");
            }
            catch { /* 無視 */ }
        }
    }
}