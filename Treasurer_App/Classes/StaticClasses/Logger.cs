using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using Treasurer_App.Classes.Exceptions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Treasurer_App.Classes.StaticClasses
{
    internal static class Logger
    {

        private static string? _path = null;
        private static bool enable_console = true;
        private static bool enable_file = true;

        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warn = 2,
            Error = 3,
            Fatal = 4,
        }

        private static string logMessage<T>(T obj, LogLevel level, string message) {
            return $"( {obj.ToString()} ) [{DateTime.Now}] {level.ToString()}: {message}";
        }

        private static string logMessage(string str, LogLevel level, string message) {
            return $"( {str} ) [{DateTime.Now}] {level.ToString()}: {message}";
        }

        private static string logExceptionMessage<T>(T obj, LogLevel level, Exception ex) {
            return $"( {obj.ToString()} ) [{DateTime.Now}] {level.ToString()}: {ex.ToString()}";
        }

        private static void WriteLog (string msg) {
            int attempts = 100;
            int attempt = 0;
            while (true) {
                try {
                    using (StreamWriter file = File.AppendText(_path)) {
                        file.WriteLine(msg);
                    }

                    break;
                } catch {
                    if (attempt++ < attempts) {
                        System.Threading.Thread.Sleep(100);
                    } else {
                        throw;
                    }
                }
            }
        }

        private static string StaticToString => "Logger";

        public static void Init()
        {

            // Set path either relative or absolute
            #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string log_dir = bool.Parse(ConfigurationManager.AppSettings["relative_log_file_path"]) ?
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\"
                    + ConfigurationManager.AppSettings["log_file_path"] : ConfigurationManager.AppSettings["log_file_path"];
            #pragma warning restore CS8600

            // Create directory if needed
            if (!Directory.Exists(log_dir)) { Directory.CreateDirectory(log_dir); }

            // Path to session log file
            _path = log_dir + @"\" + ConfigurationManager.AppSettings["log_file_base"] + "-" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + @".txt";

            // Create Log File
            using (StreamWriter file = File.CreateText(_path)) ;
            Logger.Log(StaticToString, LogLevel.Info, "Initialized Successfully");
        }

        public static void Log<T>(T obj, LogLevel level, string message)
        {
            // Init not called if evaluated to true
            if (_path == null && enable_file) { throw new InitException("_path not set -\"{message}\""); }

            // Console Logging Functionality
            if ((level == LogLevel.Error || level == LogLevel.Fatal) && enable_console)
            {
                Console.Error.WriteLine(logMessage(obj, level, message));
            }
            else if (enable_console)
            {
                Console.WriteLine(logMessage(obj, level, message));
            }

            // File Logging Functionality
            if (enable_file)
            {
                WriteLog(logMessage(obj, level, message));
            }
        }

        public static void Log<T>(T obj, LogLevel level, Exception ex) {
            // Init not called if evaluated to true
            if (_path == null && enable_file) { throw new InitException($"_path not set - \"{ex.ToString()}\""); }

            // Console Logging Functionality
            if ((level == LogLevel.Error || level == LogLevel.Fatal) && enable_console) {
                Console.Error.WriteLine(logExceptionMessage(obj, level, ex));
            } else if (enable_console) {
                Console.WriteLine(logExceptionMessage(obj, level, ex));
            }

            // File Logging Functionality
            if (enable_file) {
                WriteLog(logExceptionMessage(obj, level, ex));
            }
        }

        // Static Log: Used by static classes who have instance to pass
        public static void Log(string name, LogLevel level, string message) {
            // Init not called if evaluated to true
            if (_path == null && enable_file) { throw new InitException("_path not set -\"{message}\""); }

            // Console Logging Functionality
            if ((level == LogLevel.Error || level == LogLevel.Fatal) && enable_console) {
                Console.Error.WriteLine(logMessage(name, level, message));
            } else if (enable_console) {
                Console.WriteLine(logMessage(name, level, message));
            }

            // File Logging Functionality
            if (enable_file) {
                WriteLog(logMessage(name, level, message));
            }
        }

    }
}
