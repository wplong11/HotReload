using System;

namespace Xamarin.Forms.HotReload.Observer
{
    public class XamlFileChangedEventArgs : EventArgs
    {
        public XamlFileChangedEventArgs(
            string fileFullPath, DateTime changedTime)
        {
            FileFullPath = fileFullPath ?? throw new ArgumentNullException(nameof(fileFullPath));
            ChangedTime = changedTime;
        }

        public string FileFullPath { get; }

        public DateTime ChangedTime { get; }
    }
}
