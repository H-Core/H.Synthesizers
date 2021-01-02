using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Recorders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Synthesizers.WebSynthesizers.IntegrationTests
{
    [TestClass]
    public class SimpleTests
    {
        private static void CheckDevices()
        {
            var devices = NAudioRecorder.GetAvailableDevices().ToList();
            if (!devices.Any())
            {
                Assert.Inconclusive("No available devices for NAudioRecorder.");
            }

            Console.WriteLine("Available devices:");
            foreach (var device in devices)
            {
                Console.WriteLine($" - Name: {device.ProductName}, Channels: {device.Channels}");
            }
        }

        [TestMethod]
        public async Task ConvertTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            CheckDevices();

            using var synthesizer = new YandexSynthesizer
            {
                UseCache = false,
            };
            using var player = new NAudioPlayer();
            var settings = synthesizer.SupportedSettings.First();

            var bytes = await synthesizer.ConvertAsync(nameof(ConvertTest), settings, cancellationToken);

            await player.PlayAsync(bytes, settings, cancellationToken);
        }
    }
}
