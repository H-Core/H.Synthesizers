using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Synthesizers;
using H.Core.Utilities;

namespace H.Synthesizers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CachedSynthesizer : Synthesizer
    {
        #region Properties

        private ByteArrayCache Cache { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        protected CachedSynthesizer()
        {
            UseCache = true;
            Cache = new ByteArrayCache(GetType());
        }

        #endregion

        #region ISynthesizer

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<byte[]> ConvertAsync(string text, AudioSettings? settings = null, CancellationToken cancellationToken = default)
        {
            settings ??= SupportedSettings.First();

            var key = TextToKey(text, settings);
            if (UseCache && Cache.Contains(key))
            {
                return Cache[key]?.ToArray() ?? EmptyArray<byte>.Value;
            }

            var bytes = await InternalConvertAsync(text, settings, cancellationToken).ConfigureAwait(false);
            Cache[key] = bytes;
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<byte[]> InternalConvertAsync(string text, AudioSettings settings, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected abstract string TextToKey(string text, AudioSettings settings);

        #endregion
    }
}
