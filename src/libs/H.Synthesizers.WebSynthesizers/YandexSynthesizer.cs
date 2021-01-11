using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using H.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace H.Synthesizers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class YandexSynthesizer : CachedSynthesizer
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string Language { get; set; } = string.Empty;

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

        private HttpClientHandler HttpClientHandler { get; }
        private HttpClient HttpClient { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public YandexSynthesizer()
        {
            HttpClientHandler = new HttpClientHandler();
            HttpClient = new HttpClient(HttpClientHandler, false);

            AddEnumerableSetting(nameof(Language), o => Language = o, NoEmpty, new[] { "en-US", "ru-RU", "tr-TR" });
            AddEnumerableSetting(nameof(Voice), o => Voice = o, NoEmpty, new[] { "alena", "oksana", "jane", "alyss", "omazh", "zahar", "ermil", "filipp" });
            AddEnumerableSetting(nameof(Emotion), o => Emotion = o, NoEmpty, new[] { "good", "evil", "neutral" });
            AddEnumerableSetting(nameof(Speed), o => Speed = o, NoEmpty, new[] { "1.0", "0.1", "0.25", "0.5", "0.75", "1.25", "1.5", "2.0", "3.0" });

            SupportedSettings.Add(new AudioSettings(AudioFormat.Raw, 48000));
            SupportedSettings.Add(new AudioSettings(AudioFormat.Ogg));
        }

        #endregion

        #region Protected methods

        private string? GetXsrfToken()
        {
            var cookies = HttpClientHandler.CookieContainer.GetCookies(new Uri("https://cloud.yandex.ru/"));

            return cookies["XSRF-TOKEN"]?.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<byte[]> InternalConvertAsync(
            string text,
            AudioSettings settings,
            CancellationToken cancellationToken = default)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            settings = settings ?? throw new ArgumentNullException(nameof(settings));

            var token = GetXsrfToken();
            for (var i = 0; token == null && i < 5; i++)
            {
                await HttpClient.GetAsync(new Uri("https://cloud.yandex.ru/services/speechkit"), cancellationToken)
                    .ConfigureAwait(false);

                token = GetXsrfToken();
            }

            var bytes = (byte[]?) null;
            var lastException = (Exception?) null;
            for (var i = 0; bytes == null && i < 5; i++)
            {
                var json = JsonConvert.SerializeObject(new YandexSettings
                {
                    Message = text,
                    Language = Voice switch
                    {
                        "alena" => "ru-RU",
                        "filipp" => "ru-RU",
                        _ => Language,
                    },
                    Speed = Convert.ToDouble(Speed, CultureInfo.InvariantCulture),
                    Emotion = Emotion,
                    Format = settings.Format switch
                    {
                        AudioFormat.Ogg => "oggopus",
                        _ or AudioFormat.Raw => "lpcm",
                    },
                    Voice = Voice,
                }, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                });
                using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://cloud.yandex.ru/api/speechkit/tts"))
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                    Headers =
                    {
                        { "x-csrf-token", HttpUtility.UrlDecode(token) },
                    },
                };

                using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                var value = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    response.EnsureSuccessStatusCode();

                    bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
                catch (HttpRequestException exception)
                {
                    lastException = new InvalidOperationException(value, exception);
                }
            }

            lastException ??= new InvalidOperationException("Bytes is null.");

            return bytes ?? throw lastException;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected override string TextToKey(string text, AudioSettings settings)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            settings = settings ?? throw new ArgumentNullException(nameof(settings));

            return $"{text}_{Voice}_{Language}_{Emotion}_{Speed}_{Quality}_{settings.Format}";
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            HttpClient.Dispose();
            HttpClientHandler.Dispose();

            base.Dispose();
        }

        #endregion
    }
}
