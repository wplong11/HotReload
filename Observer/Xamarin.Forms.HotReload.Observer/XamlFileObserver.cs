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
        private FileSystemWatcher _observer;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Start(string path, string url)
        {
            void VerifyPathArgument()
            {
                try
                {
                    Directory.GetDirectories(path);
                }
                catch
                {
                    throw new ArgumentException("MAKE SURE YOU PASSED RIGHT PATH TO PROJECT DIRECTORY AS 'P={PATH}' ARGUMENT.");
                }
            }

            void VerifyUrlArgument()
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    throw new ArgumentException("MAKE SURE YOU PASSED RIGHT DEVICE URL AS 'U={DEVICE_URL}' ARGUMENT.");
                }
            }

            VerifyPathArgument();
            VerifyUrlArgument();

            _observer = new FileSystemWatcher
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

            _observer.Changed += OnFileChanged;
            _observer.Created += OnFileChanged;
            _observer.Renamed += OnFileChanged;
        }

        public void Stop()
        {
            _observer.Changed -= OnFileChanged;
            _observer.Created -= OnFileChanged;
            _observer.Renamed -= OnFileChanged;
            _observer = null;
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
