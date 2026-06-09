using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WorkOps.Helpers
{
    public static class ExportHelper
    {
        // Export any IEnumerable to CSV
        public static byte[] ToCsv<T>(IEnumerable<T> data)
        {
            if (data == null || !data.Any())
                return Encoding.UTF8.GetBytes("No data available");

            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Write header
            var header = string.Join(",", properties.Select(p => $"\"{p.Name}\""));
            sb.AppendLine(header);

            // Write data
            foreach (var item in data)
            {
                var row = new List<string>();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item, null);
                    var stringValue = value?.ToString() ?? "";
                    stringValue = stringValue.Replace("\"", "\"\"");
                    if (stringValue.Contains(",") || stringValue.Contains("\"") || stringValue.Contains("\n"))
                    {
                        stringValue = $"\"{stringValue}\"";
                    }
                    row.Add(stringValue);
                }
                sb.AppendLine(string.Join(",", row));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // Export anonymous types to CSV
        public static byte[] ToCsvDynamic(IEnumerable<dynamic> data, string[] headers)
        {
            if (data == null || !data.Any())
                return Encoding.UTF8.GetBytes("No data available");

            var sb = new StringBuilder();

            // Write headers
            sb.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

            // Write data
            foreach (var item in data)
            {
                var dict = item as IDictionary<string, object>;
                if (dict == null) continue;

                var row = new List<string>();
                foreach (var header in headers)
                {
                    var value = dict.ContainsKey(header) ? dict[header]?.ToString() : "";
                    var stringValue = value ?? "";
                    stringValue = stringValue.Replace("\"", "\"\"");
                    if (stringValue.Contains(",") || stringValue.Contains("\"") || stringValue.Contains("\n"))
                    {
                        stringValue = $"\"{stringValue}\"";
                    }
                    row.Add(stringValue);
                }
                sb.AppendLine(string.Join(",", row));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}