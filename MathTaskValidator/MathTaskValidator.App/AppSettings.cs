using System;
using System.IO;
using System.Text.Json;

namespace MathTaskValidator.App
{
    internal static class AppSettings
    {
        private static string? _apiBaseUrl;

        public static string GetApiBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(_apiBaseUrl))
                return _apiBaseUrl;

            var env = Environment.GetEnvironmentVariable("API_BASE_URL");
            if (!string.IsNullOrWhiteSpace(env))
            {
                _apiBaseUrl = env.Trim().TrimEnd('/');
                return _apiBaseUrl;
            }

            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var file = Path.Combine(basePath, "appsettings.json");
                if (File.Exists(file))
                {
                    using var fs = File.OpenRead(file);
                    var doc = JsonDocument.Parse(fs);
                    if (doc.RootElement.TryGetProperty("ApiBaseUrl", out var prop) && prop.ValueKind == JsonValueKind.String)
                    {
                        var v = prop.GetString();
                        if (!string.IsNullOrWhiteSpace(v))
                        {
                            _apiBaseUrl = v.Trim().TrimEnd('/');
                            return _apiBaseUrl;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error reading appsettings.json: {ex.Message}");
            }

            // 3. Default
            _apiBaseUrl = "https://localhost:44376";
            return _apiBaseUrl;
        }
    }
}
