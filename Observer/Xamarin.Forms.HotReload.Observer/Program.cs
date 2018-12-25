using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Xamarin.Forms.HotReload.Observer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            (string Url, string Path) configuration = GetConfiguration(args);

            var xamlFileObserver = new XamlFileObserver();
            try
            {
                xamlFileObserver.Start(configuration.Path, configuration.Url);
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception);
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\n\n> HOTRELOADER STARTED AT {DateTime.Now}");
            Console.WriteLine($"\n> PATH: {configuration.Path}");
            Console.WriteLine($"\n> URL: {configuration.Url}\n");

            do
            {
                Console.WriteLine("\nPRESS \'ESC\' TO STOP.");
            } while (Console.ReadKey().Key != ConsoleKey.Escape);

            xamlFileObserver.Stop();
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
    }
}