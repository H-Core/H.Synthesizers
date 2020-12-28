using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core.Synthesizers;

namespace H.Synthesizers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CachedSynthesizer : Synthesizer, ISynthesizer
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> ConvertAsync(string text, CancellationToken cancellationToken = default)
        {
            var key = TextToKey(text);
            if (UseCache && Cache.Contains(key))
            {
                return Cache[key]?.ToArray() ?? Array.Empty<byte>();
            }

            var bytes = await InternalConvertAsync(text, cancellationToken).ConfigureAwait(false);
            Cache[key] = bytes;
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<byte[]> InternalConvertAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected abstract string TextToKey(string text);

        #endregion
    }
}
