using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Xamarin.Forms.HotReload.Observer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            (string Url, string Path) configuration = GetConfiguration(args);

            try
            {
                Directory.GetDirectories(configuration.Path);
            }
            catch
            {
                Console.WriteLine("MAKE SURE YOU PASSED RIGHT PATH TO PROJECT DIRECTORY AS 'P={PATH}' ARGUMENT.");
                Console.ReadKey();
                return;
            }

            if (!Uri.IsWellFormedUriString(configuration.Url, UriKind.Absolute))
            {
                Console.WriteLine("MAKE SURE YOU PASSED RIGHT DEVICE URL AS 'U={DEVICE_URL}' ARGUMENT.");
                Console.ReadKey();
                return;
            }

            new Program().Run(configuration.Path, configuration.Url);
        }

        private static (string Url, string Path) GetConfiguration(
            string[] commandLineArgs)
        {
            string RetrieveCommandLineArgument(
                string key, string defaultValue, string[] args)
            {
                var value = args.FirstOrDefault(
                    x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
                return value != null
                    ? value.Substring(2, value.Length - 2)
                    : defaultValue;
            }

            var defaultReloaderServerIP
                = NetworkInterface.GetAllNetworkInterfaces()
                    .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => x.Address.MapToIPv4().ToString())
                    .FirstOrDefault(x => x != "127.0.0.1")
                ?? "127.0.0.1";

            string url = RetrieveCommandLineArgument("u=", $"http://{defaultReloaderServerIP}:8000", commandLineArgs);
            string path = RetrieveCommandLineArgument("p=", Environment.CurrentDirectory, commandLineArgs);
            return (url, path);
        }

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly XamlFileObserver xamlFileObserver = new XamlFileObserver();

        private void Run(string path, string url)
        {
            _httpClient.BaseAddress = new Uri(url);

            xamlFileObserver.XamlFileChanged += XamlFileChanged;
            xamlFileObserver.Start(path);

            Console.WriteLine($"\n\n> HOTRELOADER STARTED AT {DateTime.Now}");
            Console.WriteLine($"\n> PATH: {path}");
            Console.WriteLine($"\n> URL: {url}\n");

            do
            {
                Console.WriteLine("\nPRESS \'ESC\' TO STOP.");
            } while (Console.ReadKey().Key != ConsoleKey.Escape);

            xamlFileObserver.XamlFileChanged -= XamlFileChanged;
            xamlFileObserver.Stop();
        }

        private async void XamlFileChanged(object sender, XamlFileChangedEventArgs e)
        {
            Console.WriteLine($"CHANGED {e.ChangedTime}: {e.FileFullPath}");

            HttpContent httpContent
                 = new ByteArrayContent(
                    Encoding.UTF8.GetBytes(
                        await File.ReadAllTextAsync(e.FileFullPath)));
            await _httpClient.PostAsync("reload", httpContent);
        }
    }
}