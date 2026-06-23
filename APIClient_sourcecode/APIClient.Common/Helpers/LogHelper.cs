using Serilog.Events;
using System.Configuration;
using System.Reflection;

namespace APIClient.Common.Helpers
{
    internal class LogHelper
    {
        internal static LogEventLevel LogLevel
        {
            get
            {
                LogEventLevel logEventLevel;

                switch (ConfigurationManager.AppSettings["LogLevel"].ToLower())
                {
                    case "debug":
                        logEventLevel = LogEventLevel.Debug;
                        break;
                    case "error":
                        logEventLevel = LogEventLevel.Error;
                        break;
                    default:
                        logEventLevel = LogEventLevel.Information;
                        break;
                }
                return logEventLevel;
            }
        }

        internal static long FileSizeLimitBytes
        {
            get
            {
                return long.TryParse(ConfigurationManager.AppSettings["LogFileSizeLimitBytes"], out long FileSizeLimitBytes) ? FileSizeLimitBytes : 5242880;
            }
        }

        internal static int RetainedFileCountLimit
        {
            get
            {
                return int.TryParse(ConfigurationManager.AppSettings["RetainLogFileCountLimit"], out int RetainLogFileCount) ? RetainLogFileCount : 1000;
            }
        }

        internal static string LogPath
        {
            get
            {
                var logPath = ConfigurationManager.AppSettings["LogPath"];
                return $"{(!string.IsNullOrWhiteSpace(logPath) ? logPath : $"Logs")}\\{Assembly.GetEntryAssembly().GetName().Name}-.log";
            }
        }
    }
}
