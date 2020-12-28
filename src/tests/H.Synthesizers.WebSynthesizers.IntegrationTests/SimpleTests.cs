using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            using var synthesizer = new YandexSynthesizer();

            var bytes = await synthesizer.ConvertAsync(nameof(ConvertTest), cancellationToken);

            File.WriteAllBytes("D:/test.opus", bytes);
        }
    }
}
