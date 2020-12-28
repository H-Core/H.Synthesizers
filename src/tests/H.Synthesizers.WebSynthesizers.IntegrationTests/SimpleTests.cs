using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Recorders;
using H.Recorders.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;

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

            var bytes = await synthesizer.ConvertAsync(nameof(ConvertTest), cancellationToken);

            await bytes.PlayAsync(new WaveFormat(48000, 16, 1), cancellationToken);
        }
    }
}
