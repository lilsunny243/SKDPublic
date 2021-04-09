using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SKD.Common.Utils;

namespace PartnerClient.Services
{
    public static class TranslationService
    {
        // According to https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate

        private const string TranslatorEndpoint = @""; # REDACTED
        private const string TranslatorAPIKey = ""; # REDACTED
        private static readonly HttpClient client = new HttpClient();

        static TranslationService()
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", TranslatorAPIKey);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<string[]> TranslateAsync(params string[] texts)
        {
            try
            {
                using var message = new HttpRequestMessage(HttpMethod.Post, GetRequestURI())
                {
                    Content = new StringContent(JsonConvert.SerializeObject(texts.Select(text => new { Text = text ?? string.Empty }).ToArray()), 
                    Encoding.UTF8, "application/json")
                };
                var resp = await client.SendAsync(message);
                var respBody = await resp.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Translation - Status: {resp.StatusCode}, Response Body: {respBody}");
                return JsonConvert.DeserializeObject<JArray>(respBody).Select(x => (string)x["translations"]![0]!["text"]!).ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Translation Error: " + ex.Message);
                return texts.Select(x => string.Empty).ToArray();
            }
        }

        public static async Task<(string[] listTranslations, string[] paramsTranslations)> TranslateAsync(List<string> listTexts, params string[] paramsTexts)
        {
            var listTextsCount = listTexts.Count;
            try
            {
                listTexts.AddRange(paramsTexts);
                using var message = new HttpRequestMessage(HttpMethod.Post, GetRequestURI())
                {
                    Content = new StringContent(JsonConvert.SerializeObject(listTexts.Select(text => new { Text = text }).ToArray()), 
                    Encoding.UTF8, "application/json")
                };
                var resp = await client.SendAsync(message);
                var respBody = await resp.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Translation - Status: {resp.StatusCode}, Response Body: {respBody}");
                var result = JsonConvert.DeserializeObject<JArray>(respBody).Select(x => (string)x["translations"]![0]!["text"]!).ToArray();
                return (result.Take(listTextsCount).ToArray(), result.Skip(listTextsCount).ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Translation Error: " + ex.Message);
                return (listTexts.Take(listTextsCount).Select(x => string.Empty).ToArray(), paramsTexts.Select(x => string.Empty).ToArray());
            }
        }

        private static Uri GetRequestURI()
        {
            var (from, to) = Culture.IsSpanish ? ("es", "en") : ("en", "es");
            return new Uri($@"{TranslatorEndpoint}&to={to}&from={from}");
        }

    }
}
