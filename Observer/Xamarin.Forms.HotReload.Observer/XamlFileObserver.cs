using System;
using System.IO;
using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using static System.Math;

namespace Xamarin.Forms.HotReload.Observer
{
    public class XamlFileObserver
    {
        private readonly object _locker = new object();
        private HttpClient _client;
        private DateTime _lastChangeTime;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Run(string path, string url)
        {
            try
            {
                Directory.GetDirectories(path);
            }
            catch
            {
                Console.WriteLine("MAKE SURE YOU PASSED RIGHT PATH TO PROJECT DIRECTORY AS 'P={PATH}' ARGUMENT.");
                Console.ReadKey();
                return;
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Console.WriteLine("MAKE SURE YOU PASSED RIGHT DEVICE URL AS 'U={DEVICE_URL}' ARGUMENT.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\n\n> HOTRELOADER STARTED AT {DateTime.Now}");
            Console.WriteLine($"\n> PATH: {path}");
            Console.WriteLine($"\n> URL: {url}\n");

            var observer = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.xaml",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            _client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };

            observer.Changed += OnFileChanged;
            observer.Created += OnFileChanged;
            observer.Renamed += OnFileChanged;
            do
            {
                Console.WriteLine("\nPRESS \'ESC\' TO STOP.");
            } while (Console.ReadKey().Key != ConsoleKey.Escape);

            observer.Changed -= OnFileChanged;
            observer.Created -= OnFileChanged;
            observer.Renamed -= OnFileChanged;
        }
        
        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            var now = DateTime.Now;
            lock (_locker)
            {
                if (Abs((now - _lastChangeTime).TotalMilliseconds) < 900)
                {
                    return;
                }
                _lastChangeTime = now;
            }

            var filePath = e.FullPath.Replace("/.#", "/");
            Console.WriteLine($"CHANGED {now}: {filePath}");
            SendFile(filePath);
        }

        private void SendFile(string filePath)
        {
            var xaml = File.ReadAllText(filePath);
            var data = Encoding.UTF8.GetBytes(xaml);
            var content = new ByteArrayContent(data);
            _client.PostAsync("reload", content);
        }
    }
}
