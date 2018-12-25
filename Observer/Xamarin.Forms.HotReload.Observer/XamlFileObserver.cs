using System;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using static System.Math;

namespace Xamarin.Forms.HotReload.Observer
{
    public class XamlFileObserver
    {
        private readonly object _locker = new object();
        private DateTime _lastChangeTime;
        private FileSystemWatcher _observer;

        public event EventHandler<XamlFileChangedEventArgs> XamlFileChanged;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Start(string projectRootDirectory)
        {
            if (string.IsNullOrWhiteSpace(projectRootDirectory))
                throw new ArgumentNullException(nameof(projectRootDirectory), "Value cannot be null or white space.");
            
            if (Directory.Exists(projectRootDirectory) == false)
                throw new ArgumentException("The directory should exists.", nameof(projectRootDirectory));

            _observer = new FileSystemWatcher
            {
                Path = projectRootDirectory,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.xaml",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
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
            XamlFileChanged?.Invoke(
                this, new XamlFileChangedEventArgs(filePath, now));
        }
    }
}
