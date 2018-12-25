using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Xamarin.Forms.HotReload.Observer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var reloaderServerIP
                = NetworkInterface.GetAllNetworkInterfaces()
                    .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => x.Address.MapToIPv4().ToString())
                    .FirstOrDefault(x => x != "127.0.0.1")
                ?? "127.0.0.1";

            new XamlFileObserver().Run(
                path: RetrieveCommandLineArgument("p=", Environment.CurrentDirectory, args),
                url: RetrieveCommandLineArgument("u=", $"http://{reloaderServerIP}:8000", args)
            );
        }

        private static string RetrieveCommandLineArgument(
            string key, string defaultValue, string[] args)
        {
            var value = args.FirstOrDefault(x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
            return value != null ? value.Substring(2, value.Length - 2) : defaultValue;
        }
    }
}