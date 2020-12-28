using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace H.Synthesizers
{
    /// <summary>
    /// 
    /// </summary>
    public class YandexSynthesizer : CachedSynthesizer
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string Language { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string Format { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string Voice { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string Emotion { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string Quality { get; set; } = string.Empty;
        
        /// <summary>
        /// 
        /// </summary>
        public string Speed { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public YandexSynthesizer()
        {
            AddEnumerableSetting(nameof(Language), o => Language = o, NoEmpty, new []{ "en-US", "ru-RU", "tr-TR" });
            AddEnumerableSetting(nameof(Format), o => Format = o, NoEmpty, new[] { "oggopus" });
            AddEnumerableSetting(nameof(Voice), o => Voice = o, NoEmpty, new[] { "alena", "oksana", "jane", "alyss", "omazh", "zahar", "ermil", "filipp" });
            AddEnumerableSetting(nameof(Emotion), o => Emotion = o, NoEmpty, new[] { "good", "evil", "neutral" });
            AddEnumerableSetting(nameof(Speed), o => Speed = o, NoEmpty, new[] { "1.0", "0.1", "0.25", "0.5", "0.75", "1.25", "1.5", "2.0", "3.0" });
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<byte[]> InternalConvertAsync(string text, CancellationToken cancellationToken = default)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            
            var container = new CookieContainer();
            using var handler = new HttpClientHandler
            {
                CookieContainer = container,
            };
            using var client = new HttpClient(handler, false);

            await client.GetAsync(new Uri("https://cloud.yandex.ru/services/speechkit"), cancellationToken)
                .ConfigureAwait(false);

            var cookies = container.GetCookies(new Uri("https://cloud.yandex.ru/"));
            var token = cookies["XSRF-TOKEN"]?.Value ?? throw new InvalidOperationException("XSRF-TOKEN is null.");

            {
                using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://cloud.yandex.ru/api/constructor/speechkit?"))
                {
                    Headers =
                    {
                        { "x-csrf-token", token.Replace("%3A", ":") },
                    },
                };

                using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            {
                var json = JsonConvert.SerializeObject(new YandexSettings
                {
                    Message = text,
                    Language = Language,
                    Speed = Convert.ToDouble(Speed, CultureInfo.InvariantCulture),
                    Emotion = Emotion,
                    Format = Format,
                    //Voice = Voice,
                }, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                });
                using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://cloud.yandex.ru/api/speechkit/tts"))
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                    Headers =
                    {
                        { "x-csrf-token", token.Replace("%3A", ":") },
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36" },
                        { "sec-ch-ua", "\"Google Chrome\";v=\"87\", \" Not;A Brand\";v=\"99\", \"Chromium\";v=\"87\"" },
                        { "sec-ch-ua-mobile", "?0" },
                        { "Sec-Fetch-Dest", "empty" },
                        { "Sec-Fetch-Mode", "cors" },
                        { "Sec-Fetch-Site", "same-origin" },
                        { "Referer", "https://cloud.yandex.ru/services/speechkit" },
                        { "Pragma", "no-cache" },
                        { "Origin", "https://cloud.yandex.ru" },
                        { "Host", "cloud.yandex.ru" },
                    },
                };

                using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

                var value = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException exception)
                {
                    throw new InvalidOperationException(value, exception);
                }

                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected override string TextToKey(string text)
        {
            return $"{text}_{Voice}_{Language}_{Emotion}_{Speed}_{Format}_{Quality}";
        }

        #endregion
    }
}
