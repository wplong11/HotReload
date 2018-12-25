using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Xamarin.Forms.HotReload.Observer
{
    public class Program
    {
        public static void Main()
        {
            new XamlFileObserver().Run();
        }
    }
}