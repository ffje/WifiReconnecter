using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Internet
{
    class Program
    {
        private static int outagesCounter;

        private const string wifiSSID = "15-181"; //your wifi name here
        private static readonly string connectInstruction = $"/c netsh wlan connect ssid={wifiSSID} name={wifiSSID}";
        private static readonly string websiteName = "www.google.com";

        private static readonly char exitKey = 'q';
        private static readonly int delayBetweenPings = 2000;
        private static readonly int delayAfterConnect = 5000;

        static async Task Main(string[] args)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            _ = Task.Factory.StartNew(() =>
              {
                  while (Console.ReadKey().KeyChar != exitKey)
                  {

                  }

                  tokenSource.Cancel();
              });

            await PingWithReconnectedAsync(token);
            tokenSource.Dispose();
            Console.WriteLine("Outages amount: " + outagesCounter);
            Console.ReadKey();
        }

        private static async Task<bool> PingWithReconnectedAsync(CancellationToken token)
        {
            Ping ping = new Ping();

            while (true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var response = await ping.SendPingAsync(websiteName);
                    if (response.Status == IPStatus.TimedOut)
                    {
                        await ReconnectAsync();
                    }

                    Console.WriteLine("Status: " + response.Status + " " + "time taken: " + response.RoundtripTime);
                }
                catch (PingException)
                {
                    await ReconnectAsync();
                }
                catch (OperationCanceledException)
                {
                    return await Task.FromResult(false);
                }

                await Task.Delay(delayBetweenPings);
            }
        }

        private static async Task ReconnectAsync()
        {
            Process.Start("cmd.exe", connectInstruction);
            await Task.Delay(delayAfterConnect);
            outagesCounter++;
        }
    }
}
