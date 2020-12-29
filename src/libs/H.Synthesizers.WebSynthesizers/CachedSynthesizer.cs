using System;
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
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<byte[]> ConvertAsync(string text, AudioFormat format = AudioFormat.Raw, CancellationToken cancellationToken = default)
        {
            var key = TextToKey(text);
            if (UseCache && Cache.Contains(key))
            {
                return Cache[key]?.ToArray() ?? EmptyArray<byte>.Value;
            }

            var bytes = await InternalConvertAsync(text, format, cancellationToken).ConfigureAwait(false);
            Cache[key] = bytes;
            return bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<byte[]> InternalConvertAsync(string text, AudioFormat format = AudioFormat.Raw, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected abstract string TextToKey(string text);

        #endregion
    }
}
