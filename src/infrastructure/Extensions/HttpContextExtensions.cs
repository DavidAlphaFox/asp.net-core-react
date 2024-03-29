using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace infrastructure.extensions
{
    public static class HttpContextExtensions
    {
        public static Dictionary<string, StringValues> GetContextDict(this HttpContext context)
        {
            return context.GetContextValuesAsync().GetAwaiter().GetResult();
        }
        public static async Task<Dictionary<string, StringValues>> GetContextValuesAsync(this HttpContext context)
        {
            Dictionary<string, StringValues> values = new Dictionary<string, StringValues>();
            string text = await context.GetStringAsync();
            if (string.IsNullOrEmpty(text))
            {
                return values;
            }
            object value = JsonConvert.DeserializeObject(text.FormatJson());
            //store
            AddToBackingStore(values, value);
            return values;
        }
        private static void AddToBackingStore(Dictionary<string, StringValues> store, object value)
        {
            JProperty val1;
            JValue val2;
            if (value is JObject || value is JArray)
            {
                foreach (JToken item in (value as JToken))
                {
                    AddToBackingStore(store, item);
                }
            }
            else if ((val1 = (value as JProperty)) != null)
            {
                AddToBackingStore(store, val1.Value);
            }
            else if ((val2 = (value as JValue)) != null && val2.Value != null)
            {
                store[val2.Path] = val2.Value.ToString();
            }
        }

        public static string GetContextString(this HttpContext context)
        {
            string text = context.GetStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Trim();
            }
            return text;
        }

        public static async Task<string> GetStringAsync(this HttpContext context)
        {
            HttpRequest request = context.Request;
            request.Body.Position = 0;
            string text = await new StreamReader(request.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Trim();
            }
            return text;
        }
    }
}