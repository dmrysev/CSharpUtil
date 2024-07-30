using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var kvp in source)
            {
                target[kvp.Key] = kvp.Value; // This will override existing keys
            }
        }
    }

    public class Json
    {
        public static string RemoveFieldCaseInsensitive(string jsonString, string fieldNameToRemove)
        {
            JObject json = JObject.Parse(jsonString);
            var normalizedProperties = json.Properties().ToDictionary(p => p.Name.ToLower(), p => p);

            if (normalizedProperties.TryGetValue(fieldNameToRemove.ToLower(), out JProperty propertyToRemove))
            {
                propertyToRemove.Remove();
            }

            return json.ToString();
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            // Ensure the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found!", filePath);
            }

            // Open the filestream and deserialize the JSON data
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            using (JsonTextReader jr = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                T obj = serializer.Deserialize<T>(jr);
                return obj;
            }
        }
    }

    namespace Diagnostics
    {
        public class Report
        {
            public DateTime DateTimeUtc = DateTime.UtcNow;
            public List<LogEntry> Log = new List<LogEntry>();
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
            public DateTime DateTimeUtc = DateTime.UtcNow;
            public LogEntryType Type = LogEntryType.Info;
            public string StackTrace = "";
            public string Message = "";
            public Dictionary<string, object> Data = new Dictionary<string, object>();

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
                var allExceptions = Util.Diagnostics.Error.GetAllExceptions(e);
                Data.Add("Exceptions", allExceptions);
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

        public class Event
        {
            public static Subject<string> NewInfoMsg = new Subject<string>();
            public static void InfoMsg(string msg) => NewInfoMsg.OnNext(msg);
        }

    }
}
