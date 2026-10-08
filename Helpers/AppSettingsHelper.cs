using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PaperPaint.Helpers
{
    public static class AppSettingsHelper
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PaperPaint",
            "settings.json");

        private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
        private static bool _loaded;
        private static readonly object _lock = new object();

        private static void EnsureLoaded()
        {
            lock (_lock)
            {
                if (_loaded) return;
                _loaded = true;
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        var json = File.ReadAllText(SettingsPath);
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        if (dict != null)
                        {
                            foreach (var kvp in dict)
                            {
                                _cache[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore read errors
                }
            }
        }

        public static string? GetString(string key)
        {
            EnsureLoaded();
            lock (_lock)
            {
                return _cache.TryGetValue(key, out var val) ? val : null;
            }
        }

        public static void SetString(string key, string value)
        {
            EnsureLoaded();
            lock (_lock)
            {
                _cache[key] = value;
                try
                {
                    var dir = Path.GetDirectoryName(SettingsPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);
                    File.WriteAllText(SettingsPath, json);
                }
                catch
                {
                    // Ignore write errors
                }
            }
        }
    }
}
