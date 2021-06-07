using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BlueStacks.hyperDroid.Common
{
    public class Logger
    {
        public delegate void WriterDelegate(string line);

        private enum WriterType
        {
            None,
            Core,
            Delegate
        }

        public delegate void HdLoggerCallback(int prio, uint tid, string tag, string msg);

        private class Native
        {
            [DllImport("HD-Logger-Native.dll", CharSet = CharSet.Unicode)]
            public static extern void LoggerDllInit(string prog, string file, bool toConsole);

            [DllImport("HD-Logger-Native.dll")]
            public static extern void LoggerDllReinit();

            [DllImport("HD-Logger-Native.dll", CharSet = CharSet.Unicode)]
            public static extern void LoggerDllPrint(string line);
        }

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

        public const string LOG_TO_CONSOLE = "-";

        public const int LOG_LEVEL_FATAL = 1;

        public const int LOG_LEVEL_ERROR = 2;

        public const int LOG_LEVEL_WARNING = 3;

        public const int LOG_LEVEL_DEBUG = 4;

        public const int LOG_LEVEL_INFO = 5;

        private const int HDLOG_PRIORITY_FATAL = 0;

        private const int HDLOG_PRIORITY_ERROR = 1;

        private const int HDLOG_PRIORITY_WARNING = 2;

        private const int HDLOG_PRIORITY_INFO = 3;

        private const int HDLOG_PRIORITY_DEBUG = 4;

        private const string DEFAULT_FILE_NAME = "BlueStacksUsers";

        private const string LOG_STRING_FATAL = "FATAL";

        private const string LOG_STRING_ERROR = "ERROR";

        private const string LOG_STRING_WARNING = "WARNING";

        private const string LOG_STRING_INFO = "INFO";

        private const string LOG_STRING_DEBUG = "DEBUG";

        private static string s_logLevels;

        private static WriterDelegate s_WriterDelegate;

        private static WriterType s_WriterType;

        private static HdLoggerCallback s_HdLoggerCallback;

        [CompilerGenerated]
        private static WaitOrTimerCallback _003C_003E9__CachedAnonymousMethodDelegate1;

        [CompilerGenerated]
        private static Writer.WriteFunc _003C_003E9__CachedAnonymousMethodDelegate3;

        private static string GetLogDir()
        {
            object obj = null;
            object obj2 = null;
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            if (registryKey != null)
            {
                obj2 = (string)registryKey.GetValue("DataDir");
                obj = (string)registryKey.GetValue("UserDataDir");
                registryKey.Close();
            }
            string path;
            if (obj2 != null)
            {
                path = (string)obj2;
            }
            else if (obj != null)
            {
                path = (string)obj;
            }
            else
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path = Path.Combine(path, "Bluestacks");
            }
            return Path.Combine(path, "Logs");
        }

        public static void InitLog(WriterDelegate writer)
        {
            Logger.s_WriterDelegate = writer;
            Logger.s_WriterType = WriterType.Delegate;
            Logger.LogLevelsInit();
        }

        public static void InitUserLog()
        {
            Logger.InitLog(null, null);
        }

        public static void InitLog(string logFileName, string logRotatorTag)
        {
            Logger.s_WriterType = WriterType.Core;
            Logger.s_HdLoggerCallback = Logger.HdLogger;
            string processName = Process.GetCurrentProcess().ProcessName;
            string file = null;
            bool toConsole = true;
            if (logFileName != "-")
            {
                toConsole = false;
                if (logFileName == null)
                {
                    logFileName = "BlueStacksUsers";
                }
                string logDir = Logger.GetLogDir();
                file = logDir + "\\" + logFileName + ".log";
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
            }
            Native.LoggerDllInit(processName, file, toConsole);
            Logger.LogLevelsInit();
            if (logRotatorTag != null)
            {
                Logger.InitLogRotation(logRotatorTag);
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

        private static void InitLogRotation(string tag)
        {
            string text = "Global\\BlueStacks_LogRotate_" + tag;
            bool flag = false;
            Logger.Print("Using event {0} for log rotation", text);
            try
            {
                EventWaitHandle waitObject = new EventWaitHandle(false, EventResetMode.AutoReset, text, out flag);
                if (!flag)
                {
                    Logger.Print("Log rotation event for " + tag + " already exists");
                }
                else
                {
                    WaitOrTimerCallback callBack = delegate
                    {
                        Logger.Print("Reopening log Prebundled");
                        Native.LoggerDllReinit();
                    };
                    ThreadPool.RegisterWaitForSingleObject(waitObject, callBack, null, -1, false);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
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

        public static TextWriter GetWriter()
        {
            return new Writer(delegate(string msg)
            {
                Logger.Print(msg);
            });
        }

        private static void Print(int level, string tag, string fmt, params object[] args)
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            string text = "UNKNOWN";
            switch (level)
            {
                case 1:
                    text = "FATAL";
                    break;
                case 2:
                    text = "ERROR";
                    break;
                case 3:
                    text = "WARNING";
                    break;
                case 5:
                    text = "INFO";
                    break;
                case 4:
                    text = "DEBUG";
                    break;
            }
            if (level == 4 && !Logger.IsLogLevelEnabled(tag, text))
            {
                return;
            }
            string str = (tag == null) ? string.Format("{0,5:D0} {1} ", managedThreadId, text) : string.Format("{0,5:D0} {1} {2} ", managedThreadId, tag, text);
            Logger.WriteMessageToLog(str + string.Format(fmt, args));
        }

        private static void WriteMessageToLog(string msg)
        {
            char[] separator = new char[1]
			{
				'\n'
			};
            char[] trimChars = new char[1]
			{
				'\r'
			};
            string[] array = msg.Split(separator);
            foreach (string text in array)
            {
                string text2 = text.Trim(trimChars);
                if (Logger.s_WriterType == WriterType.Core)
                {
                    Native.LoggerDllPrint(text2);
                }
                else if (Logger.s_WriterType == WriterType.Delegate)
                {
                    Logger.s_WriterDelegate(text2);
                }
                else
                {
                    Console.Error.WriteLine(text2);
                }
            }
        }

        private static void Print(string fmt, params object[] args)
        {
            Logger.Print(5, null, fmt, args);
        }

        private static void Print(string msg)
        {
            Logger.Print("{0}", msg);
        }

        public static void Fatal(string fmt, params object[] args)
        {
            Logger.Print(1, null, fmt, args);
        }

        public static void Fatal(string msg)
        {
            Logger.Fatal("{0}", msg);
        }

        public static void Error(string fmt, params object[] args)
        {
            Logger.Print(2, null, fmt, args);
        }

        public static void Error(string msg)
        {
            Logger.Error("{0}", msg);
        }

        public static void Warning(string fmt, params object[] args)
        {
            Logger.Print(3, null, fmt, args);
        }

        public static void Warning(string msg)
        {
            Logger.Warning("{0}", msg);
        }

        public static void Info(string fmt, params object[] args)
        {
            Logger.Print(5, null, fmt, args);
        }

        public static void Info(string msg)
        {
            Logger.Info("{0}", msg);
        }

        public static void Debug(string fmt, params object[] args)
        {
            Logger.Print(4, null, fmt, args);
        }

        public static void Debug(string msg)
        {
            Logger.Debug("{0}", msg);
        }

        private static void HdLogger(int prio, uint tid, string tag, string msg)
        {
            int level = 0;
            switch (prio)
            {
                case 0:
                    level = 1;
                    break;
                case 1:
                    level = 2;
                    break;
                case 2:
                    level = 3;
                    break;
                case 3:
                    level = 5;
                    break;
                case 4:
                    level = 4;
                    break;
            }
            Logger.Print(level, tag, "{0}", msg);
        }

        public static HdLoggerCallback GetHdLoggerCallback()
        {
            return Logger.s_HdLoggerCallback;
        }
    }
}
