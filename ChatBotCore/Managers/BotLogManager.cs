using NLog;
using NLog.Config;
using NLog.Layouts;
using System;

namespace BotCore.Managers
{
    public class BotLogManager
    {
        private string Log { get; set; }
        private string TraceLog { get; set; }

        public BotLogManager()
        {
            
        }

        public Logger GetManager<T>()
        {
            SetFiles();
            LogManager.Configuration = GetConf();
            return LogManager.GetLogger(typeof(T).ToString());
        }

        public Logger GetManager(string name)
        {
            SetFiles();
            LogManager.Configuration = GetConf();
            return LogManager.GetLogger(name);
        }

        private LoggingConfiguration GetConf()
        {
            var conf = new LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget
            {
                Name = "FileLog",
                FileName = Log,
                Layout = new SimpleLayout("=>${time}|${level}|${callsite:className=true:methodName=true}|${message};"),
                ArchiveAboveSize = 500000,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Rolling,
            };
            var logDebug = new NLog.Targets.DebuggerTarget
            {
                Name = "DebugLog",
                Layout = new SimpleLayout("=>${time}|${level}|${callsite:className=true:methodName=true}|${message};")
            };

            var tracefile = new NLog.Targets.FileTarget("FileLog")
            {
                Name = "FileLog",
                FileName = TraceLog,
                Layout = new SimpleLayout(">>>${time}|${callsite}|${stacktrace}|${message};"),
                ArchiveAboveSize = 500000,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Rolling,
            };
            conf.AddRule(LogLevel.Trace, LogLevel.Fatal, logDebug);
            conf.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            conf.AddRule(LogLevel.Trace, LogLevel.Fatal, tracefile);

            return conf;
        }

        private void SetFiles()
        {
            Log = $"{ DateTime.Today.Day}.{ DateTime.Today.Month}.{ DateTime.Today.Year}.log";
            TraceLog = $"{ DateTime.Today.Day}.{ DateTime.Today.Month}.{ DateTime.Today.Year}-stacktrace.log";
        }

    }
}
