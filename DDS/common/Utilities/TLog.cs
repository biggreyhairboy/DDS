using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace OMS.common.Utilities
{
    /// <summary>
    ///  This class is used for writing log,after writing the log,the writer stream will not be closed.
    ///  So,it will be quick to  writer the log,but the log file could not be deleted when the program is run...
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// In coming message type
        /// </summary>
        IN = 0,
        /// <summary>
        /// Out going message type
        /// </summary>
        OUT,
        /// <summary>
        /// Error message type
        /// </summary>
        ERROR,
        /// <summary>
        /// Information message type
        /// </summary>
        INFO,
        /// <summary>
        /// Default message type
        /// </summary>
        DEFAULT
    }
    /// <summary>
    /// <para>This class is used for writing log,after writing the log,the writer stream will not be closed.	So,the file could not be delete when the program is running,but it will spend less time to write log.</para>
    /// <para>if u use the way new to get an instance(firstlog) and then use getinstance to get another instance(Seconde log),in this case the (first log) will write to the (sencond log file)</para>
    /// <para>so it's better not to use this two way at the same program</para>
    /// </summary>
    public class TLog : IDisposable
    {
        /// <summary>
        /// Gets or sets the indicator for auto reset Non-Today-Log file, default TRUE
        /// </summary>
        public static bool AutoReset = true;
        
        private const string DEFAULTLOGPATH = "CommonDefaultLog{0}.txt";
        private const string EVENTSRC = "CommLibError";
        private static Dictionary<string, TLog> logMap = new Dictionary<string, TLog>();
        private static TLog FDefaultInstance;
        private static volatile object syncRoot = new object();

        private FileStream fileStream;
        private StreamWriter FLogWriter;
        private StreamWriter dumpWriter;
        private Assembly workingAsm;
        private string workingFile;
        private string logPrefix;
        private bool withTimeStamp;
        private bool allowDump = false;
        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="filename">Log file path</param>
        private TLog(string filename)
            : this(filename, null)
        { }

        private TLog(string filename, Assembly asm)
        {
            try
            {
                logPrefix = "";
                workingFile = filename;
                workingAsm = asm;
                withTimeStamp = true;
                bool append = true;
                string folder = Path.GetDirectoryName(filename);
                if (folder != null && folder.Trim() != "")
                {
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                }
                if (!File.Exists(filename)) append = false;
                if (AutoReset)
                {
                    if (File.Exists(filename))
                    {
                        FileInfo info = new FileInfo(filename);
                        if (info.CreationTime.ToShortDateString() != DateTime.Now.ToShortDateString())
                        {
                            append = false;
                            info.CreationTime = DateTime.Now;
                        }
                    }
                }
                fileStream = new FileStream(filename, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                FLogWriter = new StreamWriter(fileStream);
                if (append) FLogWriter.WriteLine("");//starts with an empty line
                AppendAssemblyInfo(asm);
            }
            catch (Exception ex)
            {
                if (fileStream != null) fileStream.Dispose();
                if (FLogWriter != null) FLogWriter.Dispose();
                FLogWriter = null;
                WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
            }
        }

        public void AssignDump(string dumpFile)
        {
            if (dumpFile == null || dumpFile.Trim() == "") return;
            try
            {
                string folder = Path.GetDirectoryName(dumpFile);
                if (folder != null && folder.Trim() != "")
                {
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                }
                dumpWriter = new StreamWriter(dumpFile, false);
                allowDump = true;
            }
            catch (Exception ex)
            {
                allowDump = false;
                WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
            }
        }

        private void AppendAssemblyInfo(Assembly asm)
        {
            try
            {
                if (asm == null)
                    asm = Assembly.GetEntryAssembly();
                if (asm != null)
                {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(asm.Location);
                    WriteLog(string.Format("{0} ({1}) {2}", info.ProductName, info.CompanyName, info.ProductVersion), LogType.DEFAULT, true, false);
                    WriteLog(info.LegalCopyright, LogType.DEFAULT, true, false);
                }
            }
            catch { }
        }
        /// <summary>
        /// Internal function for writting event log
        /// </summary>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        public static void WriteEventLog(string source, string msg, EventLogEntryType type)
        {
            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }
                EventLog log = new EventLog();
                log.Source = source;
                log.WriteEntry(msg, type);
            }
            catch { }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the StreamWriter will flush its buffer after every call Write function
        /// </summary>
        public bool AutoFlush
        {
            get
            {
                if (FLogWriter != null)
                {
                    return FLogWriter.AutoFlush;
                }
                return false;
            }
            set
            {
                if (FLogWriter != null)
                {
                    FLogWriter.AutoFlush = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the log prefix for identifying the log info
        /// </summary>
        public string LogPrefix
        {
            get { return logPrefix; }
            set { logPrefix = value; }
        }
        /// <summary>
        /// Gets or sets a value indicating whether or not the TimeStamp will be shown on log file, TRUE by default.
        /// </summary>
        public bool WithTimeStamp
        {
            get { return withTimeStamp; }
            set { withTimeStamp = value; }
        }
        /// <summary>
        /// Release all resources used by TLog
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (fileStream != null) fileStream.Dispose();
                if (FLogWriter != null)
                {
                    FLogWriter.Close();
                    FLogWriter = null;
                }
            }
            catch { }
        }
        /// <summary>
        /// Get a log instance via a specified filename, if it isn't exist, a new instance will be created.
        /// </summary>
        /// <param name="filename">Log file path</param>
        /// <returns>TLog</returns>
        public static TLog GetInstance(string filename)
        {
            try
            {
                lock (syncRoot)
                {
                    if (logMap.ContainsKey(filename)) return logMap[filename];
                    Assembly asm = Assembly.GetEntryAssembly();
                    if (asm == null)
                        asm = Assembly.GetCallingAssembly();
                    TLog log = new TLog(filename, asm);
                    logMap[filename] = log;
                    return log;
                }
            }
            catch (Exception ex)
            {
                WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
                return null;
            }
        }
        /// <summary>
        /// Initialize a log instance by specified a filename
        /// </summary>
        /// <param name="filename">Log file path</param>
        public static void SetDefaultInstance(string filename)
        {
            lock (syncRoot)
            {
                try
                {
                    if (FDefaultInstance != null)
                    {
                        FDefaultInstance.CloseFile();
                        FDefaultInstance = null;
                    }
                    Assembly asm = Assembly.GetEntryAssembly();
                    if (asm == null)
                        asm = Assembly.GetCallingAssembly();
                    FDefaultInstance = new TLog(filename, asm);
                }
                catch (Exception ex)
                {
                    WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
                }
            }
        }
        /// <summary>
        /// Gets a log instance.
        /// <para>By default, the log file will be the name of the calling assembly.</para>
        /// <para>If the calling assembly not available, use current date as file name</para>
        /// </summary>
        /// <returns>TLog</returns>
        public static TLog DefaultInstance
        {
            get
            {
                try
                {
                    lock (syncRoot)
                    {
                        if (FDefaultInstance != null) return FDefaultInstance;
                        string path = string.Format(DEFAULTLOGPATH, DateTime.Now.ToString("yyyyMMddhhmmssfff"));
                        Assembly asm = Assembly.GetEntryAssembly();
                        if (asm == null)
                            asm = Assembly.GetCallingAssembly();
                        if (asm != null) path = asm.GetName().Name + "Log.txt";
                        FDefaultInstance = new TLog(path, asm);
                        return FDefaultInstance;
                    }
                }
                catch (Exception ex)
                {
                    WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
                    return null;
                }
            }
        }
        /// <summary>
        /// Writes log message with Default LogType and no flush, and also, will mask the password
        /// </summary>
        /// <param name="msg">Messages to be wrote to a log file</param>
        public void WriteMaskLog(string msg)
        {
            if (msg == null || msg.Trim() == "") return;
            try
            {
                msg = Regex.Replace(msg, @"\|21\|.*?\|", "|21|****|");
                msg = Regex.Replace(msg, @"\|510\|.*?\|", "|510|****|");
                msg = Regex.Replace(msg, @"\|25\|.*?\|", "|25|****|");
            }
            catch { }
            WriteLog(msg);
        }
        /// <summary>
        /// Writes log message with DEFAULT LogType and no flush
        /// </summary>
        /// <param name="msg">Messages to write to a log file</param>
        public void WriteLog(string msg)
        {
            WriteLog(msg, LogType.DEFAULT);
        }
        /// <summary>
        /// Writes log message with DEFAULT LogType
        /// </summary>
        /// <param name="msg">Messages to write to a log file</param>
        /// <param name="flush">Indicates whether or not to flush the log writer</param>
        public void WriteLog(string msg, bool flush)
        {
            WriteLog(msg, LogType.DEFAULT, flush);
        }
        /// <summary>
        /// Writes log message with no flush
        /// </summary>
        /// <param name="msg">Messages to write to a log file</param>
        /// <param name="logType">Log type</param>
        public void WriteLog(string msg, LogType logType)
        {
            WriteLog(msg, logType, false);
        }
        /// <summary>
        /// Writes log message with timeStamp
        /// </summary>
        /// <param name="msg">Messages to write to a log file</param>
        /// <param name="logType">Log type</param>
        /// <param name="flush">Indicates whether or not to flush the log writer</param>
        public void WriteLog(string msg, LogType logType, bool flush)
        {
            WriteLog(msg, logType, flush, withTimeStamp);
        }
        /// <summary>
        /// Writes log message
        /// </summary>
        /// <param name="msg">Messages to write to a log file</param>
        /// <param name="logType">Log type</param>
        /// <param name="flush">Indicates whether or not to flush the log writer</param>
        /// <param name="timeStamp">Indicates whether or not to turn on the timeStamp for the log file</param>
        public void WriteLog(string msg, LogType logType, bool flush, bool timeStamp)
        {
            lock (syncRoot)
            {
                try
                {
                    if (FLogWriter == null) return;
                    StringBuilder buffer = new StringBuilder();
                    if (timeStamp) buffer.Append(string.Format("[{0}] ", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
                    switch (logType)
                    {
                        case LogType.IN:
                            buffer.Append("<--");
                            break;
                        case LogType.OUT:
                            buffer.Append("-->");
                            break;
                        case LogType.ERROR:
                            buffer.Append("Error: ");
                            break;
                        case LogType.INFO:
                            buffer.Append("Info: ");
                            break;
                        default:
                            break;
                    }

                    if (logPrefix != null && logPrefix.Trim() != "")
                    {
                        buffer.Append(logPrefix);
                        buffer.Append(" ");
                    }
                    buffer.Append(msg);

                    FLogWriter.WriteLine(buffer.ToString());
                    if (flush) FLogWriter.Flush();
                }
                catch (Exception ex)
                {
                    WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
                }
            }
        }

        public void WriteDumpLog(string msg)
        {
            if (msg == null) return;
            if (dumpWriter == null) return;
            if (!allowDump) return;
            lock (dumpWriter)
            {
                try
                {
                    dumpWriter.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), msg));
                    dumpWriter.Flush();
                }
                catch (Exception ex)
                {
                    WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
                }
            }
        }

        public void WriteLogWithDebugConsole(string msg)
        {
            WriteLog(msg);
#if DEBUG
            Console.WriteLine(msg);
#endif
        }

        public void WriteDebugLog(string msg)
        {
#if DEBUG
            WriteLog(msg);
#endif
        }

        public void WriteDebugLog(string msg, LogType logType)
        {
#if DEBUG
            WriteLog(msg, logType);
#endif
        }

        public void WriteDebugLog(string msg, LogType logType, bool flush, bool timeStamp)
        {
#if DEBUG
            WriteLog(msg, logType, flush, timeStamp);
#endif
        }
        /// <summary>
        /// Flush log writer
        /// </summary>
        public void Flush()
        {
            try
            {
                if (FLogWriter != null)
                {
                    FLogWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
            }
        }
        /// <summary>
        /// Close log writer
        /// </summary>
        public void CloseFile()
        {
            try
            {
                if (FLogWriter != null)
                {
                    FLogWriter.Flush();
                    FLogWriter.Close();
                    FLogWriter = null;
                }
                if (fileStream != null) fileStream.Dispose();
            }
            catch (Exception ex)
            {
                WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
            }
        }

        public void ResetLog(string filename)
        {
            try
            {
                CloseFile();
                fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                FLogWriter = new StreamWriter(fileStream);
                WriteLog("**************** Reset Log -     " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ****************");
                Assembly asm = Assembly.GetEntryAssembly();
                if (asm == null)
                    asm = Assembly.GetCallingAssembly();
                AppendAssemblyInfo(asm);
            }
            catch (Exception ex)
            {
                WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
            }
        }

        public void Reset()
        {
            try
            {
                CloseFile();
                if (File.Exists(workingFile))
                {
                    FileInfo info = new FileInfo(workingFile);
                    info.CreationTime = DateTime.Now;
                }
                fileStream = new FileStream(workingFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                FLogWriter = new StreamWriter(fileStream);
                WriteLog("**************** Reset Log -     " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ****************");
                AppendAssemblyInfo(workingAsm);
            }
            catch (Exception ex)
            {
                WriteEventLog(EVENTSRC, ex.ToString(), EventLogEntryType.Error);
            }
        }
    }
}