using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util
{
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

        public static async Task<T> DeserializeFromFileAsync<T>(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var streamReader = new StreamReader(fileStream))
            {
                var fileContent = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(fileContent);
            }
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
}
