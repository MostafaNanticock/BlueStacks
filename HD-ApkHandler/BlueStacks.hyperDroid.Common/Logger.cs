using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BlueStacks.hyperDroid.Common
{
    public class Logger
    {
        public delegate void HdLoggerCallback(int prio, uint tid, string tag, string msg);

        public class Writer : TextWriter
        {
            public delegate void WriteFunc(string msg);

            private WriteFunc writeFunc;

            public override Encoding Encoding
            {
                get
                {
                    return Encoding.UTF8;
                }
            }

            public Writer(WriteFunc writeFunc)
            {
                this.writeFunc = writeFunc;
            }

            public override void WriteLine(string msg)
            {
                this.writeFunc(msg);
            }

            public override void WriteLine(string fmt, object obj)
            {
                this.writeFunc(string.Format(fmt, obj));
            }

            public override void WriteLine(string fmt, object[] objs)
            {
                this.writeFunc(string.Format(fmt, objs));
            }
        }

        public static int LOG_LEVEL_FATAL = 1;

        public static int LOG_LEVEL_ERROR = 2;

        public static int LOG_LEVEL_WARNING = 3;

        public static int LOG_LEVEL_DEBUG = 4;

        public static int LOG_LEVEL_INFO = 5;

        private static int HDLOG_PRIORITY_FATAL = 0;

        private static int HDLOG_PRIORITY_ERROR = 1;

        private static int HDLOG_PRIORITY_WARNING = 2;

        private static int HDLOG_PRIORITY_INFO = 3;

        private static int HDLOG_PRIORITY_DEBUG = 4;

        private static object s_sync = new object();

        private static TextWriter writer = Console.Error;

        private static int s_logRotationTime = 30000;

        public static int s_logFileSize = 10485760;

        public static int s_totalLogFileNum = 5;

        private static string s_logFileName = "BlueStacks";

        private static string s_logFilePath = null;

        private static bool s_consoleLogging = false;

        private static bool s_loggerInited = false;

        private static int s_processId = -1;

        private static string s_processName = "Unknown";

        private static string s_logLevels = null;

        private static FileStream s_fileStream;

        private static string s_logDir = null;

        private static string s_logStringFatal = "FATAL";

        private static string s_logStringError = "ERROR";

        private static string s_logStringWarning = "WARNING";

        private static string s_logStringInfo = "INFO";

        private static string s_logStringDebug = "DEBUG";

        private static string s_commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        private static string s_bstCommonAppData = Path.Combine(Logger.s_commonAppData, "BlueStacks");

        private static string s_bstUserDataDir = Path.Combine(Logger.s_bstCommonAppData, "UserData");

        public static HdLoggerCallback s_HdLoggerCallback;

        [CompilerGenerated]
        private static ThreadStart _003C_003E9__CachedAnonymousMethodDelegate1;

        [CompilerGenerated]
        private static Writer.WriteFunc _003C_003E9__CachedAnonymousMethodDelegate3;

        private static string GetLogDir(bool userSpecificLog)
        {
            if (Logger.s_logDir != null)
            {
                return Logger.s_logDir;
            }
            string result;
            try
            {
                if (userSpecificLog)
                {
                    result = Path.Combine(Logger.s_bstUserDataDir, "Logs");
                }
                else
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
                    result = (string)registryKey.GetValue("LogDir");
                }
            }
            catch (Exception)
            {
                result = Path.Combine(Logger.s_bstUserDataDir, "Logs");
            }
            Logger.s_logDir = result;
            return result;
        }

        public static void SetLogDir(string logDir)
        {
            Logger.s_logDir = logDir;
        }

        public static void InitUserLog()
        {
            Logger.InitLog(null, true, false);
        }

        public static void InitUserLogWithRotation()
        {
            Logger.InitLog(null, true, true);
        }

        public static void InitSystemLog()
        {
            Logger.InitLog(null, false, false);
        }

        public static void InitSystemLogWithRotation()
        {
            Logger.InitLog(null, false, true);
        }

        public static void InitConsoleLog()
        {
            Logger.InitLog("-", true, false);
        }

        private static void HdLogger(int prio, uint tid, string tag, string msg)
        {
            int level = 0;
            if (prio == Logger.HDLOG_PRIORITY_FATAL)
            {
                level = Logger.LOG_LEVEL_FATAL;
            }
            else if (prio == Logger.HDLOG_PRIORITY_ERROR)
            {
                level = Logger.LOG_LEVEL_ERROR;
            }
            else if (prio == Logger.HDLOG_PRIORITY_WARNING)
            {
                level = Logger.LOG_LEVEL_WARNING;
            }
            else if (prio == Logger.HDLOG_PRIORITY_INFO)
            {
                level = Logger.LOG_LEVEL_INFO;
            }
            else if (prio == Logger.HDLOG_PRIORITY_DEBUG)
            {
                level = Logger.LOG_LEVEL_DEBUG;
            }
            Logger.Print(level, tag, "{0:X8}: {1}", tid, msg);
        }

        public static void InitLog(string logFileName, bool userSpecificLog, bool doLogRotation)
        {
            Logger.s_loggerInited = true;
            Logger.s_HdLoggerCallback = Logger.HdLogger;
            Logger.s_processId = Process.GetCurrentProcess().Id;
            Logger.s_processName = Process.GetCurrentProcess().ProcessName;
            if (logFileName == "-")
            {
                Logger.writer = Console.Error;
                Logger.s_consoleLogging = true;
            }
            else
            {
                if (logFileName == null)
                {
                    logFileName = Logger.s_logFileName;
                }
                if (userSpecificLog)
                {
                    logFileName += "Users";
                }
                string logDir = Logger.GetLogDir(userSpecificLog);
                string text = logDir + "\\" + logFileName + ".log";
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                Logger.s_logFilePath = text;
                Logger.LogLevelsInit();
                if (doLogRotation)
                {
                    Thread thread = new Thread((ThreadStart)delegate
                    {
                        Logger.DoLogRotation();
                    });
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
        }

        private static void LogLevelsInit()
        {
            string name = "Software\\BlueStacks\\Guests\\Android\\Config";
            try
            {
                RegistryKey registryKey;
                using (registryKey = Registry.LocalMachine.OpenSubKey(name))
                {
                    Logger.s_logLevels = (string)registryKey.GetValue("DebugLogs");
                }
            }
            catch (Exception)
            {
                return;
            }
            if (Logger.s_logLevels != null)
            {
                Logger.s_logLevels = Logger.s_logLevels.ToUpper();
            }
        }

        private static bool IsLogLevelEnabled(string tag, string level)
        {
            if (Logger.s_logLevels == null)
            {
                return false;
            }
            if (Logger.s_logLevels.StartsWith("ALL"))
            {
                return true;
            }
            return Logger.s_logLevels.Contains((tag + ":" + level).ToUpper());
        }

        private static void DoLogRotation()
        {
            while (true)
            {
                Thread.Sleep(Logger.s_logRotationTime);
                try
                {
                    lock (Logger.s_sync)
                    {
                        FileInfo fileInfo = new FileInfo(Logger.s_logFilePath);
                        if (fileInfo.Length >= Logger.s_logFileSize)
                        {
                            string destFileName = Logger.s_logFilePath + ".1";
                            string path = Logger.s_logFilePath + "." + Logger.s_totalLogFileNum;
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            for (int num = Logger.s_totalLogFileNum - 1; num >= 1; num--)
                            {
                                string text = Logger.s_logFilePath + "." + num;
                                string destFileName2 = Logger.s_logFilePath + "." + (num + 1);
                                if (File.Exists(text))
                                {
                                    File.Move(text, destFileName2);
                                }
                            }
                            File.Move(Logger.s_logFilePath, destFileName);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private static void Open()
        {
            if (!Logger.s_consoleLogging)
            {
                if (!Logger.s_loggerInited)
                {
                    Logger.InitLog("-", false, false);
                    Logger.s_loggerInited = true;
                }
                else
                {
                    Logger.s_fileStream = new FileStream(Logger.s_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Write | FileShare.Delete);
                    Logger.writer = new StreamWriter(Logger.s_fileStream, Encoding.UTF8);
                }
            }
        }

        private static void Close()
        {
            if (!Logger.s_consoleLogging)
            {
                Logger.writer.Close();
                Logger.s_fileStream.Dispose();
                Logger.writer.Dispose();
            }
        }

        public static TextWriter GetWriter()
        {
            return new Writer(delegate(string msg)
            {
                Logger.Print(msg);
            });
        }

        private static void Print(int level, string tag, string fmt, params object[] args)
        {
            string text = "UNKNOWN";
            if (level == Logger.LOG_LEVEL_FATAL)
            {
                text = Logger.s_logStringFatal;
            }
            else if (level == Logger.LOG_LEVEL_ERROR)
            {
                text = Logger.s_logStringError;
            }
            else if (level == Logger.LOG_LEVEL_WARNING)
            {
                text = Logger.s_logStringWarning;
            }
            else if (level == Logger.LOG_LEVEL_INFO)
            {
                text = Logger.s_logStringInfo;
            }
            else if (level == Logger.LOG_LEVEL_DEBUG)
            {
                text = Logger.s_logStringDebug;
            }
            if (level == Logger.LOG_LEVEL_DEBUG && !Logger.IsLogLevelEnabled(tag, text))
            {
                return;
            }
            lock (Logger.s_sync)
            {
                Logger.Open();
                //Logger.writer.WriteLine(Logger.GetPrefix(tag, text) + fmt, args);
                //Logger.writer.Flush();
                Logger.Close();
            }
        }

        private static void Print(string fmt, params object[] args)
        {
            Logger.Print(Logger.LOG_LEVEL_INFO, Logger.s_processName, fmt, args);
        }

        private static void Print(string msg)
        {
            Logger.Print("{0}", msg);
        }

        public static void Fatal(string fmt, params object[] args)
        {
            Logger.Print(Logger.LOG_LEVEL_FATAL, Logger.s_processName, fmt, args);
        }

        public static void Fatal(string msg)
        {
            Logger.Fatal("{0}", msg);
        }

        public static void Error(string fmt, params object[] args)
        {
            Logger.Print(Logger.LOG_LEVEL_ERROR, Logger.s_processName, fmt, args);
        }

        public static void Error(string msg)
        {
            Logger.Error("{0}", msg);
        }

        public static void Warning(string fmt, params object[] args)
        {
            Logger.Print(Logger.LOG_LEVEL_WARNING, Logger.s_processName, fmt, args);
        }

        public static void Warning(string msg)
        {
            Logger.Warning("{0}", msg);
        }

        public static void Info(string fmt, params object[] args)
        {
            Logger.Print(Logger.LOG_LEVEL_INFO, Logger.s_processName, fmt, args);
        }

        public static void Info(string msg)
        {
            Logger.Info("{0}", msg);
        }

        public static void Debug(string fmt, params object[] args)
        {
            Logger.Print(Logger.LOG_LEVEL_DEBUG, Logger.s_processName, fmt, args);
        }

        public static void Debug(string msg)
        {
            Logger.Debug("{0}", msg);
        }

        private static string GetPrefix(string tag, string logLevel)
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            DateTime now = DateTime.Now;
            return "{now.Year:D4}-{now.Month:D2}-{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}.{now.Millisecond:D3} " + Logger.s_processId + ":{managedThreadId:X8} (" + tag + "). " + logLevel + ": ";
        }
    }
}
