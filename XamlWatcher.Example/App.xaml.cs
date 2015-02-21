using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XamlWatcher.WPF;

namespace XamlWatcher.Example
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Watcher _watcher;
        public App()
        {
            _watcher = new Watcher(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)
            {
                OnError = Console.WriteLine, 
                OnLog = Console.WriteLine
            };
        }
    }
}
