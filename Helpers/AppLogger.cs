using System;
using System.IO;
using System.Threading;

namespace PaperPaint.Helpers
{
    /// <summary>Simple append-only file logger to LocalAppData\PaperPaint\logs.</summary>
    public static class AppLogger
    {
        private static readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        public static void Log(string message)
        {
            _ = WriteAsync($"[INFO] {message}");
        }

        public static void LogException(Exception ex, string context)
        {
            _ = WriteAsync($"[ERROR] {context}: {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        private static async System.Threading.Tasks.Task WriteAsync(string line)
        {
            try
            {
                await _gate.WaitAsync();
                try
                {
                    string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PaperPaint", "logs");
                    Directory.CreateDirectory(logDir);
                    string logFile = Path.Combine(logDir, "paperpaint.log");
                    await File.AppendAllTextAsync(logFile, $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} {line}{Environment.NewLine}");
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (Exception)
            {
                // Logging must never crash the app.
            }
        }
    }
}
