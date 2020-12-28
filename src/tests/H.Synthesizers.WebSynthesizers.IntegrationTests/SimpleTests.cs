using System;
using System.Threading;
using System.Threading.Tasks;
using H.Recorders.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;

namespace H.Synthesizers.WebSynthesizers.IntegrationTests
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public async Task ConvertTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            using var synthesizer = new YandexSynthesizer
            {
                UseCache = false,
            };

            var bytes = await synthesizer.ConvertAsync(nameof(ConvertTest), cancellationToken);

            await bytes.PlayAsync(new WaveFormat(48000, 16, 1), cancellationToken);
        }
    }
}
