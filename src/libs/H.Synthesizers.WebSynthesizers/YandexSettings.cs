namespace H.Synthesizers
{
    /// <summary>
    /// https://cloud.yandex.ru/services/speechkit
    /// </summary>
    public class YandexSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string Language { get; set; } = "ru-RU";

        /// <summary>
        /// 
        /// </summary>
        public double Speed { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        public string Voice { get; set; } = "alyss";

        /// <summary>
        /// 
        /// </summary>
        public string Emotion { get; set; } = "good";

        /// <summary>
        /// 
        /// </summary>
        public string Format { get; set; } = "oggopus";
    }
}
