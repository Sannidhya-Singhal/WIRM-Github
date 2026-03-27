using System.Reflection;
using System.Text.Json;
using WIRM.API.Models;

namespace WIRM.API.Extensions
{
    public static class Extensions
    {
        public async static Task<HttpRequestException> FormatBadRequestMessage(this HttpResponseMessage message, string requestDescription, params object[] parameters)
        {
            string content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            var formattedMessage = $"HTTP request Method '{requestDescription}' failed.\nStatus Code: {(int)message.StatusCode}\nReason: {message.ReasonPhrase}\n{content}";
            if (parameters.Length > 0)
                formattedMessage = $"{formattedMessage}\nParameters: {String.Join(",", parameters)}";

            return new HttpRequestException(formattedMessage, new Models.ADORequestException { ResponseContent = content });
        }

        public async static Task<Exception> HandleUnsuccessfulResponse(this HttpResponseMessage response, string requestDescription)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            content = content.Trim();
            if (content.StartsWith('{') && content.EndsWith('}'))
            {
                var badRequestInfo = JsonSerializer.Deserialize<ADOBadRequestMessage>(content);
                return await response.FormatBadRequestMessage(requestDescription).ConfigureAwait(false);
            }
            else
                return new Exception(content);
        }

        public static string GetInnerMostExceptionMessage(this Exception ex)
        {
            if (ex.InnerException != null) 
            {
                var adoRequest = ex.InnerException as ADORequestException;
                if (adoRequest != null)
                {
                    if (!string.IsNullOrEmpty(adoRequest.ResponseContent)) 
                        return adoRequest.ResponseContent;
                }
                else
                    return GetInnerMostExceptionMessage(ex.InnerException);
            }
            return ex.Message;
        }

        public static DateTime ToDateTime(this object? obj)
        {
            if (obj == null) return DateTime.MinValue;
            return DateTime.TryParse(obj.ToString(), out var res) ? res : DateTime.MinValue;
        }
        public static int ToInt(this object? obj)
        {
            if (obj == null) return 0;
            return Convert.ToInt32(obj);
        }

        public static int GetIntValue(this Dictionary<string, object?> dictionary, string key)
        {
            if (dictionary == null) return 0;
            return dictionary.TryGetValue(key, out var value) && value != null ? Convert.ToInt32(value.ToString()) : 0;
        }
        public static string? GetStringValue(this Dictionary<string, object?> dictionary, string key, string defaultValue = "")
        {
            if (dictionary == null) return defaultValue;
            return dictionary.TryGetValue(key, out var value) && value != null ? value.ToString() : defaultValue;
        }

        public static DateTime GetDateTimeValue(this Dictionary<string, object?> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                if (value is DateTime datetime) return datetime;
                if (DateTime.TryParse(value?.ToString(), out var parsed)) return parsed;
            }   
            return DateTime.MinValue;
        }

        public static string KebabCaseToWords(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            var words = str.Split('-');
            for (int i = 0; i < words.Length; i++)
                words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
            return String.Join(" ", words);
        }

        public static string ToHtmlString(this string str)
        {
            return str.Replace("\n", "<br>");
        }
    }
}
