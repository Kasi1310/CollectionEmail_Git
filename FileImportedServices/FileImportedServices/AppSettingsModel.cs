using Newtonsoft.Json;

namespace FileImportedServices
{
    public class AppSettings
    {
        public string server { get; set; }
        public string db { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string from_path { get; set; }
        public string to_path { get; set; }
    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }

        [JsonProperty("Microsoft.AspNetCore")]
        public string MicrosoftAspNetCore { get; set; }
    }
    internal class AppSettingsModel
    {
        public Logging Logging { get; set; }
        public AppSettings AppSettings { get; set; }
        public string AllowedHosts { get; set; }
        
    }
}