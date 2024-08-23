using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Util.Diagnostics
{
    public class Report
    {
        public DateTime DateTimeUtc = DateTime.UtcNow;
        public ConcurrentQueue<LogEntry> Log = new ConcurrentQueue<LogEntry>();
        public void AddLogEntry(LogEntry entry) { Log.Enqueue(entry); }
        public string Save(string outputDirPath, string name)
        {
            var reportsJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            Directory.CreateDirectory(outputDirPath);
            var reportFileName = $"{DateTime.UtcNow.ToString("yyyy.MM.dd_HH.mm.ss")}_{name}_report.json";
            var reportOutputFilePath = Path.Combine(outputDirPath, reportFileName);
            File.WriteAllText(reportOutputFilePath, reportsJson);
            return reportOutputFilePath;
        }
    }

    public enum LogEntryType
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
    }

    public class LogEntry
    {
        public string Message = "";
        public DateTime DateTimeUtc = DateTime.UtcNow;
        public LogEntryType Type = LogEntryType.Info;
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        public string StackTrace = "";

        public LogEntry()
        {
            StackTrace = (new System.Diagnostics.StackTrace(true)).ToString();
        }

        public LogEntry(string msg)
        {
            StackTrace = (new System.Diagnostics.StackTrace(true)).ToString();
            Message = msg;
        }

        public LogEntry(string msg, Dictionary<string, object> data)
        {
            StackTrace = (new System.Diagnostics.StackTrace(true)).ToString();
            Message = msg;
            Data = data;
        }

        public LogEntry(string msg, string dataKey, object dataValue)
        {
            StackTrace = (new System.Diagnostics.StackTrace(true)).ToString();
            Message = msg;
            Data.Add(dataKey, dataValue);
        }

        public LogEntry(Exception e)
        {
            StackTrace = (new System.Diagnostics.StackTrace(true)).ToString();
            Type = LogEntryType.Error;
            Message = e.Message;
            Data.Add("Exception", e);
        }

        public LogEntry(string msg, Exception e)
        {
            StackTrace = (new System.Diagnostics.StackTrace(true)).ToString();
            Type = LogEntryType.Error;
            Message = msg;
            Data.Add("Exception", e);
        }
    }

    public class Error
    {
        public static List<Exception> GetAllExceptions(Exception ex)
        {
            var exceptions = new List<Exception>();
            while (ex != null)
            {
                exceptions.Add(ex);
                ex = ex.InnerException;
            }
            return exceptions;
        }

        public static string GetFullExceptionMessage(IEnumerable<Exception> exceptions)
        {
            return string.Join(". ", exceptions.Select(e => e.Message));
        }

        public static string GetFullExceptionMessage(Exception ex)
        {
            var exceptions = GetAllExceptions(ex);
            return GetFullExceptionMessage(exceptions);
        }
    }

    public static class Event
    {
        public static Subject<LogEntry> NewLogEntry = new Subject<LogEntry>();
        public static void LogEntry(LogEntry logEntry) => NewLogEntry.OnNext(logEntry);
        public static void Info(string msg) => NewLogEntry.OnNext(new LogEntry(msg));
        public static void Info(string msg, string dataKey, object dataValue) => NewLogEntry.OnNext(new LogEntry(msg, dataKey, dataValue));
        public static void Error(string msg) => NewLogEntry.OnNext(new LogEntry(msg) { Type = LogEntryType.Error });
        public static void Error(Exception e) => NewLogEntry.OnNext(new LogEntry(e));
        public static void Error(string msg, Exception e) => NewLogEntry.OnNext(new LogEntry(msg, e));
    }

}
